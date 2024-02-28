using ICSharpCode.Decompiler.CSharp.Syntax;
using slicing.graph;
using System;
using System.Collections.Generic;
using System.Linq;

/*
 * Builds the Control Dependence Subgraph
 */
namespace slicing.builder
{
    class CDGBuilder
    {
        private Dictionary<string, int> unmatchedAstNodes;
        private readonly SyntaxTree syntaxTree;
        private VertexCreator vtxCreator;
        private CFGBuilder cfgBuilder;

        private Stack<string> clsStack;
        private Dictionary<string, KeyValuePair<Vertex, List<Vertex>>> methodParams;
        private Dictionary<string, HashSet<KeyValuePair<Vertex, List<Vertex>>>> calls;
        private Dictionary<Vertex, Vertex> methodFormalOut;
        private PDG cdg;
        private Stack<List<Vertex>> inScopeStack;
        private List<Vertex> inScope;
        private Stack<Vertex> loopStack;
        private Stack<Vertex> formalOutStack;

        public CDGBuilder(SyntaxTree syntaxTree)
        {
            this.syntaxTree = syntaxTree;
        }

        public void Build()
        {
            unmatchedAstNodes = new Dictionary<string, int>();
            vtxCreator = new VertexCreator();
            vtxCreator.SetId(1);
            cfgBuilder = new CFGBuilder();
            cdg = new PDG();
            clsStack = new Stack<string>();
            inScopeStack = new Stack<List<Vertex>>();
            inScope = new List<Vertex>();
            loopStack = new Stack<Vertex>();
            formalOutStack = new Stack<Vertex>();
            methodParams = new Dictionary<string, KeyValuePair<Vertex, List<Vertex>>>();
            calls = new Dictionary<string, HashSet<KeyValuePair<Vertex, List<Vertex>>>>();
            methodFormalOut = new Dictionary<Vertex, Vertex>();
            var root = syntaxTree;
            foreach (var member in root.Members)
            {
                _Build(member);
            }
        }

        private ControlFlow _Build(AstNode n)
        {
            ControlFlow result = null;

            switch (n)
            {
                case NamespaceDeclaration namespaceDeclaration:
                    HandleNamespaceDeclaration(namespaceDeclaration);
                    break;
                case TypeDeclaration typeDeclaration:
                    result = HandleTypeDeclaration(typeDeclaration);
                    break;
                case ConstructorDeclaration constructorDeclaration:
                    result = HandleConstructorDeclaration(constructorDeclaration);
                    break;
                case MethodDeclaration methodDeclaration:
                    result = HandleMethodDeclaration(methodDeclaration);
                    break;
                case BlockStatement blockStatement:
                    result = HandleBlockStatement(blockStatement);
                    break;
                case ParameterDeclaration parameterDeclaration:
                    result = HandleParameterDeclaration(parameterDeclaration);
                    break;
                case IfElseStatement ifElseStatement:
                    result = HandleIfElseStatement(ifElseStatement);
                    break;
                case ForeachStatement foreachStatement:
                    result = HandleForeachStatement(foreachStatement);
                    break;
                case ForStatement forStatement:
                    //result = HandleForStatement(forStatement);
                    break;
                case WhileStatement whileStatement:
                    result = HandleWhileStatement(whileStatement);
                    break;
                case DoWhileStatement doWhileStatement:
                    result = HandleDoWhileStatement(doWhileStatement);
                    break;
                case ExpressionStatement expressionStatement:
                    result = HandleExpressionStatement(expressionStatement);
                    break;
                case ThrowStatement throwStatement:
                    result = HandleThrowStatement(throwStatement);
                    break;
                case VariableDeclarationStatement variableDeclaration:
                    result = HandleVariableDeclarationStatement(variableDeclaration);
                    break;
                case VariableInitializer variableInitializer:

                    break;
                case AssignmentExpression assignmentExpression:
                    result = HandleAssignmentExpression(assignmentExpression);
                    break;
                default:
                    LogUnmatched(n);
                    break;

            }


            return result;
        }

        private void LogUnmatched(AstNode n)
        {
            string nodeName = n.GetType().Name;
            Console.WriteLine($"No match for {nodeName}\n\t{n.ToString()}");

            if (!unmatchedAstNodes.ContainsKey(nodeName))
            {
                unmatchedAstNodes[nodeName] = 0;
            }

            unmatchedAstNodes[nodeName]++;
        }

        private ControlFlow HandleBlockStatement(BlockStatement n)
        {
            List<ControlFlow> flow = new List<ControlFlow>();
            foreach (Statement s in n.Statements)
            {
                Console.WriteLine($"\tin block");
                flow.Add(_Build(s));
            }
            return cfgBuilder.Seq(flow);
        }

        private ControlFlow HandleVariableDeclarationStatement(VariableDeclarationStatement n)
        {
            List<ControlFlow> flow = new List<ControlFlow>();
            foreach (var v in n.Variables)
            {
                flow.Add(_Build(v));
            }
            return cfgBuilder.Seq(flow);
        }

        private ControlFlow HandleVariableInitializer(VariableInitializer n)
        {

        }

        private ControlFlow HandleExpressionStatement(ExpressionStatement n)
        {
            Expression expr = n.Expression;
            return _Build(expr);
        }

        private void HandleNamespaceDeclaration(NamespaceDeclaration n)
        {
            foreach (var ast in n.Members)
            {
                _Build(ast);
            }
        }

        private ControlFlow HandleTypeDeclaration(TypeDeclaration n)
        {
            clsStack.Push(n.Name.ToString());
            Console.WriteLine($"\tclsStack push :{n.Name.ToString()}");
            Vertex v = vtxCreator.TypeDeclaration(n);
            cdg.AddVertex(v);

            // Constructors
            var constructors = n.Members.OfType<ConstructorDeclaration>();
            foreach (ConstructorDeclaration c in constructors)
            {
                _Build(c);
            }

            // Methods
            var methods = n.Members.OfType<MethodDeclaration>();
            foreach (MethodDeclaration m in methods)
            {
                _Build(m);
            }

            // Classes
            var classes = n.Members.OfType<TypeDeclaration>().Where(t => t != n);
            foreach (TypeDeclaration c in classes)
            {
                _Build(c);
            }

            AddEdges(EdgeType.MEMBER_OF, v, inScope);
            clsStack.Pop();
            return null;
        }

        private ControlFlow HandleConstructorDeclaration(ConstructorDeclaration n)
        {
            Vertex v = vtxCreator.ConstructorDeclaration(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            Console.WriteLine($"\tinScope add v contructor");
            PushScope();
            var parameters = n.Parameters.ToList();
            List<ControlFlow> paramFlow = Params(parameters, v, n.Name);
            ControlFlow bodyFlow = _Build(n.Body);
            AddEdges(EdgeType.CTRL_TRUE, v, inScope);
            PopScope();
            ControlFlow result = cfgBuilder.MethodDeclaration(v, paramFlow, bodyFlow);
            cfgBuilder.Put(v);
            return result;
        }

        private ControlFlow HandleParameterDeclaration(ParameterDeclaration n)
        {
            Vertex v = vtxCreator.Parameter(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            Console.WriteLine($"\tinScope add v ParameterDeclaration");
            ControlFlow flow = new ControlFlow(v, v);
            Console.WriteLine($"\t{flow.ToString()}");
            return flow;
        }

        //private ControlFlow ExplicitConstructorInvocationStmt(ConstructorInitializer n)
        //{
        //    Vertex v = vtxCreator.ExplicitConstructorInvocationStmt(n);
        //    cdg.AddVertex(v);
        //    inScope.Add(v);
        //    return Args(v, n);
        //}

        private ControlFlow HandleMethodDeclaration(MethodDeclaration n)
        {
            Vertex v = vtxCreator.MethodDeclaration(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            PushScope();
            Vertex outVar = FormalOut(n);
            if (outVar != null)
            {
                formalOutStack.Push(outVar);
                methodFormalOut[v] = outVar;
            }
            var parameters = n.Parameters.ToList();
            List<ControlFlow> paramFlow = Params(parameters, v, n.Name);
            BlockStatement body = n.Body;
            ControlFlow bodyFlow = null;
            if (body != null)
                bodyFlow = _Build(body);
            AddEdges(EdgeType.CTRL_TRUE, v, inScope);
            PopScope();

            // CFG
            Vertex exit = vtxCreator.Exit();
            ControlFlow exitFlow = new ControlFlow(exit, exit);
            bodyFlow = cfgBuilder.Seq(bodyFlow, exitFlow);
            ControlFlow result = cfgBuilder.MethodDeclaration(v, paramFlow, bodyFlow);
            cfgBuilder.Put(v);
            if (outVar != null)
                formalOutStack.Pop();
            return result;
        }

        /// <summary>
        /// not done yet
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private ControlFlow HandleIfElseStatement(IfElseStatement n)
        {
            Vertex v = vtxCreator.IfStmt(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            PushScope();

            // if-then branch
            Statement thenStmt = n.TrueStatement;
            ControlFlow thenFlow = _Build(thenStmt);
            AddEdges(EdgeType.CTRL_TRUE, v, inScope);

            // reset scope
            ClearScope();

            // else branch
            Statement elseStmt = n.FalseStatement;
            ControlFlow elseFlow = null;
            if (elseStmt is BlockStatement || elseStmt is IfElseStatement)
            {
                elseFlow = _Build(elseStmt);
                AddEdges(EdgeType.CTRL_FALSE, v, inScope);
            }

            PopScope();

            ControlFlow result = cfgBuilder.IfStmt(v, thenFlow, elseFlow);
            return result;
        }

        private void HandleForStatement(ForStatement n)
        {
            ///????
            List<Statement> init = n.Initializers.ToList();


        }

        private ControlFlow HandleForeachStatement(ForeachStatement n)
        {
            Vertex v = vtxCreator.ForEachStmt(n);
            cdg.AddVertex(v);
            loopStack.Push(v);
            inScope.Add(v);
            PushScope();

            Statement body = n.EmbeddedStatement;
            ControlFlow bodyFlow = _Build(body);

            AddEdges(EdgeType.CTRL_TRUE, v, inScope);
            // Self edge
            AddEdge(EdgeType.CTRL_TRUE, v, v);

            loopStack.Pop();
            PopScope();

            ControlFlow result = cfgBuilder.ForeachStmt(v, bodyFlow);
            return result;
        }

        private ControlFlow HandleWhileStatement(WhileStatement n)
        {
            Vertex v = vtxCreator.WhileStmt(n);
            cdg.AddVertex(v);
            loopStack.Push(v);
            inScope.Add(v);
            PushScope();

            Statement body = n.EmbeddedStatement;
            ControlFlow bodyFlow = _Build(body);
            AddEdges(EdgeType.CTRL_TRUE, v, inScope);
            //self edge
            AddEdge(EdgeType.CTRL_TRUE, v, v);
            loopStack.Pop();
            PopScope();
            ControlFlow result = cfgBuilder.WhileStmt(v, bodyFlow);
            return result;
        }

        private ControlFlow HandleDoWhileStatement(DoWhileStatement n)
        {
            Vertex v = vtxCreator.DoWhileStmt(n);
            cdg.AddVertex(v);
            loopStack.Push(v);
            inScope.Add(v);
            PushScope();
            Statement body = n.EmbeddedStatement;
            ControlFlow bodyFlow = _Build(body);
            AddEdges(EdgeType.CTRL_TRUE, v, inScope);
            AddEdge(EdgeType.CTRL_TRUE, v, v);
            loopStack.Pop();
            List<Vertex> oldScope = new List<Vertex>(inScope);
            PopScope();
            // Restore old scope so that edges from the outer control vertex are created
            inScope.AddRange(oldScope);
            ControlFlow result = cfgBuilder.DoStmt(v, bodyFlow);
            return result;
        }

        private ControlFlow HandleThrowStatement(ThrowStatement n)
        {
            Vertex v = vtxCreator.ThrowStmt(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            ControlFlow result = new ControlFlow(v, CFGBuilder.EXIT);
            return result;
        }

        /// <summary>
        /// god, not done yet
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private ControlFlow HandleAssignmentExpression(AssignmentExpression n)
        {
            Vertex v = vtxCreator.AssignExpr(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            ControlFlow result = new ControlFlow(v, v);

            Expression target = n.Left;
            if (target is IndexerExpression indexer)
            {
                var ind = indexer.Arguments;
                //v.GetUses().UnionWith(VertexCreator.Names(ind));
            }

            //check for call
            Expression value = n.Right;
            if (value is MemberReferenceExpression memberReference)
            {

            }
            return result;
        }


        private Vertex FormalOut(MethodDeclaration n)
        {
            if ("void".Equals(n.ReturnType))
                return null;
            Vertex v = vtxCreator.FormalOut();
            cdg.AddVertex(v);
            inScope.Add(v);
            return v;
        }

        private ControlFlow ActualOut(Vertex v, AstNode n)
        {
            Vertex a = vtxCreator.ActualOut(n);
            cdg.AddVertex(a);
            AddEdge(EdgeType.CTRL_TRUE, v, a);
            return new ControlFlow(a, a);
        }

        private ControlFlow ArrayIdx(Vertex v, AstNode n)
        {
            Vertex a = vtxCreator.ArrayIdx(n);
            cdg.AddVertex(a);
            AddEdge(EdgeType.CTRL_TRUE, v, a);
            return new ControlFlow(a, a);
        }

        private List<ControlFlow> Params(List<ParameterDeclaration> parameters, Vertex v, string name)
        {
            List<ControlFlow> result = new List<ControlFlow>();
            List<Vertex> paramVtcs = new List<Vertex>();

            foreach (ParameterDeclaration parameter in parameters)
            {
                ControlFlow f = _Build(parameter);
                result.Add(f);
                Vertex paramVtx = f.GetIn();
                paramVtcs.Add(paramVtx);
            }

            string methodName = CallName(name);
            methodParams[methodName] = new KeyValuePair<Vertex, List<Vertex>>(v, paramVtcs);

            return result;
        }

        private void AddEdge(EdgeType type, Vertex source, Vertex target)
        {
            cdg.AddEdge(new Edge(source, target, type));
        }
        private void AddEdges(EdgeType type, Vertex source, List<Vertex> target)
        {
            foreach (Vertex v in target)
            {
                AddEdge(type, source, v);
            }
        }

        private void PushScope()
        {
            inScopeStack.Push(inScope);
            ClearScope();
        }

        private void ClearScope()
        {
            inScope = new List<Vertex>();
        }

        private void PopScope()
        {
            inScope = inScopeStack.Pop();
        }

        private string CallName(string name)
        {
            return clsStack.Peek() + "." + name;
        }

        private string CallName(string name, Expression scope)
        {
            string result = clsStack.Peek() + ".";
            if (scope != null)
                result = scope.ToString() + ".";
            result += name;
            return result;
        }
    }
}
