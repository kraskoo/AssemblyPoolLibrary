namespace Library
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class AssemblyPool
    {
        private static readonly IDictionary<Type, object> ObjectsByType = new ConcurrentDictionary<Type, object>();

        public static T GetGenericInstance<T>()
        {
            var type = typeof(T);
            if (!ObjectsByType.ContainsKey(type) && (type.IsClass || type.IsInterface))
            {
                var suitableConstructor = GetOrderedConstructors(type).FirstOrDefault();
                var parameters = suitableConstructor.GetConstructorParameterTypes().Where(pt => !ObjectsByType.ContainsKey(pt.ParameterType)).ToArray();
                if (parameters.Any())
                {
                    parameters.ExecuteInstantiation(p => GetNonGenericInstance(p.ParameterType));
                    var instatiatedParameters = parameters.Select(ip => Expression.Parameter(ip.ParameterType, ip.Name)).ToArray();
                    ObjectsByType.Add(type, suitableConstructor.GetInstanceByExpression<T>(instatiatedParameters));
                }
                else
                {
                    ObjectsByType.Add(type, suitableConstructor.GetInstanceByExpression<T>());
                }
            }

            if (ObjectsByType.ContainsKey(type))
            {
                return (T)ObjectsByType[type];
            }

            return default(T);
        }

        public static object GetNonGenericInstance(Type type)
        {
            if (!ObjectsByType.ContainsKey(type) && (type.IsClass || type.IsInterface))
            {
                var suitableConstructor = GetOrderedConstructors(type).FirstOrDefault();
                var parameters = suitableConstructor.GetConstructorParameterTypes().Where(pt => !ObjectsByType.ContainsKey(pt.ParameterType)).ToArray();
                if (parameters.Any())
                {
                    parameters.ExecuteInstantiation(p => GetNonGenericInstance(p.ParameterType));
                    var instatiatedParameters = parameters.Select(ip => Expression.Parameter(ip.ParameterType, ip.Name)).ToArray();
                    var instance = suitableConstructor.GetInstanceByExpression(instatiatedParameters);
                    ObjectsByType.Add(type, instance);
                }
                else
                {
                    var instance = suitableConstructor.GetInstanceByExpression();
                    ObjectsByType.Add(type, instance);
                }
            }

            if (ObjectsByType.ContainsKey(type))
            {
                return ObjectsByType[type];
            }

            return null;
        }

        private static T GetInstanceByExpression<T>(
            this ConstructorInfo suitableConstructor,
            params ParameterExpression[] parameterExpressions)
        {
            NewExpression newExpression;
            LambdaExpression lambda;
            if (parameterExpressions.Length > 0)
            {
                newExpression = Expression.New(suitableConstructor, parameterExpressions);
                lambda = Expression.Lambda(newExpression, parameterExpressions);
                return (T)lambda.Compile().DynamicInvoke(parameterExpressions.Select(ae => ObjectsByType[ae.Type]).ToArray());
            }

            newExpression = Expression.New(suitableConstructor);
            lambda = Expression.Lambda(newExpression);
            return (T)lambda.Compile().DynamicInvoke();
        }

        private static object GetInstanceByExpression(
            this ConstructorInfo suitableConstructor,
            params ParameterExpression[] parameterExpressions)
        {
            NewExpression newExpression;
            LambdaExpression lambda;
            if (parameterExpressions.Length > 0)
            {
                newExpression = Expression.New(suitableConstructor, parameterExpressions);
                lambda = Expression.Lambda(newExpression, parameterExpressions);
                return lambda.Compile().DynamicInvoke(parameterExpressions.Select(ae => ObjectsByType[ae.Type]).ToArray());
            }

            newExpression = Expression.New(suitableConstructor);
            lambda = Expression.Lambda(newExpression);
            return lambda.Compile().DynamicInvoke();
        }

        private static void ExecuteInstantiation<T>(this IEnumerable<T> enumerable, Func<T, object> instantiationFunc)
        {
            foreach (var item in enumerable)
            {
                instantiationFunc(item);
            }
        }

        private static ConstructorInfo[] GetOrderedConstructors(Type type)
        {
            if (type.IsInterface)
            {
                type = GetSuitableImplementation(type);
            }
            else if (!type.IsClass && !type.IsInterface)
            {
                throw new ArgumentException("The method requires class or interface type");
            }

            var classConstructors = type.GetTypeConstructors();
            return GetUsableOrderedConstructors(classConstructors);
        }

        private static ConstructorInfo[] GetUsableOrderedConstructors(IEnumerable<ConstructorInfo> classConstructors)
        {
            return classConstructors
                .Where(cc => cc.IsPublic && !cc.IsStatic && !cc.IsAbstract)
                .OrderByDescending(cc => cc.GetConstructorParameterTypes().Count())
                .ThenBy(cc => cc.GetConstructorParameterTypes().Count(pt => ObjectsByType.ContainsKey(pt.ParameterType)))
                .ThenBy(cc => cc.GetConstructorParameterTypes().Count(pt => pt.ParameterType.IsGenericType))
                .ThenByDescending(cc => cc.GetConstructorParameterTypes().Count(pt => !pt.ParameterType.IsGenericType))
                .ToArray();
        }

        private static Type GetSuitableImplementation(Type type)
        {
            type.TryExplicitIfNotContains();
            var classes = type.GetClassesByInterfaceType().ToArray();
            if (classes.Length == 0)
            {
                throw new ArgumentException(
                    "There are no class types in currently selected projects, which implements this concrete interface. Please make sure that you pass an abstraction of existence instance.");
            }

            if (classes.Length == 1)
            {
                return classes.First();
            }

            var friendlyInterfaceName = type.Name.Substring(1);
            var mostSuitableInstance = classes.FirstOrDefault(c => c.FullName.Contains(friendlyInterfaceName));
            return mostSuitableInstance == null ? classes.FirstOrDefault() : mostSuitableInstance;
        }
    }
}