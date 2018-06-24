using System;
using System.Collections.Generic;
using System.Text;

namespace NetSharp.DynamicNullObject
{
    /// <summary>
    /// 提供返回指定接口类型的空实现类型的操作。
    /// </summary>
    public interface IEmptyClassProvider
    {
        /// <summary>
        /// 返回指定接口类型的空实现类型。
        /// </summary>
        /// <param name="interfaceType">接口类型。</param>
        /// <returns>指定接口类型的空实现类型。</returns>
        Type ClassOf(Type interfaceType);

        /// <summary>
        /// 返回指定接口类型参数的空实现类型。
        /// </summary>
        /// <typeparam name="TInterface">接口类型。</typeparam>
        /// <returns>指定接口类型参数的空实现类型。</returns>
        Type ClassOf<TInterface>();
    }
}
