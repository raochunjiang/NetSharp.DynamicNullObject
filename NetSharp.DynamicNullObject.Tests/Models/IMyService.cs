using System;
using System.Collections.Generic;
using System.Text;

namespace NetSharp.DynamicNullObject.Tests.Models
{
    public interface IMyService
    {
        int Id { get; set; }
        void MethodA(int arg);

        string MethodB();

    }
}
