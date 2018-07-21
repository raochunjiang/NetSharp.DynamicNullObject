using NetSharp.Extensions.Reflection;
using NetSharp.Extensions.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

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
                    _default = new EmptyClassGenerator(assembly, "NetSharp.DynamicNullObject.Generated");
                }
                return _default;
            }
        }
        private static EmptyClassGenerator _default;

        private static readonly Type _objectType = typeof(object);
        private static readonly Type _voidReturnType = typeof(void);
        private static readonly Type _taskType = typeof(Task);
        private static readonly MethodAttributes _interfaceMethodAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;

        private ModuleBuilder _module;
        private readonly string _namespace;

        private volatile object _blocker = new object();

        private Dictionary<string, int> _classNameCounter = new Dictionary<string, int>();
        private Dictionary<Type, Type> _definedTypes = new Dictionary<Type, Type>();

        internal EmptyClassGenerator(AssemblyBuilder dynamicAssembly, string @namespace)
        {
            _module = dynamicAssembly.DefineDynamicModule("NetSharp");
            _namespace = @namespace;
        }

        internal Type GetEmptyClass(Type interfaceType)
        {
            lock (_blocker)
            {
                if (!_definedTypes.TryGetValue(interfaceType, out Type implClass))
                {
                    var className = GetEmptyClassName(interfaceType, _classNameCounter, _namespace);
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

        private static string GetEmptyClassName(Type interfaceType, Dictionary<string, int> classNameCounter, string @namespace)
        {
            var className = interfaceType.GetProgrammingName();
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
                if (interfaceType.GetTypeInfo().IsGenericType)
                {
                    className = className.Insert(className.IndexOf('<'), index.ToString());
                }
                else
                {
                    className = className + (index.ToString());
                }
            }
            return $"{@namespace}.Empty{className}";
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
                if (templateMethod.ReturnType.IsTask())
                {
                    il.Emit(OpCodes.Ldc_I4, 0);
                    il.Emit(OpCodes.Callvirt, ReflectedMethods.TaskFromResult);
                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Ret);
                }
                else if (templateMethod.ReturnType.IsTaskWithResult())
                {
                    var taskReturnType = templateMethod.ReturnType.GetGenericArguments().Single();
                    il.EmitDefault(taskReturnType);
                    il.Emit(OpCodes.Callvirt, ReflectedMethods.TaskFromResult);
                    il.Emit(OpCodes.Ret);
                }
                else
                {
                    il.EmitDefault(templateMethod.ReturnType);
                    il.Emit(OpCodes.Ret);
                }
            }
            else
            {
                il.Emit(OpCodes.Ret);
            }
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