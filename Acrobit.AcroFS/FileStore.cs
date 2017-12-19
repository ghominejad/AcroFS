using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO.Compression;

// Gita FileSystem Storage
namespace Acrobit.AcroFS
{
	/// <summary>
	/// Storing and retriving files as gita hashed structured file system 
	/// </summary>
	public  class FileStore
	{
        static Dictionary<string , FileStore> stores = new Dictionary<string, FileStore>();

        private Core _core = null;
        public  static FileStore GetStore(string repositoryRoot)
        {
            if (stores.ContainsKey(repositoryRoot))
                return stores[repositoryRoot];

            var core = new Core(repositoryRoot);
            var filestore = new FileStore(core);
            stores[repositoryRoot] = filestore;

            return filestore;
        }
        public FileStore(Core core)
        {
            _core = core;
        }
        
		#region Storing
		
		// Storing a binary content 
		public long  Store(Stream content,   string clusterPath="", StoreOptions options = StoreOptions.None, long id=0, long clusterId=0)
		{
			// Checking content validation
			if(content==null)
				return 0;
			
			// Generating Store Id if needed
			long storeId=id;
			if(storeId==0)			
				storeId = _core.GetNewStoreId(clusterId, clusterPath);
			
			// full path of the file
			string hashedPath =
				_core.GetHashedPath(storeId, clusterId, clusterPath);
			
			// Creating top directories
			_core.PrepaireDirectoryByFilename(hashedPath);
			
			// Saving stream to the file 
			_core.SaveStreamToFile(hashedPath, content, 
			                      options== StoreOptions.Compress);
			
			// Returns new store id
			return storeId;
		}
		
		// Storing a utf8 text content 
		public  long  StoreText(string content, string clusterPath="", StoreOptions options = StoreOptions.None, long id=0, long clusterId=0)
		{	
			// Converting utf8 text to Memory Stream
			var stream = content.ToMemoryStream();
			
			// Storing to disk
			var result = Store(stream, clusterPath,options, id, clusterId);
			
			stream.Close();			
			return result;
		}
		
		// Attaching a binary content to a stored item 
		public  bool  Attach(long storeId, string attachName, Stream attachContent, string clusterPath="", StoreOptions options = StoreOptions.None, long clusterId=0)
		{
			// Checking content validation
			if(attachContent==null)
				return false;
			
			// full path of the file
			string hashedPath = _core.GetHashedPath(storeId, clusterId, clusterPath);
			
			// Prepairing top directories
			_core.PrepaireDirectoryByFilename(hashedPath);
			
			// Saving stream to the file 
			_core.SaveStreamToFile(hashedPath+"-"+attachName, attachContent, options== StoreOptions.Compress);
			
			// Returns new store id
			return true;
		}
		
		// Attaching a text content to a stored item 
		public  bool  AttachText(long storeId, string attachName, string attachContent, string clusterPath="",StoreOptions options = StoreOptions.None, long clusterId=0)
		{
			// Converting utf8 text to Memory Stream
			var stream = attachContent.ToMemoryStream();
			
			// Storing to disk
			var result = Attach(storeId, attachName, stream,clusterPath,options, clusterId);
			
			stream.Close();			
			return result;
		}
		
		// Returns a new store id, may be needed for only attachments
		public  long GetNewStoreId(string clusterPath="", long clusterId=0 )
		{
			return _core.GetNewStoreId(clusterId, clusterPath);
		}
		
		#endregion
		
		
		#region Loading
		// Loading a binary content 
		public  Stream Load(long id, string clusterPath="", LoadOptions options = LoadOptions.None, long clusterId=0)
		{
			
			// Optaining pathes
			string hashedPath = _core.GetHashedPath(id, clusterId, clusterPath);
			
			// Chekcking for existing 
			if(!System.IO.File.Exists(hashedPath))
			   return null;
			
			// Load file from disk
			Stream content= _core.LoadStreamFromFile(hashedPath, 
			               options == LoadOptions.Decompress);
			
			
			return content;
		}
		
		// Loading a utf8 text content
		public  string LoadTextUtf8(long id, string clusterPath="",LoadOptions options = LoadOptions.None, long clusterId=0)
		{
			var stream = Load( id,  clusterPath,  options, clusterId);
			if(stream==null) return null;
			
			// Converting to utf8 text
			string result = stream.ToUtf8String();
			stream.Close();	
			
			return result;
		}
        public string LoadText(long id, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var stream = Load(id, clusterPath, options, clusterId);
            if (stream == null) return null;

            string result = stream.ReadToEnd();
            stream.Close();

            return result;
        }
            // Loading a binary attached item 
        public  Stream LoadAttach(long id, string attachName, string clusterPath="", LoadOptions options = LoadOptions.None, long clusterId=0)
		{
			// Optaining pathes
			string hashedPath = _core.GetHashedPath(id, clusterId, clusterPath);
			string fullPath = hashedPath+"-"+attachName;
			
			// Checking for existing 
			if(!System.IO.File.Exists(fullPath))
			   return null;
			
			// Load file from disk
			Stream content= _core.LoadStreamFromFile(fullPath, 
			               options == LoadOptions.Decompress);
			
			return content;
		}
        public List<Stream> LoadAttachs(long id, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            // Optaining pathes
            string hashedPath = _core.GetHashedPath(id, clusterId, clusterPath);

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
        public List<string> LoadTextAttachs(long id, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var list = new List<string>();

            var streamList = LoadAttachs(id, clusterPath, options, clusterId);
         
            foreach(var stream in streamList)
            {
                list.Add(stream.ReadToEnd());
                stream.Close();
            }
            // Converting to utf8 text
       
            return list;

        }
        public  string LoadTextAttach(long id, string attachName, string clusterPath="", LoadOptions options = LoadOptions.None,long clusterId=0)
		{
			var stream = LoadAttach( id,  attachName,  clusterPath,options,  clusterId);
			if(stream==null) return null;

            // Converting to utf8 text
            string result = stream.ReadToEnd();
            stream.Close();
			
			return result;
			
		}

        // Loading a utf8 text attached item 
        public string LoadTextAttachUtf8(long id, string attachName, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var stream = LoadAttach(id, attachName, clusterPath, options, clusterId);
            if (stream == null) return null;

            // Converting to utf8 text
            string result = stream.ToUtf8String();
            stream.Close();

            return result;

        }
        #endregion


        #region Removing

        // Loading a binary content 
        public  void Remove(long id, string clusterPath="", long clusterId=0)
		{
			
			// Optaining pathes
			string hashedPath = _core.GetHashedPath(id, clusterId, clusterPath);
			
			// Chekcking for existing 
			if(!System.IO.File.Exists(hashedPath))
			   return;
			
			// Removing the file
			System.IO.File.Delete(hashedPath);
			
		}
		
		 int ccc=0;
		// Loading a binary attached item 
		public  void RemoveAttach(long id, string attachName, string clusterPath="", long clusterId=0)
		{
		
		
			
			// Optaining pathes
			string hashedPath = _core.GetHashedPath(id, clusterId, clusterPath);
			string fullPath = hashedPath+"-"+attachName;
			
		
			
			// Checking for existing 
			if(!System.IO.File.Exists(fullPath))
			   return;
			
			
			// Removing the file
			try{
			System.IO.File.Delete(fullPath);	
			}
			catch{
				
			}
			
			//ccc++;
		
		}
		
		#endregion
	}
	
	// Future options :
		// Load by gfs path : /clusterId/clusterPath/fileid/attachName/options
}

