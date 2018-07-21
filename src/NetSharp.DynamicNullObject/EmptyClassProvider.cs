using NetSharp.DynamicNullObject.Globalization;
using NetSharp.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NetSharp.DynamicNullObject
{
    /// <summary>
    /// 提供返回指定接口类型的空实现类型的操作。
    /// </summary>
    public class EmptyClassProvider : IEmptyClassProvider
    {
        private EmptyClassGenerator _generator;
        public AssemblyBuilder Assembly { get; }
        public EmptyClassProvider()
        {
            _generator = EmptyClassGenerator.Default;
        }

        public EmptyClassProvider(AssemblyBuilder dynamicAssembly)
            : this(dynamicAssembly, dynamicAssembly.FullName)
        {
        }

        public EmptyClassProvider(AssemblyBuilder dynamicAssembly, string @namespace)
        {
            // TODO:缓存命名空间对应的生成器，避免不明确的类型引用
            _generator = new EmptyClassGenerator(dynamicAssembly, @namespace);
            Assembly = dynamicAssembly;
        }

        /// <summary>
        /// 返回指定接口类型的空实现类型。
        /// </summary>
        /// <param name="interfaceType">接口类型。</param>
        /// <returns>指定接口类型的空实现类型。</returns>
        public Type GetEmptyClass(Type interfaceType)
        {
            var interfaceTypeInfo = interfaceType?.GetTypeInfo() ?? throw new ArgumentNullException(nameof(interfaceType));
            if (!interfaceTypeInfo.IsInterface)
            {
                throw new InvalidOperationException(string.Format(ExceptionMessage.TheSpecifiedTypeShouldBeAnInterfaceType, nameof(interfaceType)));
            }
            if (!interfaceTypeInfo.IsNestedVisible())
            {
                throw new InvalidOperationException(string.Format(ExceptionMessage.UnableToAccessTheSpecifiedType, nameof(interfaceType)));
            }
            return _generator.GetEmptyClass(interfaceType);
        }

        /// <summary>
        /// 返回指定接口类型参数的空实现类型。
        /// </summary>
        /// <typeparam name="TInterface">接口类型。</typeparam>
        /// <returns>指定接口类型参数的空实现类型。</returns>
        public Type GetEmptyClass<TInterface>()
        {
            var interfaceTypeInfo = typeof(TInterface).GetTypeInfo();
            if (!interfaceTypeInfo.IsInterface)
            {
                throw new InvalidOperationException(string.Format(ExceptionMessage.TheSpecifiedTypeShouldBeAnInterfaceType, nameof(TInterface)));
            }
            if (!interfaceTypeInfo.IsNestedVisible())
            {
                throw new InvalidOperationException(string.Format(ExceptionMessage.UnableToAccessTheSpecifiedType, nameof(TInterface)));
            }

            return _generator.GetEmptyClass(interfaceTypeInfo);
        }
    }
}
