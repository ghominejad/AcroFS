
### Cross-platform, small and super fast file-based storage library

`AcroFS` is a very small, cross-platform and super fast file system based storage library that can manage huge amounts of files on your file system




``` csharp

// defining the store
var _storage = FileStore.GetStore("c:\\store1");

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

### Download

[![NuGet version](https://img.shields.io/nuget/v/Acrobit.AcroFS.svg)](https://www.nuget.org/packages/Acrobit.AcroFS)

### How it works

It keeps the last ID in memory and generates new ones by a simple atomic +1 operation and so it's very fast.
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
var _storage = FileStore.GetStore("c:\\store1");

// store    
long docId = _storage.StoreText("the content");

// load
Assert.Equal("the content", _storage.LoadText(docId));
```
 <br/>

- Working with Streams :
``` csharp
// store    
long docId = _storage.Store(theStream);

// load
var myStream = _storage.Load(docId));
```
<br/>

- Attachments :
``` csharp
// create doc
long docId = _storage.StoreText("the content");

// store two attachments
_storage.AttachText(docId, "attach-name-1", "attachment content 1");
_storage.AttachText(docId, "attach-name-2", "attachment content 2");

// load all attachments as list of strings
IList<string> attachs = _storage.LoadTextAttachs(docId);

// load all attachments as list of streams
IList<Stream> attachs = _storage.LoadAttachs(docId);

// load "attach-name-1" 
string myAttachmentText = _storage.LoadTextAttach(docId, "attach-name-1");

```

<br/>

- GZip Compresion for Texts :
``` csharp
// store    
long docId = _storage.StoreText("a large text", 
    options: StoreOptions.Compress);

// load
var myText = _storage.LoadText(docId, 
    options: LoadOptions.Decompress);
```

<br/>

- Sub Storages 
``` csharp
//  creating news docs
long newsId1 = _storage.StoreText("news content 1", "news");
long newsId2 = _storage.StoreText("news content 2", "news");

//  creating articles docs
long articleId1 = _storage.StoreText("article content 1", "article");
long articleId2 = _storage.StoreText("article content 2", "article");

//  loading
Assert.Equal("news content 1", _storage.LoadText(newsId1, "news"));
Assert.Equal("article content 2", _storage.LoadText(articleId2, "article"));


```
The output File System :

```
StorageRoot\news\...\01
                 ...\02

StorageRoot\article\...\01
                    ...\02
````




