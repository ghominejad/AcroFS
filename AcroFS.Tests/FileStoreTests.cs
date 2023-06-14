using Acrobit.AcroFS.Tests.Helpers;

using AcroFS.Tests;

using System;
using System.IO;
using System.Threading.Tasks;

using Xunit;

namespace Acrobit.AcroFS.Tests
{
    public class FileStoreTests
    {
        public FileStoreTests()
        {
        }

        [Fact]
        public void GetStore_Should_Return_DefaultStore_If_Path_Not_Specified()
        {
            var defaultPath = Core.GetDefaultRepositoryPath();

            try
            {
                var store = FileStore.CreateStore();
                long docId = store.StoreText("the content");

                Assert.Equal(1, docId);
                Assert.Equal("the content", store.LoadText(docId));

                // any data inside default folder?
                Assert.True(Directory.GetDirectories(defaultPath).Length > 0);
            }
            finally
            {
                FileStore.RemoveStore(defaultPath);
            }
        }

        [Fact]
        public async Task GetStore_Should_Return_DefaultStore_If_Path_Not_Specified_Async()
        {
            var defaultPath = Core.GetDefaultRepositoryPath();

            try
            {
                var store = FileStore.CreateStore();
                long docId = await store.StoreTextAsync("the content");

                Assert.Equal(1, docId);
                Assert.Equal("the content", await store.LoadTextAsync(docId));

                // any data inside default folder?
                Assert.True(Directory.GetDirectories(defaultPath).Length > 0);
            }
            finally
            {
                FileStore.RemoveStore(defaultPath);
            }
        }

        [Fact]
        public void TextTest()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                var content = GetRandomText();
                long docId = store.StoreText(content);

                Assert.Equal(content, store.LoadText(docId));
            }
            finally
            {
                FileStore.RemoveStore(rootPath);
            }
        }

        [Fact]
        public async Task TextAsyncTest()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                var content = GetRandomText();
                long docId = await store.StoreTextAsync(content);

                Assert.Equal(content, await store.LoadTextAsync(docId));
            }
            finally
            {
                FileStore.RemoveStore(rootPath);
            }
        }

        [Fact]
        public void AttachmentTest()
        {
            var rootPath = Path.GetRandomFileName();

            try
            {

                var store = FileStore.CreateStore(rootPath);

                // create doc
                long docId = store.StoreText(GetRandomText());

                string attachmentName1 = GetRandomText();
                string attachmentName2 = GetRandomText();
                string attachmentContent1 = GetRandomText();
                string attachmentContent2 = GetRandomText();

                // store attachments
                store.AttachText(docId, attachmentName1, attachmentContent1);
                store.AttachText(docId, attachmentName2, attachmentContent2);

                // retrive attachments
                Assert.Equal(attachmentContent1, store.LoadTextAttachment(docId, attachmentName1));
                Assert.Equal(attachmentContent2, store.LoadTextAttachment(docId, attachmentName2));

                var contents = store.LoadTextAttachments(docId);

                Assert.Equal(2, contents.Count);

            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task AttachmentAsyncTest()
        {
            var rootPath = Path.GetRandomFileName();

            try
            {
                var store = FileStore.CreateStore(rootPath);

                // create doc
                long docId = await store.StoreTextAsync(GetRandomText());

                string attachmentName1 = GetRandomText();
                string attachmentName2 = GetRandomText();
                string attachmentContent1 = GetRandomText();
                string attachmentContent2 = GetRandomText();

                // store attachments
                await store.AttachTextAsync(docId, attachmentName1, attachmentContent1);
                await store.AttachTextAsync(docId, attachmentName2, attachmentContent2);

                // retrive attachments
                Assert.Equal(attachmentContent1, await store.LoadTextAttachmentAsync(docId, attachmentName1));
                Assert.Equal(attachmentContent2, await store.LoadTextAttachmentAsync(docId, attachmentName2));

                var contents = await store.LoadTextAttachmentsAsync(docId);

                Assert.Equal(2, contents.Count);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public void MultipleStoreTest()
        {
            var rootPath1 = Path.GetRandomFileName();
            var rootPath2 = Path.GetRandomFileName();

            try
            {
                var store1 = FileStore.CreateStore(rootPath1);
                var store2 = FileStore.CreateStore(rootPath2);

                var content1 = GetRandomText();
                var content2 = GetRandomText();

                long docId1 = store1.StoreText(content1);
                long docId2 = store2.StoreText(content2);

                Assert.Equal(content1, store1.LoadText(docId1));
                Assert.Equal(content2, store2.LoadText(docId2));
            }
            finally
            {
                Directory.Delete(rootPath1, true);
                Directory.Delete(rootPath2, true);
            }
        }

        [Fact]
        public async Task MultipleStoreAsyncTest()
        {
            var rootPath1 = Path.GetRandomFileName();
            var rootPath2 = Path.GetRandomFileName();

            try
            {
                var store1 = FileStore.CreateStore(rootPath1);
                var store2 = FileStore.CreateStore(rootPath2);

                var content1 = GetRandomText();
                var content2 = GetRandomText();

                long docId1 = await store1.StoreTextAsync(content1);
                long docId2 = await store2.StoreTextAsync(content2);

                Assert.Equal(content1,await store1.LoadTextAsync(docId1));
                Assert.Equal(content2,await store2.LoadTextAsync(docId2));
            }
            finally
            {
                Directory.Delete(rootPath1, true);
                Directory.Delete(rootPath2, true);
            }
        }

        [Fact]
        public void MultipleStore_Should_Generate_Own_Ids()
        {
            var rootPath1 = Path.GetRandomFileName();
            var rootPath2 = Path.GetRandomFileName();

            try
            {
                var store1 = FileStore.CreateStore(rootPath1);
                var store2 = FileStore.CreateStore(rootPath2);

                long docId1 = store1.StoreText(GetRandomText());
                long docId2 = store2.StoreText(GetRandomText());

                Assert.Equal(1, docId1);
                Assert.Equal(1, docId2);

                docId1 = store1.StoreText(GetRandomText());
                docId2 = store2.StoreText(GetRandomText());

                Assert.Equal(2, docId1);
                Assert.Equal(2, docId2);
            }
            finally
            {
                Directory.Delete(rootPath1, true);
                Directory.Delete(rootPath2, true);
            }
        }

        [Fact]
        public async Task MultipleStore_Should_Generate_Own_Ids_Async()
        {
            var rootPath1 = Path.GetRandomFileName();
            var rootPath2 = Path.GetRandomFileName();

            try
            {
                var store1 = FileStore.CreateStore(rootPath1);
                var store2 = FileStore.CreateStore(rootPath2);

                long docId1 = await store1.StoreTextAsync(GetRandomText());
                long docId2 = await store2.StoreTextAsync(GetRandomText());

                Assert.Equal(1, docId1);
                Assert.Equal(1, docId2);

                docId1 = await store1.StoreTextAsync(GetRandomText());
                docId2 = await store2.StoreTextAsync(GetRandomText());

                Assert.Equal(2, docId1);
                Assert.Equal(2, docId2);
            }
            finally
            {
                Directory.Delete(rootPath1, true);
                Directory.Delete(rootPath2, true);
            }
        }

        [Fact]
        public void StreamTest()
        {
            var rootPath = Path.GetRandomFileName();

            try
            {
                var store = FileStore.CreateStore(rootPath);

                using var stream = new MemoryStream();
                var content = GetRandomText();

                stream.TestWrite(content);
                var docId = store.Store(stream);

                Assert.Equal(content, store.LoadText(docId));
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task StreamAsyncTest()
        {
            var rootPath = Path.GetRandomFileName();

            try
            {
                var store = FileStore.CreateStore(rootPath);

                using var stream = new MemoryStream();
                var content = GetRandomText();

                stream.TestWrite(content);
                var docId = await store.StoreAsync(stream);

                Assert.Equal(content, await store.LoadTextAsync(docId));
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public void GzipCompressionTest()
        {
            var rootPath = Path.GetRandomFileName();

            try
            {
                var store = FileStore.CreateStore(rootPath);

                //  creating new doc 
                long docId = store.StoreText(SampleTexts.Text1, options: StoreOptions.Compress);

                // comparing stream length and decompressed length
                using var stream = store.Load(docId);
                var text = store.LoadText(docId, options: LoadOptions.Decompress);

                Assert.True(stream!.Length < text!.Length);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task GzipCompressionAsyncTest()
        {
            var rootPath = Path.GetRandomFileName();

            try
            {
                var store = FileStore.CreateStore(rootPath);

                //  creating new doc 
                long docId = await store.StoreTextAsync(SampleTexts.Text1, options: StoreOptions.Compress);

                // comparing stream length and decompressed length
                using var stream = await store.LoadAsync(docId);
                var text = await store.LoadTextAsync(docId, options: LoadOptions.Decompress);

                Assert.True(stream!.Length < text!.Length);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public void ObjectStoreTests()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                var data = new SimpleModel
                {
                    Name = GetRandomText(),
                    Email = GetRandomText(),
                };

                //  creating new doc 
                long docId = store.Store(data);

                var loadedData = store.Load<SimpleModel>(docId);

                Assert.Equal(loadedData!.Email, data.Email);
                Assert.Equal(loadedData!.Name, data.Name);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task ObjectStoreAsyncTests()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                var data = new SimpleModel
                {
                    Name = GetRandomText(),
                    Email = GetRandomText(),
                };

                //  creating new doc 
                long docId = await store.StoreAsync(data);

                var loadedData = await store.LoadAsync<SimpleModel>(docId);

                Assert.Equal(loadedData!.Email, data.Email);
                Assert.Equal(loadedData!.Name, data.Name);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public void SubStorageTest()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                //  creating news docs
                long newsId1 = store.StoreText("news content 1", "news");
                _ = store.StoreText("news content 2", "news");

                //  creating articles docs
                _ = store.StoreText("article content 1", "article");
                long articleId2 = store.StoreText("article content 2", "article");

                //  loading
                Assert.Equal("news content 1", store.LoadText(newsId1, "news"));
                Assert.Equal("article content 2", store.LoadText(articleId2, "article"));
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task SubStorageAsyncTest()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                //  creating news docs
                long newsId1 = await store.StoreTextAsync("news content 1", "news");
                _ = await store.StoreTextAsync("news content 2", "news");

                //  creating articles docs
                _ = await store.StoreTextAsync("article content 1", "article");
                long articleId2 = await store.StoreTextAsync("article content 2", "article");

                //  loading
                Assert.Equal("news content 1", await store.LoadTextAsync(newsId1, "news"));
                Assert.Equal("article content 2", await store.LoadTextAsync(articleId2, "article"));
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public void Sub_Storage_AS_A_Path()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                //  creating news docs
                long newsId1 = store.StoreText("economic news content 1", "news/economic");
                long newsId2 = store.StoreText("health news content 1", "news/health");

                //  loading
                Assert.Equal("economic news content 1", store.LoadText(newsId1, "news/economic"));
                Assert.Equal("health news content 1", store.LoadText(newsId2, "news/health"));
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task Sub_Storage_AS_A_Path_Async()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                //  creating news docs
                long newsId1 = await store.StoreTextAsync("economic news content 1", "news/economic");
                long newsId2 = await store.StoreTextAsync("health news content 1", "news/health");

                //  loading
                Assert.Equal("economic news content 1", await store.LoadTextAsync(newsId1, "news/economic"));
                Assert.Equal("health news content 1", await store.LoadTextAsync(newsId2, "news/health"));
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public void Store_By_Key()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                var data = new SimpleModel
                {
                    Name = GetRandomText(),
                    Email = GetRandomText(),
                };

                var key = GetRandomText();

                // store by a key
                store.StoreByKey(key, data);

                // load by a key
                var loadedData = store.Load<SimpleModel>(key);

                Assert.Equal(loadedData!.Email, data.Email);
                Assert.Equal(loadedData!.Name, data.Name);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task Store_By_Key_Async()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath);

                var data = new SimpleModel
                {
                    Name = GetRandomText(),
                    Email = GetRandomText(),
                };

                var key = GetRandomText();

                // store by a key
                await store.StoreByKeyAsync(key, data);

                // load by a key
                var loadedData = await store.LoadAsync<SimpleModel>(key);

                Assert.Equal(loadedData!.Email, data.Email);
                Assert.Equal(loadedData!.Name, data.Name);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public void Use_Simple_Path_Instead_Of_Hashed_Path_As_An_Option()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath)
                    .UseSimplePath();

                var data = new SimpleModel
                {
                    Name = GetRandomText(),
                    Email = GetRandomText(),
                };

                var key = GetRandomText();

                // store by a key
                store.StoreByKey(key, data);

                // load by a key
                var loadedData = store.Load<SimpleModel>(key);

                Assert.Equal(loadedData!.Email, data.Email);
                Assert.Equal(loadedData!.Name, data.Name);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task Use_Simple_Path_Instead_Of_Hashed_Path_As_An_Option_Async()
        {
            var rootPath = Path.GetRandomFileName();
            try
            {
                var store = FileStore.CreateStore(rootPath)
                    .UseSimplePath();

                var data = new SimpleModel
                {
                    Name = GetRandomText(),
                    Email = GetRandomText(),
                };

                var key = GetRandomText();

                // store by a key
                await store.StoreByKeyAsync(key, data);

                // load by a key
                var loadedData = await store.LoadAsync<SimpleModel>(key);

                Assert.Equal(loadedData!.Email, data.Email);
                Assert.Equal(loadedData!.Name, data.Name);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        private static string GetRandomText()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
