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
        readonly string StoragePath1 = "";
        readonly string StoragePath2 = "";

        public FileStoreTests()
        {
            StoragePaths.CleanRoots();
            StoragePath1 = StoragePaths.CreateStorageFolder();
            StoragePath2 = StoragePaths.CreateStorageFolder();
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
        public async Task TextTestAsync()
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
            var store = FileStore.CreateStore(Path.GetRandomFileName());

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

        [Fact]
        public async Task AttachmentTestAsync()
        {
            var store = FileStore.CreateStore(Path.GetRandomFileName());

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

        [Fact]
        public void MultipleStoreTest()
        {
            var _storeOne = FileStore.CreateStore(StoragePath1);
            var _storeTwo = FileStore.CreateStore(StoragePath2);

            long docId1 = _storeOne.StoreText("content 1");
            long docId2 = _storeTwo.StoreText("content 2");

            Assert.Equal("content 1", _storeOne.LoadText(docId1));
            Assert.Equal("content 2", _storeTwo.LoadText(docId2));
        }

        [Fact]
        public void MultipleStore_Should_Generate_Own_Ids()
        {
            var _storeOne = FileStore.CreateStore(StoragePath1);
            var _storeTwo = FileStore.CreateStore(StoragePath2);

            long docId1 = _storeOne.StoreText("content 1");
            long docId2 = _storeTwo.StoreText("content 2");

            Assert.Equal(1, docId1);
            Assert.Equal(1, docId2);

            docId1 = _storeOne.StoreText("content 3");
            docId2 = _storeTwo.StoreText("content 4");

            Assert.Equal(2, docId1);
            Assert.Equal(2, docId2);
        }

        [Fact]
        public void StreamTest()
        {
            var _store = FileStore.CreateStore(StoragePath1);

            long docId;

            using (var stream = new MemoryStream())
            {
                stream.TestWrite("the content");
                docId = _store.Store(stream);
            }

            Assert.Equal("the content", _store.LoadText(docId));
        }

        [Fact]
        public void GzipCompressionTest()
        {
            var _store = FileStore.CreateStore()
                .Root(StoragePath1);

            //  creating new doc 
            long docId = _store.StoreText(SampleTexts.Text1, options: StoreOptions.Compress);

            // comparing stream length and decompressed length
            var stream = _store.Load(docId);
            var text = _store.LoadText(docId, options: LoadOptions.Decompress);

            Assert.True(stream!.Length < text!.Length);
        }

        [Fact]
        public void ObjectStoreTests()
        {
            var _store = FileStore.CreateStore();

            var data = new SimpleModel
            {
                Name = "hassan",
                Email = "ghominejad@gmail.com"
            };

            //  creating new doc 
            long docId = _store.Store(data);

            var loadedData = _store.Load<SimpleModel>(docId);

            Assert.Equal(loadedData!.Email, data.Email);
            Assert.Equal(loadedData!.Name, data.Name);
        }

        [Fact]
        public void SubStorageTests()
        {
            var _store = FileStore.CreateStore(StoragePath1);

            //  creating news docs
            long newsId1 = _store.StoreText("news content 1", "news");
            _ = _store.StoreText("news content 2", "news");

            //  creating articles docs
            _ = _store.StoreText("article content 1", "article");
            long articleId2 = _store.StoreText("article content 2", "article");

            //  loading
            Assert.Equal("news content 1", _store.LoadText(newsId1, "news"));
            Assert.Equal("article content 2", _store.LoadText(articleId2, "article"));
        }

        [Fact]
        public void Sub_Storage_AS_A_Path()
        {
            var _store = FileStore.CreateStore(StoragePath1);

            //  creating news docs
            long newsId1 = _store.StoreText("economic news content 1", "news/economic");
            long newsId2 = _store.StoreText("health news content 1", "news/health");

            //  loading
            Assert.Equal("economic news content 1", _store.LoadText(newsId1, "news/economic"));
            Assert.Equal("health news content 1", _store.LoadText(newsId2, "news/health"));
        }

        [Fact]
        public void Store_By_Key()
        {
            var _store = FileStore.CreateStore()
                .UseSimplePath();

            var data = new SimpleModel
            {
                Name = "hassan",
                Email = "ghominejad@gmail.com"
            };

            var key = "SimpleModel.json";

            // store by a key
            _store.StoreByKey(key, data);

            // load by a key
            var loadedData = _store.Load<SimpleModel>(key);

            Assert.Equal(loadedData!.Email, data.Email);
            Assert.Equal(loadedData!.Name, data.Name);
        }

        [Fact]
        public void Use_Simple_Path_Instead_Of_Hashed_Path_As_An_Option()
        {
            var _store = FileStore.CreateStore()
                .UseSimplePath();

            var data = new SimpleModel
            {
                Name = "hassan",
                Email = "ghominejad@gmail.com"
            };

            var key = "SimpleModel.json";

            // store by a key
            _store.StoreByKey(key, data);

            // load by a key
            var loadedData = _store.Load<SimpleModel>(key);

            Assert.Equal(loadedData!.Email, data.Email);
            Assert.Equal(loadedData!.Name, data.Name);
        }

        private static string GetRandomText()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
