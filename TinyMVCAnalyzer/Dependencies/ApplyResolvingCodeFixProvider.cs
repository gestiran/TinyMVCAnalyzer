using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Document = Microsoft.CodeAnalysis.Document;

namespace TinyMVCAnalyzer.Dependencies {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApplyResolvingCodeFixProvider)), Shared]
    public sealed class ApplyResolvingCodeFixProvider : CodeFixProvider {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ApplyResolvingAnalyzer.DIAGNOSTIC_ID);
        
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
        
        public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
            if (root == null) {
                return;
            }
            
            Diagnostic diagnostic = context.Diagnostics[0];
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            SyntaxNode parent = root.FindToken(diagnosticSpan.Start).Parent;
            
            if (parent == null) {
                return;
            }
            
            ClassDeclarationSyntax declaration = parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            
            context.RegisterCodeFix(
                CodeAction.Create(title: "Add IApplyResolving interface", createChangedDocument: c => AddInterfaceAsync(context.Document, declaration, c),
                                  equivalenceKey: nameof(ApplyResolvingCodeFixProvider)), diagnostic);
        }
        
        private async Task<Document> AddInterfaceAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken) {
            CompilationUnitSyntax root = await document.GetSyntaxRootAsync(cancellationToken) as CompilationUnitSyntax;
            
            if (root == null) {
                return document;
            }
            
            string targetNamespace = "TinyMVC.Dependencies";
            bool hasUsing = false;
            
            foreach (UsingDirectiveSyntax usingDirective in root.Usings) {
                if (usingDirective.Name.ToString() == targetNamespace) {
                    hasUsing = true;
                    break;
                }
            }
            
            TypeSyntax interfaceType = SyntaxFactory.ParseTypeName("IApplyResolving");
            SimpleBaseTypeSyntax baseType = SyntaxFactory.SimpleBaseType(interfaceType);
            
            ClassDeclarationSyntax newClassDeclaration;
            
            if (classDeclaration.BaseList == null) {
                BaseListSyntax baseList = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType));
                newClassDeclaration = classDeclaration.WithBaseList(baseList);
            } else {
                newClassDeclaration = classDeclaration.AddBaseListTypes(baseType);
            }
            
            CompilationUnitSyntax newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
            
            if (!hasUsing) {
                NameSyntax nameSyntax = SyntaxFactory.ParseName(targetNamespace);
                UsingDirectiveSyntax usingDirective = SyntaxFactory.UsingDirective(nameSyntax).WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
                
                newRoot = newRoot.AddUsings(usingDirective);
            }
            
            return document.WithSyntaxRoot(newRoot);
        }
    }
}