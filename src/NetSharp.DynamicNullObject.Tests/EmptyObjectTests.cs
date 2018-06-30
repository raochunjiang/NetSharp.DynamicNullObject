using NetSharp.DynamicNullObject.Tests.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NetSharp.DynamicNullObject.Tests
{
    public class EmptyObjectTests
    {
        [Fact]
        public void ShouldObjectBeInitialize()
        {
            var emptyMyService = EmptyObject.Of(typeof(IMyService));
            Assert.IsAssignableFrom<IMyService>(emptyMyService);

            var emptyMyServiceInitializeByGeneric = EmptyObject.Of<IMyService>();
            Assert.IsAssignableFrom<IMyService>(emptyMyServiceInitializeByGeneric);

            var emptyGenericMyService = EmptyObject.Of(typeof(Models.Generic.IMyService<>),typeof(int));
            Assert.IsAssignableFrom<Models.Generic.IMyService<int>>(emptyGenericMyService);

            var emptyGenericMyServiceInitializeByGeneric = EmptyObject.Of<Models.Generic.IMyService<int>>();
            Assert.IsAssignableFrom<Models.Generic.IMyService<int>>(emptyGenericMyServiceInitializeByGeneric);
        }
    }
}
