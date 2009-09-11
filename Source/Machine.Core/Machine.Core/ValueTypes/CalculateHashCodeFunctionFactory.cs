using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using Machine.Core.Utility;

namespace Machine.Core.ValueTypes
{
  public delegate Int32 CalculateHashCodeFunction(object a);

  public class CalculateHashCodeFunctionFactory : AbstractValueTypeInspector
  {
    public CalculateHashCodeFunction CreateCalculateHashCodeFunction<TType>()
    {
      return CreateCalculateHashCodeFunction(typeof(TType));
    }

    public CalculateHashCodeFunction CreateCalculateHashCodeFunction(Type type)
    {
      MethodInfo arrayGetHashCode = typeof(ArrayHelpers).GetMethod("GetHashCode", new[] { typeof(Array) });
      DynamicMethod method = NewMethod(type, typeof(Int32), new Type[] {typeof(object)});
      ILGenerator il = method.GetILGenerator();

      ThrowIfArg0CastFailsOrNull(il, type);

      bool hasState = false;
      foreach (FieldInfo field in AllFields(type))
      {
        MethodInfo getHashCode = field.FieldType.GetMethod("GetHashCode", new Type[0]);
        if (getHashCode == null)
        {
          getHashCode = typeof(Object).GetMethod("GetHashCode", new Type[0]);
        }
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, type);
        if (field.FieldType.IsArray)
        {
          il.Emit(OpCodes.Ldfld, field);
          il.Emit(OpCodes.Call, arrayGetHashCode);
        }
        else if (field.FieldType.IsValueType)
        {
          if (field.FieldType.IsEnum)
          {
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Box, field.FieldType);
            il.Emit(OpCodes.Callvirt, getHashCode);
          }
          else
          {
            il.Emit(OpCodes.Ldflda, field);
            il.Emit(OpCodes.Call, getHashCode);
          }
        }
        else
        {
          il.Emit(OpCodes.Ldfld, field);
          IfNull(il, delegate() { il.Emit(OpCodes.Ldc_I4_0); }, delegate()
          {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, type);
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Callvirt, getHashCode);
          }
            );
        }
        if (hasState)
        {
          il.Emit(OpCodes.Ldc_I4, 29);
          il.Emit(OpCodes.Mul);
          il.Emit(OpCodes.Add);
        }
        hasState = true;
      }
      if (!hasState)
      {
        il.Emit(OpCodes.Ldc_I4_0);
      }
      il.Emit(OpCodes.Ret);
      return (CalculateHashCodeFunction)method.CreateDelegate(typeof(CalculateHashCodeFunction));
    }
  }
}