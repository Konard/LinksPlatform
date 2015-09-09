using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using NetLibrary;

namespace ConsoleTester
{
	static class Compilier
	{
		static private bool SaveAssemblyToFile = false;

		static private AssemblyBuilder DynamicGeneratedAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DynamicGenerated"), SaveAssemblyToFile ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run);
		static private ModuleBuilder ModuleBuilder = CreateModuleBuilder();
		static private long SelectionChecksMethodsCounter = 0;

		static public ModuleBuilder CreateModuleBuilder()
		{
			ModuleBuilder module;

			if (SaveAssemblyToFile)
				module = DynamicGeneratedAssembly.DefineDynamicModule("DynamicGenerated", "DynamicGenerated.dll");
			else
				module = DynamicGeneratedAssembly.DefineDynamicModule("DynamicGenerated");

			// Add an assembly attribute that tells the CLR to wrap non-CLS-compliant exceptions in a RuntimeWrappedException object
			// (so the cast to Exception in the code will always succeed).  C# and VB always do this, C++/CLI doesn't.
			DynamicGeneratedAssembly.SetCustomAttribute(new CustomAttributeBuilder(
				typeof(RuntimeCompatibilityAttribute).GetConstructor(new Type[] { }),
				new object[] { },
				new PropertyInfo[] { typeof(RuntimeCompatibilityAttribute).GetProperty("WrapNonExceptionThrows") },
				new object[] { true }));

			// Add an assembly attribute that tells the CLR not to attempt to load PDBs when compiling this assembly 
			DynamicGeneratedAssembly.SetCustomAttribute(new CustomAttributeBuilder(
				typeof(DebuggableAttribute).GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) }),
				new object[] { DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints }));

			return module;
		}

		static public Func<Link, bool> CompileSelectionCheck(Action<ILGenerator> emitter)
		{
			SelectionChecksMethodsCounter++;
			string methodName = "c" + SelectionChecksMethodsCounter;

			// Разобраться можно ли обойтись без множественной генерации классов, чтобы можно было тупо добавлять методы в класс.
			var typeBuilder = ModuleBuilder.DefineType("selection_checks_" + methodName, TypeAttributes.Class | TypeAttributes.Public);

			// Так же имеет смысл задуматься об удалении методов.
			// А может быть даже о возможности просто создавать делегаты, без привязки к каким либо типам, чисто лямбда выражения (динамические созданные), ведь класс то в сущности не обязателен.
			var argTypes = new Type[] { typeof(Link) };
			MethodBuilder meth = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, typeof(bool), argTypes);

			emitter(meth.GetILGenerator());

			var bakedType = typeBuilder.CreateType();

			// Construct a delegate to the filter function and return it
			var bakedMeth = bakedType.GetMethod(methodName);
			var del = Delegate.CreateDelegate(typeof(Func<Link, bool>), bakedMeth);

			if (SaveAssemblyToFile)
				DynamicGeneratedAssembly.Save("DynamicGenerated.dll");

			return (Func<Link, bool>)del;
		}
	}
}
