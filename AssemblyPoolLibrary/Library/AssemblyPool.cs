namespace Library
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Exceptions;

    public static class AssemblyPool
    {
        private static readonly ConcurrentDictionary<Type, object> ObjectsByType = new ConcurrentDictionary<Type, object>();

        public static T GetInstance<T>()
        {
            var typeofT = typeof(T);
            T instance = (T)GetInstance(typeofT);
            return instance;
        }

        private static object GetInstance(Type typeofT)
        {
            object instance = null;
            if (!ObjectsByType.ContainsKey(typeofT))
            {
                if (typeofT.IsClass)
                {
                    var constructors = typeofT
                        .GetConstructors()
                        .OrderByDescending(cc => cc.GetParameters().Length)
                        .ToArray();
                    if (constructors.Length == 0)
                    {
                        var properties = TypeWorker.GetInstanceTypes(typeofT);
                        if (properties.Any(t => t.DeclaringType.FullName == typeofT.FullName))
                        {
                            var instanceTypeProperty = properties.FirstOrDefault(t => t.DeclaringType.FullName == typeofT.FullName);
                            var getterAccessor = instanceTypeProperty
                                .GetAccessors()
                                .FirstOrDefault(accessor => accessor.Name.StartsWith("get_"));
                            instance = GetInstanceByGetter(typeof(Func<object>), getterAccessor);
                            ObjectsByType[typeofT] = instance;
                            return instance;
                        }
                        else
                        {
                            throw new InstantiationException(
                                $"The {nameof(AssemblyPool)} cannot get instance from object without constructors.");
                        }
                    }

                    var emptyConstructor = constructors.FirstOrDefault(cc => cc.GetParameters().Length == 0);
                    if (emptyConstructor != null)
                    {
                        var constructorNonParameter = emptyConstructor.GetParameters();
                        var parameters = constructorNonParameter.Select(cp => Expression.Parameter(cp.ParameterType));
                        instance = GetInstance(emptyConstructor, parameters.ToArray());
                        ObjectsByType[typeofT] = instance;
                    }
                    else
                    {
                        var firstConstructor = constructors.FirstOrDefault();
                        if (firstConstructor != null)
                        {
                            var constructorParameters = firstConstructor.GetParameters();
                            foreach (var parameter in constructorParameters)
                            {
                                TypeCheckInsurence(parameter);
                                GetInstance(parameter.ParameterType);
                            }

                            var parameters = constructorParameters.Select(cp => Expression.Parameter(cp.ParameterType));
                            instance = GetInstance(firstConstructor, parameters.ToArray());
                            ObjectsByType[typeofT] = instance;
                        }
                    }
                }
                else if (typeofT.IsInterface)
                {
                    var instances = TypeWorker.GetInheritedInterfaceByTypes(typeofT);
                    if (!instances.Any())
                    {
                        throw new InstantiationException(
                            "There's no type in self-type assembly, which inherit current interface.");
                    }

                    StringWorker stringWorker = new StringWorker();
                    var nameOfType = typeofT.Name;
                    var mostCommonWordsInTypeName =
                        stringWorker
                            .GetTheMostCommonWordsInString(
                                nameOfType,
                                instances.Select(inst => inst.Name).ToArray());
                    var typeOfMostCommonTypeSimilarToInterface =
                        instances.FirstOrDefault(t => t.Name == mostCommonWordsInTypeName);
                    instance = GetInstance(typeOfMostCommonTypeSimilarToInterface);
                }
                else if (typeofT.IsEnum)
                {
                    throw new InstantiationException(
                            $"The {nameof(AssemblyPool)} cannot get instance enumeration.");
                }
            }
            else
            {
                instance = ObjectsByType[typeofT];
            }

            return instance;
        }

        private static object GetInstanceByGetter(Type delegateType, MethodInfo getter)
        {
            MethodCallExpression methodCallExpression = Expression.Call(getter);
            LambdaExpression lambdaExpression = Expression.Lambda(delegateType, methodCallExpression);
            return lambdaExpression.Compile().DynamicInvoke();
        }

        private static object GetInstance(ConstructorInfo constructor, params ParameterExpression[] parameterExpressions)
        {
            NewExpression newExpression;
            LambdaExpression lambdaExpression;
            if (parameterExpressions.Length == 0)
            {
                newExpression = Expression.New(constructor);
                lambdaExpression = Expression.Lambda(newExpression);
                return lambdaExpression.Compile().DynamicInvoke();
            }

            newExpression = Expression.New(constructor, parameterExpressions);
            lambdaExpression = Expression.Lambda(newExpression, parameterExpressions);

            return lambdaExpression.Compile().DynamicInvoke(parameterExpressions.Select(p => ObjectsByType[p.Type]).ToArray());
        }

        private static void TypeCheckInsurence(ParameterInfo parameter)
        {
            switch (parameter.ParameterType.FullName)
            {
                default:
                    break;
            }
        }
    }
}