using NetSharp.Extensions.Reflection;
using NetSharp.Extensions.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NetSharp.DynamicNullObject
{
    internal class EmptyClassGenerator
    {
        public static EmptyClassGenerator Default
        {
            get
            {
                if (_default == null)
                {
                    var assembly = AssemblyBuilder.DefineDynamicAssembly(
                        new AssemblyName("NetSharp.DynamicNullObject.Generated"),
                        AssemblyBuilderAccess.RunAndCollect);
                    _default = new EmptyClassGenerator(assembly);
                }
                return _default;
            }
        }
        private static EmptyClassGenerator _default;

        private static readonly Type _objectType = typeof(object);
        private static readonly Type _voidReturnType = typeof(void);
        private static readonly MethodAttributes _interfaceMethodAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        internal ModuleBuilder Module { get; }

        private string _assemblyName { get; }
        private volatile object _blocker = new object();

        private Dictionary<string, int> _classNameCounter = new Dictionary<string, int>();
        private Dictionary<Type, Type> _definedTypes = new Dictionary<Type, Type>();

        internal EmptyClassGenerator(AssemblyBuilder dynamicAssembly)
        {
            _assemblyName = dynamicAssembly.GetName().Name;
            var existedModule = (ModuleBuilder)dynamicAssembly.GetDynamicModule(_assemblyName);
            Module = existedModule ?? dynamicAssembly.DefineDynamicModule(_assemblyName);
        }

        internal Type GetEmptyClass(Type interfaceType)
        {
            lock (_blocker)
            {
                if (!_definedTypes.TryGetValue(interfaceType, out Type implClass))
                {
                    var className = $"{_assemblyName}.{GetEmptyClassName(interfaceType, _classNameCounter)}";
                    var interfaceTypes = interfaceType.GetNestedVisibleInterfaces().Concat(new Type[] { interfaceType }).ToArray();
                    var typeBuilder = Module.DefineType(className, TypeAttributes.Public | TypeAttributes.Class, _objectType, interfaceTypes);
                    typeBuilder.DefineGenericParameters(interfaceType);
                    typeBuilder.DefineConstructor(MethodAttributes.Public);
                    DefineEmptyMethods(typeBuilder, interfaceTypes);
                    DefineProperties(typeBuilder, interfaceTypes);
                    _definedTypes[interfaceType] = implClass = typeBuilder.CreateTypeInfo().AsType();
                }
                return implClass;
            }
        }

        private static string GetEmptyClassName(Type interfaceType, Dictionary<string, int> classNameCounter)
        {
            var className = interfaceType.Name;
            var gIndex = className.IndexOf("`");
            className = gIndex > 0 ? className.Substring(0, gIndex) : className;
            if (className.StartsWith("I", StringComparison.Ordinal))
            {
                className = className.Substring(1);
            }
            if (!classNameCounter.TryGetValue(className, out int index))
            {
                classNameCounter.Add(className, value: 0);
            }
            else
            {
                classNameCounter[className] = ++index;
                className = className + (index.ToString());
            }
            return $"Empty{className}";
            ;
        }

        private static void DefineEmptyMethods(TypeBuilder typeBuilder, IEnumerable<Type> interfaceTypes)
        {
            foreach (var interfaceType in interfaceTypes)
            {
                foreach (var declaredMethod in interfaceType.GetTypeInfo().DeclaredMethods.Where(m => !m.IsPropertyBinding()))
                {
                    DefineEmptyMethod(typeBuilder, declaredMethod);
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
                if (templateMethod.ReturnType.IsTask())
                {
                    il.Emit(OpCodes.Ldc_I4, 0);
                    il.Emit(OpCodes.Call, ReflectedMethods.TaskFromResult.MakeGenericMethod(typeof(int)));
                }
                else if (templateMethod.ReturnType.IsTaskWithResult())
                {
                    var taskReturnType = templateMethod.ReturnType.GetGenericArguments().Single();
                    il.EmitDefault(taskReturnType);
                    il.Emit(OpCodes.Call, ReflectedMethods.TaskFromResult.MakeGenericMethod(taskReturnType));
                }
                else
                {
                    il.EmitDefault(templateMethod.ReturnType);
                }
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
                    if (declaredProperty.CanRead)
                    {
                        var getMethodBuilder = DefineEmptyMethod(typeBuilder, declaredProperty.GetMethod);
                        propertyBuilder.SetGetMethod(getMethodBuilder);
                    }
                    if (declaredProperty.CanWrite)
                    {
                        var setMethodBuilder = DefineEmptyMethod(typeBuilder, declaredProperty.SetMethod);
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