using ICSharpCode.Decompiler.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slicing.graph
{
    public enum VertexType
    {
        CUNIT,
        PKG,
        IMPORT,
        CLASS,
        ENTRY,
        EXIT,
        FORMAL_IN,
        FORMAL_OUT,
        ACTUAL_IN,
        ACTUAL_OUT,
        ARRAY_IDX,
        CALL,
        ASSIGN,
        RETURN,
        INITIAL_STATE,
        BREAK,
        CONTINUE,
        CTRL,
        THROW,
        TRY,
        CATCH,
        FINALLY,
        INIT
    }


    //[Serializable]
    class Vertex
    {
        private long id;
        private string label;
        private string assignment;
        private string submission;
        private VertexType type;
        private HashSet<string> subtypes;

        private int? startLine;
        private int? endLine;
        private int? line;

        private PDG pdg;
        private string def;
        private HashSet<string> uses;

        private string pseudoUse;
        private readonly HashSet<Vertex> inVertices;
        private HashSet<Vertex> outVertices;
        private AstNode ast;

        private bool visited;

        public Vertex(string label)
        {
            this.label = label.Replace("\n", " ");
            uses = new HashSet<string>();
            subtypes = new HashSet<string>();
            inVertices = new HashSet<Vertex>();
            outVertices = new HashSet<Vertex>();
        }

        public Vertex(long id)
        {
            this.id = id;
            uses = new HashSet<string>();
            subtypes = new HashSet<string>();
            inVertices = new HashSet<Vertex>();
            outVertices = new HashSet<Vertex>();
        }

        public Vertex(VertexType type, string label, AstNode ast)
        {
            id = -1;
            this.type = type;
            this.label = label.Replace("\n", " ");
            uses = new HashSet<string>();
            subtypes = new HashSet<string>();
            inVertices = new HashSet<Vertex>();
            outVertices = new HashSet<Vertex>();
            this.ast = ast;
        }

        public Vertex(VertexType type, string label) : this(-1, type, label)
        {
        }
        public Vertex(long id, VertexType type, string label)
        {
            this.id = id;
            this.type = type;
            this.label = label.Replace("\n", " ");
            uses = new HashSet<string>();
            subtypes = new HashSet<string>();
            inVertices = new HashSet<Vertex>();
            outVertices = new HashSet<Vertex>();
        }

        public long GetId()
        {
            return id;
        }

        public void SetId(long id)
        {
            this.id = id;
        }

        public string GetLabel()
        {
            return label;
        }

        public void SetLabel(string label)
        {
            this.label = label.Replace("\n", " ");
        }

        public string GetDef()
        {
            return def;
        }

        public void SetDef(string def)
        {
            this.def = def;
        }

        public HashSet<string> GetUses()
        {
            if (uses == null)
                uses = new HashSet<string>();
            return uses;
        }

        public string GetPseudoUse()
        {
            return pseudoUse;
        }

        public void SetPseudoUse(string pseudoUse)
        {
            this.pseudoUse = pseudoUse;
        }

        public void SetUses(HashSet<string> uses)
        {
            this.uses = uses;
        }

        public void ClearDefUses()
        {
            def = null;
            uses = new HashSet<string>();
        }

        public void ClearUses()
        {
            uses = new HashSet<string>();
        }

        public int? GetStartLine()
        {
            return startLine;
        }

        public void SetStartLine(int? startLine)
        {
            this.startLine = startLine;
        }

        public int? GetEndLine()
        {
            return endLine;
        }

        public void SetEndLine(int? endLine)
        {
            this.endLine = endLine;
        }

        public HashSet<Vertex> GetInVertices()
        {
            return inVertices;
        }

        public HashSet<Vertex> GetOutVertices()
        {
            return outVertices;
        }

        public void SetOutVertices(HashSet<Vertex> outVertices)
        {
            this.outVertices = outVertices;
        }

        public VertexType GetTypeVertex()
        {
            return type;
        }

        public void SetType(VertexType type)
        {
            this.type = type;
        }

        public HashSet<string> GetSubtypes()
        {
            return subtypes;
        }

        public void SetSubtypes(HashSet<string> subtypes)
        {
            this.subtypes = subtypes;
        }

        public HashSet<string> GetTypeAndSubtypes()
        {
            HashSet<string> result = new HashSet<string>(subtypes);
            result.Add(type.ToString());
            return result;
        }

        public AstNode GetAst()
        {
            return ast;
        }

        public void SetVisited(Boolean visited)
        {
            this.visited = visited;
        }

        public Boolean IsVisited()
        {
            return visited;
        }

        public PDG GetPDG()
        {
            return pdg;
        }

        public void SetPDG(PDG pdg)
        {
            this.pdg = pdg;
        }

        public string GetAssigment()
        {
            return assignment;
        }

        public void SetAssignment(string assignment)
        {
            this.assignment = assignment;
        }

        public string GetSubmission()
        {
            return submission;
        }

        public void SetSubmission(string submission)
        {
            this.submission = submission;
        }

        //public override string ToString()
        //{
        //    string result = id + "-" + type;
        //    if (label != null && label != "")
        //    {
        //        result += "-" + label;
        //    }
        //    return result;
        //}
    }
}
