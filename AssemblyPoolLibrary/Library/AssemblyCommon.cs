namespace Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class AssemblyCommon
    {
        private static readonly IDictionary<Type, HashSet<Type>> InterfacesByClass;
        private static readonly IDictionary<Type, HashSet<Type>> ClassesByInterface;

        static AssemblyCommon()
        {
            ClassesByInterface = new Dictionary<Type, HashSet<Type>>();
            InterfacesByClass = new Dictionary<Type, HashSet<Type>>();
            Assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = Assemblies
                .SelectMany(asm => asm.GetTypes().Where(t => t.ToString().StartsWith(ProjectNameContainer.Project)));
            NotInterfaceTypes = new HashSet<Type>(types.Where(t => !t.IsInterface));
            InterfacesByClass = new Dictionary<Type, HashSet<Type>>(
                NotInterfaceTypes
                .Select(nit => new
                {
                    Key = nit,
                    Value = GetTypeInterfaces(nit.GetInterfaces(), nit)
                })
                .ToDictionary(t => t.Key, t => t.Value));
        }

        public static Assembly[] Assemblies { get; }

        public static HashSet<Type> NotInterfaceTypes { get; }

        public static IEnumerable<Type> GetInterfacesByClassType(this Type classType)
        {
            return InterfacesByClass[classType];
        }

        public static IEnumerable<Type> GetClassesByInterfaceType(this Type interfaceType)
        {
            return ClassesByInterface[interfaceType];
        }

        public static void TryExplicitIfNotContains(this Type withType)
        {
            if (withType.IsInterface && !ClassesByInterface.ContainsKey(withType))
            {
                TryToFindSuitableClass(withType);
            }
            else if (withType.IsClass && !InterfacesByClass.ContainsKey(withType))
            {
                TryToFindSuitableInterface(withType);
            }
        }

        // The methods below serve for instantiations class types logic.
        public static IEnumerable<ConstructorInfo> GetTypeConstructors(
            this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors;
        }

        public static IEnumerable<ParameterInfo> GetConstructorParameterTypes(
            this ConstructorInfo constructor)
        {
            return constructor.GetParameters();
        }

        private static HashSet<Type> GetTypeInterfaces(
            IEnumerable<Type> interfaces, Type implementation)
        {
            foreach (var @interface in interfaces)
            {
                if (!InterfacesByClass.ContainsKey(@interface))
                {
                    InterfacesByClass.Add(@interface, new HashSet<Type>());
                }

                InterfacesByClass[@interface].Add(implementation);
            }

            return new HashSet<Type>(interfaces);
        }

        private static void TryToFindSuitableClass(Type @interface)
        {
            var classes = InterfacesByClass
                .Where(t => t.Value
                    .Any(i => $"{i.Namespace}.{i.Name}" == $"{@interface.Namespace}.{@interface.Name}"))
                .Select(t => t.Key)
                .ToArray();
            if (classes.Length > 0)
            {
                foreach (var @class in classes)
                {
                    if (!InterfacesByClass.ContainsKey(@class))
                    {
                        InterfacesByClass.Add(@class, new HashSet<Type>());
                    }

                    InterfacesByClass[@class].Add(@interface);
                    if (!ClassesByInterface.ContainsKey(@interface))
                    {
                        ClassesByInterface.Add(@interface, new HashSet<Type>());
                    }

                    ClassesByInterface[@interface].Add(@class);
                }
            }
        }

        private static void TryToFindSuitableInterface(Type @class)
        {
            var interfaces = ClassesByInterface
                .Where(t => t.Value
                    .Any(c => $"{c.Namespace}.{c.Name}" == $"{@class.Namespace}.{@class.Name}"))
                .Select(t => t.Key)
                .ToArray();
            if (interfaces.Length > 0)
            {
                foreach (var @interface in interfaces)
                {
                    if (!ClassesByInterface.ContainsKey(@interface))
                    {
                        ClassesByInterface.Add(@interface, new HashSet<Type>());
                    }

                    ClassesByInterface[@interface].Add(@class);
                    if (!InterfacesByClass.ContainsKey(@class))
                    {
                        InterfacesByClass.Add(@class, new HashSet<Type>());
                    }

                    InterfacesByClass[@class].Add(@interface);
                }
            }
        }
    }
}