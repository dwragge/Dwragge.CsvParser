using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Dwragge.CsvParser
{
    internal class PropertyMapperFactory
    {
        public static IPropertyMapper<TEntity> CreateMapper<TEntity>(string propertyName)
        {
            var factory = new PropertyMapperFactory();
            var propertyInfo = typeof(TEntity).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            var assemblyBuilder = factory.GetAssemblyBuilder("Mappers");
            var moduleBuilder = factory.GetModuleBuilder(assemblyBuilder);
            var typeBuilder = factory.GetType(moduleBuilder, typeof(TEntity).Name, typeof(IPropertyMapper<>).MakeGenericType(typeof(TEntity)));
            var method = factory.GetMethod(typeBuilder, "Map", typeof(TEntity), typeof(ReadOnlySpan<>).MakeGenericType(typeof(char)));

            var ilGen = method.GetILGenerator();
            factory.GenerateParseCall(ilGen, propertyInfo.PropertyType);
            factory.GenerateSetProperty(ilGen, propertyInfo);
            ilGen.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(method, typeof(IPropertyMapper<>).MakeGenericType(typeof(TEntity)).GetMethod("Map"));

            var type = typeBuilder.CreateType();
            var constructor = type.GetConstructor(Type.EmptyTypes);
            var casted = (IPropertyMapper<TEntity>)constructor.Invoke(null);
            return casted;
        }

        public static IUtf8PropertyMapper<TEntity> CreateMapperUtf8<TEntity>(string propertyName)
        {
            var factory = new PropertyMapperFactory();
            var propertyInfo = typeof(TEntity).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            var assemblyBuilder = factory.GetAssemblyBuilder("Mappers");
            var moduleBuilder = factory.GetModuleBuilder(assemblyBuilder);
            var typeBuilder = factory.GetType(moduleBuilder, typeof(TEntity).Name, typeof(IUtf8PropertyMapper<>).MakeGenericType(typeof(TEntity)));
            var method = factory.GetMethod(typeBuilder, "Map", typeof(TEntity), typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)));

            var ilGen = method.GetILGenerator();
            factory.GenerateParseCallUtf8(ilGen, propertyInfo.PropertyType);
            factory.GenerateSetProperty(ilGen, propertyInfo);
            ilGen.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(method, typeof(IUtf8PropertyMapper<>).MakeGenericType(typeof(TEntity)).GetMethod("Map"));

            var type = typeBuilder.CreateType();
            var constructor = type.GetConstructor(Type.EmptyTypes);
            var casted = (IUtf8PropertyMapper<TEntity>)constructor.Invoke(null);
            return casted;
        }

        private AssemblyBuilder GetAssemblyBuilder(string assemblyName)
        {
            var name = new AssemblyName(assemblyName);
            var builder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            return builder;
        }

        private ModuleBuilder GetModuleBuilder(AssemblyBuilder assemblyBuilder)
        {
            var builder = assemblyBuilder.DefineDynamicModule("PropertyMappers");
            return builder;
        }

        private TypeBuilder GetType(ModuleBuilder moduleBuilder, string className, params Type[] interfaceTypes)
        {
            var typeName = $"PropertyMapper_{className}_{Guid.NewGuid()}";
            var typeBuilder = moduleBuilder.DefineType(typeName,
                TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.Sealed |
                TypeAttributes.AnsiClass | TypeAttributes.Public,
                typeof(object)
                );

            foreach (var interfaceType in interfaceTypes)
            {
                typeBuilder.AddInterfaceImplementation(interfaceType);
            }

            return typeBuilder;
        }

        private MethodBuilder GetMethod(TypeBuilder typeBuilder, string methodName, params Type[] parameterTypes)
        {
            MethodBuilder builder = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.Virtual | MethodAttributes.NewSlot, CallingConventions.HasThis, typeof(void), parameterTypes);
            return builder;
        }
        

        private void GenerateParseCall(ILGenerator gen, Type propertyType)
        {
            if (propertyType == typeof(string))
            {
                gen.DeclareLocal(typeof(string));
                gen.Emit(OpCodes.Ldarg_2);
                var constructor = typeof(string).GetConstructor(new []{typeof(ReadOnlySpan<>).MakeGenericType(typeof(char))});
                gen.Emit(OpCodes.Newobj, constructor);
            }
            else if (propertyType == typeof(int))
            {
                gen.DeclareLocal(typeof(int));
                gen.Emit(OpCodes.Ldarg_2);
                gen.EmitCall(OpCodes.Call, typeof(ParseUtils).GetMethod("ParseInt", new [] {typeof(ReadOnlySpan<>).MakeGenericType(typeof(char))}), null);
            }
            else if (propertyType == typeof(float))
            {
                gen.DeclareLocal(typeof(float));
                gen.Emit(OpCodes.Ldarg_2);
                gen.EmitCall(OpCodes.Call, typeof(ParseUtils).GetMethod("ParseFloat", new[] { typeof(ReadOnlySpan<>).MakeGenericType(typeof(char)) }), null);
            }

            gen.Emit(OpCodes.Stloc_0);
        }

        private void GenerateParseCallUtf8(ILGenerator gen, Type propertyType)
        {
            if (propertyType == typeof(string))
            {
                gen.DeclareLocal(typeof(string));
                gen.Emit(OpCodes.Ldarg_2);
                gen.Emit(OpCodes.Call, typeof(ParseUtils).GetMethod("SpanToString"));
            }
            
            else if (propertyType == typeof(int))
            {
                gen.DeclareLocal(typeof(int));
                gen.Emit(OpCodes.Ldarg_2);
                gen.EmitCall(OpCodes.Call, typeof(ParseUtils).GetMethod("ParseInt", new [] {typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte))}), null);
            }
            else if (propertyType == typeof(float))
            {
                gen.DeclareLocal(typeof(float));
                gen.Emit(OpCodes.Ldarg_2);
                gen.EmitCall(OpCodes.Call, typeof(ParseUtils).GetMethod("ParseFloat", new[] { typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)) }), null);
            }

            gen.Emit(OpCodes.Stloc_0);
        }

        private void GenerateSetProperty(ILGenerator gen, PropertyInfo property)
        {
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Callvirt, property.SetMethod);
        }
    }
}
