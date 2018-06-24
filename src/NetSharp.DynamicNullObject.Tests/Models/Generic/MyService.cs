using System;
using System.Collections.Generic;
using System.Text;

namespace NetSharp.DynamicNullObject.Tests.Models.Generic
{
    public class MyService<T>
    {
        public interface IMyNestedService<TNested1, TNested2>
            where TNested1 : struct
            where TNested2 : class
        {

        }
    }
}
