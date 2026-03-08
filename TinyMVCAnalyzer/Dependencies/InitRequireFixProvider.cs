using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace TinyMVCAnalyzer.Dependencies {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InitRequireFixProvider)), Shared]
    public sealed class InitRequireFixProvider : InterfaceRequireFixProvider {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Labels.ID_INIT);
        
        protected override string _title { get; }
        protected override string _key { get; }
        protected override string _namespace { get; }
        protected override string _interfaceName { get; }
        
        public InitRequireFixProvider() {
            _title = "Add IInit interface";
            _key = nameof(InitRequireFixProvider);
            _namespace = "TinyMVC.Loop";
            _interfaceName = "IInit";
        }
    }
}