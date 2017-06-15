namespace Library
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class TypeWorker
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> PropertiesByType =
               new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        public static IEnumerable<PropertyInfo> GetInstanceTypes<TInstance>()
        {
            var instanceType = typeof(TInstance);
            return GetInstanceTypes(instanceType);
        }

        public static IEnumerable<PropertyInfo> GetInstanceTypes(Type instanceType)
        {
            if (!PropertiesByType.ContainsKey(instanceType))
            {
                var instanceTypeInfo = instanceType.GetTypeInfo();
                var properties = instanceTypeInfo
                    .GetProperties(
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.Public |
                        BindingFlags.Static |
                        BindingFlags.SetProperty |
                        BindingFlags.GetProperty);
                PropertiesByType.AddOrUpdate(instanceType, properties, (t, s) => PropertiesByType[t]);
            }

            return PropertiesByType[instanceType];
        }

        public static bool IsInstanceContainsType<TInstance, TType>()
        {
            var typeofInstance = typeof(TInstance);
            var typeofType = typeof(TType);
            var isContainsKey = PropertiesByType.ContainsKey(typeofInstance);
            bool hasAnyTypeEqualToSearched = false;
            if (isContainsKey)
            {
                string fullnameType = typeofType.FullName;

                int indexOfLastDotInFullname = fullnameType.LastIndexOf('.') + 1;
                string typename = fullnameType.Substring(indexOfLastDotInFullname);
                string possibleInterfaceName = $"I{typename}";
                hasAnyTypeEqualToSearched = PropertiesByType[typeofInstance]
                    .FirstOrDefault(pt => pt.PropertyType.FullName == fullnameType || pt.PropertyType.Name == possibleInterfaceName) != null;
            }

            return isContainsKey && hasAnyTypeEqualToSearched;
        }

        public static IEnumerable<TypeInfo> GetInheritedInterfaceByTypes<TInterface>()
        {
            var interfaceType = typeof(TInterface);
            return GetInheritedInterfaceByTypes(interfaceType);
        }

        public static IEnumerable<TypeInfo> GetInheritedInterfaceByTypes(Type @interface)
        {
            var assemblyByInterface = @interface.Assembly;
            var assemblyTypes = assemblyByInterface.DefinedTypes;
            var instances = assemblyTypes.Where(@interface.IsAssignableFrom);
            return instances.Where(t => t.IsClass);
        }
    }
}