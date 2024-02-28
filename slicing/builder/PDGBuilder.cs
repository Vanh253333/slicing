using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using slicing.graph;

namespace slicing.builder
{
    class PDGBuilder
    {
        private PDG pdg;
        private List<CFG> cfgs;
        private CDGBuilder cdgBuilder;

        public PDGBuilder()
        {

        }

        public void Build(string filePath)
        {
            var decompiler = new CSharpDecompiler(filePath, new DecompilerSettings());
            var syntaxTree = decompiler.DecompileWholeModuleAsSingleFile();
            Build(syntaxTree);
        }

        private void Build(SyntaxTree syntaxTree)
        {
            cdgBuilder = new CDGBuilder(syntaxTree);
            cdgBuilder.Build();

        }
    }
}
