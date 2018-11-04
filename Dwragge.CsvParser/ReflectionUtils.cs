using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Dwragge.CsvParser
{
    public class ReflectionUtils
    {
        public static Func<T> CreateConstructorCallFunc<T>()
        {
            var type = typeof(T);
            ConstructorInfo emptyConstructor = type.GetConstructor(Type.EmptyTypes);
            var dynamicMethod = new DynamicMethod("CreateInstance", type, Type.EmptyTypes, true);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
            ilGenerator.Emit(OpCodes.Ret);
            return (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<>).MakeGenericType(typeof(T)));
        }
    }
}
