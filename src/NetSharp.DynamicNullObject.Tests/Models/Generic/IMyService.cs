using System;
using System.Collections.Generic;
using System.Text;

namespace NetSharp.DynamicNullObject.Tests.Models.Generic
{
    public interface IMyService<T>
    {
        T Id { get; set; }
        void MethodA<TArg>(Func<TArg> arg1,Func<T> arg2);
        T MethodB();
    }
}
