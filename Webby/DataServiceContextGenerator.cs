﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Webby
{
using System;
using System.Data.Services.Client;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;


    public class DataServiceContextGenerator<TInterface>
           where TInterface : class
    {
        private readonly AssemblyBuilder _assemblyBuilder;

        private readonly ModuleBuilder _moduleBuilder;

        private readonly string _assemblyName;

        public DataServiceContextGenerator()
        {
            _assemblyName = "Generated." + typeof(TInterface).Namespace + "." + typeof(TInterface).Name + ".DataServiceContext.ID_" + Guid.NewGuid().ToString().Replace("-", string.Empty);

            var name = new AssemblyName { Name = _assemblyName, Version = new Version(1, 0, 0) };

            var domain = Thread.GetDomain();

            _assemblyBuilder = domain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave, AppDomain.CurrentDomain.BaseDirectory);

            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyName + ".dll", true);
        }

        /// <summary>
        ///     Creates a TypeBuilder based on <typeparamref name="TInterface" />.
        /// </summary>
        /// <returns>A TypeBuilder</returns>
        private TypeBuilder CreateTypeBuilder()
        {
            // create type that implements TInterface
            TypeAttributes TypeAttributes =
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit
                | TypeAttributes.AutoLayout;

            var typeBuilder = _moduleBuilder.DefineType(
                "AutoGenerated." + typeof(TInterface).Name + "_ID_" + Guid.NewGuid().ToString().Replace("-", string.Empty), TypeAttributes, typeof(ConfigurableDataServiceContext), new[] { typeof(TInterface) });

            typeBuilder.AddInterfaceImplementation(typeof(TInterface));

            return typeBuilder;
        }

        /// <summary>
        ///     Creates the Type representing the WCF proxy that implements <typeparamref name="TInterface" />.
        /// </summary>
        /// <returns>
        ///     A type that implements <typeparamref name="TInterface" />
        /// </returns>
        public Type CreateContextType()
        {
            var typeBuilder = CreateTypeBuilder();

            CreateConstructors(typeBuilder);

            CreateProperties(typeBuilder);

            var t = typeBuilder.CreateType();

            // You can uncomment this line to save the assembly to disk if you want to use ILDASM.
            // _assemblyBuilder.Save(_assemblyName + ".dll");
            return t;
        }

        /// <summary>
        ///     Creates the constructors that call those on ExceptionHandlingProxyBase.
        /// </summary>
        /// <param name="typeBuilder">The type builder.</param>
        private static void CreateConstructors(TypeBuilder typeBuilder)
        {
            var constructors = typeof(ConfigurableDataServiceContext).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var con in constructors)
            {
                var parameterTypes = con.GetParameters().Select(p => p.ParameterType).ToArray();

                var ctor = typeBuilder.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, parameterTypes);

                var generator = ctor.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Call, con);
                generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Ret);
            }
        }

        /// <summary>
        ///     Creates methods to implement <typeparamref name="TInterface" />.
        /// </summary>
        /// <param name="typeBuilder">The type builder.</param>
        private static void CreateProperties(TypeBuilder typeBuilder)
        {
            var interfaceType = typeof(TInterface);
            var properties = interfaceType.GetProperties();

            foreach (var property in properties)
            {
                var backingFieldType = typeof(DataServiceQuery<>).MakeGenericType(property.PropertyType.GenericTypeArguments);
                var backingFieldBuilder = typeBuilder.DefineField("_" + property.Name, backingFieldType, FieldAttributes.Private);
                var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);

                MethodBuilder getMethod = null;
                MethodBuilder setMethod = null;
                if (property.CanRead)
                {
                    var currentGetMethod = property.GetGetMethod(true);
                    if (currentGetMethod != null)
                    {
                        getMethod = DefineGetter(currentGetMethod, typeBuilder, backingFieldBuilder, property.Name);
                    }
                }

                if (property.CanWrite)
                {
                    var currentSetMethod = property.GetSetMethod(true);
                    if (currentSetMethod != null)
                    {
                        setMethod = DefineSetter(currentSetMethod, typeBuilder, backingFieldBuilder);
                    }
                }

                if (getMethod != null)
                {
                    propertyBuilder.SetGetMethod(getMethod);
                }
                if (setMethod != null)
                {
                    propertyBuilder.SetSetMethod(setMethod);
                }
            }
        }

        private static MethodBuilder DefineSetter(MethodInfo method, TypeBuilder typeBuilder, FieldBuilder backingField)
        {
            const MethodAttributes Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            var newMethodBuilder = typeBuilder.DefineMethod(method.Name, Attributes, method.ReturnType, Type.EmptyTypes);
            var generator = newMethodBuilder.GetILGenerator();
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, backingField);
                generator.Emit(OpCodes.Ret);
            }

            return newMethodBuilder;
        }

        private static MethodBuilder DefineGetter(MethodInfo method, TypeBuilder typeBuilder, FieldInfo backingField, string propertyName)
        {
            const MethodAttributes Attributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;

            var createQuery = typeof(ConfigurableDataServiceContext).GetMethod("CreateQuery", BindingFlags.Instance | BindingFlags.Public);

            if (backingField.FieldType.IsGenericType)
            {
                createQuery = createQuery.MakeGenericMethod(backingField.FieldType.GenericTypeArguments);
            }

            var newMethodBuilder = typeBuilder.DefineMethod(method.Name, Attributes, method.ReturnType, Type.EmptyTypes);
            var generator = newMethodBuilder.GetILGenerator();
            {
                var lblBackingFieldIsNull = generator.DefineLabel();
                var noOpLabel = generator.DefineLabel();
                generator.DeclareLocal(method.ReturnType);
                generator.DeclareLocal(backingField.FieldType);
                generator.Emit(OpCodes.Nop);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, backingField);
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Brtrue_S, lblBackingFieldIsNull);
                generator.Emit(OpCodes.Pop);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldstr, propertyName);
                generator.Emit(OpCodes.Call, createQuery);
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Stloc_1);
                generator.Emit(OpCodes.Stfld, backingField);
                generator.Emit(OpCodes.Ldloc_1);
                generator.MarkLabel(lblBackingFieldIsNull);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Br_S, noOpLabel);
                generator.MarkLabel(noOpLabel);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ret);
            }

            return newMethodBuilder;
        }
    }
}
