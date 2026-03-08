using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TinyMVCAnalyzer.Dependencies {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ApplyResolvingAnalyzer : InterfaceAnalyser {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);
        protected override string _methodName { get; }
        protected override string _interfaceName { get; }
        
        protected override DiagnosticDescriptor _rule { get; }
        
        private const string _TITLE = "Missing IApplyResolving interface";
        private const string _MESSAGE_FORMAT = "Class '{0}' has ApplyResolving method but does not implement IApplyResolving";
        
        public ApplyResolvingAnalyzer() {
            _methodName = "ApplyResolving";
            _interfaceName = "IApplyResolving";
            _rule = new DiagnosticDescriptor(Labels.ID_APPLY_RESOLVING, _TITLE, _MESSAGE_FORMAT, Labels.CATEGORY, DiagnosticSeverity.Warning, true);
        }
    }
}