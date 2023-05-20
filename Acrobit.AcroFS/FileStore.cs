using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Threading.Tasks;

// Gita FileSystem Storage
namespace Acrobit.AcroFS
{
    /// <summary>
    /// Storing and retriving files as gita hashed structured file system 
    /// </summary>
    public class FileStore
    {
        static Dictionary<string, FileStore> stores = new Dictionary<string, FileStore>();
        private Core _core = null;

        public static FileStore CreateStore(string repositoryRoot = null, StoreConfig config = null)
        {
            if (string.IsNullOrEmpty(repositoryRoot))
                repositoryRoot = Core.GetDefaultRepositoryPath();

            if (repositoryRoot != null && stores.ContainsKey(repositoryRoot))
                return stores[repositoryRoot];

            var core = new Core(repositoryRoot, config);
            var filestore = new FileStore(core);

            stores[repositoryRoot] = filestore;

            return filestore;
        }

        [Obsolete("This method is obsolete. use CreateStore instead")]
        public static FileStore GetStore(string repositoryRoot = null, StoreConfig config = null) => CreateStore(repositoryRoot, config);

        public bool Exists(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            string hashedPath = _core.GetHashedPath(docKey, clusterId, clusterPath);
            return File.Exists(hashedPath);
        }

        public FileStore(Core core)
        {
            _core = core;
        }

        #region Save

        // Storing a binary content 
        public long StoreStream(Stream content, string clusterPath = "", StoreOptions options = StoreOptions.None, long id = 0, long clusterId = 0)
        {
            // Checking content validation
            if (content == null)
                return 0;

            // Generating Store Id if needed
            long storeId = id;
            if (storeId == 0)
                storeId = _core.GetNewStoreId(clusterId, clusterPath);

            // full path of the file
            string hashedPath =
                _core.GetHashedPath(storeId, clusterId, clusterPath);

            // Creating top directories
            _core.PrepaireDirectoryByFilename(hashedPath);

            // Saving stream to the file 
            _core.SaveStreamToFile(hashedPath, content,
                                  options == StoreOptions.Compress);

            // Returns new store id
            return storeId;
        }

        public void StoreStreamByKey(object key, Stream content, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            // Checking content validation
            if (content == null || key == null)
                throw new Exception("Content or key not valid!");

            // full path of the file
            string hashedPath =
                _core.GetHashedPath(key, clusterId, clusterPath);

            // Creating top directories
            _core.PrepaireDirectoryByFilename(hashedPath);

            // Saving stream to the file 
            _core.SaveStreamToFile(hashedPath, content, options == StoreOptions.Compress);
        }

        public async Task StoreStreamByKeyAsync(object key, Stream content, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            // Checking content validation
            if (content == null || key == null)
                throw new Exception("Content or key not valid!");

            // full path of the file
            string hashedPath =
                _core.GetHashedPath(key, clusterId, clusterPath);

            // Creating top directories
            _core.PrepaireDirectoryByFilename(hashedPath);

            // Saving stream to the file 
            await _core.SaveStreamToFileAsync(hashedPath, content, options == StoreOptions.Compress);
        }

        // Storing a utf8 text content 
        public long StoreText(string content, string clusterPath = "", StoreOptions options = StoreOptions.None, long id = 0, long clusterId = 0)
        {
            // Converting utf8 text to Memory Stream
            var stream = content.ToMemoryStream();

            // Storing to disk
            var result = StoreStream(stream, clusterPath, options, id, clusterId);

            stream.Close();
            return result;
        }

        public void StoreTextByKey(object key, string content, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            if (content == null || key == null)
                throw new Exception("Content or key not valid!");

            // Converting utf8 text to Memory Stream
            var stream = content.ToMemoryStream();

            // Storing to disk
            StoreStreamByKey(key, stream, clusterPath, options, clusterId);

            stream.Close();
        }

        public async Task StoreTextByKeyAsync(object key, string content, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            if (content == null || key == null)
                throw new Exception("Content or key not valid!");

            // Converting utf8 text to Memory Stream
            var stream = content.ToMemoryStream();

            // Storing to disk
            await StoreStreamByKeyAsync(key, stream, clusterPath, options, clusterId);

            stream.Close();
        }

        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public long Store<T>(T data, string clusterPath = "", StoreOptions options = StoreOptions.None, long id = 0, long clusterId = 0)
        {
            if (data is Stream stream)
            {
                return StoreStream(stream, clusterPath, options, id, clusterId);
            }
            else if (data is string s)
            {
                return StoreText(s, clusterPath, options, id, clusterId);
            }

            var jsonData = JsonConvert.SerializeObject(data);
            return StoreText(jsonData, clusterPath, options, id, clusterId);
        }

        public void StoreByKey<T>(object key, T data, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            if (data is Stream stream)
            {
                StoreStreamByKey(key, stream, clusterPath, options, clusterId);
            }
            else if (data is string s)
            {
                StoreTextByKey(key, s, clusterPath, options, clusterId);
            }

            var jsonData = JsonConvert.SerializeObject(data);
            StoreTextByKey(key, jsonData, clusterPath, options, clusterId);
        }

        public async Task StoreByKeyAsync<T>(object key, T data, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            if (data is Stream stream)
            {
                await StoreStreamByKeyAsync(key, stream, clusterPath, options, clusterId);
            }
            else if (data is string s)
            {
                await StoreTextByKeyAsync(key, s, clusterPath, options, clusterId);
            }

            var jsonData = JsonConvert.SerializeObject(data);
            await StoreTextByKeyAsync(key, jsonData, clusterPath, options, clusterId);
        }

        // Attaching a binary content to a stored item
        public void AttachStream(object key, string attachName, Stream attachContent, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            // Checking content validation
            if (attachContent == null)
                throw new Exception("The attachment content is null");

            // full path of the file
            string hashedPath = _core.GetHashedPath(key, clusterId, clusterPath);

            // Prepairing top directories
            _core.PrepaireDirectoryByFilename(hashedPath);

            // Saving stream to the file 
            _core.SaveStreamToFile(hashedPath + "-" + attachName, attachContent, options == StoreOptions.Compress);
        }

        public async Task AttachStreamAsync(object key, string attachName, Stream attachContent, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            // Checking content validation
            if (attachContent == null)
                throw new Exception("The attachment content is null");

            // full path of the file
            string hashedPath = _core.GetHashedPath(key, clusterId, clusterPath);

            // Prepairing top directories
            _core.PrepaireDirectoryByFilename(hashedPath);

            // Saving stream to the file 
            await _core.SaveStreamToFileAsync(hashedPath + "-" + attachName, attachContent, options == StoreOptions.Compress);
        }

        // Attaching a text content to a stored item 
        public void AttachText(object key, string attachName, string attachContent, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            // Converting utf8 text to Memory Stream
            var stream = attachContent.ToMemoryStream();

            // Store into disk
            AttachStream(key, attachName, stream, clusterPath, options, clusterId);

            stream.Close();
        }

        public async Task AttachTextAsync(object key, string attachName, string attachContent, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            // Converting utf8 text to Memory Stream
            var stream = attachContent.ToMemoryStream();

            // Store into disk
            await AttachStreamAsync(key, attachName, stream, clusterPath, options, clusterId);

            stream.Close();
        }

        public void Attach<T>(object key, string attachName, T attachContent, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            if (attachContent is Stream stream)
            {
                AttachStream(key, attachName, stream, clusterPath, options, clusterId);

            }
            else if (attachContent is string s)
            {
                AttachText(key, attachName, s, clusterPath, options, clusterId);
            }

            var jsonData = JsonConvert.SerializeObject(attachContent);
            AttachText(key, attachName, jsonData, clusterPath, options, clusterId);
        }

        public async Task AttachAsync<T>(object key, string attachName, T attachContent, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
        {
            if (attachContent is Stream stream)
            {
                await AttachStreamAsync(key, attachName, stream, clusterPath, options, clusterId);
            }
            else if (attachContent is string s)
            {
                await AttachTextAsync(key, attachName, s, clusterPath, options, clusterId);
            }

            var jsonData = JsonConvert.SerializeObject(attachContent);
            await AttachTextAsync(key, attachName, jsonData, clusterPath, options, clusterId);
        }

        // Returns a new store id, may be needed for only attachments
        public long GetNewStoreId(string clusterPath = "", long clusterId = 0)
        {
            return _core.GetNewStoreId(clusterId, clusterPath);
        }

        #endregion

        #region Load

        // Loading a binary content 
        public Stream Load(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            string hashedPath = _core.GetHashedPath(docKey, clusterId, clusterPath);

            if (!File.Exists(hashedPath))
                return null;

            Stream content = _core.LoadStreamFromFile(hashedPath, options == LoadOptions.Decompress);

            return content;
        }

        public async Task<Stream> LoadAsync(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            string hashedPath = _core.GetHashedPath(docKey, clusterId, clusterPath);

            if (!File.Exists(hashedPath))
                return null;

            Stream content = await _core.LoadStreamFromFileAsync(hashedPath, options == LoadOptions.Decompress);

            return content;
        }

        // Loading a utf8 text content
        public string LoadTextUtf8(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var stream = Load(docKey, clusterPath, options, clusterId);
            if (stream == null)
            {
                return null;
            }

            // Converting to utf8 text
            string result = stream.ToUtf8String();
            stream.Close();

            return result;
        }

        public async Task<string> LoadTextUtf8Async(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var stream = await LoadAsync(docKey, clusterPath, options, clusterId);
            if (stream == null)
            {
                return null;
            }

            // Converting to utf8 text
            string result = await stream.ToUtf8StringAsync();
            stream.Close();

            return result;
        }

        public T Load<T>(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var jsonData = LoadTextUtf8(docKey, clusterPath, options, clusterId);
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        public async Task<T> LoadAsync<T>(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var jsonData = await LoadTextUtf8Async(docKey, clusterPath, options, clusterId);
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        public string LoadText(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var stream = Load(docKey, clusterPath, options, clusterId);
            if (stream == null) return null;

            string result = stream.ReadToEnd();
            stream.Close();

            return result;
        }

        // Loading a binary attached item 
        public Stream LoadStreamAttachment(object key, string attachName, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            // Optaining pathes
            string hashedPath = _core.GetHashedPath(key, clusterId, clusterPath);
            string fullPath = hashedPath + "-" + attachName;

            // Checking for existing 
            if (!File.Exists(fullPath))
                return null;

            // Load file from disk
            Stream content = _core.LoadStreamFromFile(fullPath,
                           options == LoadOptions.Decompress);

            return content;
        }

        public List<Stream> LoadStreamAttachments(object key, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            // Optaining pathes
            string hashedPath = _core.GetHashedPath(key, clusterId, clusterPath);

            var pos = hashedPath.LastIndexOf('$');
            var docName = hashedPath.Substring(pos);
            var parentFolder = hashedPath.Substring(0, pos);

            var paths = _core.GetFilesStartByTerm(parentFolder, docName + "-");

            var contents = new List<Stream>();
            foreach (var path in paths)
            {
                contents.Add(_core.LoadStreamFromFile(path,
                           options == LoadOptions.Decompress));
            }

            return contents;
        }

        public List<string> LoadTextAttachments(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var list = new List<string>();

            var streamList = LoadStreamAttachments(docKey, clusterPath, options, clusterId);

            foreach (var stream in streamList)
            {
                list.Add(stream.ReadToEnd());
                stream.Close();
            }
            // Converting to utf8 text

            return list;

        }

        public string LoadTextAttachment(object docKey, string attachName, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var stream = LoadStreamAttachment(docKey, attachName, clusterPath, options, clusterId);
            if (stream == null) return null;

            // Converting to utf8 text
            string result = stream.ReadToEnd();
            stream.Close();

            return result;

        }

        // Loading a utf8 text attached item 
        public string LoadTextAttachmentUtf8(object docKey, string attachName, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var stream = LoadStreamAttachment(docKey, attachName, clusterPath, options, clusterId);
            if (stream == null) return null;

            // Converting to utf8 text
            string result = stream.ToUtf8String();
            stream.Close();

            return result;

        }

        public T LoadAttachment<T>(object docKey, string attachName, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var jsonData = LoadTextAttachmentUtf8(docKey, attachName, clusterPath, options, clusterId);

            if (jsonData == null) return default(T);

            return JsonConvert.DeserializeObject<T>(jsonData);

        }


        public IList<T> LoadAttachments<T>(object docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var list = new List<T>();

            var jsonList = LoadTextAttachments(docKey, clusterPath, options, clusterId);

            foreach (var jsonData in jsonList)
            {
                list.Add(JsonConvert.DeserializeObject<T>(jsonData));
            }

            return list;
        }

        #endregion

        #region Remove

        // Loading a binary content 
        public void Remove(object key, string clusterPath = "", long clusterId = 0)
        {

            // Optaining pathes
            string hashedPath = _core.GetHashedPath(key, clusterId, clusterPath);

            // Chekcking for existing 
            if (!System.IO.File.Exists(hashedPath))
                return;

            // Removing the file
            System.IO.File.Delete(hashedPath);

        }

        // Loading a binary attached item 
        public void RemoveAttachment(object key, string attachName, string clusterPath = "", long clusterId = 0)
        {
            // Optaining pathes
            string hashedPath = _core.GetHashedPath(key, clusterId, clusterPath);
            string fullPath = hashedPath + "-" + attachName;

            // Checking for existing 
            if (!File.Exists(fullPath))
                return;

            // Removing the file
            try
            {
                File.Delete(fullPath);
            }
            catch
            {

            }
        }

        #endregion

        #region Options

        public FileStore UseSimplePath(bool useSimplePath = true)
        {
            _core.UseSimplePath(useSimplePath);
            return this;
        }
        public FileStore Root(string repositoryRoot)
        {
            _core.Root(repositoryRoot);
            return this;
        }

        #endregion
    }

    // Future options :
    // Load by gfs path : /clusterId/clusterPath/fileid/attachName/options
}

