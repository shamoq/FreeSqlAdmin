using System.Xml.Linq;

namespace Simple.AdminApplication.Helpers
{
    internal class WpsHelper
    {
        /// <summary>
        /// 将wps公文域转换成标准word域
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Stream Parse2Word(string file)
        {
            // 创建临时文件夹
            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);

            try
            {
                // 解压word文档到临时文件夹
                System.IO.Compression.ZipFile.ExtractToDirectory(file, tempPath);

                // 读取document.xml内容
                var documentPath = Path.Combine(tempPath, "word", "document.xml");
                if (!File.Exists(documentPath))
                {
                    throw new Exception("无法找到document.xml文件");
                }

                // 加载XML文档
                XDocument doc = XDocument.Load(documentPath);
                XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
                XNamespace mc = "http://schemas.openxmlformats.org/markup-compatibility/2006";
                XNamespace wpsCustomData = "http://www.wps.cn/officeDocument/2013/wpsCustomData";

                // 修改查找逻辑，只查找 docfieldStart 节点
                var startNodes = doc.Descendants(mc + "AlternateContent")
                    .Where(x => x.Elements(mc + "Choice")
                        .Any(c => c.Elements(wpsCustomData + "docfieldStart").Any()))
                    .ToList();

                foreach (var startNode in startNodes)
                {
                    // 获取开始节点信息
                    var docField = startNode.Element(mc + "Choice")
                        .Element(wpsCustomData + "docfieldStart");
                    var fieldId = docField.Attribute("id").Value;
                    var fieldName = string.Empty;
                    var docfieldname = docField.Attribute("docfieldname").Value;
                    var fieldCode = docfieldname.Split('@')[0];

                    // 获取文本节点
                    var textNode = startNode.NextNode as XElement;
                    if (textNode?.Element(w + "t") != null)
                    {
                        fieldName = textNode.Element(w + "t").Value;
                    }

                    // 打印域代码信息
                    Console.WriteLine($"Field Info - Name: {fieldName}, ID: {fieldCode}");

                    // 查找对应的结束节点
                    var endNode = textNode?.NodesAfterSelf()
                        .OfType<XElement>()
                        .FirstOrDefault(x => x.Name == mc + "AlternateContent" &&
                            x.Elements(mc + "Choice")
                                .Any(c => c.Elements(wpsCustomData + "docfieldEnd")
                                    .Any(e => e.Attribute("id")?.Value == fieldId)));

                    if (textNode != null && endNode != null && fieldName != null)
                    {
                        // 创建标准Word域结构
                        var rPr = new XElement(w + "rPr",
                            new XElement(w + "rFonts", new XAttribute(w + "hint", "eastAsia"), new XAttribute(w + "eastAsiaTheme", "minorEastAsia")),
                            new XElement(w + "vertAlign", new XAttribute(w + "val", "baseline")),
                            new XElement(w + "lang", new XAttribute(w + "eastAsia", "zh")));

                        // 创建域开始标记
                        var beginNode = new XElement(w + "r",
                            new XElement(rPr),
                            new XElement(w + "fldChar", new XAttribute(w + "fldCharType", "begin")));

                        // 创建域指令文本
                        var onlyName = fieldName.Split('@')[0];
                        onlyName = onlyName.TrimStart('{').TrimEnd('}');
                        var instrNode = new XElement(w + "r",
                            new XElement(w + "rPr",
                                new XElement(w + "rFonts", new XAttribute(w + "hint", "eastAsia")),
                                new XElement(w + "vertAlign", new XAttribute(w + "val", "baseline")),
                                new XElement(w + "lang",
                                    new XAttribute(w + "val", "en-US"),
                                    new XAttribute(w + "eastAsia", "zh-CN"))),
                            new XElement(w + "instrText",
                                new XAttribute(XNamespace.Xml + "space", "preserve"),
                                //$" MERGEFIELD {fieldName.Split('@')[0]} "));
                                $" {onlyName} "));

                        // 创建域结束标记
                        var endFieldNode = new XElement(w + "r",
                            new XElement(rPr),
                            new XElement(w + "fldChar", new XAttribute(w + "fldCharType", "end")));

                        // 替换节点
                        startNode.Remove();
                        textNode.AddBeforeSelf(beginNode);
                        textNode.AddBeforeSelf(instrNode);
                        textNode.AddBeforeSelf(endFieldNode);
                        textNode.Remove();
                        endNode.Remove();
                    }

                    // 查找对应的自定义字段并更新值
                    //var customField = customFields.FirstOrDefault(t => t.Name == fieldName);
                    //if (customField != null && data.TryGetValue(customField.FullCode, out var fieldValue))
                    //{
                    //    if (currentNode?.Element(w + "t") != null)
                    //    {
                    //        currentNode.Element(w + "t").Value = "新的" + fieldName + fieldCode; //fieldValue.ToString();
                    //    }
                    //}
                    //else
                    //{
                    //    // 写死数据测试
                    //    currentNode.Element(w + "t").Value = "新的" + fieldName + fieldCode; //fieldValue.ToString();
                    //}
                }

                // 保存修改后的document.xml
                doc.Save(documentPath);

                // 创建新的zip文件
                var resultStream = new MemoryStream();
                System.IO.Compression.ZipFile.CreateFromDirectory(tempPath, resultStream);
                resultStream.Position = 0;
                return resultStream;
            }
            finally
            {
                // 清理临时文件夹
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }
    }
}