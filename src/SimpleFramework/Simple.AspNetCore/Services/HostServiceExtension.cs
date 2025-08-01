using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Simple.AspNetCore.Filters;
using Simple.RedisClient;
using Simple.Utils.Attributes;
using Simple.Utils.Helper;
using System.Reflection;
using System.Runtime.Loader;

namespace Simple.AspNetCore
{
    /// <summary>主机服务扩展 模块加载及配置</summary>
    public static class HostServiceExtension
    {
        // 使用完成后释放
        private static List<ISimpleModule> _simpleModules;

        private static IHttpContextAccessor _httpContextAccessor;

        public static AppProfileDataDb AppProfileData;

        static HostServiceExtension()
        {
            AppProfileData = AppProfileDataDb.GetInstance();
        }

        /// <summary>获取服务提供者</summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>获取当前访问的上下文</summary>
        public static HttpContext CurrentHttpContext
        {
            get { return _httpContextAccessor.HttpContext; }
        }

        /// <summary>获取全部注释Xml文档</summary>
        public static List<string> XmlCommentsFilePath()
        {
            var basePath = AppContext.BaseDirectory;
            DirectoryInfo d = new DirectoryInfo(basePath);
            FileInfo[] files = d.GetFiles("*.xml");
            var xmls = files.Select(a => Path.Combine(basePath, a.FullName)).ToList();
            return xmls;
        }

        /// <summary>注册领域内都使用的管道服务</summary>
        private static void RegistDomainServices(IServiceCollection serviceCollection)
        {
            //MediatR
            serviceCollection.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblies(GetAssemblyList().ToArray()); });
            ConsoleHelper.Debug("已注册 MediatR 服务通道，可以使用领域服务");
        }

        /// <summary>注册运行模块第一次初始化工作</summary>
        private static void RegistModulesFirstInit(List<ISimpleModule> appModules)
        {
            ConsoleHelper.Debug("检测到配置模块需要注册初始化事件，将开始注册各模块初始化事件 AppFirstInit ");
            foreach (var appmodule in appModules)
            {
                var moduleName = appmodule.GetType().Name;
                ConsoleHelper.Info($"开始注册模块 {moduleName} 初始化事件 AppFirstInit ");
                appmodule.AppFirstInit();
            }
        }

        /// <summary>注册模块服务配置</summary>
        private static void RegistSimpleModulesServices(List<ISimpleModule> appModules, List<Type> serviceTypes,
            IServiceCollection serviceCollection)
        {
            // 模块服务配置
            foreach (var appmodule in appModules)
            {
                appmodule.ConfigServices(serviceCollection, ConfigHelper.Configuration);
            }

            //注册所有需要自动注册的服务
            foreach (var service in serviceTypes)
            {
                var attr = service.GetCustomAttributes();
                var impInterFace = service.GetInterfaces();

                if (attr.Any(p => p.GetType() == typeof(TransientAttribute)))
                {
                    serviceCollection.AddTransient(service);
                    foreach (var interfaceImpl in impInterFace)
                    {
                        if (service.IsGenericType)
                        {
                            var constructedInterface = GetGeneric(interfaceImpl, service);
                            serviceCollection.AddTransient(constructedInterface, service);
                        }
                        else
                        {
                            serviceCollection.AddTransient(interfaceImpl, service);
                        }
                    }
                }

                if (attr.Any(p => p.GetType() == typeof(ScopedAttribute)))
                {
                    serviceCollection.AddScoped(service);
                    foreach (var interfaceImpl in impInterFace)
                    {
                        if (service.IsGenericType)
                        {
                            var constructedInterface = GetGeneric(interfaceImpl, service);
                            serviceCollection.AddScoped(constructedInterface, service);
                        }
                        else
                        {
                            serviceCollection.AddScoped(interfaceImpl, service);
                        }
                    }
                }

                if (attr.Any(p => p.GetType() == typeof(SingletonAttribute)))
                {
                    serviceCollection.AddSingleton(service);
                    foreach (var interfaceImpl in impInterFace)
                    {
                        if (service.IsGenericType)
                        {
                            var constructedInterface = GetGeneric(interfaceImpl, service);
                            serviceCollection.AddSingleton(constructedInterface, service);
                        }
                        else
                        {
                            serviceCollection.AddSingleton(interfaceImpl, service);
                        }
                    }
                }
            }
        }

        private static Type GetGeneric(Type interfaceDin, Type interfaceImpl)
        {
            // 获取泛型类型参数
            Type genericTypeArgument = interfaceDin.GetGenericArguments()[0];

            // 构建泛型类型实例
            Type constructedRepositoryType = interfaceImpl.MakeGenericType(genericTypeArgument);
            return constructedRepositoryType;
        }

        /// <summary>注册模块服务配app</summary>
        private static void RegistSimpleModulesApp(IApplicationBuilder app, List<ISimpleModule> appModules)
        {
            foreach (var appmodule in appModules)
            {
                ConsoleHelper.Info($"正在配置 Module {appmodule}");
                appmodule.ConfigApp(app, ConfigHelper.Configuration);
            }
        }

        //获取模块的程序集，构建模块列表，待注入列表
        private static (List<ISimpleModule>, List<Type>) GetAppModules()
        {
            var assemblies = GetAssemblyList();
            var appModules = new List<ISimpleModule>();
            var serviceTypes = new List<Type>();

            foreach (var item in assemblies)
            {
                var appModule = item.ExportedTypes.FirstOrDefault(w => w.BaseType == typeof(SimpleModule));
                if (appModule != null)
                {
                    var module = Activator.CreateInstance(appModule, true) as ISimpleModule;
                    appModules.Add(module);
                }

                // 获取等待注册的服务服务
                var service = item.ExportedTypes
                    .Where(t => t.CustomAttributes
                        .Any(attr =>
                            attr.AttributeType == typeof(TransientAttribute)
                            || attr.AttributeType == typeof(ScopedAttribute)
                            || attr.AttributeType == typeof(SingletonAttribute)
                        )
                    );
                serviceTypes.AddRange(service);
            }

            return (appModules, serviceTypes);
        }

        /// <summary>获取所有的 程序集</summary>
        /// <param name="where"></param>
        /// <returns></returns>
        private static List<Assembly> GetAssemblyList(Func<Assembly, bool> where = null)
        {
            var list = new List<Assembly>();

            // 获取入口程序集和直接引用的程序集
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null) return list;

            var referencedAssemblies = entryAssembly.GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Where(a => !a.FullName.Contains("Microsoft") && !a.FullName.Contains("System"));

            list.AddRange(new List<Assembly> { entryAssembly }
                .Union(referencedAssemblies));

            // 优化：只加载必要的DLL，减少内存使用
            var essentialDlls = new[]
            {
                "Simple.AdminApplication.dll",
                "Simple.ContractApplication.dll",
                "Simple.PrintApplication.dll",
                "Simple.TenantsApplication.dll",
                "Simple.Interfaces.dll",
                "Simple.AdminController.dll"
            };

            var dllPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory)
                .Where(path => path.EndsWith(".dll") &&
                               !path.Contains("Microsoft") &&
                               !path.Contains("System") &&
                               essentialDlls.Any(essential => path.Contains(essential)));

            foreach (var path in dllPaths)
            {
                try
                {
                    var assembly = Assembly.Load(AssemblyLoadContext.GetAssemblyName(path));
                    if (!list.Contains(assembly))
                    {
                        list.Add(assembly);
                    }
                }
                catch
                {
                }
            }

            return @where == null ? list : list.Where(@where).ToList();
        }

        /// <summary>注册主机服务，同时注册所有模块的Service，配置于服务最后</summary>
        public static void BuildHostService(IServiceCollection serviceCollection)
        {
            var (appModules, serviceTypes) = GetAppModules();

            RegistSimpleModulesServices(appModules, serviceTypes, serviceCollection);
            RegistDomainServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
            _httpContextAccessor = ServiceProvider.GetService<IHttpContextAccessor>();

            _simpleModules = appModules;
        }

        /// <summary>构建主机，同时配置所有模块的服务配置</summary>
        /// <param name="app"></param>
        public static void BuildHostApp(IApplicationBuilder app)
        {
            RegistSimpleModulesApp(app, _simpleModules);
            RegistModulesFirstInit(_simpleModules);
            _simpleModules.Clear();
        }

        /// <summary>创建服务域</summary>
        /// <returns></returns>
        public static IServiceScope CreateScope() => ServiceProvider.CreateScope();

        /// <summary>创建服务,参数若已经注入，则自动获取</summary>
        /// <returns></returns>
        public static T CreateInstance<T>() => ActivatorUtilities.CreateInstance<T>(ServiceProvider);

        /// <summary>创建服务,参数若已经注入，则自动获取</summary>
        /// <returns></returns>
        public static object CreateInstance(Type type) => ActivatorUtilities.CreateInstance(ServiceProvider, type);

        /// <summary>添加控制器配置文件，包括全局异常配置、json序列化配置</summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddControllerConfig(this IServiceCollection services)
        {
            services.AddControllers(option =>
            {
                //关闭不可为空引用类型的属性 例如String
                option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                option.Filters.Add<ValidateModelAttribute>();
                option.Filters.Add<GlobalExceptionFilter>();
                //option.Filters.Add<AuthenticationFilter>();
                option.ValueProviderFactories.Add(new JQueryQueryStringValueProviderFactory());
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver(); //序列化时key为驼峰样式
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; //忽略循环引用
            });
            return services;
        }

        /// <summary>添加基于Redis的Session</summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRedisCacheAndSession(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            //将Redis分布式缓存服务添加到服务中
            // services.AddStackExchangeRedisCache(options =>
            // {
            //     //Redis实例名
            //     options.InstanceName = "DistributedCache";
            //     options.ConfigurationOptions = new ConfigurationOptions()
            //     {
            //         ConnectTimeout = 2000,
            //         DefaultDatabase = ConfigHelper.GetValue<int>("Redis:Database"),
            //         //Password = "xxxxxx",
            //         AllowAdmin = true,
            //         AbortOnConnectFail = false,//当为true时，当没有可用的服务器时则不会创建一个连接
            //     };
            //     options.ConfigurationOptions.EndPoints.Add(ConfigHelper.GetValue("Redis:Hosts"));
            // });
            services.AddDistributedMemoryCache(); // 开发和测试环境模拟，不推荐在生产环境使用
            // services.AddMemoryCache(); // 本地内存缓存，单实例适用

            services.AddSession(op => { op.IdleTimeout = TimeSpan.FromMinutes(30); });
            return services;
        }

        /// <summary>添加自定义跨域</summary>
        /// <param name="services"></param>
        public static void AddCustomerCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    var urls = ConfigHelper.GetValue("CorsUrl");
                    builder.WithOrigins(urls.Split(',')) // 允许部分站点跨域请求
                        .AllowAnyMethod() // 允许所有请求方法
                        .AllowAnyHeader() // 允许所有请求头
                        .AllowCredentials(); // 允许Cookie信息
                });
            });
        }

        /// <summary>配置跨域管道</summary>
        /// <param name="app"></param>
        public static void UseCustomerCors(this IApplicationBuilder app)
        {
            app.UseCors("AllowSpecificOrigin");
        }
    }
}