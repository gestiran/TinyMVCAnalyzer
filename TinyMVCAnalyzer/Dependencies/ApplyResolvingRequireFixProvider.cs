using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace TinyMVCAnalyzer.Dependencies {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApplyResolvingRequireFixProvider)), Shared]
    public sealed class ApplyResolvingRequireFixProvider : InterfaceRequireFixProvider {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Labels.ID_APPLY_RESOLVING);
        
        protected override string _title { get; }
        protected override string _key { get; }
        protected override string _namespace { get; }
        protected override string _interfaceName { get; }
        
        public ApplyResolvingRequireFixProvider() {
            _title = "Add IApplyResolving interface";
            _key = nameof(ApplyResolvingRequireFixProvider);
            _namespace = "TinyMVC.Dependencies";
            _interfaceName = "IApplyResolving";
        }
    }
}