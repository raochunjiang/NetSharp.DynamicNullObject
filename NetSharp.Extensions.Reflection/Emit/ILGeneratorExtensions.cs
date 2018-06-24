using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NetSharp.Extensions.Reflection.Emit
{
    public static class ILGeneratorExtensions
    {
        public static void EmitDefault(this ILGenerator ilGenerator, Type type)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                case TypeCode.DateTime:
                    if (type.GetTypeInfo().IsValueType)
                    {
                        LocalBuilder lb = ilGenerator.DeclareLocal(type);
                        ilGenerator.Emit(OpCodes.Ldloca, lb);
                        ilGenerator.Emit(OpCodes.Initobj, type);
                        ilGenerator.Emit(OpCodes.Ldloc, lb);
                    }
                    else
                    {
                        ilGenerator.Emit(OpCodes.Ldnull);
                    }
                    break;

                case TypeCode.Empty:
                case TypeCode.String:
                    ilGenerator.Emit(OpCodes.Ldnull);
                    break;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    ilGenerator.Emit(OpCodes.Conv_I8);
                    break;

                case TypeCode.Single:
                    ilGenerator.Emit(OpCodes.Ldc_R4, default(Single));
                    break;

                case TypeCode.Double:
                    ilGenerator.Emit(OpCodes.Ldc_R8, default(Double));
                    break;

                case TypeCode.Decimal:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    ilGenerator.Emit(OpCodes.Newobj, typeof(Decimal).GetTypeInfo().GetConstructor(new Type[] { typeof(int) }));
                    break;

                default:
                    throw new InvalidOperationException("Code supposed to be unreachable.");
            }
        }
    }
}
