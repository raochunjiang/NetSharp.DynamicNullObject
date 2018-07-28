using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetSharp.DynamicNullObject.Samples
{
    public interface IFakeScenario
    {
        int ValueProperty { get; set; }

        object ReferenceProperty { get; set; }

        int GetValueObject();

        object GetReferenceObject();

        void SetObject(object obj);

        Task AsyncSetObject(object obj);

        Task<int> AsyncSetObjectReturnValue(object obj);

        Task<object> AsyncSetObjectReturnReference(object obj);
    }
}
