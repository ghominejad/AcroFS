# AcroFS

[![NuGet version](https://img.shields.io/nuget/v/Acrobit.AcroFS.svg)](https://www.nuget.org/packages/Acrobit.AcroFS)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A tiny, cross-platform .NET library built around two primitives:

1. **A directory-sharded blob store.** Each stored item gets a sequential id that is
   fanned out into a multi-level hex directory tree (e.g. `…/12/31/26/5A/17`), so
   millions of small files can live on a single filesystem without hitting
   files-per-directory limits — the same sharding idea behind Git's object store and
   Docker's layer store. Ids are sequential and hex-fanned for sharding; this is *not*
   content-addressable storage (nothing is hashed by content).

2. **A write-through persistent cache over .NET `IMemoryCache`.** `GetOrCreate` /
   `GetOrCreateAsync` tier memory → disk, so cached values survive process restarts.

Objects are serialized with [Newtonsoft.Json](https://www.newtonsoft.com/json). Text and
binary blobs can be GZip-compressed on write.

> **Compatibility:** .NET Standard 2.1 — runs on .NET 5 through .NET 10.

## When to use it

- **Persistent cache for expensive computations** — e.g. LLM responses or embeddings —
  where the memory → disk tiering keeps results across restarts. AcroFS has no
  AI-specific code; it's just a good fit for caching anything costly to recompute.
- **Local-first / edge artifact storage** for millions of small blobs, without the
  per-request cost and latency of a cloud object store.
- **High-volume document / blob archives** where each document carries its own
  attachments.

## Features

- **Async and synchronous APIs** for every operation
- **Directory-sharded layout** that scales to millions of small files
- **Persistent write-through cache** over .NET `IMemoryCache` — survives process restarts
- **Store and load** objects, text, and streams — by sequential id or by key
- **Per-document attachments** (objects, text, or streams)
- **Sub-storages** — named clusters, each with its own id sequence
- **Multiple independent stores** at custom filesystem locations
- **Optional GZip compression** on write
- **JSON serialization** via Newtonsoft.Json
- **Cross-platform** — .NET Standard 2.1, runs on .NET 5 through .NET 10


## Install

```bash
dotnet add package Acrobit.AcroFS
```

## Quick start

The async methods are the recommended surface; every async method below has a
synchronous equivalent (drop the `Async` suffix).

```csharp
// Get the default store (files land in ./Data/default-store)
var store = FileStore.CreateStore();

// Store any object — it's JSON-serialized and assigned a new sequential id
long id = await store.StoreAsync(myModel);

// Load it back
MyModel? model = await store.LoadAsync<MyModel>(id);
```

Persistent cache — compute on a miss, reuse on a hit, survive restarts:

```csharp
IMemoryCache memoryCache;            // injected via DI
var cache = memoryCache.Persistent();

var answer = await cache.GetOrCreateAsync("prompt:" + promptHash, async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
    return await CallLlmAsync(prompt);   // only runs when the value isn't cached
});
```

## File store

### Store and load by id

```csharp
long id = await store.StoreAsync(model);          // returns a new sequential id
MyModel? model = await store.LoadAsync<MyModel>(id);
```

### Store and load by a key

```csharp
await store.StoreByKeyAsync("user-42", model);
MyModel? model = await store.LoadAsync<MyModel>("user-42");
```

### Texts

```csharp
long id = await store.StoreTextAsync("the content");
string? text = await store.LoadTextAsync(id);
```

### Streams

```csharp
long id = await store.StoreStreamAsync(stream);
using Stream? data = await store.LoadAsync(id);
```

### Attachments

Any stored item can carry named attachments (objects, text or streams).

```csharp
long id = await store.StoreTextAsync("the content");

await store.AttachAsync(id, "metadata", myModel);
await store.AttachTextAsync(id, "note", "attachment content");

MyModel? meta = await store.LoadAttachmentAsync<MyModel>(id, "metadata");
string?  note = await store.LoadTextAttachmentAsync(id, "note");

List<string> allText = await store.LoadTextAttachmentsAsync(id);
```

### GZip compression

```csharp
long id = await store.StoreTextAsync(largeText, options: StoreOptions.Compress);
string? text = await store.LoadTextAsync(id, options: LoadOptions.Decompress);
```

> Objects and streams support compression through the same `options` argument.

### Sub-storages

Group documents under a named cluster path (the second argument). Each cluster keeps
its own id sequence.

```csharp
long newsId    = await store.StoreTextAsync("news content",    "news");
long articleId = await store.StoreTextAsync("article content", "article");

await store.LoadTextAsync(newsId,    "news");
await store.LoadTextAsync(articleId, "article");
```

Resulting layout:

```text
StorageRoot\news\...\01
                ...\02
StorageRoot\article\...\01
                   ...\02
```

### Custom location

```csharp
var store = FileStore.CreateStore("/var/data/store1");
```

### Simple paths

```csharp
// Store keys verbatim as paths instead of hex-fanning them
var store = FileStore.CreateStore().UseSimplePath();

var settings = new AppSettings { /* ... */ };
await store.StoreByKeyAsync("app-settings", settings);

var loaded = await store.LoadAsync<AppSettings>("app-settings");
```

## FileCache

A persistent, write-through layer over `IMemoryCache`. Writes go to both memory and
disk; reads fall back to disk when the in-memory entry is gone (e.g. after a restart),
honoring the original expiration.

```csharp
IMemoryCache memoryCache;            // injected via DI
FileCache cache = memoryCache.Persistent();
```

> `Persistent()` has no overhead, so you can call it inline whenever you need the file
> cache: `memoryCache.Persistent().Set(...)`.

### Set and get with expiration

```csharp
// Write to memory and disk together
cache.Set("myKey", "myValue", TimeSpan.FromMinutes(10));

bool found = cache.TryGetValue("myKey", out string? value);
```

Async equivalents:

```csharp
await cache.SetAsync("myKey", model, TimeSpan.FromMinutes(10));
var (found, value) = await cache.TryGetValueAsync<MyModel>("myKey");
```

> Following `IMemoryCache` conventions, a `DateTimeOffset` is treated as an absolute
> expiration and a `TimeSpan` as an absolute expiration relative to now. Pass
> `isSlidingExpiration: true` to the `TimeSpan` overload for sliding expiration.

### GetOrCreate

```csharp
var result = await cache.GetOrCreateAsync("myKey", async entry =>
{
    entry.SlidingExpiration = TimeSpan.FromSeconds(10);
    return await LoadMyDataAsync();
});
```

A synchronous `GetOrCreate(key, Func<ICacheEntry, TItem>)` is also available.

## How it works

The store keeps the last id in memory and hands out new ones with an atomic `+1`, so id
generation is fast and lock-free per cluster. Each id is converted to a zero-padded hex
number and split into two-character segments to form a directory tree:

```text
StorageRoot\12\31\26\5A\17
```

The last segment (`17` here) is the file itself; the rest are directories. Spreading
ids across this tree keeps any single directory small, even with millions of files.

Attachments are stored next to their document as `Filename-AttachName`:

```text
StorageRoot\...\17
            ...\17-attachmentFile1
            ...\17-attachmentFile2
```

## License

[MIT](LICENSE)
