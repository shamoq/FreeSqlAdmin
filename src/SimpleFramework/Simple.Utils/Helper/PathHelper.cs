using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Utils.Helper
{
    public class PathHelper
    {
        /// <summary>
        /// 获取文件在目录下的相对地址
        /// </summary>
        /// <param name="baseDir"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(string baseDir, string fullPath)
        {
            if (string.IsNullOrEmpty(baseDir) || string.IsNullOrEmpty(fullPath))
            {
                return null;
            }

            Uri baseUri = new Uri(Path.GetFullPath(baseDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
            Uri fullUri = new Uri(Path.GetFullPath(fullPath));

            return baseUri.MakeRelativeUri(fullUri).ToString().Replace('/', Path.DirectorySeparatorChar);
        }
        
        /// <summary>
        /// 替换文件名的不合法字符一律为下划线，防止文件名包含不合法字符导致文件无法正常打开
        /// </summary>
        public static string GetRightFileName(string fileName)
        {
            return string.Join("_", fileName.Trim().Split(Path.GetInvalidFileNameChars()));
        }
        
        
        /// <summary>
        /// 路径合并，兼容windows和linux路径分隔符
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            var combinedPath = paths[0].Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            for (int i = 1; i < paths.Length; i++)
            {
                combinedPath = Path.Combine(combinedPath,
                    paths[i].Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
            }

            return combinedPath;
        }
        
        /// <summary>
        /// 获取当前项目的父目录
        /// </summary>
        /// <param name="path"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetParent(string path, int index)
        {
            if (index == 0)
            {
                return path;
            }

            var parent = Path.GetDirectoryName(path);
            return GetParent(parent, index - 1);
        }
    }
}