using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Acrobit.AcroFS
{

    public static class StringExtensions

    {
        public static MemoryStream ToMemoryStream(this string source)
        {
            return source.ToMemoryStream(ASCIIEncoding.UTF8);
        }

        public static MemoryStream ToMemoryStream(this string source, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(source));
        }

        public static string ToUtf8String(this Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);

            stream.Read(buffer, 0, buffer.Length);

            Encoding ae = Encoding.GetEncoding("utf-8");
            string result = ae.GetString(buffer);

            return result;
        }

        public static async Task<string> ToUtf8StringAsync(this Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);

            await stream.ReadAsync(buffer, 0, buffer.Length);

            Encoding ae = Encoding.GetEncoding("utf-8");
            string result = ae.GetString(buffer);

            return result;
        }

        public static string ReadToEnd(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}

