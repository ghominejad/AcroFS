using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acrobit.AcroFS.Tests.Helpers
{
    public static class StreamExtensions
    {
        public static void TestWrite(this Stream stream, string content)
        {
            StreamHelper.Write(stream, content);
        }
    }


}
