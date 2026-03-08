using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TinyMVCAnalyzer.Dependencies {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ApplyResolvingAnalyzer : DiagnosticAnalyzer {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);
        
        public const string DIAGNOSTIC_ID = "TINYMVC001";
        
        private const string _TITLE = "Missing IApplyResolving interface";
        private const string _MESSAGE_FORMAT = "Class '{0}' has ApplyResolving method but does not implement IApplyResolving";
        private const string _CATEGORY = "Tiny MVC";
        
        private static readonly DiagnosticDescriptor _rule;
        
        static ApplyResolvingAnalyzer() {
            _rule = new DiagnosticDescriptor(DIAGNOSTIC_ID, _TITLE, _MESSAGE_FORMAT, _CATEGORY, DiagnosticSeverity.Warning, isEnabledByDefault: true);
        }
        
        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }
        
        private void AnalyzeClass(SyntaxNodeAnalysisContext context) {
            ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
            
            INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            
            if (symbol == null) {
                return;
            }
            
            IEnumerable<MethodDeclarationSyntax> methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
            MethodDeclarationSyntax methodDeclaration = methods.FirstOrDefault(syntax => syntax.Identifier.Text == "ApplyResolving");
            
            if (methodDeclaration == null) {
                return;
            }
            
            bool isImplementInterface = false;
            
            foreach (INamedTypeSymbol current in symbol.AllInterfaces) {
                if (current.Name == "IApplyResolving") {
                    isImplementInterface = true;
                    break;
                }
            }
            
            if (!isImplementInterface) {
                Diagnostic classDiagnostic = Diagnostic.Create(_rule, classDeclaration.Identifier.GetLocation(), symbol.Name);
                Diagnostic methodDiagnostic = Diagnostic.Create(_rule, methodDeclaration.Identifier.GetLocation(), symbol.Name);
                
                context.ReportDiagnostic(classDiagnostic);
                context.ReportDiagnostic(methodDiagnostic);
            }
        }
    }
}