using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NetSharp.Extensions.Reflection;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace NetSharp.DynamicNullObject.Tests
{
    public class MethodExtensionsTests
    {
        [Fact]
        public async void ShouldIsAsycnchronousWork()
        {
            var type = this.GetType();
            var method = type.GetMethod("NotAsynchronous");
            Assert.False(method.IsAsynchronous());
            method = type.GetMethod("NotAsynchronousTask");
            Assert.False(method.IsAsynchronous());
            method = type.GetMethod("Asynchronous");
            Assert.True(method.IsAsynchronous());
            var a = new AsynchronousClass();
            var b = (AsynchronousInterface)a;
            await b.Get();
        }

        public void NotAsynchronous() { }

        public Task NotAsynchronousTask() { throw new NotImplementedException(); }

        public async void Asynchronous() { }
    }

    public interface AsynchronousInterface
    {
        [AsyncStateMachine(typeof(Task<int?>))]
        Task<int?> Get();
    }

    public class AsynchronousClass : AsynchronousInterface
    {
        public async Task<int?> Get()
        {
            return null;
        }
    }
}
