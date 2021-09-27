using System.IO;
using Moq;
using Xunit;
using Acrobit.AcroFS.Tests.Helpers;

namespace Acrobit.AcroFS.Tests
{


    public class FileStoreTests
    {
        readonly string StoragePath1 = "";
        readonly string StoragePath2 = "";
        public FileStoreTests()
        {
            StoragePaths.CleanRoots();
            StoragePath1  = StoragePaths.CreateStorageFolder();
            StoragePath2 = StoragePaths.CreateStorageFolder();
        }

        [Fact]
        public void GetStore_Should_Return_DefaultStore_If_Path_Not_Specified()
        {
            var core = new Core();
            var _store = FileStore.GetStore();
            long docId = _store.StoreText("the content");

            Assert.Equal(1, docId);
            Assert.Equal("the content", _store.LoadText(docId));

            // any data inside default folder?
            var defaultPath = core.GetDefaultRepositoryPath();
            Assert.True(Directory.GetDirectories(defaultPath).Length > 0);

        }

        [Fact]
        public void TextTest()
        {
            var rootPath = StoragePaths.CreateStorageFolder();

            var _store = FileStore.GetStore(rootPath);
            
            long docId = _store.StoreText("the content");

            Assert.Equal("the content", _store.LoadText(docId));
        }
        [Fact]
        public void AttachmentTest()
        {
            

            var _store = FileStore.GetStore(StoragePath1);

            // create doc
            long docId = _store.StoreText("the content");

            // store attachments
            _store.AttachText(docId, "attach-name-1", "attachment content 1");
            _store.AttachText(docId, "attach-name-2", "attachment content 2");

            // retrive attachments
            Assert.Equal("attachment content 1", _store.LoadTextAttach(docId, "attach-name-1"));
            Assert.Equal("attachment content 2", _store.LoadTextAttach(docId, "attach-name-2"));

            var contents = _store.LoadTextAttachs(docId);

            Assert.Equal(2, contents.Count);
            Assert.Equal("attachment content 1", contents[0] );
            Assert.Equal("attachment content 2", contents[1] );


        }
        [Fact]
        public void MultipleStoreTest()
        {
            var _storeOne = FileStore.GetStore(StoragePath1);
            var _storeTwo = FileStore.GetStore(StoragePath2);

            long docId1 = _storeOne.StoreText("content 1");
            long docId2 = _storeTwo.StoreText("content 2");

            Assert.Equal("content 1", _storeOne.LoadText(docId1) );
            Assert.Equal("content 2", _storeTwo.LoadText(docId2) );

        }

        [Fact]
        public void MultipleStore_Should_Generate_Own_Ids()
        {
            var _storeOne = FileStore.GetStore(StoragePath1);
            var _storeTwo = FileStore.GetStore(StoragePath2);

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
            var _store = FileStore.GetStore(StoragePath1);

            long docId;

            using (var stream = new MemoryStream())
            {
                stream.TestWrite("the content");
                docId = _store.Store(stream);
            }

            Assert.Equal("the content", _store.LoadText(docId) );

        }

        [Fact]
        public void GzipCompressionTest()
        {
            var _store = FileStore.GetStore(StoragePath1);

            //  creating new doc 
            long docId = _store.StoreText(SampleTexts.Text1, options: StoreOptions.Compress);

            // comparing stream length and decompressed length
            var stream = _store.Load(docId);
            var text = _store.LoadText(docId, options: LoadOptions.Decompress);

            Assert.True(stream.Length < text.Length);

        }


        [Fact]
        public void SubStorageTests()
        {
            var _store = FileStore.GetStore(StoragePath1);

            //  creating news docs
            long newsId1 = _store.StoreText("news content 1", "news");
            long newsId2 = _store.StoreText("news content 2", "news");

            //  creating articles docs
            long articleId1 = _store.StoreText("article content 1", "article");
            long articleId2 = _store.StoreText("article content 2", "article");

            //  loading
            Assert.Equal("news content 1", _store.LoadText(newsId1, "news"));
            Assert.Equal("article content 2", _store.LoadText(articleId2, "article"));



        }

        [Fact]
        public void Sub_Storage_AS_A_Path()
        {
            var _store = FileStore.GetStore(StoragePath1);

            //  creating news docs
            long newsId1 = _store.StoreText("economic news content 1", "news/economic");
            long newsId2 = _store.StoreText("health news content 1", "news/health");

 
            //  loading
            Assert.Equal("economic news content 1", _store.LoadText(newsId1, "news/economic"));
            Assert.Equal("health news content 1", _store.LoadText(newsId2, "news/health"));

        }

    }
}
