using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Namespacer.Configuration;
using Namespacer.Engine;

namespace Namespacer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NamespacerAnalyzer : DiagnosticAnalyzer
    {
        public const string TransgressionDiagnosticId = "NSER001";
        private static readonly LocalizableString TransgressionTitle = new LocalizableResourceString(nameof(Resources.TransgressionTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString TransgressionMessageFormat = new LocalizableResourceString(nameof(Resources.TransgressionMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString TransgressionDescription = new LocalizableResourceString(nameof(Resources.TransgressionDescription), Resources.ResourceManager, typeof(Resources));
        private const string TransgressionCategory = "Correctness";
        private static DiagnosticDescriptor TransgressionRule = new DiagnosticDescriptor(TransgressionDiagnosticId, TransgressionTitle, TransgressionMessageFormat, TransgressionCategory, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: TransgressionDescription);

        public const string ConfigurationDiagnosticId = "NSER002";
        private static readonly LocalizableString ConfigurationTitle = new LocalizableResourceString(nameof(Resources.ConfigurationDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ConfigurationMessageFormat = new LocalizableResourceString(nameof(Resources.ConfigurationMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ConfigurationDescription = new LocalizableResourceString(nameof(Resources.ConfigurationDescription), Resources.ResourceManager, typeof(Resources));
        private const string ConfigurationCategory = "Naming";
        private static DiagnosticDescriptor ConfigurationRule = new DiagnosticDescriptor(ConfigurationDiagnosticId, ConfigurationTitle, ConfigurationMessageFormat, ConfigurationCategory, DiagnosticSeverity.Error, isEnabledByDefault: true, description: ConfigurationDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(TransgressionRule, ConfigurationRule); } }

        private ConfigFile? configFile;

        public NamespacerAnalyzer() : base()
        {

        }

        public NamespacerAnalyzer(ConfigFile configFile) : base()
        {
            this.configFile = configFile;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CompilationStart);
            context.RegisterSyntaxNodeAction(AnalyzeMethodInvocations, SyntaxKind.InvocationExpression);
        }

        private void CompilationStart(CompilationAnalysisContext context)
        {
            if (configFile != null)
            {
                return; // introduced for unit testing, for now
            }

            var additionalFiles = context.Options.AdditionalFiles.Where(f => f.Path.ToLower().EndsWith(".namespacer"));

            if (!additionalFiles.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(ConfigurationRule, null, "There is no configuration file added to compilation as an AdditionalFile with a '.namespacer' suffix. Consider adding '<AdditionalFiles Include=\".namespacer\" />' to your csproj."));
                return;
            }

            configFile = ConfigFile.LoadFromFile(additionalFiles.First().Path);

            if (configFile == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(ConfigurationRule, null, $"Configuration file '{additionalFiles.First().Path}' was found, but failed parsing."));
            }
        }

        private void AnalyzeMethodInvocations(SyntaxNodeAnalysisContext context)
        {
            if (configFile == null)
            {
                return;
            }

            var invocationExpr = context.Node as InvocationExpressionSyntax;

            if (invocationExpr == null)
            {
                return;
            }

            var callerNs = GetCallerNs(invocationExpr);

            if (string.IsNullOrEmpty(callerNs))
            {
                return;
            }

            var returnTypeCalleeNs = GetCalleeReturnType(context, invocationExpr);

            if (!EvaluationEngine.IsOk(callerNs, returnTypeCalleeNs, configFile.Value))
            {
                context.ReportDiagnostic(Diagnostic.Create(TransgressionRule, invocationExpr.GetLocation(), returnTypeCalleeNs, callerNs));
                return;
            }

            var methodTypeCalleeNs = context.SemanticModel.GetSymbolInfo(invocationExpr).Symbol?.ContainingType?.GetNs();

            if (!EvaluationEngine.IsOk(callerNs, methodTypeCalleeNs, configFile.Value))
            {
                context.ReportDiagnostic(Diagnostic.Create(TransgressionRule, invocationExpr.GetLocation(), methodTypeCalleeNs, callerNs));
            }
        }

        private string GetCallerNs(InvocationExpressionSyntax invocationExpr)
        {
            var ns = invocationExpr.FirstAncestorOrSelf<NamespaceDeclarationSyntax>(n => n is NamespaceDeclarationSyntax);
            return ns.ChildNodes().FirstOrDefault()?.ChildTokens().FirstOrDefault().ValueText;
        }

        private string GetCalleeReturnType(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpr)
        {
            var returnType = context.SemanticModel.GetTypeInfo(invocationExpr).Type;

            if (returnType == null || returnType.Kind == SymbolKind.ErrorType)
            {
                return null;
            }

            return returnType.GetNs();
        }
    }

    public static class RoslynHelpers
    {
        public static string GetNs(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable
            ));
        }
    }
}
