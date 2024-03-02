using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using slicing.builder;

namespace slicing
{
    class Program
    {
        static void Main(string[] args)
        {
            PDGBuilder pdgBuilder = new PDGBuilder();
            var filePath = "C:\\File_VA\\c#\\NET\\00a1c7dff517266b7e001dd607952072";
            //var filePath = "C:\\File_VA\\copyfolder1.exe";
            pdgBuilder.Build(filePath);
            //var decompiler = new CSharpDecompiler(filePath, new DecompilerSettings());
            //var syntaxTree = decompiler.DecompileWholeModuleAsSingleFile();
            //Console.WriteLine(syntaxTree.ToString());
        }
    }
}


//using System;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.FlowAnalysis;

//public class Program
//{
//    public static void Main()
//    {
//        var code = @"
//            public class C
//            {
//                public void M(int x)
//                {
//                    if (x > 0)
//                    {
//                        Console.WriteLine(x);
//                    }
//                }
//            }
//        ";

//        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
//        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

//        var compilation = CSharpCompilation.Create("CFGDemo")
//            .AddSyntaxTrees(tree)
//            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

//        var semanticModel = compilation.GetSemanticModel(tree);

//        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
//        var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(method);

//        Console.WriteLine("Variables declared inside the method:");
//        foreach (var symbol in dataFlowAnalysis.VariablesDeclared)
//        {
//            Console.WriteLine(symbol.Name);
//        }

//        var cfg = ControlFlowGraph.Create(method, semanticModel);
//        Console.WriteLine("Control flow graph for the method:");
//        foreach (var block in cfg.Blocks)
//        {
//            Console.WriteLine(block.Kind);
//            foreach (var operation in block.Operations)
//            {
//                Console.WriteLine(operation.Kind);
//                Console.WriteLine(operation.Syntax.ToString());
//            }
//        }
//    }
//}