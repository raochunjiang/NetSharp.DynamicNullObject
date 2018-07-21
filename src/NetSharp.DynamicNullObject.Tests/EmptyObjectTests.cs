using NetSharp.DynamicNullObject.Tests.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NetSharp.DynamicNullObject.Tests
{
    public class EmptyObjectTests
    {
        [Fact]
        public void ShouldObjectBeInitialize()
        {
            var emptyClassProvider = new EmptyClassProvider();
            var emptyObjectProvider = new EmptyObjectProvider(emptyClassProvider);

            var emptyMyService = emptyObjectProvider.GetEmptyObject(typeof(IMyService));
            Assert.IsAssignableFrom<IMyService>(emptyMyService);

            var emptyMyServiceInitializeByGeneric = emptyObjectProvider.GetEmptyObject<IMyService>();
            Assert.IsAssignableFrom<IMyService>(emptyMyServiceInitializeByGeneric);

            var emptyGenericMyService = emptyObjectProvider.GetEmptyObject(typeof(Models.Generic.IMyService<>), typeof(int));
            Assert.IsAssignableFrom<Models.Generic.IMyService<int>>(emptyGenericMyService);

            var emptyGenericMyServiceInitializeByGeneric = emptyObjectProvider.GetEmptyObject<Models.Generic.IMyService<int>>();
            Assert.IsAssignableFrom<Models.Generic.IMyService<int>>(emptyGenericMyServiceInitializeByGeneric);

            var emptyGenericNestedMyService = emptyObjectProvider.GetEmptyObject(typeof(Models.Generic.MyService<>.IMyNestedService<,>), typeof(int), typeof(int), typeof(MyService));
            Assert.IsAssignableFrom<Models.Generic.MyService<int>.IMyNestedService<int, MyService>>(emptyGenericNestedMyService);

            var emptyGenericNestedMyServiceInitializeByGeneric = emptyObjectProvider.GetEmptyObject<Models.Generic.MyService<int>.IMyNestedService<int, MyService>>();
            Assert.IsAssignableFrom<Models.Generic.MyService<int>.IMyNestedService<int, MyService>>(emptyGenericNestedMyServiceInitializeByGeneric);
        }

        [Fact]
        public void ShouldGenericConstraintBeEffective()
        {
            var emptyClassProvider = new EmptyClassProvider();
            var emptyObjectProvider = new EmptyObjectProvider(emptyClassProvider);

            Assert.Throws<ArgumentException>(() => emptyObjectProvider.GetEmptyObject(typeof(Models.Generic.MyService<>.IMyNestedService<,>), typeof(int), typeof(MyService), typeof(MyService)));
            Assert.Throws<ArgumentException>(() => emptyObjectProvider.GetEmptyObject(typeof(Models.Generic.MyService<>.IMyNestedService<,>), typeof(int), typeof(int), typeof(int)));
            Assert.Throws<ArgumentException>(() => emptyObjectProvider.GetEmptyObject(typeof(Models.Generic.MyService<>.IMyNestedServiceWithConstructorConstraint<>), typeof(int), typeof(MyServiceWithNonePublicConstructors)));

        }

        // should getter and setters be empty(no effection)
        [Fact]
        public void ShouldBeEmpty()
        {
            var emptyClassProvider = new EmptyClassProvider();
            var emptyObjectProvider = new EmptyObjectProvider(emptyClassProvider);

            var emptyMyService = (IMyService)emptyObjectProvider.GetEmptyObject(typeof(IMyService));
            emptyMyService.Id = 100;
            Assert.Equal(default(int), emptyMyService.Id);
            emptyMyService.MethodA(10);

            //var emptyNestedMyService = (Models.Generic.MyService<int>.IMyNestedService<int, MyService>)EmptyObject.Of<Models.Generic.MyService<int>.IMyNestedService<int, MyService>>();
            //emptyNestedMyService.Id = 100;
            //Assert.Equal(default(int), emptyNestedMyService.Id);
            //emptyNestedMyService.MethodA(100);
            //var methodBResult = emptyNestedMyService.MethodB();
            //Assert.Equal(default(MyService), methodBResult);
            //var methodCResult = emptyNestedMyService.MethodC();

            //var methodDResult = emptyNestedMyService.MethodD();
            //Assert.Equal(Task<MyService>.FromResult(default(MyService)), methodDResult);
        }
    }
}
