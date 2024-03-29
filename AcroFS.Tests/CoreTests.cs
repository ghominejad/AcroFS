﻿using System;
using System.IO;
using Moq;
using Xunit;
using Acrobit.AcroFS.Tests.Helpers;

namespace Acrobit.AcroFS.Tests
{
    public class CoreTests
    {
        [Fact]

        public void Id_Generateor_Tests()
        {
            var core = new Core(Path.GetRandomFileName());

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
        public void Hashed_Path_Generateor_Test()
        {
            var storagePath = Path.GetRandomFileName();
            var core = new Core(storagePath);

            // zero should return Empty
            Assert.Equal("", core.GenerateHashedPath(0));

            // docId 5
            Assert.Equal("$00/$00/$00/$00/$05", core.GenerateHashedPath(5));


            // big docId as 1099496034834 = 0xFFFF121212 
            Assert.Equal("$FF/$FF/$12/$12/$12", core.GenerateHashedPath(0xFFFF121212));


            var docId = 5; // $00/$00/$00/$00/$05

            Assert.Equal($"{storagePath}/news/$00/$00/$00/$00/$05",
                core.GetHashedPath(docId, "news"));
        }

        [Fact]
        public void String_Key_Hashed_Path_Generateor_Test()
        {
            var storagePath = Path.GetRandomFileName();
            var core = new Core(storagePath);

            // zero should return Emply
            Assert.Equal("", core.GenerateHashedPath(""));

            // docId "custom_key"
            Assert.Equal("$cu/$st/$om/$_k/$ey", core.GenerateHashedPath("custom_key"));

            var docKey = "custom_key"; // $cu/$st/$om/$_k/$ey

            Assert.Equal($"{storagePath}/news/$cu/$st/$om/$_k/$ey",
                core.GetHashedPath(docKey, "news"));
        }

        [Fact]
        public void Hashed_Path_Generateor_Generates_Simple_Path_Option_Test()
        {
            var storagePath = Path.GetRandomFileName();
            var core = new Core(storagePath, new StoreConfig { UseSimplePath = true });

            // zero should return Emply
            Assert.Equal("", core.GenerateHashedPath(""));

            var docKey = "custom_key";
            Assert.Equal("custom_key", core.GenerateHashedPath(docKey));

            var path = core.GetHashedPath(docKey, "news");

            Assert.Equal($"{storagePath}/news/custom_key",
                path);
        }
    }
}
