using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetSharp.Extensions.Reflection
{
    public static class TypeExtensions
    {
        /// <summary>
        /// 返回当前类型的编程名称。
        /// </summary>
        public static string GetProgrammingName(this Type type)
        {
            var typeInfo = type?.GetTypeInfo() ?? throw new ArgumentNullException(nameof(type));

            var name = typeInfo.Name.Replace('+', '.');
            if (typeInfo.IsGenericParameter)
            {
                return name;
            }
            if (typeInfo.IsGenericType)
            {
                var contained = typeInfo.ContainsGenericParameters;
                var arguments = typeInfo.IsGenericTypeDefinition
                 ? typeInfo.GenericTypeParameters
                 : typeInfo.GenericTypeArguments;
                var index = name.IndexOf("`");
                name = index > 0 ? name.Substring(0, index) : name;
                name += $"<{GetProgrammingName(arguments[0])}";
                for (var i = 1; i < arguments.Length; i++)
                {
                    name = name + ", " + GetProgrammingName(arguments[i]);
                }
                name += ">";
            }
            if (!typeInfo.IsNested)
                return name;
            return $"{name}";
        }

        /// <summary>
        /// 返回当前类型的完全限定编程名称，包含其命名空间。
        /// </summary>
        public static string GetProgrammingFullName(this Type type)
        {
            var typeInfo = type?.GetTypeInfo() ?? throw new ArgumentNullException(nameof(type));

            var name = typeInfo.Name.Replace('+', '.');
            if (typeInfo.IsGenericParameter)
            {
                return name;
            }
            if (!typeInfo.IsNested)
            {
                name = $"{typeInfo.Namespace}." + name;
            }
            else
            {
                name = $"{GetProgrammingFullName(typeInfo.DeclaringType.GetTypeInfo())}.{name}";
            }
            if (typeInfo.IsGenericType)
            {
                var arguments = typeInfo.IsGenericTypeDefinition
                 ? typeInfo.GenericTypeParameters
                 : typeInfo.GenericTypeArguments;
                var index = name.IndexOf("`");
                name = index > 0 ? name.Substring(0, index) : name;
                name += $"<{GetProgrammingFullName(arguments[0].GetTypeInfo())}";
                for (var i = 1; i < arguments.Length; i++)
                {
                    name += ", " + GetProgrammingFullName(arguments[i].GetTypeInfo());
                }
                name += ">";
            }
            return name;
        }

        /// <summary>
        /// 验证是否可在所属程序集之外透过其嵌套层次访问当前类型。
        /// </summary>
        /// <remarks>如果类型的嵌套层次中每个类型都可以在其所属程序集之外被访问，则该类型嵌套可见。</remarks>
        public static bool IsNestedVisible(this Type type)
        {
            var typeInfo = type?.GetTypeInfo() ?? throw new ArgumentNullException(nameof(type));

            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            if (typeInfo.IsNested)
            {
                if (!typeInfo.DeclaringType.GetTypeInfo().IsNestedVisible())
                {
                    return false;
                }
                if (!typeInfo.IsVisible || !typeInfo.IsNestedPublic)
                {
                    return false;
                }
            }
            else
            {
                if (!typeInfo.IsVisible || !typeInfo.IsPublic)
                {
                    return false;
                }
            }
            if (typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
            {
                foreach (var argument in typeInfo.GenericTypeArguments)
                {
                    if (!argument.GetTypeInfo().IsNestedVisible())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 查找并返回当前类型所实现的嵌套可见的接口集合。
        /// </summary>
        /// <param name="exceptInterfaces">结果集中不应包含的接口类型集合。</param>
        public static IEnumerable<Type> GetNestedVisibleInterfaces(this Type type, params Type[] exceptInterfaces)
        {
            var typeInfo = type?.GetTypeInfo() ?? throw new ArgumentNullException(nameof(type));

            var hashSet = new HashSet<Type>(exceptInterfaces);
            foreach (var interfaceType in typeInfo.GetInterfaces().Distinct())
            {
                if (!interfaceType.IsNestedVisible())
                {
                    continue;
                }
                if (!hashSet.Contains(interfaceType))
                {
                    if (interfaceType.GetTypeInfo().ContainsGenericParameters && typeInfo.ContainsGenericParameters)
                    {
                        if (!hashSet.Contains(interfaceType.GetGenericTypeDefinition()))
                            yield return interfaceType;
                    }
                    else
                    {
                        yield return interfaceType;
                    }
                }
            }
        }

        /// <summary>
        /// 验证当前类型是否为 <see cref="Task"/> 类型。
        /// </summary>
        public static bool IsTask(this Type type)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            return type == typeof(Task);
        }

        /// <summary>
        /// 验证当前类型是否为 <see cref="Task{}"/> 类型。
        /// </summary>
        public static bool IsTaskWithResult(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return type.IsGenericType && typeof(Task).GetTypeInfo().IsAssignableFrom(type);
        }

        // 后期兼容 C# 7.0 时采用
        //public static bool IsValueTask(this TypeInfo typeInfo)
        //{
        //    if (typeInfo == null)
        //    {
        //        throw new ArgumentNullException(nameof(typeInfo));
        //    }
        //    return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ValueTask<>);
        //}
    }
}
