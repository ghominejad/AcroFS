﻿using System;
using System.IO;


namespace Acrobit.AcroFS.Tests.Helpers
{
    public static class StoragePaths
    {
        public static string RootFolder
        {
            get
            {
                var root = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "root");
                
                return root;
            }
        }

        public static string CreateStorageFolder()
        {
            var root = Path.Combine(
                RootFolder,
                $"{Guid.NewGuid()}\\");

            Directory.CreateDirectory(root);
            return root;
        }

      

        static int Cleaned = 0;
        static object lockObject=new Object();
        public static void CleanRoots()
        {
            System.Diagnostics.Debug.Write("hello " + Cleaned);
            lock (lockObject)
            {
                Cleaned++;

                if (Cleaned > 1) return;

           
                if (Directory.Exists(RootFolder))
                {
                    Directory.Delete(RootFolder, true);
                }
            }
        }

    }
}
