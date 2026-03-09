using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TinyAnalyzer {
    public abstract class InterfaceRequireAnalyser : DiagnosticAnalyzer {
        protected abstract DiagnosticDescriptor _rule { get; }
        protected abstract string _methodName { get; }
        protected abstract string _interfaceName { get; }
        
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
            
            bool isHave = false;
            MethodDeclarationSyntax methodDeclaration = null;
            
            foreach (MethodDeclarationSyntax method in methods) {
                if (method.Identifier.Text.Equals(_methodName) == false) {
                    continue;
                }
                
                if (method.ParameterList.Parameters.Count > 0) {
                    continue;
                }
                
                methodDeclaration = method;
                isHave = true;
                break;
            }
            
            if (isHave == false) {
                return;
            }
            
            bool isImplementInterface = false;
            
            foreach (INamedTypeSymbol current in symbol.AllInterfaces) {
                if (current.Name == _interfaceName) {
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