using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NetSharp.Extensions.Reflection.Emit
{
    public static class TypeBiulderExtensions
    {
        private static readonly ConstructorInfo _objectTypeConstructor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();

        /// <summary>
        /// 为当前类型定义泛型类型参数，从指定的模板类型获取参数的类型、个数、约束等。
        /// </summary>
        /// <param name="templateType">用于提取泛型类型参数类型、个数、约束等信息的模板类型。</param>
        public static void DefineGenericParameters(this TypeBuilder builder, Type templateType)
        {
            if (!templateType.GetTypeInfo().IsGenericTypeDefinition)
            {
                return;
            }
            var genericArguments = templateType.GetTypeInfo().GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
            var genericArgumentsBuilders = builder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(ToClassGenericParameterAttributes(genericArguments[index].GenericParameterAttributes));
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                }
            }
        }

        private static GenericParameterAttributes ToClassGenericParameterAttributes(GenericParameterAttributes attributes)
        {
            if (attributes == GenericParameterAttributes.None)
            {
                return GenericParameterAttributes.None;
            }
            if (attributes.HasFlag(GenericParameterAttributes.SpecialConstraintMask))
            {
                return GenericParameterAttributes.SpecialConstraintMask;
            }
            if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                return GenericParameterAttributes.NotNullableValueTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.DefaultConstructorConstraint;
            }
            return GenericParameterAttributes.None;
        }

        /// <summary>
        /// 用给定的属性向类型中添加无参数的新构造函数。
        /// </summary>
        /// <param name="attributes">构造函数的属性。</param>
        public static void DefineConstructor(this TypeBuilder builder, MethodAttributes attributes)
        {
            DefineConstructor(builder, attributes, Type.EmptyTypes, null);
        }

        /// <summary>
        /// 用给定的属性和中间语言委托向类型中添加无参数的新构造函数。
        /// </summary>
        /// <param name="attributes">构造函数的属性。</param>
        /// <param name="ilGeneration">包含中间语言指令定义的委托。</param>
        public static void DefineConstructor(this TypeBuilder builder, MethodAttributes attributes, Action<ILGenerator> ilGeneration)
        {
            DefineConstructor(builder, attributes, Type.EmptyTypes, ilGeneration);
        }

        /// <summary>
        /// 用给定的属性和签名向类型中添加新的构造函数。  
        /// </summary>
        /// <param name="attributes">构造函数的属性。</param>
        /// <param name="parameterTypes">构造函数的参数类型。</param>
        public static void DefineConstructor(this TypeBuilder builder, MethodAttributes attributes, Type[] parameterTypes)
        {
            DefineConstructor(builder, attributes, parameterTypes, null);
        }

        /// <summary>
        /// 用给定的属性、签名、中间语言委托向类型中添加新的构造函数。
        /// </summary>
        /// <param name="attributes">构造函数的属性。</param>
        /// <param name="parameterTypes">构造函数的参数类型。</param>
        /// <param name="ilGeneration">包含中间语言指令定义的委托。</param>
        public static void DefineConstructor(this TypeBuilder builder, MethodAttributes attributes, Type[] parameterTypes, Action<ILGenerator> ilGeneration)
        {
            var constructorBuilder = builder.DefineConstructor(attributes, _objectTypeConstructor.CallingConvention, parameterTypes);
            var il = constructorBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _objectTypeConstructor);
            ilGeneration?.Invoke(constructorBuilder.GetILGenerator());
            il.Emit(OpCodes.Ret);
        }
    }
}
