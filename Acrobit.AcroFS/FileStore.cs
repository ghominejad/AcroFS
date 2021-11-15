using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO.Compression;
using Newtonsoft.Json;

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
        public  static FileStore GetStore(string repositoryRoot = null, StoreConfig config = null)
        {
			if (string.IsNullOrEmpty(repositoryRoot))
				repositoryRoot = Core.GetDefaultRepositoryPath();

			if (repositoryRoot!=null && stores.ContainsKey(repositoryRoot))
                return stores[repositoryRoot];

            var core = new Core(repositoryRoot, config);
            var filestore = new FileStore(core);
			
            stores[repositoryRoot] = filestore;

            return filestore;
		}
        //public static FileStore GetStore(StoreConfig config = null)
        //{

        //    if (stores.ContainsKey("default"))
        //        return stores["default"];

        //    var core = new Core(config);
        //    var filestore = new FileStore(core);
        //    stores["default"] = filestore;

        //    return filestore;
        //}


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
		public void StoreStreamByKey(string key, Stream content,   string clusterPath="", StoreOptions options = StoreOptions.None, long clusterId=0)
		{
			// Checking content validation
			if(content==null || string.IsNullOrEmpty(key))
				throw new Exception("Content or key not valid!");

			// full path of the file
			string hashedPath =
				_core.GetHashedPath(key, clusterId, clusterPath);
			
			// Creating top directories
			_core.PrepaireDirectoryByFilename(hashedPath);
			
			// Saving stream to the file 
			_core.SaveStreamToFile(hashedPath, content, 
			                      options== StoreOptions.Compress);
			
		}
		
		// Storing a utf8 text content 
		public  long  StoreText(string content, string clusterPath="", StoreOptions options = StoreOptions.None, long id=0, long clusterId=0)
		{	
			// Converting utf8 text to Memory Stream
			var stream = content.ToMemoryStream();
			
			// Storing to disk
			var result = StoreStream(stream, clusterPath,options, id, clusterId);
			
			stream.Close();			
			return result;
		}
		public void StoreTextByKey(string key, string content, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
		{
			if (content == null || string.IsNullOrEmpty(key))
				throw new Exception("Content or key not valid!");

			// Converting utf8 text to Memory Stream
			var stream = content.ToMemoryStream();

			// Storing to disk
			StoreStreamByKey(key, stream, clusterPath, options, clusterId);

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
            if (IsSubclassOfRawGeneric(typeof(Stream), typeof(T) ) )
            {
                return StoreStream(data as Stream, clusterPath, options, id, clusterId);
            }
            else if (typeof(T) == typeof(string))
            {
                return StoreText(data as string, clusterPath, options, id, clusterId);
            }

            var jsonData = JsonConvert.SerializeObject(data);
            return StoreText(jsonData, clusterPath, options, id, clusterId);
        }

		public void StoreByKey<T>(string key,T data,string clusterPath = "",  StoreOptions options = StoreOptions.None,  long clusterId = 0)
		{
			if (IsSubclassOfRawGeneric(typeof(Stream), typeof(T)))
			{
				StoreStreamByKey(key, data as Stream, clusterPath, options, clusterId);
			}
			else if (typeof(T) == typeof(string))
			{
				StoreTextByKey(key, data as string, clusterPath, options, clusterId);

			}

			var jsonData = JsonConvert.SerializeObject(data);
			StoreTextByKey(key, jsonData, clusterPath, options, clusterId);
		}


		// Attaching a binary content to a stored item 
		public bool  AttachStream(long storeId, string attachName, Stream attachContent, string clusterPath="", StoreOptions options = StoreOptions.None, long clusterId=0)
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
			var result = AttachStream(storeId, attachName, stream,clusterPath,options, clusterId);
			
			stream.Close();			
			return result;
		}

		public bool Attach<T>(long storeId, string attachName, T attachContent, string clusterPath = "", StoreOptions options = StoreOptions.None, long clusterId = 0)
		{
			if (IsSubclassOfRawGeneric(typeof(Stream), typeof(T)))
			{
				return AttachStream(storeId, attachName, attachContent as Stream, clusterPath, options, clusterId);

			}
			else if (typeof(T) == typeof(string))
			{
				return AttachText(storeId, attachName, attachContent as string, clusterPath , options, clusterId);
			}


			var jsonData = JsonConvert.SerializeObject(attachContent);
			return AttachText(storeId, attachName, jsonData, clusterPath, options, clusterId);

		}


		// Returns a new store id, may be needed for only attachments
		public  long GetNewStoreId(string clusterPath="", long clusterId=0 )
		{
			return _core.GetNewStoreId(clusterId, clusterPath);
		}
		
		#endregion
		
		
		#region Load
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
		public Stream Load(string docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
		{

			// Optaining pathes
			string hashedPath = _core.GetHashedPath(docKey, clusterId, clusterPath);

			// Chekcking for existing 
			if (!System.IO.File.Exists(hashedPath))
				throw new Exception("Document not found!");

			// Load file from disk
			Stream content = _core.LoadStreamFromFile(hashedPath,
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
		public string LoadTextUtf8(string key, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
		{
			var stream = Load(key, clusterPath, options, clusterId);
			if (stream == null) return null;

			// Converting to utf8 text
			string result = stream.ToUtf8String();
			stream.Close();

			return result;
		}

		public T Load<T>(string docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
		{
			var jsonData = LoadTextUtf8(docKey, clusterPath, options, clusterId);
			return JsonConvert.DeserializeObject<T>(jsonData);

		}

		public T Load<T>(long docKey, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
			var jsonData = LoadTextUtf8(docKey, clusterPath, options, clusterId);
			return JsonConvert.DeserializeObject<T>(jsonData);

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
        public  Stream LoadStreamAttachment(long id, string attachName, string clusterPath="", LoadOptions options = LoadOptions.None, long clusterId=0)
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
        public List<Stream> LoadStreamAttachments(long id, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
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
        public List<string> LoadTextAttachments(long id, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var list = new List<string>();

            var streamList = LoadStreamAttachments(id, clusterPath, options, clusterId);
         
            foreach(var stream in streamList)
            {
                list.Add(stream.ReadToEnd());
                stream.Close();
            }
            // Converting to utf8 text
       
            return list;

        }
        public  string LoadTextAttachment(long id, string attachName, string clusterPath="", LoadOptions options = LoadOptions.None,long clusterId=0)
		{
			var stream = LoadStreamAttachment( id,  attachName,  clusterPath,options,  clusterId);
			if(stream==null) return null;

            // Converting to utf8 text
            string result = stream.ReadToEnd();
            stream.Close();
			
			return result;
			
		}

        // Loading a utf8 text attached item 
        public string LoadTextAttachmentUtf8(long id, string attachName, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
        {
            var stream = LoadStreamAttachment(id, attachName, clusterPath, options, clusterId);
            if (stream == null) return null;

            // Converting to utf8 text
            string result = stream.ToUtf8String();
            stream.Close();

            return result;

        }
		public T LoadAttachment<T>(long id, string attachName, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
		{
			var jsonData = LoadTextAttachmentUtf8(id, attachName, clusterPath, options, clusterId);
			if (jsonData == null) throw new Exception("The attachment can't be loaded!");

			return JsonConvert.DeserializeObject<T>(jsonData);

		}

		public IList<T> LoadAttachments<T>(long id, string clusterPath = "", LoadOptions options = LoadOptions.None, long clusterId = 0)
		{
			var list = new List<T>();

			var jsonList = LoadTextAttachments(id, clusterPath, options, clusterId);

			foreach (var jsonData in jsonList)
			{
				list.Add(JsonConvert.DeserializeObject<T>(jsonData));
			}

			return list;

		}

		#endregion

		#region Remove

		// Loading a binary content 
		public void Remove(long id, string clusterPath="", long clusterId=0)
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
		public  void RemoveAttachment(long id, string attachName, string clusterPath="", long clusterId=0)
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

