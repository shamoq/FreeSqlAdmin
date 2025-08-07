using System;
using System.IO;

namespace Simple.Utils.Helper
{
    public class StreamHelper
    {
        /// <summary>
        /// 将流转换为字节数组
        /// </summary>
        /// <param name="stream">输入流</param>
        /// <returns>字节数组</returns>
        public static byte[] ToByteArray(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 将字节数组转换为流
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>流</returns>
        public static Stream FromByteArray(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return new MemoryStream(bytes);
        }
    }
}