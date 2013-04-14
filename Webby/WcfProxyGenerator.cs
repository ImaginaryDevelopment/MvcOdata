namespace Webby
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Threading;

	internal class WcfProxyGenerator<TInterface>
		where TInterface : class
	{
		private readonly AssemblyBuilder _assemblyBuilder;

		private readonly ModuleBuilder _moduleBuilder;

		internal WcfProxyGenerator()
		{
			var name = new AssemblyName { Name = typeof(TInterface).Namespace + "." + typeof(TInterface).Name + ".WcfProxy" };

			var domain = Thread.GetDomain();

			this._assemblyBuilder = domain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);

			this._moduleBuilder = this._assemblyBuilder.DefineDynamicModule(this._assemblyBuilder.GetName().Name, false);
		}

		/// <summary>
		/// Creates a TypeBuilder based on <typeparamref name="TInterface" />.
		/// </summary>
		/// <returns>A TypeBuilder</returns>
		private TypeBuilder CreateTypeBuilder()
		{
			// create type that implements TInterface
			var typeBuilder = this._moduleBuilder.DefineType(
				typeof(TInterface).Name + Guid.NewGuid(),
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
				typeof(ConfigurableExceptionHandlingProxyBase<TInterface>),
				new[] { typeof(TInterface) });

			typeBuilder.AddInterfaceImplementation(typeof(TInterface));

			return typeBuilder;
		}

		/// <summary>
		///     Creates the Type representing the WCF proxy that implements <typeparamref name="TInterface" />.
		/// </summary>
		/// <returns>
		///     A type that implements <typeparamref name="TInterface" />
		/// </returns>
		internal Type CreateProxyType()
		{
			var typeBuilder = this.CreateTypeBuilder();

			CreateConstructors(typeBuilder);

			CreateMethods(typeBuilder);

			var t = typeBuilder.CreateType();

			return t;
		}

		/// <summary>
		/// Creates the constructors that call those on ExceptionHandlingProxyBase.
		/// </summary>
		/// <param name="typeBuilder">The type builder.</param>
		private static void CreateConstructors(TypeBuilder typeBuilder)
		{
			var constructors = typeof(ConfigurableExceptionHandlingProxyBase<TInterface>).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var con in constructors)
			{
				var parameterTypes = con.GetParameters().Select(p => p.ParameterType).ToArray();

				var ctor = typeBuilder.DefineConstructor(
					MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, parameterTypes);

				var generator = ctor.GetILGenerator();
				generator.Emit(OpCodes.Ldarg_0);

				for (var i = 0; i < parameterTypes.Count(); i++)
				{
					generator.Emit(OpCodes.Ldarg, i + 1);
				}

				generator.Emit(OpCodes.Call, con);
				generator.Emit(OpCodes.Ret);
			}
		}

		/// <summary>
		/// Creates methods to implement <typeparamref name="TInterface" />.
		/// </summary>
		/// <param name="typeBuilder">The type builder.</param>
		private static void CreateMethods(TypeBuilder typeBuilder)
		{
			var interfaceType = typeof(TInterface);

			// create implementations of TInterface methods that call the corresponding method on the Channel object
			// of ExceptionHandlingProxyBase<T>
			// interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			var methods = GetMembers(interfaceType, BindingFlags.Public | BindingFlags.Instance);

			var channelGet = typeof(ConfigurableExceptionHandlingProxyBase<TInterface>).GetMethod("get_Channel", BindingFlags.Instance | BindingFlags.Public);

			foreach (var method in methods)
			{
				var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
				var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameterTypes);

				var generator = methodBuilder.GetILGenerator();

				{
					generator.Emit(OpCodes.Ldarg_0);
					generator.Emit(OpCodes.Call, channelGet);
					for (var i = 0; i < parameterTypes.Count(); i++)
					{
						generator.Emit(OpCodes.Ldarg, i + 1);
					}

					generator.Emit(OpCodes.Callvirt, method);
					generator.Emit(OpCodes.Ret);
				}
			}
		}

		private static IEnumerable<MethodInfo> GetMembers(Type type, BindingFlags flags)
		{
			var methods = new List<MethodInfo>();

			GetMethodsRecursively(type, flags, methods);
			return methods;
		}

		private static void GetMethodsRecursively(Type type, BindingFlags flags, List<MethodInfo> members)
		{
			members.AddRange(type.GetMethods(flags));

			foreach (var interfaceType in type.GetInterfaces().Where(x => x != typeof(IDisposable)))
			{
				GetMethodsRecursively(interfaceType, flags, members);
			}
		}
	}
}