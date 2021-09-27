using System;
using System.IO;
using Moq;
using Xunit;
using Acrobit.AcroFS.Tests.Helpers;

namespace Acrobit.AcroFS.Tests
{
    public class CoreTests 
    {
        readonly string StoragePath = "";
        public CoreTests()
        {
            StoragePaths.CleanRoots();
            StoragePath = StoragePaths.CreateStorageFolder();
        }

        [Fact]
        public void Core_Throws_If_Repository_Path_Not_Exists()
        {
            Assert.Throws<RepositoryNotFoundException>(
                () => new Core("an invalid path"));
        }

        [Fact]
 
        public void Id_Generateor_Tests()
        {
            var storagePath = StoragePaths.CreateStorageFolder();
            var core = new Core(storagePath);

            // generating ids in root without any clusters
            Assert.Equal(1, core.GetNewStoreId());
            Assert.Equal(2, core.GetNewStoreId());

            // generating ids in `news` cluster
            Assert.Equal(1, core.GetNewStoreId("news"));
            Assert.Equal(2, core.GetNewStoreId("news"));
            Assert.Equal(3, core.GetNewStoreId("news"));
            Assert.Equal(4, core.GetNewStoreId("news"));

            // generating ids in `articles` cluster
            Assert.Equal(1, core.GetNewStoreId("articles"));
            Assert.Equal(2, core.GetNewStoreId("articles"));
            Assert.Equal(3, core.GetNewStoreId("articles"));
        }

        [Fact]
        public void DefaultRepository_Tests()
        {

            var core = new Core();

            Assert.Equal(StoragePaths.DefaultFolder, core.GetDefaultRepositoryPath());
        }


        [Fact]
        public void Hashed_Path_Generateor_Test()
        {
            var core = new Core(StoragePath);

            // zero should return Emply
            Assert.Equal("", core.GenerateHashedPath(0));

            // docId 5
            Assert.Equal("$00/$00/$00/$00/$05", core.GenerateHashedPath(5));


            // big docId as 1099496034834 = 0xFFFF121212 
            Assert.Equal("$FF/$FF/$12/$12/$12", core.GenerateHashedPath(0xFFFF121212));


            var docId = 5; // $00/$00/$00/$00/$05
            var path = core.GetHashedPath(docId, "news");

            Assert.Equal($"{StoragePath}news/$00/$00/$00/$00/$05",
                core.GetHashedPath(docId, "news"));
        }
    }
}
