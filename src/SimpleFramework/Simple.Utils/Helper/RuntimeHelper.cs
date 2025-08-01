using System.Reflection;
using System.Runtime.Loader;
using System.Security.AccessControl;

namespace Simple.Utils.Helper
{
    /// <summary>运行时帮助类</summary>
    public class RuntimeHelper
    {
        private const string DllNamePrefix = "Simple.";

        /// <summary>获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包</summary>
        /// <returns></returns>
        public static IList<Assembly> GetAllAssemblies()
        {
            var list = AssemblyLoadContext.Default.Assemblies
                .Where(p => !p.FullName.Contains("Microsoft.")
                            && !p.FullName.Contains("System.") &&
                            (string.IsNullOrEmpty(DllNamePrefix) || p.FullName.Contains(DllNamePrefix))
                ).ToList();
            return list;
        }

        /// <summary>获取指定名称程序集</summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static Assembly GetAssembly(string assemblyName)
        {
            return GetAllAssemblies().FirstOrDefault(assembly => assembly.FullName.Contains(assemblyName));
        }

        /// <summary>获取所有类型</summary>
        /// <returns></returns>
        public static List<Type> GetAllTypes()
        {
            var list = new List<Type>();
            var essentialAssemblies = GetAllAssemblies();
            foreach (var assembly in essentialAssemblies)
            {
                var typeInfos = assembly.DefinedTypes;
                foreach (var typeInfo in typeInfos)
                {
                    list.Add(typeInfo.AsType());
                }
            }

            return list;
        }

        /// <summary>获取所有类型</summary>
        /// <returns></returns>
        public static List<Type> GetAllTypes(Predicate<Type> predicate)
        {
            var list = new List<Type>();
            foreach (var assembly in GetAllAssemblies())
            {
                var typeInfos = assembly.DefinedTypes;
                foreach (var typeInfo in typeInfos)
                {
                    var type = typeInfo.AsType();
                    if (predicate(type))
                    {
                        list.Add(type);
                    }
                }
            }

            return list;
        }

        public static IList<Type> GetTypesByAssembly(string assemblyName)
        {
            var list = new List<Type>();
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
            var typeInfos = assembly.DefinedTypes;
            foreach (var typeInfo in typeInfos)
            {
                list.Add(typeInfo.AsType());
            }

            return list;
        }

        public static Type GetImplementType(string typeName, Type baseInterfaceType)
        {
            return GetAllTypes().FirstOrDefault(t =>
            {
                if (t.Name == typeName &&
                    t.GetTypeInfo().GetInterfaces().Any(b => b.Name == baseInterfaceType.Name))
                {
                    var typeInfo = t.GetTypeInfo();
                    return typeInfo.IsClass && !typeInfo.IsAbstract && !typeInfo.IsGenericType;
                }

                return false;
            });
        }
    }
}