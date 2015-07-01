using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using IteratorTasks;

namespace IteratorTasksGenerator.Test
{
    [TestClass]
    public class UnitTest : ConventionCodeFixVerifier
    {
        protected override IEnumerable<MetadataReference> References
        {
            get
            {
                var x86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                var programFilesPath = Directory.Exists(x86) ? x86 : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                var asmRoot = Path.Combine(programFilesPath, "Reference Assemblies", "Microsoft", "Framework", ".NETFramework", "v3.5", "Profile", "Client");
                yield return MetadataReference.CreateFromFile(Path.Combine(asmRoot, "mscorlib.dll"));       //typeof(object).Assembly
                yield return MetadataReference.CreateFromFile(Path.Combine(asmRoot, "System.Core.dll"));    //typeof(Enumerable).Assembly
                yield return MetadataReference.CreateFromAssembly(typeof(CSharpCompilation).Assembly);
                yield return MetadataReference.CreateFromAssembly(typeof(Compilation).Assembly);
                yield return MetadataReference.CreateFromAssembly(typeof(Task).Assembly);
                yield return MetadataReference.CreateFromAssembly(typeof(TaskEx).Assembly);
            }
        }

        [TestMethod]
        public void SimpleTask() => VerifyCSharpByConvention();

        [TestMethod]
        public void HasResultTask() => VerifyCSharpByConvention();

        [TestMethod]
        public void ConflictAll() => VerifyCSharpByConvention();

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new YieldReturnCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new YieldReturnAnalyzer();
        }
    }
}