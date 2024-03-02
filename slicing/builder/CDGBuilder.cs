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

            cdg.PrintGraph();
        }

        private ControlFlow _Build(AstNode n)
        {
            ControlFlow result = null;

            switch (n)
            {
                case NamespaceDeclaration namespaceDeclaration:
                    HandleNamespaceDeclaration(namespaceDeclaration);
                    break;
                case AttributeSection attribute:
                case UsingDeclaration usingDeclaration:
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
                    result = HandleForStatement(forStatement);
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
                case InvocationExpression invocationExpression:
                    result = HandleInvocationExpression(invocationExpression);
                    break;
                case AssignmentExpression assignmentExpression:
                    result = HandleAssignmentExpression(assignmentExpression);
                    break;
                case ReturnStatement returnStatement:
                    result = HandleReturnStatement(returnStatement);
                    break;
                case BreakStatement breakStatement:
                    result = HandleBreakStatement(breakStatement);
                    break;
                case ContinueStatement continueStatement:
                    result = HandleContinueStatement(continueStatement);
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
                flow.Add(_Build(s));
            }
            return cfgBuilder.Seq(flow);
        }

        /// <summary>
        /// VariableDeclarationStatement.Variables chỉ có 1 ?? 
        /// IndexerExpression not done yet, mảng đa chiều
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private ControlFlow HandleVariableDeclarationStatement(VariableDeclarationStatement n)
        {
            Vertex v = vtxCreator.VariableDeclarator(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            ControlFlow result = new ControlFlow(v, v);

            //check for call
            Expression init = n.Variables.First().Initializer;
            if (init is InvocationExpression call)
            {
                result = DelegatedMethodCall(call, v, n);
            }
            else if (init is IndexerExpression indexer)
            {
                Console.WriteLine("sorry IndexerExpression not finshed yet");
            }
            else if (init is ArrayCreateExpression)
            {
                Console.WriteLine("sorry ArrayCreateExpression not finshed yet");
            }
            else
            {
                Console.WriteLine($"Variable Declaration that not handle yet: {init.GetType().Name}");
            }

            return result;
        }

        /// <summary>
        /// god, not done yet, same as Variable 
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
                //var ind = indexer.Arguments;
                v.GetUses().UnionWith(VertexCreator.Names(indexer));
            }

            //check for call
            Expression value = n.Right;
            if (value is InvocationExpression call)
            {
                result = DelegatedMethodCall(call, v, n);
            }
            else if (value is IndexerExpression ind)
            {
                Console.WriteLine("sorry IndexerExpression not finshed yet");
            }
            else
            {
                Console.WriteLine($"Assignment that not handle yet: {value.GetType().Name}");
            }
            return result;
        }


        private ControlFlow DelegatedMethodCall(InvocationExpression call, Vertex v, AstNode n)
        {
            // Def and uses are set in the corresponding ACTUAL_OUT and ACTUAL_IN vertices
            v.ClearDefUses();
            // Set uses w.r.t. invoked objects
            var scope = call.Target as IdentifierExpression;
            if (scope != null)
            {
                string scopeVar = scope.Identifier;
                var uses = new HashSet<string>();
                uses.Add(scopeVar);
                v.SetUses(uses);
            }
            var inFlow = Args(v, call);
            var outFlow = ActualOut(v, n);
            return cfgBuilder.Seq(inFlow, outFlow);
        }

        private ControlFlow HandleInvocationExpression(InvocationExpression n)
        {
            Vertex v = vtxCreator.InvocationExpr(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            return Args(v, n);
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
            //Console.WriteLine($"\tclsStack push :{n.Name.ToString()}");
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

            foreach(var mem in n.Members)
            {
                if(mem is ConstructorDeclaration ||mem is MethodDeclaration || mem is FieldDeclaration || mem is PropertyDeclaration)
                {
                }
                else
                {
                    Console.WriteLine($"TypeDeclaration not handle yet: {mem.GetType().Name}");
                    //FieldDeclaration, PropertyDeclaration, 
                }
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
            //Console.WriteLine($"\tinScope add v contructor");
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
            //Console.WriteLine($"\tinScope add v ParameterDeclaration");
            ControlFlow flow = new ControlFlow(v, v);
            //Console.WriteLine($"\t{flow.ToString()}");
            return flow;
        }

        /// <summary>
        /// finish
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
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
        /// finish
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

        /// <summary>
        /// finish
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private ControlFlow HandleForStatement(ForStatement n)
        {
            List<Statement> init = n.Initializers.ToList();
            List<ControlFlow> initFlow = new List<ControlFlow>();
            foreach (Statement e in init)
            {
                initFlow.Add(_Build(e));
            }

            Vertex v = vtxCreator.ForStmt(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            PushScope();
            List<Statement> update = n.Iterators.ToList();
            List<ControlFlow> updateFlow = new List<ControlFlow>();
            foreach (Statement e in update)
            {
                updateFlow.Add(_Build(e));
            }
            // Control flow after a continue should go to update node if present, and guard
            // otherwise.
            Vertex loopVtx;
            if (updateFlow != null && updateFlow.Count > 0 && updateFlow[0] != null)
            {
                loopVtx = updateFlow[0].GetIn();
            }
            else
            {
                loopVtx = v;
            }
            loopStack.Push(loopVtx);
            Statement body = n.EmbeddedStatement;
            ControlFlow bodyFlow = _Build(body);
            AddEdges(EdgeType.CTRL_TRUE, v, inScope);
            //self edge
            AddEdge(EdgeType.CTRL_TRUE, v, v);
            loopStack.Pop();
            PopScope();
            ControlFlow result = cfgBuilder.ForStmt(v, initFlow, updateFlow, bodyFlow);
            return result;
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

        /// <summary>
        /// finish
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
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

        /// <summary>
        /// finish
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
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

        private ControlFlow HandleReturnStatement(ReturnStatement n)
        {
            Vertex v = vtxCreator.ReturnStmt(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            if (n.Expression != null)
            {
                Vertex formalOut = formalOutStack.Peek();
                if(formalOut == null)
                {
                    throw new InvalidOperationException($"Is {n.ToString()} inside a method that returns void?");
                }
                else
                {
                    AddEdge(EdgeType.DATA, v, formalOut);
                }
            }
            return new ControlFlow(v, CFGBuilder.EXIT);
        }

        private ControlFlow HandleBreakStatement(BreakStatement n)
        {
            Vertex v = vtxCreator.BreakStmt(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            ControlFlow result = new ControlFlow(v, CFGBuilder.EXIT);
            result.GetBreaks().Add(v);
            return result;
        }

        private ControlFlow HandleContinueStatement(ContinueStatement n)
        {
            Vertex v = vtxCreator.ContinueStmt(n);
            cdg.AddVertex(v);
            inScope.Add(v);
            ControlFlow result = cfgBuilder.ContinueStmt(v, loopStack.Peek());
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

        private Vertex ArgumentExpr(AstNode e)
        {
            Vertex v = vtxCreator.ArgumentExpr(e);
            cdg.AddVertex(v);
            return v;
        }

        private ControlFlow Args(Vertex v, InvocationExpression n)
        {
            MemberReferenceExpression methodRef = n.Target as MemberReferenceExpression;
            string methodName = methodRef != null ? methodRef.MemberName : "";
            return Args(v, n.Arguments.ToList(), methodName, n.Target);
        }

        private ControlFlow Args(Vertex v, List<Expression> args, string name, AstNode scope)
        {
            var result = new List<ControlFlow>();
            var paramVertices = new List<Vertex>();
            foreach (var e in args)
            {
                var a = ArgumentExpr(e);

                // Remove uses in parent node.
                v.GetUses().ExceptWith(v.GetUses());
                AddEdge(EdgeType.CTRL_TRUE, v, a);
                result.Add(new ControlFlow(a, a));
                paramVertices.Add(a);
            }
            result.Add(new ControlFlow(v, v));
            var methodName = CallName(name, scope);
            PutCall(methodName, new KeyValuePair<Vertex, List<Vertex>>(v, paramVertices));
            return cfgBuilder.Seq(result);
        }

        private void PutCall(string method, KeyValuePair<Vertex, List<Vertex>> pair)
        {
            if (!calls.ContainsKey(method))
            {
                calls[method] = new HashSet<KeyValuePair<Vertex, List<Vertex>>>();
            }
            calls[method].Add(pair);
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
                //Console.WriteLine($"\t{parameter.ToString()}");
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

        private string CallName(string name, AstNode scope)
        {
            string result = clsStack.Peek() + ".";
            if (scope != null)
                result = scope.ToString() + ".";
            result += name;
            return result;
        }
    }
}
