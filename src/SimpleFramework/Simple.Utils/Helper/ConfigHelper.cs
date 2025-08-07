﻿using Microsoft.Extensions.Configuration;
using NLog;
using Simple.Utils.Exceptions;
using Simple.Utils.Extensions;
using System;
using System.Runtime.InteropServices;

namespace Simple.Utils.Helper
{
    /// <summary>注入的配置文件获取</summary>
    public class ConfigHelper
    {
        /// <summary>配置文件</summary>
        public static IConfiguration Configuration;

        /// <summary>获取指定的配置节点</summary>
        /// <returns></returns>
        public static T GetValue<T>(string key, T defaultValue = default)
        {
            try
            {
                return Configuration.GetSection(key).Get<T>();
            }
            catch (Exception)
            {
                if (defaultValue != null) return defaultValue;

                throw new FatalException("配置（" + key + "）不存在", "配置不存在");
            }
        }

        /// <summary>获取指定的配置节点</summary>
        /// <returns></returns>
        public static string GetValue(string key, string defaultValue = "")
        {
            try
            {
                return Configuration.GetSection(key).Get<string>();
            }
            catch (Exception)
            {
                if (!defaultValue.IsNullOrEmpty()) return defaultValue;
                throw new FatalException("配置（" + key + "）不存在", "配置不存在");
            }
        }

        /// <summary>初始化</summary>
        /// <param name="jsonFiles">json配置文件</param>
        /// <param name="optional">文件是否可选</param>
        /// <param name="reloadOnChange">是否在文件内容发生变化时重新加载</param>
        /// <exception cref="System.IO.IOException">文件不存在</exception>
        /// <returns>IConfiguration 对象</returns>
        public static void Init(string[] jsonFiles, bool optional = false, bool reloadOnChange = true)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            if (jsonFiles != null)
            {
                foreach (String jsonFile in jsonFiles)
                {
                    builder.AddJsonFile(jsonFile, optional, reloadOnChange);
                }
            }
            Configuration = builder.Build();
        }

        /// <summary>初始化</summary>
        /// <param name="_configuration"></param>
        public static void Init(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        /// <summary>获取节对象</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IConfigurationSection GetSection(string key)
        {
            return Configuration.GetSection(key);
        }

        /// <summary>获取key</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfigurationValue(string key)
        {
            return Configuration[key];
        }

        /// <summary>获取节中的key</summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfigurationValue(string section, string key)
        {
            return GetSection(section)?[key];
        }

        public static (string ProviderName, string ConnectionString) GetConnectionStringSetting(string key)
        {
            try
            {
                IConfigurationSection section = Configuration.GetSection(key);
                var setting = new
                {
                    ConnectionString = section["ConnectionString"],
                    ProviderName = section["ProviderName"],
                };

                if (setting == null)
                {
                    throw new FatalException("数据库连接配置（" + key + "）不存在", "数据库连接不存在");
                }
                if (String.IsNullOrWhiteSpace(setting.ProviderName))
                {
                    throw new FatalException("数据库连接配置（" + key + "）缺少特性 \"ProviderName\" 或内容为空",
                                               "数据库连接获配置有误");
                }
                if (String.IsNullOrWhiteSpace(setting.ConnectionString))
                {
                    throw new FatalException("数据库连接配置（" + key + "）缺少特性 \"ConnectionString\" 或内容为空",
                                               "数据库连接获配置有误");
                }

                return (setting.ProviderName, setting.ConnectionString);
            }
            catch (Exception ex)
            {
                throw new FatalException("读取数据库连接配置(" + key + ")出错\r\n", "获取数据库连接配置出错", ex);
            }
        }
    }
}