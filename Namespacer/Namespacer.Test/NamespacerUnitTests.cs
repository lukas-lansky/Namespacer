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
        public void SimpleForbiddenSituation()
        {
            var testCodeFile = @"
using System;

namespace SourceNamespace
{
    class TypeName
    {
        public static void A()
        {
            TargetNamespace.AnotherTypeName.B();
        }
    }
}

namespace TargetNamespace
{
    class AnotherTypeName
    {
        public static void B()
        {
            SourceNamesapce.TypeName.A();
        }
    }
}
";

            var configFile = ConfigFile.LoadFromString(@"
SourceNamespace => TargetNamespace:
    * -!> *
").Value;

            VerifyCSharpDiagnostic(
                testCodeFile,
                new NamespacerAnalyzer(configFile),
                new DiagnosticResult {
                    Id = "NSER001",
                    Severity = DiagnosticSeverity.Warning,
                    Message = "You should not mention symbol 'TargetNamespace.AnotherTypeName' in 'SourceNamespace' because of rule '' from the .namespacer configuration file",
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 13) }
                });
        }

        [TestMethod]
        public void SimpleAllowedSituation()
        {
            var testCodeFile = @"
using System;

namespace SourceNamespace
{
    class TypeName
    {
        public static void A()
        {
            TargetNamespace.AnotherTypeName.B();
        }
    }
}

namespace TargetNamespace
{
    class AnotherTypeName
    {
        public static void B()
        {
            SourceNamesapce.TypeName.A();
        }
    }
}
";

            var configFile = ConfigFile.LoadFromString(@"
SourceNamespace => TargetNamespace:
    * -> *
").Value;

            VerifyCSharpDiagnostic(
                testCodeFile,
                new NamespacerAnalyzer(configFile));
        }
    }
}
