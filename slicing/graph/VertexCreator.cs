using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace slicing.graph
{
    class VertexCreator
    {
        private long id;
        public Vertex Exit()
        {
            Vertex result = new Vertex(VertexType.EXIT, "exit");
            SetId(result);
            return result;
        }

        public Vertex TypeDeclaration(TypeDeclaration n)
        {
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CLASS, label, n);
            SetId(result);
            //SetLine(result, n);
            return result;
        }

        public Vertex ConstructorDeclaration(ConstructorDeclaration n)
        {
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.INIT, label, n);
            SetId(result);
            //SetLine(result, n);
            return result;
        }

        public Vertex MethodDeclaration(MethodDeclaration n)
        {
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.ENTRY, label, n);
            SetId(result);
            return result;
        }

        public Vertex Parameter(ParameterDeclaration n)
        {
            string label = n.Name.ToString();
            Vertex result = new Vertex(VertexType.FORMAL_IN, label, n);
            SetId(result);
            //SetLine(result, n);
            //SetDef(n, result);
            return result;
        }

        public Vertex FormalOut()
        {
            Vertex result = new Vertex(VertexType.FORMAL_OUT, "");
            SetId(result);
            return result;
        }

        public Vertex ActualOut(AstNode n)
        {
            Vertex result = new Vertex(VertexType.ACTUAL_OUT, n.ToString());
            SetId(result);
            return result;
        }

        public Vertex ArrayIdx(AstNode n)
        {
            Vertex result = new Vertex(VertexType.ARRAY_IDX, n.ToString(), n);
            SetId(result);
            return result;
        }

        public Vertex IfStmt(IfElseStatement n)
        {
            Expression cond = n.Condition;
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            return result;
        }

        public Vertex ForStmt(ForStatement n)
        {
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            return result;
        }

        public Vertex ForEachStmt(ForeachStatement n)
        {
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            return result;
        }

        public Vertex WhileStmt(WhileStatement n)
        {
            Expression cond = n.Condition;
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            return result;
        }

        public Vertex DoWhileStmt(DoWhileStatement n)
        {
            Expression cond = n.Condition;
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            return result;
        }

        public Vertex VariableDeclarator(VariableDeclarationStatement n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.ASSIGN, label, n);
            SetId(result);
            return result;
        }

        public Vertex AssignExpr(AssignmentExpression n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.ASSIGN, label, n);
            SetId(result);
            return result;
        }

        //not right for now
        public Vertex ReturnStmt(ReturnStatement n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.RETURN, label, n);
            SetId(result);
            return result;
        }

        public Vertex BreakStmt(BreakStatement n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.BREAK, label, n);
            SetId(result);
            return result;
        }

        public Vertex ContinueStmt(ContinueStatement n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.CONTINUE, label, n);
            SetId(result);
            return result;
        }

        public Vertex ThrowStmt(ThrowStatement n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.THROW, label, n);
            SetId(result);
            return result;
        }

        //public Vertex tryStmt(TryStatement n)
        //{
        //    final String label = "try";
        //    final Vertex result = new Vertex(VertexType.TRY, label, n);
        //    setId(result);
        //    setLine(result, n);
        //    return result;
        //}

        public static HashSet<string> Names(AstNode ast)
        {
            if (ast == null)
            {
                return new HashSet<string>();
            }
            return new HashSet<string>(ast.DescendantNodesAndSelf().OfType<IdentifierExpression>().Select(n => n.Identifier));
        }

        public void SetId(Vertex v)
        {
            v.SetId(id++);
        }

        public long GetId()
        {
            return id;
        }

        public void SetId(long id)
        {
            this.id = id;
        }

        public string returnLabel(string n)
        {
            return $"{n.Split('\n')[0].Replace('{', ' ')}";
        }
    }
}
