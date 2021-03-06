﻿using NetSharp.DynamicNullObject.Tests.Models;
using System;
using Xunit;

namespace NetSharp.DynamicNullObject.Tests
{
    public class EmptyTypeTests
    {
        [Fact]
        public void ShouldValidationFail()
        {
            var provider = new EmptyClassProvider();
            // could not create implementation type for class
            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass(typeof(MyService)));
            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass<MyService>());

            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass(typeof(Models.Generic.MyService<>)));

            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass(typeof(Models.Generic.MyService<int>)));
            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass<Models.Generic.MyService<int>>());

            // could not create implementation type for non-public interface
            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass(typeof(IMyServiceInternal)));
            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass<IMyServiceInternal>());

            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass(typeof(Models.Generic.IMyServiceInternal<>)));

            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass(typeof(Models.Generic.IMyServiceInternal<int>)));
            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass<Models.Generic.IMyServiceInternal<int>>());

            // could not create implementation type for public interface which cantains non-public type parameters
            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass(typeof(Models.Generic.MyService<int>.IMyNestedService<int, IMyServiceInternal>)));
            Assert.Throws<InvalidOperationException>(() => provider.GetEmptyClass(typeof(Models.Generic.MyService<int>.IMyNestedService<int, Models.Generic.IMyServiceInternal<int>>)));

        }

        [Fact]
        public void ShouldClassBeGenerated()
        {
            var provider = new EmptyClassProvider();

            var emptyClass = provider.GetEmptyClass(typeof(IMyService));
            Assert.True(typeof(IMyService).IsAssignableFrom(emptyClass));

            var emptyGenericClassWithoutArgs = provider.GetEmptyClass(typeof(Models.Generic.IMyService<>));
            var emptyGenericClassWithArgs =  emptyGenericClassWithoutArgs.MakeGenericType(typeof(int));
            Assert.True(typeof(Models.Generic.IMyService<int>).IsAssignableFrom(emptyGenericClassWithArgs));

            var emptyGenericClass = provider.GetEmptyClass(typeof(Models.Generic.IMyService<int>));
            Assert.True(typeof(Models.Generic.IMyService<int>).IsAssignableFrom(emptyGenericClass));

            var emptyNestedGenericClass = provider.GetEmptyClass(typeof(Models.Generic.MyService<int>.IMyNestedService<int, MyService>));
            Assert.True(typeof(Models.Generic.MyService<int>.IMyNestedService<int, MyService>).IsAssignableFrom(emptyNestedGenericClass));
        }

        [Fact]
        public void ShouldBeGeneratedOnceForEveryInterface()
        {
            var provider = new EmptyClassProvider();

            var class1 = provider.GetEmptyClass(typeof(IMyService));
            var class2 = provider.GetEmptyClass<IMyService>();
            Assert.Equal(class1, class2);

            var genericClass1 = provider.GetEmptyClass<Models.Generic.IMyService<int>>();
            var genericClass2 = provider.GetEmptyClass<Models.Generic.IMyService<int>>();
            Assert.Equal(genericClass1, genericClass2);
        }

        [Fact]
        public void ShouldClassNameBeCountedForInterfaceWithTheSameNameFromDifferentNamespace()
        {
            var provider = new EmptyClassProvider();

            var class1 = provider.GetEmptyClass<Models.Namespace1.IMyServiceName>();
            var class2 = provider.GetEmptyClass<Models.Namespace2.IMyServiceName>();
            Assert.Equal("EmptyMyServiceName", class1.Name);
            Assert.Equal("EmptyMyServiceName1", class2.Name);
            var genericClass1 = provider.GetEmptyClass<Models.Generic.Namespace1.IMyServiceName<int>>();
            var genericClass2 = provider.GetEmptyClass<Models.Generic.Namespace2.IMyServiceName<int>>();
            Assert.Equal("EmptyMyServiceName2", genericClass1.Name);
            Assert.Equal("EmptyMyServiceName3", genericClass2.Name);
        }
    }
}
