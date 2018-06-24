
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
        /// Ϊ��ǰ�������巺�����Ͳ�������ָ����ģ�巽����ȡ���������͡�������Լ���ȡ�
        /// </summary>
        /// <param name="templateType">������ȡ�������Ͳ������͡�������Լ������Ϣ��ģ�����͡�</param>
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