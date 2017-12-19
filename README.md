
### Simple, scalable and super fast file system based storage library

`AcroFS` is a very small, simple, scalable and super fast file system based storage library that can manage huge amounts of files on your file system




``` csharp

// store
var docId = _storage.Store(data);

// load
var data = _storage.Load(docId);

```


### Features
- Generates unique ids as integer
- Multiple Storage
- Sub Storages
- GZip Compression / Decompression for text files
- Store/Load Texts and Streams
- Store/Load Attachments related to a doc
- .Net Core / Mono / Linux / Mac Support 

### How it works

It keeps the last ID in memory and generates new ones by a simple atomic +1 operation and it is very fast.
It converts a doc id into a hex number and stores it as 4 hirarchy folders

    StorageRoot\12\31\26\5A\17

The last hex number ( in this example `17` ) is the stored file.

[//]: # ( By default configuration it currently can store billions of files or even more, simply by changing the configuration!)

You can assign attachments to a file. they are stored as `Filename`[-]`AttachName`

For example, two attachment files are assigned to `documentId  17`  as following file names:

```
StorageRoot\...\17
            ...\17-attachmentFile1
            ...\17-attachmentFile2
````


### File Storage Examples
- Storing / Loading Texts :
``` csharp
// defining the store
var _store = FileStore.GetStore("c:\\store1");

// store    
long docId = _store.StoreText("the content");

// load
Assert.Equal("the content", _store.LoadText(docId));
```
 <br/>

- Storing / Loading Streams :
``` csharp
// store    
long docId = _store.Store(theStream);

// load
var myStream = _store.Load(docId));
```
<br/>

- Attachments :
``` csharp
// create doc
long docId = _store.StoreText("the content");

// store two attachments
_store.AttachText(docId, "attach-name-1", "attachment content 1");
_store.AttachText(docId, "attach-name-2", "attachment content 2");

// load all attachments as list of strings
IList<string> attachs = _store.LoadTextAttachs(docId);

// load all attachments as list of streams
IList<Stream> attachs = _store.LoadAttachs(docId);

// load "attach-name-1" 
string myAttachmentText = _store.LoadTextAttach(docId, "attach-name-1");

```

<br/>

- GZip Compresion for Texts :
``` csharp
// store    
long docId = _store.StoreText("a large text", 
    options: StoreOptions.Compress);

// load
var myText = _store.LoadText(docId, 
    options: LoadOptions.Decompress);
```

<br/>

- Sub Storages 
``` csharp
//  creating news docs
long newsId1 = _store.StoreText("news content 1", "news");
long newsId2 = _store.StoreText("news content 2", "news");

//  creating articles docs
long articleId1 = _store.StoreText("article content 1", "article");
long articleId2 = _store.StoreText("article content 2", "article");

//  loading
Assert.Equal("news content 1", _store.LoadText(newsId1, "news"));
Assert.Equal("article content 2", _store.LoadText(articleId2, "article"));


```
The output File System :

```
StorageRoot\news\...\01
                 ...\02

StorageRoot\article\...\01
                    ...\02
````




