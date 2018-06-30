using NetSharp.DynamicNullObject.Globalization;
using NetSharp.Extensions.Reflection;
using NetSharp.Extensions.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NetSharp.DynamicNullObject
{
    /// <summary>
    /// 提供返回指定接口类型的空实现类型的操作。
    /// </summary>
    public sealed class EmptyClass
    {
        private static readonly string _assemblyName = "NetSharp.DynamicNullObject.Generated";
        private static readonly string _defaultNamespace = "NetSharp.DynamicNullObject.Generated";

        private static volatile object _blocker = new object();

        private static Dictionary<string, int> _classNameCounter = new Dictionary<string, int>();
        private static Dictionary<Type, Type> _definedTypes = new Dictionary<Type, Type>();

        private static readonly Type _objectType = typeof(object);
        private static readonly Type _voidReturnType = typeof(void);

        private static readonly MethodAttributes _interfaceMethodAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        private static readonly ModuleBuilder _module;
        static EmptyClass()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_assemblyName), AssemblyBuilderAccess.RunAndCollect);
            _module = assembly.DefineDynamicModule("Default");
        }

        /// <summary>
        /// 返回指定接口类型的空实现类型。
        /// </summary>
        /// <param name="interfaceType">接口类型。</param>
        /// <returns>指定接口类型的空实现类型。</returns>
        public static Type Of(Type interfaceType)
        {
            var interfaceTypeInfo = interfaceType?.GetTypeInfo() ?? throw new ArgumentNullException(nameof(interfaceType));
            if (!interfaceTypeInfo.IsInterface)
            {
                throw new InvalidOperationException(string.Format(ExceptionMessage.TheSpecifiedTypeShouldBeAnInterfaceType, nameof(interfaceType)));
            }
            if (!interfaceTypeInfo.IsNestedVisible())
            {
                throw new InvalidOperationException(string.Format(ExceptionMessage.UnableToAccessTheSpecifiedType, nameof(interfaceType)));
            }

            return GetEmptyClass(interfaceTypeInfo);
        }

        /// <summary>
        /// 返回指定接口类型参数的空实现类型。
        /// </summary>
        /// <typeparam name="TInterface">接口类型。</typeparam>
        /// <returns>指定接口类型参数的空实现类型。</returns>
        public static Type Of<TInterface>()
        {
            var interfaceTypeInfo = typeof(TInterface).GetTypeInfo();
            if (!interfaceTypeInfo.IsInterface)
            {
                throw new InvalidOperationException(string.Format(ExceptionMessage.TheSpecifiedTypeShouldBeAnInterfaceType, nameof(TInterface)));
            }
            if (!interfaceTypeInfo.IsNestedVisible())
            {
                throw new InvalidOperationException(string.Format(ExceptionMessage.UnableToAccessTheSpecifiedType, nameof(TInterface)));
            }

            return GetEmptyClass(interfaceTypeInfo);
        }

        private static Type GetEmptyClass(Type interfaceType)
        {
            lock (_blocker)
            {
                if (!_definedTypes.TryGetValue(interfaceType, out Type implClass))
                {
                    var className = GetEmptyClassName(interfaceType);
                    var interfaceTypes = interfaceType.GetNestedVisibleInterfaces().Concat(new Type[] { interfaceType }).ToArray();
                    var typeBuilder = _module.DefineType(className, TypeAttributes.Public, _objectType, interfaceTypes);
                    typeBuilder.DefineGenericParameters(interfaceType);
                    typeBuilder.DefineConstructor(MethodAttributes.Public);
                    DefineEmptyMethods(typeBuilder, interfaceTypes);
                    DefineProperties(typeBuilder, interfaceTypes);
                    _definedTypes[interfaceType] = implClass = typeBuilder.CreateTypeInfo().AsType();
                }
                return implClass;
            }
        }

        private static string GetEmptyClassName(Type interfaceType)
        {
            var className = interfaceType.GetProgrammingName();
            if (className.StartsWith("I", StringComparison.Ordinal))
            {
                className = className.Substring(1);
            }
            if (!_classNameCounter.TryGetValue(className, out int index))
            {
                _classNameCounter.Add(className, value: 0);
            }
            else
            {
                _classNameCounter[className] = ++index;
                if (interfaceType.GetTypeInfo().IsGenericType)
                {
                    className = className.Insert(className.IndexOf('<'), index.ToString());
                }
                else
                {
                    className = className + (index.ToString());
                }
            }
            return $"{_defaultNamespace}.Empty{className}";
            ;
        }

        private static void DefineEmptyMethods(TypeBuilder typeBuilder, IEnumerable<Type> interfaceTypes)
        {
            foreach (var interfaceType in interfaceTypes)
            {
                foreach (var declaredMethod in interfaceType.GetTypeInfo().DeclaredMethods.Where(m => !m.IsPropertyBinding()))
                {
                    var methodBuilder = DefineEmptyMethod(typeBuilder, declaredMethod);
                    typeBuilder.DefineMethodOverride(methodBuilder, declaredMethod);
                }
            }
        }

        private static MethodBuilder DefineEmptyMethod(TypeBuilder typeBuilder, MethodInfo templateMethod)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                                templateMethod.Name,
                                _interfaceMethodAttributes,
                                templateMethod.CallingConvention,
                                templateMethod.ReturnType,
                                templateMethod.GetParameterTypes());
            methodBuilder.DefineGenericParameters(templateMethod);
            var il = methodBuilder.GetILGenerator();
            if (templateMethod.ReturnType != _voidReturnType)
            {
                il.EmitDefault(templateMethod.ReturnType);
            }
            il.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        private static void DefineProperties(TypeBuilder typeBuilder, IEnumerable<Type> interfaceTypes)
        {
            foreach (var interfaceType in interfaceTypes)
            {
                foreach (var declaredProperty in interfaceType.GetTypeInfo().DeclaredProperties)
                {
                    var propertyBuilder = typeBuilder.DefineProperty(
                        declaredProperty.Name,
                        declaredProperty.Attributes,
                        declaredProperty.PropertyType, Type.EmptyTypes);
                    //var fieldForPropertyBuilder = typeBuilder.DefineField(
                    //    $"_fieldForProperty<{declaredProperty.Name}>",
                    //    declaredProperty.PropertyType,
                    //    FieldAttributes.Private);
                    if (declaredProperty.CanRead)
                    {
                        // property is actually a cuple of get and set method,it could be empty too.

                        //var getMethodBuilder = typeBuilder.DefineMethod(
                        //    declaredProperty.GetMethod.Name,
                        //    _interfaceMethodAttributes,
                        //    declaredProperty.GetMethod.CallingConvention,
                        //    declaredProperty.GetMethod.ReturnType,
                        //    declaredProperty.GetMethod.GetParameterTypes());
                        //var il = getMethodBuilder.GetILGenerator();
                        //il.Emit(OpCodes.Ldarg_0);
                        //il.Emit(OpCodes.Ldfld, fieldForPropertyBuilder);
                        //il.Emit(OpCodes.Ret);

                        var getMethodBuilder = DefineEmptyMethod(typeBuilder, declaredProperty.GetMethod);
                        typeBuilder.DefineMethodOverride(getMethodBuilder, declaredProperty.GetMethod);
                        propertyBuilder.SetGetMethod(getMethodBuilder);
                    }
                    if (declaredProperty.CanWrite)
                    {
                        // property is actually a cuple of get and set method,it could be empty too.

                        //var setMethodBuilder = typeBuilder.DefineMethod(
                        //    declaredProperty.SetMethod.Name,
                        //    _interfaceMethodAttributes,
                        //    declaredProperty.SetMethod.CallingConvention,
                        //    declaredProperty.SetMethod.ReturnType,
                        //    declaredProperty.SetMethod.GetParameterTypes());
                        //var il = setMethodBuilder.GetILGenerator();
                        //il.Emit(OpCodes.Ldarg_0);
                        //il.Emit(OpCodes.Ldarg_1);
                        //il.Emit(OpCodes.Stfld, fieldForPropertyBuilder);
                        //il.Emit(OpCodes.Ret);
                        var setMethodBuilder = DefineEmptyMethod(typeBuilder, declaredProperty.SetMethod);

                        typeBuilder.DefineMethodOverride(setMethodBuilder, declaredProperty.SetMethod);
                        propertyBuilder.SetSetMethod(setMethodBuilder);
                    }
                    foreach (var customAttributeData in declaredProperty.CustomAttributes)
                    {
                        propertyBuilder.SetCustomAttribute(DefineCustomAttribute(customAttributeData));
                    }
                }
            }
        }

        private static CustomAttributeBuilder DefineCustomAttribute(CustomAttributeData customAttributeData)
        {
            if (customAttributeData.NamedArguments != null)
            {
                var attributeTypeInfo = customAttributeData.AttributeType.GetTypeInfo();
                var constructor = customAttributeData.Constructor;
                var constructorArgs = new object[customAttributeData.ConstructorArguments.Count];
                for (var i = 0; i < constructorArgs.Length; i++)
                {
                    if (customAttributeData.ConstructorArguments[i].ArgumentType.IsArray)
                    {
                        constructorArgs[i] = ((IEnumerable<CustomAttributeTypedArgument>)customAttributeData.ConstructorArguments[i].Value).
                            Select(x => x.Value).ToArray();
                    }
                    else
                    {
                        constructorArgs[i] = customAttributeData.ConstructorArguments[i].Value;
                    }
                }
                var namedProperties = customAttributeData.NamedArguments
                        .Where(n => !n.IsField)
                        .Select(n => attributeTypeInfo.GetProperty(n.MemberName))
                        .ToArray();
                var propertyValues = customAttributeData.NamedArguments
                         .Where(n => !n.IsField)
                         .Select(n => n.TypedValue.Value)
                         .ToArray();
                var namedFields = customAttributeData.NamedArguments.Where(n => n.IsField)
                         .Select(n => attributeTypeInfo.GetField(n.MemberName))
                         .ToArray();
                var fieldValues = customAttributeData.NamedArguments.Where(n => n.IsField)
                         .Select(n => n.TypedValue.Value)
                         .ToArray();
                return new CustomAttributeBuilder(customAttributeData.Constructor, constructorArgs
                   , namedProperties
                   , propertyValues, namedFields, fieldValues);
            }
            else
            {
                return new CustomAttributeBuilder(customAttributeData.Constructor,
                    customAttributeData.ConstructorArguments.Select(c => c.Value).ToArray());
            }
        }
    }
}
