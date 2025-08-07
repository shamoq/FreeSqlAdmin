using System;
using System.IO;

namespace Simple.Utils.Helper
{
    public class StreamHelper
    {
        /// <summary>
        /// ����ת��Ϊ�ֽ�����
        /// </summary>
        /// <param name="stream">������</param>
        /// <returns>�ֽ�����</returns>
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
        /// ���ֽ�����ת��Ϊ��
        /// </summary>
        /// <param name="bytes">�ֽ�����</param>
        /// <returns>��</returns>
        public static Stream FromByteArray(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return new MemoryStream(bytes);
        }
    }
}