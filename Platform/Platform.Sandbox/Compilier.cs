using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Platform.Links.DataBase.CoreNet.Triplets;

namespace Platform.Sandbox
{
    static class Compilier
    {
        private static bool SaveAssemblyToFile = false;

        private static AssemblyBuilder DynamicGeneratedAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DynamicGenerated"), SaveAssemblyToFile ? AssemblyBuilderAccess.RunAndSave : AssemblyBuilderAccess.Run);
        private static ModuleBuilder ModuleBuilder = CreateModuleBuilder();
        private static long SelectionChecksMethodsCounter = 0;

        public static ModuleBuilder CreateModuleBuilder()
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

        public static Func<Link, bool> CompileSelectionCheck(Action<ILGenerator> emitter)
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
