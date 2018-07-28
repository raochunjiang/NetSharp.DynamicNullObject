# 动态空对象模式

## 空对象模式

关于空对象模式的介绍，请参考：

[![维基百科：空对象模式](https://en.wikipedia.org/static/favicon/wikipedia.ico)](https://en.wikipedia.org/wiki/Null_object_pattern)
[![百度一下：空对象模式](https://www.baidu.com/favicon.ico)](https://www.baidu.com/s?ie=utf-8&f=8&rsv_bp=0&rsv_idx=1&tn=baidu&wd=%E7%A9%BA%E5%AF%B9%E8%B1%A1%E6%A8%A1%E5%BC%8F&rsv_pq=80715f4300009be3&rsv_t=0279BNGD7HqWS64dqZ5YH43khzfkbBnq1eKAhtpcS58rKow0eR74iJ%2FAMe0&rqlang=cn&rsv_enter=1&rsv_n=2&rsv_sug3=1)

这里不对具体场景做样例描述，所以对场景做了抽象。

动态空对象程序包是基于 .NET Standard 开发的，可在 .NET Core 和 .NET Framework 4.5+ 项目中引用/使用。 

## 场景类

```C#
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
```

## 添加动态空对象组件的引用

使用程序包管理控制台添加引用：

```CMD
install-package NetSharp.DynamicNullObject
```

使用程序包管理器添加引用：

> 鼠标右键点击项目 => 管理 NuGet 程序包 => 浏览 => 输入 NetSharp.DynamicNullObject => 选择程序包 => 安装

其他方式请参考 NuGet 使用文档

## 使用组件

```C#
    public static void Main(string[] args)
    {
        var emptyClassProvider = new EmptyClassProvider();

        var emptyClass = emptyClassProvider.GetEmptyClass<IFakeScenario>();
        var emptyObject = Activator.CreateInstance(emptyClass);
        var emptyScenario = (IFakeScenario)emptyObject;

        // 方法可正常调用,返回值是为对应类型的默认值
        emptyScenario.SetObject("这是一个对象");
        var value = emptyScenario.GetValueObject();
        Console.WriteLine(value == 0); // true
        var reference = emptyScenario.GetReferenceObject();
        Console.WriteLine(reference == null); // true

        // 异步方法总是返回一个异步操作，操作结果为对应类型的默认值
        var task = emptyScenario.AsyncSetObject("这是一个对象");
        Console.WriteLine(task.GetType() == typeof(Task<int>)); // true。对于无返回的任务，生成代码返回 Task.FromResult(0)

        var taskReturnValue = emptyScenario.AsyncSetObjectReturnValue("这是一个对象");
        Console.WriteLine(taskReturnValue.GetType() == typeof(Task<int>)); // true
        Console.WriteLine(taskReturnValue.Result == 0); // true

        var taskReturnObject = emptyScenario.AsyncSetObjectReturnReference("这是一个对象");
        Console.WriteLine(taskReturnObject.GetType() == typeof(Task<object>)); // true
        Console.WriteLine(taskReturnObject.Result == null); // true

        Action asyncAction = async () =>
        {
            // 异步操作是正常可等待的，异步结果为对应类型的默认值
            await emptyScenario.AsyncSetObject("这是一个对象");

            var returnValue = await emptyScenario.AsyncSetObjectReturnValue("这是一个对象");
            Console.WriteLine(returnValue == 0); // true

            var returnObject = await emptyScenario.AsyncSetObjectReturnReference("这是一个对象");
            Console.WriteLine(returnObject == null); // true
        };
        asyncAction();

        // 属性是语法糖,对应 getter、setter 两个方法
        emptyScenario.ValueProperty = 100;
        Console.WriteLine(emptyScenario.ValueProperty == 0); // true

        // 属性是语法糖,对应 getter、setter 两个方法
        emptyScenario.ReferenceProperty = new object();
        Console.WriteLine(emptyScenario.ReferenceProperty == null); // true

        Console.ReadKey();
    }
```

## 更多

请自行挖掘...




