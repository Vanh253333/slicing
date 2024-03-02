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

        /// <summary>
        /// class, ..
        /// setLine??
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex TypeDeclaration(TypeDeclaration n)
        {
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CLASS, label, n);
            SetId(result);
            //SetLine(result, n);
            return result;
        }

        /// <summary>
        /// setline??
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex ConstructorDeclaration(ConstructorDeclaration n)
        {
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.INIT, label, n);
            SetId(result);
            //SetLine(result, n);
            return result;
        }

        /// <summary>
        /// setline??
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex MethodDeclaration(MethodDeclaration n)
        {
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.ENTRY, label, n);
            SetId(result);
            return result;
        }

        /// <summary>
        /// setline
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex Parameter(ParameterDeclaration n)
        {
            string label = n.Name.ToString();
            Vertex result = new Vertex(VertexType.FORMAL_IN, label, n);
            SetId(result);
            //SetLine(result, n);
            SetDef(n, result);
            return result;
        }

        public Vertex FormalOut()
        {
            Vertex result = new Vertex(VertexType.FORMAL_OUT, "");
            SetId(result);
            return result;
        }

        /// <summary>
        /// setline
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex ActualOut(AstNode n)
        {
            Vertex result = new Vertex(VertexType.ACTUAL_OUT, n.ToString());
            SetId(result);
            SetDef(n, result);
            // Uses are set for array accesses in assignments.
            SetUses(n, result);
            result.GetUses().RemoveWhere(u => u == result.GetDef());
            return result;
        }

        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex ArrayIdx(AstNode n)
        {
            Vertex result = new Vertex(VertexType.ARRAY_IDX, n.ToString(), n);
            SetId(result);
            SetUses(n, result);
            
            return result;
        }

        /// <summary>
        /// setline, setsubtype
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex IfStmt(IfElseStatement n)
        {
            Expression cond = n.Condition;
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            SetUses(n, result);
            return result;
        }

        /// <summary>
        /// vertex => condition?
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex ForStmt(ForStatement n)
        {
            Expression condition = n.Condition;
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            if (condition != null)
            {
                SetUses(condition, result);
                //set subtype
            }
            return result;
        }

        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex ForEachStmt(ForeachStatement n)
        {
            var it = n.VariableDesignation;
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            SetUses(it, result);
            return result;
        }

        /// <summary>
        /// setline, setsubtype
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex WhileStmt(WhileStatement n)
        {
            Expression cond = n.Condition;
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            SetUses(cond, result);
            return result;
        }

        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex DoWhileStmt(DoWhileStatement n)
        {
            Expression cond = n.Condition;
            string label = returnLabel(n.ToString());
            Vertex result = new Vertex(VertexType.CTRL, label, n);
            SetId(result);
            SetUses(cond, result);
            return result;
        }

        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex VariableDeclarator(VariableDeclarationStatement n)
        {
            string label = n.ToString();
            Expression init = n.Variables.First().Initializer;
            Vertex result = new Vertex(VertexType.ASSIGN, label, n);
            SetId(result);
            SetDef(n, result);
            if(init != null)
            {
                SetUses(init, result);
            }
            return result;
        }


        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex AssignExpr(AssignmentExpression n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.ASSIGN, label, n);
            SetId(result);
            SetDef(n.Left, result);
            SetUses(n.Right, result);
            SetPseudoUse(n.Left, result);

            return result;
        }

        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex InvocationExpr(InvocationExpression n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.CALL, label, n);
            SetId(result);
            SetUses(n, result);
            Expression scope = n.Target;
            if(scope != null)
            {
                SetDef(scope, result);
            }
            return result;
        }

        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex ArgumentExpr(AstNode n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.ACTUAL_IN, label, n);
            SetId(result);
            SetUses(n, result);

            return result;
        }

        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex UnaryExpr(UnaryOperatorExpression n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.ACTUAL_IN, label, n);
            SetId(result);
            SetUses(n, result);
            SetPseudoUse(n, result);
            SetDef(n, result);
            return result;
        }

        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex ReturnStmt(ReturnStatement n)
        {
            Expression expr = n.Expression;
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.RETURN, label, n);
            SetId(result);
            if(expr != null)
            {
                SetUses(expr, result);
                //setsubtype 
            }
            return result;
        }

        /// <summary>
        /// setline
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex BreakStmt(BreakStatement n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.BREAK, label, n);
            SetId(result);
            return result;
        }

        /// <summary>
        /// setline
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex ContinueStmt(ContinueStatement n)
        {
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.CONTINUE, label, n);
            SetId(result);
            return result;
        }

        /// <summary>
        /// setline, setsubtype 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Vertex ThrowStmt(ThrowStatement n)
        {
            Expression expr = n.Expression;
            string label = n.ToString();
            Vertex result = new Vertex(VertexType.THROW, label, n);
            SetId(result);
            SetUses(n, result);
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
                return new HashSet<string>();
            return new HashSet<string>(
                ast.DescendantsAndSelf.OfType<Identifier>().Select(n => n.Name)
            );
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

        private void SetUses(AstNode n, Vertex v)
        {
            HashSet<string> uses = Names(n);
            v.SetUses(uses);
        }

        private void SetPseudoUse(AstNode n, Vertex v)
        {
            string pseudoUse = Names(n).FirstOrDefault();
            v.SetPseudoUse(pseudoUse);
        }

        private void SetDef(AstNode n, Vertex v)
        {
            string def = Names(n).FirstOrDefault();
            if (n is IndexerExpression idx)
            {
                def = idx.Target.ToString();
            }
            v.SetDef(def);
        }

        public string returnLabel(string n)
        {
            return $"{n.Split('\n')[0].Replace('{', ' ')}";
        }
    }
}
