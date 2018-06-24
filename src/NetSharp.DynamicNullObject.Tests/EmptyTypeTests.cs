using NetSharp.DynamicNullObject.Tests.Models;
using System;
using Xunit;

namespace NetSharp.DynamicNullObject.Tests
{
    public class EmptyTypeTests
    {
        [Fact]
        public void ShouldValidationFail()
        {
            // could not create implementation type for class
            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of(typeof(MyService)));
            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of<MyService>());

            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of(typeof(Models.Generic.MyService<>)));

            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of(typeof(Models.Generic.MyService<int>)));
            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of<Models.Generic.MyService<int>>());

            // could not create implementation type for non-public interface
            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of(typeof(IMyServiceInternal)));
            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of<IMyServiceInternal>());

            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of(typeof(Models.Generic.IMyServiceInternal<>)));

            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of(typeof(Models.Generic.IMyServiceInternal<int>)));
            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of<Models.Generic.IMyServiceInternal<int>>());

            // could not create implementation type for public interface which cantains non-public type parameters
            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of(typeof(Models.Generic.MyService<int>.IMyNestedService<int, IMyServiceInternal>)));
            Assert.Throws<InvalidOperationException>(() => EmptyClass.Of(typeof(Models.Generic.MyService<int>.IMyNestedService<int, Models.Generic.IMyServiceInternal<int>>)));

        }

        [Fact]
        public void ShouldClassBeGenerated()
        {
            var emptyClass = EmptyClass.Of(typeof(IMyService));
            Assert.True(typeof(IMyService).IsAssignableFrom(emptyClass));

            var emptyGenericClassWithoutArgs = EmptyClass.Of(typeof(Models.Generic.IMyService<>));
            var emptyGenericClassWithArgs =  emptyGenericClassWithoutArgs.MakeGenericType(typeof(int));
            Assert.True(typeof(Models.Generic.IMyService<int>).IsAssignableFrom(emptyGenericClassWithArgs));

            var emptyGenericClass = EmptyClass.Of(typeof(Models.Generic.IMyService<int>));
            Assert.True(typeof(Models.Generic.IMyService<int>).IsAssignableFrom(emptyGenericClass));

            var emptyNestedGenericClass = EmptyClass.Of(typeof(Models.Generic.MyService<int>.IMyNestedService<int, MyService>));
            Assert.True(typeof(Models.Generic.MyService<int>.IMyNestedService<int, MyService>).IsAssignableFrom(emptyNestedGenericClass));
        }

        [Fact]
        public void ShouldBeGeneratedOnceForEveryInterface()
        {
            var class1 = EmptyClass.Of(typeof(IMyService));
            var class2 = EmptyClass.Of<IMyService>();
            Assert.Equal(class1, class2);

            var genericClass1 = EmptyClass.Of<Models.Generic.IMyService<int>>();
            var genericClass2 = EmptyClass.Of<Models.Generic.IMyService<int>>();
            Assert.Equal(genericClass1, genericClass2);
        }

        [Fact]
        public void ShouldClassNameBeCountedForInterfaceWithTheSameNameFromDifferentNamespace()
        {
            var class1 = EmptyClass.Of<Models.Namespace1.IMyServiceName>();
            var class2 = EmptyClass.Of<Models.Namespace2.IMyServiceName>();
            Assert.Equal("EmptyMyServiceName", class1.Name);
            Assert.Equal("EmptyMyServiceName1", class2.Name);
            var genericClass1 = EmptyClass.Of<Models.Generic.Namespace1.IMyServiceName<int>>();
            var genericClass2 = EmptyClass.Of<Models.Generic.Namespace2.IMyServiceName<int>>();
            Assert.Equal("EmptyMyServiceName<Int32>", genericClass1.Name);
            Assert.Equal("EmptyMyServiceName1<Int32>", genericClass2.Name);
        }
    }
}
