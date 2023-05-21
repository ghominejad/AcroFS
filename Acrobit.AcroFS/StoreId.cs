using System;
using System.Linq;

namespace Acrobit.AcroFS
{
    public class StoreId
    {
        private readonly string _clusterhashedPath;
        private readonly Core _core;

        public StoreId(Core core, string clusterhashedPath)
        {
            _core = core;
            _clusterhashedPath = clusterhashedPath;
        }

        long _storeId = 0;

        public long GetNewId()
        {
            long newId;

            if (_storeId == 0)
            {
                lock (this)
                {
                    // chech again
                    if (_storeId == 0)
                    {
                        // _storeId = Core.FindNewIdFromFS(_clusterhashedPath);						
                        _storeId = LoadMaxId();
                    }
                }
            }

            newId = System.Threading.Interlocked.Increment(ref _storeId);
            return newId;
        }

        private long LoadMaxId(string root = "/")
        {
            try
            {
                string[] dirs = _core.GetDirs(_clusterhashedPath + root);
                if (dirs.Length == 0)
                {
                    string[] files = _core.GetFiles(_clusterhashedPath + root);

                    string max = files.Length == 0 ? "00" : files.Max().Substring(1, 2);

                    root += max;
                    root = root.Replace("/", "").Replace("$", "");
                    return Convert.ToInt64(root, 16);
                }

                return LoadMaxId(root + dirs.Max() + "/");
            }
            catch
            {
                return 0;
            }
        }
    }
}

