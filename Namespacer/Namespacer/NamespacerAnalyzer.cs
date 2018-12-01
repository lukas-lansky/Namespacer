using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Namespacer.Configuration;

namespace Namespacer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NamespacerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Namespacer";

        private static readonly LocalizableString TransgressionTitle = new LocalizableResourceString(nameof(Resources.TransgressionTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString TransgressionMessageFormat = new LocalizableResourceString(nameof(Resources.TransgressionMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString TransgressionDescription = new LocalizableResourceString(nameof(Resources.TransgressionDescription), Resources.ResourceManager, typeof(Resources));
        private const string TransgressionCategory = "Correctness";
        private static DiagnosticDescriptor TransgressionRule = new DiagnosticDescriptor(DiagnosticId, TransgressionTitle, TransgressionMessageFormat, TransgressionCategory, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: TransgressionDescription);

        private static readonly LocalizableString ConfigurationTitle = new LocalizableResourceString(nameof(Resources.ConfigurationDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ConfigurationMessageFormat = new LocalizableResourceString(nameof(Resources.ConfigurationMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ConfigurationDescription = new LocalizableResourceString(nameof(Resources.ConfigurationDescription), Resources.ResourceManager, typeof(Resources));
        private const string ConfigurationCategory = "Naming";
        private static DiagnosticDescriptor ConfigurationRule = new DiagnosticDescriptor(DiagnosticId, ConfigurationTitle, ConfigurationMessageFormat, ConfigurationCategory, DiagnosticSeverity.Error, isEnabledByDefault: true, description: ConfigurationDescription);


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
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
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

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (configFile == null)
            {
                return;
            }

            var containingNamespace = context.Symbol.ContainingNamespace;
            var mentionedNamespace = context.Symbol.Name;
        }
    }
}
