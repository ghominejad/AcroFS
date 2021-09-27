using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO.Compression;

// Gita FileSystem Storage
namespace Acrobit.AcroFS
{
	public class Core
	{
		
		public IStoreConfig _config;
        string _repositoryRoot = null;

        public string RepositoryRoot{
			get{
				
				return _repositoryRoot;
			}		
		}
		

        public Core(string repositoryRoot)
        {
            if (!Directory.Exists(repositoryRoot))
                throw new RepositoryNotFoundException();

            _repositoryRoot = repositoryRoot;
        }

        public string GenerateHashedPath(long val)
		{
			if(val==0)
				return "";
			
			string hex = val.ToString("X");
			hex=hex.PadLeft(10, '0');
			//if((hex.Length%2)==1)
			//	hex="0"+hex;
			
			string hashed="";
			for(int i=0; i<hex.Length; i+=2 )
			{
				hashed += "$"+hex.Substring(i,2);
				
				if(i+2<hex.Length)
					hashed+="/";
			}
			
			return hashed;
		}
		
		public ConcurrentDictionary<long, ConcurrentDictionary<string, StoreId>> dicClusters = 
			new ConcurrentDictionary<long, ConcurrentDictionary<string, StoreId>>();
	
		public string GetHashedPath(long id, long clusterId, string clusterPath)
		{
			
			string left=GenerateHashedPath(clusterId);
			
			string right=GenerateHashedPath(id);		
			
			string path = RepositoryRoot;
			
			if(left.Length>0)
				 path+= left+"/";
			
			if(clusterPath.Length>0)
				 path+= clusterPath+"/";
			
			if(right.Length>0)
				path+=right;
			
			return path;
			
		}
        public string GetHashedPath(long id,  string clusterPath)
        {
            return GetHashedPath(id, 0, clusterPath);
        }

        public string GetClusterHashedPath(long clusterId, string clusterPath)
		{
			return GetHashedPath(0, clusterId, clusterPath);
		}
		
		public long GetNewStoreId(long clusterId, string clusterPath)
		{
			string clusterhashedPath = GetClusterHashedPath(clusterId, clusterPath);
			long newStoreId=0;
			
			if(!dicClusters.ContainsKey(clusterId))
				dicClusters.TryAdd(clusterId, new ConcurrentDictionary<string, StoreId>());
			
			if(!dicClusters[clusterId].ContainsKey(clusterPath))
				dicClusters[clusterId][clusterPath]= new StoreId(this, clusterhashedPath);
			
			newStoreId = dicClusters[clusterId][clusterPath].GetNewId();

			return newStoreId;
		}
        public long GetNewStoreId(string clusterPath = "")
        {
            return GetNewStoreId(0, clusterPath);
        }

        public string [] GetDirs(string hashingPath, bool justGFS=true)
		{
			string [] dirs;
			if(justGFS)
			{
				dirs= System.IO.Directory.GetDirectories(hashingPath, "$*");
				for(int i=0; i<dirs.Length; i++)
				{	
					dirs[i] = dirs[i].Substring(
				 		dirs[i].LastIndexOf("$"));
					
				}
			}
			else 
				dirs= System.IO.Directory.GetDirectories(hashingPath);
			
			return dirs;
		}
		
		public  string [] GetFiles(string hashingPath, bool justGFS=true)
		{
			string [] files;
			if(justGFS)
			{
				files= System.IO.Directory.GetFiles(hashingPath, "$*");
				for(int i=0; i<files.Length; i++)
				{	
					files[i] = files[i].Substring(
				 		files[i].LastIndexOf("$"));
					
				}
			}
			else 
				files= System.IO.Directory.GetFiles(hashingPath);
			
			return files;
			
		}
        public string[] GetFilesStartByTerm(string hashingPath, string startByTerm)
        {
            string[] files;
   
            files = Directory.GetFiles(hashingPath, startByTerm+"*");
            
           

            return files;

        }
        public void GetBytes(Stream stream, byte[] bytesInStream)
		{
			//byte[] bytesInStream = new byte[stream.Length];
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
				if(compress){
					using (GZipStream Compress = 
	                        	new GZipStream(outFile, 
	                        	CompressionMode.Compress))                        				
					{
						// Copy the source file into 
	                    // the compression stream.
	                    stream.CopyTo(Compress);
					}
				}else{
					stream.CopyTo(outFile);
				}
			}
		}
		
		public Stream LoadStreamFromFile(string fileFullPath, bool decompress)
		{
			Stream content= System.IO.File.OpenRead(fileFullPath);
			
			// Compresstion
			if(decompress)
			{	
				var deccontent = new GZipStream(content, CompressionMode.Decompress, false);
				
				MemoryStream ms=new MemoryStream();	
				
				deccontent.CopyTo(ms);
				long ln = ms.Length;
				deccontent.Close();
				return ms;
			}
			
			return content;
		}
		
		
		public void SaveStreamToFileOld(string fileFullPath, Stream stream, bool compress)
		{
		
		    //if (stream.Length == 0) return;
		
		    // Create a FileStream object to write a stream to a file
		    using (FileStream fileStream = System.IO.File.Create(fileFullPath))
		    {
				stream.CopyTo(fileStream);
				/*
				if(stream.Length!=0){
					
					
					
		        // Fill the bytes[] array with the stream data
		        byte[] bytesInStream = new byte[stream.Length];
		        stream.Read(bytesInStream, 0, (int)bytesInStream.Length);
		
		        // Use FileStream object to write to the specified file
		        fileStream.Write(bytesInStream, 0, bytesInStream.Length);
				}*/
				
		    }
		}
		
		public void SaveStringToFile(string fileFullPath, string content)
		{
		    //if (stream.Length == 0) return;
		
		    // Create a FileStream object to write a stream to a file
		    using (FileStream fileStream = System.IO.File.Create(fileFullPath))
		    {
				if(content.Length!=0){				
					
					using(StreamWriter sw=new StreamWriter(fileStream))
						sw.Write(content);				
					
				}
		    }
		}
		
		public void PrepaireDirectoryByFilename(string filename)
		{
			string folderPath = filename.Substring(0, filename.Length-3);
			System.IO.Directory.CreateDirectory(folderPath);
		}
		
		

	}
}

