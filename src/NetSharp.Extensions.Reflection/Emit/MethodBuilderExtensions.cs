
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NetSharp.Extensions.Reflection.Emit
{
    public static class MethodBiulderExtensions
    {
        /// <summary>
        /// 为当前方法定义泛型类型参数，从指定的模板方法获取参数的类型、个数、约束等。
        /// </summary>
        /// <param name="templateType">用于提取泛型类型参数类型、个数、约束等信息的模板类型。</param>
        public static void DefineGenericParameters(this MethodBuilder builder, MethodBase templateMethod)
        {
            if (!templateMethod.IsGenericMethod)
            {
                return;
            }
            var genericArguments = templateMethod.GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
            var genericArgumentsBuilders = builder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(genericArguments[index].GenericParameterAttributes);
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                }
            }
        }
    }
}