
## Cross-platform, small and super fast file-based storage library

`AcroFS` is a tiny, cross-platform and super fast file system based storage library that can manage huge amount of files on your file system.

It also provides a persistent caching layer for .Net `IMemoryCache` to benefit both file caching and memory caching together!


## File store quick examples :
``` csharp
// Get the default store
var _repository = FileStore.CreateStore();
```
``` csharp
// store
var docId = _repository.Store(data);

// load
var data = _repository.Load(docId);

```

``` csharp
// store models with a custpm key
_repository.Store("the-key", myModel);

// load model
var model = _repository.Load<MyModel>("the-key", myModel);

```

## Persistent cache quick examples : 
``` csharp
// set cache
_memoryCache.Persistent().Set(key, value, TimeSpan.FromMinutes(1));

// get cache
var found = _memoryCache.Persistent().TryGetValue(cacheKey, out result);

// get or create
var cachedResult = await _memoryCache.Persistent().GetOrCreate(cacheKey, async entry =>
{
    entry.SlidingExpiration = TimeSpan.FromSeconds(10);

    var result = await loadMyDataAsync();
    
    return result;
});

```

# Features

- Multiple Storage
- Sub Storages
- Persistent cache layer for .Net `IMemoryCache`
- Json Serialization/Deserialization
- GZip Compression / Decompression for text files
- Store/Load Models, Objects, Texts and Streams
- Store/Load Attachments related to a doc
- Assigns unique ids automatically
- .Net Core / Mono / Linux / Mac Support 


# Download

[![NuGet version](https://img.shields.io/nuget/v/Acrobit.AcroFS.svg)](https://www.nuget.org/packages/Acrobit.AcroFS)




# Examples
<br/>

## Create or get the default repository
``` csharp
var _repository = FileStore.CreateStore();
```
> All files will be stored in `./Data/default-store` 


## Store and load models

``` csharp
// store    
long docId = _repository.Store<MyModel>(model1);

// load
var myModel = _repository.Load<MyModel>(docId));
```


## Store and load by a key
``` csharp
var key ="MyModel";
// store
_store.StoreByKey(key, data);

// load
var myModel = _repository.Load<MyModel>(key);
```

## Store and load texts
``` csharp

// store    
long docId = _repository.StoreText("the content");

// load
Assert.Equal("the content", _repository.LoadText(docId));
```

 <br/>

## Store and load Streams
``` csharp
// store    
long docId = _repository.Store(theStream);

// load
var myStream = _repository.Load(docId));
```



## Use simple path instead of hashed path
``` csharp
var _repository = FileStore.CreateStore()
    .UseSimplePath();

var key ="MyModel.json";
// store
_store.StoreByKey(key, data);

// load
var myModel = _repository.Load<MyModel>(key));
```
## Create the store in a custom location
```CSHARP
var _repository = FileStore.CreateStore("c:\\store1");
```
<br/>

## Attachments
``` csharp
// create doc
long docId = _repository.StoreText("the content");

// store two attachments
_repository.Attach(docId, "attach-name-1", "attachment content 1");
_repository.Attach(docId, "attach-name-2", "attachment content 2");

_repository.Attach(docId, "attach-name-3", myModel);


// load all attachments as list of strings
IList<string> attachs = _repository.LoadTextAttachments(docId);

// load all attachments as list of streams
IList<Stream> attachs = _repository.LoadStreamAttachments(docId);

// load "attach-name-1" 
string myAttachmentText = _repository.LoadTextAttachment(docId, "attach-name-1");

// load "attach-name-3" 
MyModel modelAttachment = _repository.LoadAttachment<MyModel>(docId, "attach-name-3");

// load all attachments as MyModel 
IList<MyModel> myModelList = _repository.LoadAttachments<MyModel>(docId);


```

<br/>

## GZip Compresion for Texts
``` csharp
// store    
long docId = _repository.StoreText("a large text", 
    options: StoreOptions.Compress);

// load
var myText = _repository.LoadText(docId, 
    options: LoadOptions.Decompress);
```
> Objects also support gzip compresstion 

<br/>

## Sub Storage
``` csharp
//  creating news docs
long newsId1 = _repository.StoreText("news content 1", "news");
long newsId2 = _repository.StoreText("news content 2", "news");

//  creating articles docs
long articleId1 = _repository.StoreText("article content 1", "article");
long articleId2 = _repository.StoreText("article content 2", "article");

//  loading
Assert.Equal("news content 1", _repository.LoadText(newsId1, "news"));
Assert.Equal("article content 2", _repository.LoadText(articleId2, "article"));


```
The output File System :

```
StorageRoot\news\...\01
                 ...\02

StorageRoot\article\...\01
                    ...\02
````

# FileCache
Its a persistent layer for `IMemoryCache`
## Instantiation
``` csharp
IMemoryCache _memoryCache; // resolve it via .net dependency injection
FileCache _cache = memoryCache.Persistent();
```
> There is no overhead on `Persistent()` method so you can use it each time you want to use file cache over memory cache : `_memoryCache.Persistent().Set(...)` or `_memoryCache.Persistent().GetOrCreate(...)`
## Create persistent cache with absolute expiration
``` csharp
var key = "myKey";
var value = "myValue";

// store into both memory and file together
_cache.Set(key, value, TimeSpan.FromMinutes(1));;

// retrive the cached value
var found = _cache.TryGetValue(key, out result);

``` 
> If the cached item wasn't found inside the memory, then the file cache will be loaded.

> According to .Net `IMemoryCache` If you specify `DateTimeOffset` it will be used as absolute expiration
> and if you specify `TimeSpan` it will be used as absolute expiration relative to now. 
## Caching models

``` csharp
var myModel = new MyModel(...);

// store into both memory and file together
_cache.Set(cacheKey, myModel, TimeSpan.FromMinutes(1));;

// retrive the cached value
MyModel result;
var found = _cache.TryGetValue(cacheKey, out result);

``` 
## GetOrCreate
``` csharp
var cachedResult = await _cache.GetOrCreate(cacheKey, async entry =>
{
    entry.SlidingExpiration = TimeSpan.FromSeconds(10);

    var result = await loadMyDataAsync();
    
    return result;
});

```



# How it works

It keeps the last ID in memory and generates new ones by a simple atomic +1 operation and so it's very fast.
It converts a doc id into a hex number and stores it as 4 hirarchy folders

    StorageRoot\12\31\26\5A\17

The last hex number ( in this example `17` ) is the final file name.

[//]: # ( By default configuration it currently can store billions of files or even more, simply by changing the configuration!)

You can assign attachments to a file. They are stored as `Filename`[-]`AttachName`

For example, two attachment files are assigned to `documentId  17`  as following file names:

```
StorageRoot\...\17
            ...\17-attachmentFile1
            ...\17-attachmentFile2
````

