using System;
using System.IO;


namespace Acrobit.AcroFS.Tests.Helpers
{
    public static class StoragePaths
    {
        static int Cleaned = 0;
        static object lockObject = new Object();

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

        public static string DefaultFolder
        {
            get
            {
                var root = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Data",
                    "default-store/");

                return root;
            }
        }

        public static string CreateStorageFolder()
        {
            var root = Path.Combine(
                RootFolder,
                $"{Guid.NewGuid()}/");

            Directory.CreateDirectory(root);
            return root;
        }

        public static void CleanRoots()
        {
            lock (lockObject)
            {
                Cleaned++;

                if (Cleaned > 1)
                {
                    return;
                }

                if (Directory.Exists(RootFolder))
                {
                    Directory.Delete(RootFolder, true);
                }

                if (Directory.Exists(DefaultFolder))
                {
                    Directory.Delete(DefaultFolder, true);
                }
            }
        }
    }
}
