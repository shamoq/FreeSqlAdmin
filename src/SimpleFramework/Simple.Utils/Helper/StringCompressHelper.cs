using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Utils.Helper
{
    public class StringCompressHelper
    {
        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <returns></returns>
        public static async Task<string> CompressData(string json, bool forceCompression = false)
        {
            // using (var memoryStream = new MemoryStream())
            // {
            //     using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
            //     using (var writer = new StreamWriter(gzipStream))
            //     {
            //         await writer.WriteAsync(json);
            //     }
            //
            //     var bytes = memoryStream.ToArray();
            //     return Convert.ToBase64String(bytes);
            // }
            
            if (string.IsNullOrEmpty(json)) return string.Empty;
    
            // 短字符串不压缩，除非强制要求
            if (!forceCompression && json.Length < 100)
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            using (var memoryStream = new MemoryStream())
            {
                // 使用Brotli算法（.NET 6+ 支持）
                using (var brotliStream = new BrotliStream(memoryStream, CompressionLevel.Optimal))
                using (var writer = new StreamWriter(brotliStream, Encoding.UTF8))
                {
                    await writer.WriteAsync(json);
                }

                var compressedBytes = memoryStream.ToArray();
        
                // 检查压缩后是否真的更小
                if (!forceCompression && compressedBytes.Length >= json.Length)
                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            
                return Convert.ToBase64String(compressedBytes);
            }
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <returns></returns>
        public static async Task<string> DecompressData(string compressedData)
        {
            // var bytes = Convert.FromBase64String(compressedData);
            //
            // using (var memoryStream = new MemoryStream(bytes))
            // using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            // using (var reader = new StreamReader(gzipStream))
            // {
            //     return await reader.ReadToEndAsync();
            // }
            
            if (string.IsNullOrEmpty(compressedData)) 
                return string.Empty;

            try
            {
                // 1. 尝试Base64解码
                var bytes = Convert.FromBase64String(compressedData);
        
                // 2. 检查是否为Brotli压缩格式（Brotli流以0xCE 0xB2 0xCF 0x81开头）
                if (bytes.Length >= 4 && 
                    bytes[0] == 0xCE && 
                    bytes[1] == 0xB2 && 
                    bytes[2] == 0xCF && 
                    bytes[3] == 0x81)
                {
                    // 使用Brotli解压
                    using (var memoryStream = new MemoryStream(bytes))
                    using (var brotliStream = new BrotliStream(memoryStream, CompressionMode.Decompress))
                    using (var reader = new StreamReader(brotliStream, Encoding.UTF8))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                else
                {
                    // 未压缩的原始数据（直接Base64编码的）
                    return Encoding.UTF8.GetString(bytes);
                }
            }
            catch (FormatException)
            {
                // 非Base64字符串，直接返回原始内容
                return compressedData;
            }
            catch (Exception ex)
            {
                // 记录异常但返回原始内容，避免系统崩溃
                Console.WriteLine($"解压失败: {ex.Message}");
                return compressedData;
            }
        }
    }
}