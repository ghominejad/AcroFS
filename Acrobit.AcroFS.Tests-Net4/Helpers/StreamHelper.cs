using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acrobit.AcroFS.Tests.Helpers
{
    public static class StreamHelper
    {
        public static void Write(Stream stream, string content)
        {
            var sWriter = new StreamWriter(stream, Encoding.UTF8);
            sWriter.Write(content);
            sWriter.Flush();
    
            stream.Position = 0L; // rewind
        }
       
    }

   
}
