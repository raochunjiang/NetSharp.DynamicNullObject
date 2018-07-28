using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace NetSharp.Extensions.Reflection
{
    internal static class ReflectedMethods
    {
        internal static readonly MethodInfo TaskFromResult = typeof(Task).GetMethod(nameof(Task.FromResult), BindingFlags.Public | BindingFlags.Static);

        internal static readonly MethodInfo TaskReturnFromZero = GetMethod<TaskDefaultInvoke>(nameof(TaskDefaultInvoke.ReturnFromZero));

        internal static readonly MethodInfo TaskReturnFromDefault = GetMethod<TaskDefaultInvoke>(nameof(TaskDefaultInvoke.ReturnFromDefault));

        internal static readonly ConstructorInfo TaskDefaultInvokeCtor = typeof(TaskDefaultInvoke).GetTypeInfo().DeclaredConstructors.First();

        private static MethodInfo GetMethod<T>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return typeof(T).GetTypeInfo().GetMethod(name);
        }
    }

    public class TaskDefaultInvoke
    {
        public Task ReturnFromZero()
        {
            return Task.FromResult(0);
        }

        public Task<TResult> ReturnFromDefault<TResult>()
        {
            return Task.FromResult(default(TResult));
        }
    }
}
