using NLog;
using NLog.Config;

namespace Simple.Utils.Helper
{
    /// <summary>log封装类</summary>
    public class LogHelper
    {
        private static ILogger logger;

        static LogHelper()
        {
            var logConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nlog.config");
            if (!File.Exists(logConfigFile))
            {
                logConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "nlog.config");
            }

            if (!File.Exists(logConfigFile))
            {
                Console.WriteLine("检测到nlog配置文件不存在，将创建默认配置文件！");
                logConfigFile = CreateConfig();
            }

            var configXmlFile = new XmlLoggingConfiguration("nlog.config");
            LogManager.Setup().LoadConfiguration(configXmlFile);
            logger = LogManager.GetCurrentClassLogger();
        }

        private static string CreateConfig()
        {
            var configTmp = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
      autoReload=""true"">
	<!-- the targets to write to -->
	<targets>
		<!-- 输出到文件,这个文件记录所有的日志 -->
		<target xsi:type=""File"" name=""logfile"" fileName=""Log\${{level}}\${{shortdate}}.log""
					layout=""${{longdate}}：${{logger}}${{message}} ${{exception}}""/>
	</targets>
	<!-- rules to map from logger name to target -->
	<rules>
		<!--All logs, including from Microsoft-->
		<logger name=""*"" minlevel=""Debug"" writeTo=""logfile""/>
		<logger name=""Microsoft.*"" maxLevel=""Info"" final=""true""/>
	</rules>
</nlog>";

            var logConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nlog.config");

            File.WriteAllText(logConfigFile, configTmp);

            return logConfigFile;
        }

        private static string FormateMessage(string message, Exception ex)
        {
            if (ex is null) return message;
            string formateMsg = $@"【抛出信息】:{message}
【异常类型】：{ex.GetType().Name}
【异常信息】:{ex.Message}
【内部信息】:{ex.InnerException?.Message}
【堆栈调用】:{ex.StackTrace}";
            return formateMsg;
        }

        /// <summary>输出错误日志到nlog</summary>
        public static void Error(string message, Exception ex = null)
        {
            logger.Error(FormateMessage(message, ex));
        }

        /// <summary>记录消息日志</summary>
        public static void Info(string message, Exception ex = null)
        {
            logger.Info(FormateMessage(message, ex));
        }

        /// <summary>记录致命错误日志 </summary>
        public static void Fatal(string message, Exception ex = null)
        {
            logger.Fatal(FormateMessage(message, ex));
        }

        /// <summary>记录Debug日志</summary>
        public static void Debug(string message, object data = null)
        {
            logger.Debug(message + JsonHelper.ToJson(data));
        }

        /// <summary>记录警告信息</summary>
        public static void Warn(string message, Exception ex = null)
        {
            logger.Warn(FormateMessage(message, ex));
        }
    }
}