using System;
using System.Threading.Tasks;

namespace NetSharp.DynamicNullObject.Samples
{
    public class Program
    {
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
    }
}
