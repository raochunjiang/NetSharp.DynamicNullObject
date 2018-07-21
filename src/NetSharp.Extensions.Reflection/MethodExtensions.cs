using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace NetSharp.Extensions.Reflection
{
    public static class MethodExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, PropertyInfo> dictionary = new ConcurrentDictionary<MethodInfo, PropertyInfo>();

        /// <summary>
        /// 验证当前方法是否为属性绑定方法(getter/setter)
        /// </summary>
        public static bool IsPropertyBinding(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return method.GetBindingProperty() != null;
        }
        /// <summary>
        /// 返回当前方法绑定的属性信息。如果当前方法不是属性绑定方法（getter/setter）则返回 null。
        /// </summary>
        public static PropertyInfo GetBindingProperty(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return dictionary.GetOrAdd(method, m =>
            {
                foreach (var property in m.DeclaringType.GetTypeInfo().GetProperties())
                {
                    if (property.CanRead && property.GetMethod == m)
                    {
                        return property;
                    }

                    if (property.CanWrite && property.SetMethod == m)
                    {
                        return property;
                    }
                }
                return null;
            });
        }

        /// <summary>
        /// 返回当前方法定义的参数类型集合。
        /// </summary>
        public static Type[] GetParameterTypes(this MethodBase method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return method.GetParameters().Select(p => p.ParameterType).ToArray();
        }

        /// <summary>
        /// 验证当前方法是否为异步(async)方法。
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsAsynchronous(this MethodBase method)
        {
            Type asncStateMachine = typeof(AsyncStateMachineAttribute);
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(asncStateMachine);
            return (attrib != null);
        }
    }
}
