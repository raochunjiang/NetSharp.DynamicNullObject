using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetSharp.Extensions.Reflection
{
    internal static class ReflectedMethods
    {
        internal static readonly MethodInfo TaskFromResult = GetMethod<Task>(nameof(Task.FromResult));

        private static MethodInfo GetMethod<T>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return typeof(T).GetTypeInfo().GetMethod(name);
        }
    }
}
