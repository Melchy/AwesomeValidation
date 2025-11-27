using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace AwesomeValidationGenerator;

[Generator]
public class ControllerClientGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var controllerDeclarations = context.SyntaxProvider
           .ForAttributeWithMetadataName("AwesomeValidation.ValidationDefinition",
                (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
                (context, cancellationToken) =>
                {
                    var method = (MethodDeclarationSyntax)context.TargetNode;
                    // Note: This assumes the method is directly inside a class. 
                    // Robust generators often handle structs/nested classes checks here.
                    var classDecl = (ClassDeclarationSyntax)method.Parent!;
                    var ns = context.TargetSymbol.ContainingNamespace.ToDisplayString();

                    return (Namespace: ns, ClassName: classDecl.Identifier.Text, MethodName: method.Identifier.Text, Content: method.ToString());
                });
    
        context.RegisterSourceOutput(controllerDeclarations,
            static (spc, source) => Execute(source, spc));
    }

    private static void Execute(
        (string Namespace, string ClassName, string MethodName, string Content) inputData,
        SourceProductionContext context)
    {
        var source = $@"
using AwesomeValidation;

namespace {inputData.Namespace}
        {{
            public class {inputData.ClassName}Generated
            {{
        {inputData.Content}
            }}
        }}";
        context.AddSource($"{inputData.ClassName}_{inputData.MethodName}.g.cs", SourceText.From(source, Encoding.UTF8));
    }
}
