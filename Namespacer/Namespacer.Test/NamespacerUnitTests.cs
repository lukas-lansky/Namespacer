using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using Namespacer;
using Namespacer.Configuration;

namespace Namespacer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void NoCodeNoConfigNoProblem()
        {
            var testCodeFile = @"";

            var emptyConfigFile = ConfigFile.LoadFromString("").Value;

            VerifyCSharpDiagnostic(testCodeFile, new NamespacerAnalyzer(emptyConfigFile));
        }

        [TestMethod]
        public void WriteLineIsForbidden()
        {
            var testCodeFile = @"
using System;

namespace ConsoleApplication1
{
    class TypeName
    {
        public static void Main()
        {
            Console.WriteLine();
        }
    }
}
";

            var emptyConfigFile = ConfigFile.LoadFromString(@"
* => System.Console:
    * -!> *
").Value;

            VerifyCSharpDiagnostic(testCodeFile, new NamespacerAnalyzer(emptyConfigFile));
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "Namespacer",
                Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NamespacerAnalyzer();
        }
    }
}
