using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Simple.Utils.Comment
{
    /// <summary> 对于注释XML的缓存
    /// </summary>
    public static class CSCommentXMLCache
    {
        /// <summary> 缓存项 Key:Assembly.FullName  Value:XmlDocument
        /// </summary>
        private static readonly Dictionary<string, XmlDocument> _cache = new Dictionary<string, XmlDocument>();

        /// <summary> 缓存项 Key:XmlPath  Value:XmlDocument
        /// </summary>
        private static readonly Dictionary<string, XmlDocument> _cacheByPath = new Dictionary<string, XmlDocument>(StringComparer.OrdinalIgnoreCase);

        /// <summary> 获取缓存项
        /// </summary>
        public static XmlDocument Get(MemberInfo member)
        {
            var key = member.Module.Assembly.FullName;
            XmlDocument xml;

            if (_cache.TryGetValue(key, out xml))
            {
                return xml;
            }
            lock (_cache)
            {
                if (_cache.TryGetValue(key, out xml))
                {
                    return xml;
                }
                var xmlfile = GetXmlPath(member.Module.FullyQualifiedName);
                if (xmlfile == null)
                {
                    return null;
                }
                var xmltext = File.ReadAllText(xmlfile);
                xml = new XmlDocument();
                xml.LoadXml(xmltext);
                _cache.Add(key, xml);
                _cacheByPath[xmlfile] = xml;
                return xml;
            }
        }

        /// <summary> 设置缓存项
        /// </summary>
        internal static void Set(Assembly ass, string xmlpath)
        {
            var key = ass.FullName;
            lock (_cache)
            {
                if (xmlpath == null || File.Exists(xmlpath) == false)
                {
                    _cacheByPath[xmlpath] = _cache[key] = new XmlDocument();
                }
                else
                {
                    var xmltext = File.ReadAllText(xmlpath);
                    var xml = new XmlDocument();

                    xml.LoadXml(xmltext);
                    _cache[key] = xml;
                    _cacheByPath[xmlpath] = xml;
                }
            }
        }


        /// <summary> 刷新缓存项
        /// </summary>
        internal static void Reset(string xmlpath)
        {
            if (xmlpath == null)
            {
                return;
            }
            lock (_cache)
            {
                XmlDocument xml;
                if (_cacheByPath.TryGetValue(xmlpath, out xml) == false)
                {
                    return;
                }
                var xmltext = File.ReadAllText(xmlpath);
                xml.LoadXml(xmltext);
            }
        }

        /// <summary> 获取指定dll的注释xml的路径,如果不存在则返回null
        /// </summary>
        /// <param name="dllpath">dll路径</param>
        internal static string GetXmlPath(string dllpath)
        {
            if (dllpath == null || dllpath.Length == 0 || dllpath[0] == '<')
            {
                return null;
            }
            var xmlfile = dllpath.Remove(dllpath.Length - Path.GetExtension(dllpath).Length) + ".xml";
            if (File.Exists(xmlfile) == false)
            {
                xmlfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", Path.GetFileNameWithoutExtension(dllpath) + ".xml");
                if (File.Exists(xmlfile) == false)
                {
                    return null;
                }
            }
            return xmlfile;
        }
    }
}
