using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetSharp.DynamicNullObject.Tests.Models.Generic
{
    public class MyService<T>
    {
        public interface IMyNestedService<TNested1, TNested2>
            where TNested1 : struct
            where TNested2 : class
        {
            [MyCustom]
            int Id { get; set; }

            void MethodA(TNested1 arg);

            TNested2 MethodB();

            Task MethodC();

            Task<TNested2> MethodD();
        }

        public interface IMyNestedServiceWithConstructorConstraint<TNested1>
            where TNested1 : new()
        {

        }
    }
    public class MyCustomAttribute : Attribute
    {

    }
}
