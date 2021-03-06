﻿using NetSharp.DynamicNullObject.Globalization;
using NetSharp.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NetSharp.DynamicNullObject
{
    /// <summary>
    /// 提供返回指定接口类型的空实现实例的操作。
    /// </summary>
    public class EmptyObjectProvider
    {
        private IEmptyClassProvider _emptyClassProvider;
        public EmptyObjectProvider(IEmptyClassProvider emptyClassProvider)
        {
            _emptyClassProvider = emptyClassProvider;
        }

        /// <summary>
        /// 返回指定接口类型的空实现实例。
        /// </summary>
        /// <param name="interfaceType">接口类型。</param>
        /// <param name="typeArguments">当指定的接口类型为泛型定义时，需要指定相应的类型参数。</param>
        /// <returns>指定接口类型的空实现实例。</returns>
        public object GetEmptyObject(Type interfaceType, params Type[] typeArguments)
        {
            var emptyClass = _emptyClassProvider.GetEmptyClass(interfaceType);
            if (emptyClass.GetTypeInfo().IsGenericTypeDefinition)
            {
                emptyClass = emptyClass.MakeGenericType(typeArguments);
            }
            return Activator.CreateInstance(emptyClass);
        }

        /// <summary>
        /// 返回指定接口类型参数的空实现实例。
        /// </summary>
        /// <typeparam name="TInterface">接口类型。</typeparam>
        /// <returns>指定接口类型参数的空实现实例。</returns>
        public TInterface GetEmptyObject<TInterface>(/*params Type[] typeArguments*/)
        {
            var emptyClass = _emptyClassProvider.GetEmptyClass<TInterface>();
            return (TInterface)Activator.CreateInstance(emptyClass);
        }
    }
}
