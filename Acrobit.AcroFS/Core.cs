using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

// Gita FileSystem Storage
namespace Acrobit.AcroFS
{
    public class Core
    {
        public StoreConfig _config;
        string _repositoryRoot;

        public string RepositoryRoot
        {
            get
            {
                return _repositoryRoot;
            }
        }

        public static string GetDefaultRepositoryPath()
        {
            var defaultPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "default-store/"
                );

            return defaultPath;
        }

        public Core(string? repositoryRoot = null, StoreConfig? config = null)
        {
            _config = config ?? new StoreConfig
            {
                UseSimplePath = false,
            };

            _repositoryRoot = Root(repositoryRoot);
        }

        public string GenerateHashedPath(object key)
        {
            if (key.GetType() == typeof(string))
                return GenerateHashedPath((string)key);
            if (key.GetType() == typeof(int))
                return GenerateHashedPath((int)key);
            else
                return GenerateHashedPath((long)key);
        }

        public string GenerateHashedPath(long val)
        {
            if (val == 0)
                return "";

            string hex = val.ToString("X");
            hex = hex.PadLeft(10, '0');

            string hashed = "";
            for (int i = 0; i < hex.Length; i += 2)
            {
                hashed += "$" + hex.Substring(i, 2);

                if (i + 2 < hex.Length)
                    hashed += "/";
            }

            return hashed;
        }
        public string GenerateHashedPath(string val)
        {
            if (string.IsNullOrEmpty(val))
                return "";

            if (_config != null && _config.UseSimplePath)
                return val;

            val = val.PadLeft(10, '0');

            if (val.Length % 2 == 1) val = "_" + val;

            string hashed = "";
            for (int i = 0; i < val.Length; i += 2)
            {
                hashed += "$" + val.Substring(i, 2);

                if (i + 2 < val.Length)
                    hashed += "/";
            }

            return hashed;
        }

        public ConcurrentDictionary<long, ConcurrentDictionary<string, StoreId>> dicClusters =
            new ConcurrentDictionary<long, ConcurrentDictionary<string, StoreId>>();

        public string GetHashedPath(object key, long clusterId, string clusterPath)
        {
            string left = GenerateHashedPath(clusterId);

            string right = GenerateHashedPath(key);

            string path = RepositoryRoot;

            if (left.Length > 0)
                path += left + "/";

            if (clusterPath.Length > 0)
                path += clusterPath + "/";

            if (right.Length > 0)
                path += right;

            return path;
        }

        public string GetHashedPath(long id, string clusterPath)
        {
            return GetHashedPath(id, 0, clusterPath);
        }

        public string GetClusterHashedPath(long clusterId, string clusterPath)
        {
            return GetHashedPath(0, clusterId, clusterPath);
        }

        public string GetHashedPath(string id, string clusterPath)
        {
            return GetHashedPath(id, 0, clusterPath);
        }

        public long GetNewStoreId(long clusterId, string clusterPath)
        {
            string clusterhashedPath = GetClusterHashedPath(clusterId, clusterPath);

            if (!dicClusters.ContainsKey(clusterId))
                dicClusters.TryAdd(clusterId, new ConcurrentDictionary<string, StoreId>());

            if (!dicClusters[clusterId].ContainsKey(clusterPath))
                dicClusters[clusterId][clusterPath] = new StoreId(this, clusterhashedPath);

            long newStoreId = dicClusters[clusterId][clusterPath].GetNewId();
            return newStoreId;
        }

        public long GetNewStoreId(string clusterPath = "")
        {
            return GetNewStoreId(0, clusterPath);
        }

        public string[] GetDirs(string hashingPath, bool justGFS = true)
        {
            string[] dirs;
            if (justGFS)
            {
                dirs = Directory.GetDirectories(hashingPath, "$*");
                for (int i = 0; i < dirs.Length; i++)
                {
                    dirs[i] = dirs[i].Substring(
                         dirs[i].LastIndexOf("$"));

                }
            }
            else
                dirs = System.IO.Directory.GetDirectories(hashingPath);

            return dirs;
        }

        public string[] GetFiles(string hashingPath, bool justGFS = true)
        {
            string[] files;
            if (justGFS)
            {
                files = Directory.GetFiles(hashingPath, "$*");
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = files[i].Substring(
                         files[i].LastIndexOf("$"));

                }
            }
            else
                files = Directory.GetFiles(hashingPath);

            return files;
        }

        public string[] GetFilesStartByTerm(string hashingPath, string startByTerm)
        {
            return Directory.GetFiles(hashingPath, startByTerm + "*");
        }

        public void GetBytes(Stream stream, byte[] bytesInStream)
        {
            stream.Read(bytesInStream, 0, (int)bytesInStream.Length);
        }

        public byte[] GetBytes(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            byte[] bytesInStream = new byte[stream.Length];
            stream.Read(bytesInStream, 0, (int)bytesInStream.Length);

            return bytesInStream;
        }

        public void SaveStreamToFile(string fileFullPath, Stream stream, bool compress)
        {
            // Create the compressed file.
            using (FileStream outFile = File.Create(fileFullPath))
            {
                if (compress)
                {
                    using (GZipStream Compress =
                                new GZipStream(outFile,
                                CompressionMode.Compress))
                    {
                        // Copy the source file into 
                        // the compression stream.
                        stream.CopyTo(Compress);
                    }
                }
                else
                {
                    stream.CopyTo(outFile);
                }
            }
        }

        public async Task SaveStreamToFileAsync(string fileFullPath, Stream stream, bool doCompress)
        {
            // Create the compressed file.
            using (FileStream outFile = File.Create(fileFullPath))
            {
                if (doCompress)
                {
                    using (GZipStream compress =
                                new GZipStream(outFile,
                                CompressionMode.Compress))
                    {
                        await stream.CopyToAsync(compress);
                    }
                }
                else
                {
                    await stream.CopyToAsync(outFile);
                }
            }
        }

        public Stream LoadStreamFromFile(string fileFullPath, bool decompress)
        {
            Stream content = System.IO.File.OpenRead(fileFullPath);

            // Compresstion
            if (decompress)
            {
                var deccontent = new GZipStream(content, CompressionMode.Decompress, false);

                MemoryStream ms = new MemoryStream();

                deccontent.CopyTo(ms);
                deccontent.Close();

                return ms;
            }

            return content;
        }

        public async Task<Stream> LoadStreamFromFileAsync(string fileFullPath, bool decompress)
        {
            Stream content = File.OpenRead(fileFullPath);

            // Compresstion
            if (decompress)
            {
                var deccontent = new GZipStream(content, CompressionMode.Decompress, false);

                MemoryStream ms = new MemoryStream();

                await deccontent.CopyToAsync(ms);
                deccontent.Close();

                return ms;
            }

            return content;
        }

        public void SaveStringToFile(string fileFullPath, string content)
        {
            // Create a FileStream object to write a stream to a file
            using FileStream fileStream = File.Create(fileFullPath);

            if (content.Length != 0)
            {
                using (StreamWriter sw = new StreamWriter(fileStream))
                    sw.Write(content);
            }
        }

        public void PrepaireDirectoryByFilename(string filename)
        {
            var index = filename.LastIndexOf('/');
            if (index < 0) return;

            string folderPath = filename.Substring(0, index + 1);

            Directory.CreateDirectory(folderPath);
        }

        #region Options
        public Core UseSimplePath(bool useSimplePath = true)
        {
            _config.UseSimplePath = useSimplePath;
            return this;
        }

        public string Root(string? repositoryRoot)
        {
            if (string.IsNullOrEmpty(repositoryRoot))
                repositoryRoot = GetDefaultRepositoryPath();

            if (repositoryRoot.Last() != '/')
                repositoryRoot += '/';

            if (!Directory.Exists(repositoryRoot))
                Directory.CreateDirectory(repositoryRoot);

            _repositoryRoot = repositoryRoot;

            return _repositoryRoot;
        }

        #endregion

    }
}

