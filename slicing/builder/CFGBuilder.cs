using slicing.graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slicing.builder
{
    class CFGBuilder
    {
        // Adds unreachable components to the CFG
        private static readonly bool WITH_UNREACHABLE_COMPONENTS = true;

        // Mapping from methods to CFGs
        private readonly Dictionary<Vertex, CFG> m;

        // CFG under construction
        private CFG cfg;

        // Exit vertex used to break flow (return, break, continue)
        public static readonly Vertex EXIT = new Vertex("EXIT");

        // Used to add continue edges.
        private readonly List<Action> deferred;

        public CFGBuilder()
        {
            m = new Dictionary<Vertex, CFG>();
            cfg = new CFG();
            deferred = new List<Action>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="parameters"></param>
        /// <param name="bodyFlow"></param>
        /// <returns></returns>
        public ControlFlow MethodDeclaration(Vertex v, List<ControlFlow> parameters, ControlFlow bodyFlow)
        {
            ControlFlow paramFlow = Seq(parameters);
            ControlFlow result = Connect(Connect(v, paramFlow), bodyFlow);
            return new ControlFlow(v, result.GetOut());
        }

        public ControlFlow WhileStmt(Vertex v, ControlFlow bodyFlow)
        {
            ControlFlow conn1 = Connect(v, bodyFlow);
            ControlFlow conn2 = Connect(conn1, v);
            ControlFlow result = new ControlFlow(v, v);
            foreach (Vertex bv in conn2.GetBreaks())
            {
                result.GetOut().Add(bv);
            }
            return result;
        }

        public ControlFlow DoStmt(Vertex v, ControlFlow bodyFlow)
        {
            if (bodyFlow != null && bodyFlow.GetOut().Count == 1 && bodyFlow.GetOut().Contains(EXIT))
            {
                ControlFlow result = new ControlFlow(bodyFlow.GetIn(), new HashSet<Vertex>());
                foreach (Vertex bv in bodyFlow.GetBreaks())
                {
                    result.GetOut().Add(bv);
                }
                if (WITH_UNREACHABLE_COMPONENTS)
                {
                    AddVertex(v);
                }
                return result;
            }
            else
            {
                ControlFlow conn1 = Connect(bodyFlow, v);
                ControlFlow conn2 = Connect(v, conn1);
                ControlFlow result = new ControlFlow(conn1.GetIn(), conn2.GetOut());
                foreach (Vertex bv in conn2.GetBreaks())
                {
                    result.GetOut().Add(bv);
                }
                return result;
            }
        }

        public ControlFlow ForStmt(Vertex v, List<ControlFlow> init, List<ControlFlow> update, ControlFlow bodyFlow)
        {
            ControlFlow initFlow = Seq(init);
            ControlFlow updateFlow = Seq(update);
            ControlFlow conn1 = Connect(initFlow, v);
            ControlFlow conn2 = Connect(conn1, bodyFlow);
            ControlFlow conn3 = Connect(conn2, updateFlow);
            ControlFlow result = Connect(conn3, v);
            foreach (Vertex bv in result.GetBreaks())
            {
                result.GetOut().Add(bv);
            }
            return result;
        }

        public ControlFlow ForeachStmt(Vertex v, ControlFlow bodyFlow)
        {
            ControlFlow conn1 = Connect(v, bodyFlow);
            ControlFlow result = Connect(conn1, v);
            foreach (Vertex bv in result.GetBreaks())
            {
                result.GetOut().Add(bv);
            }
            return result;
        }

        public ControlFlow IfStmt(Vertex v, ControlFlow thenFlow, ControlFlow elseFlow)
        {
            HashSet<Vertex> outSet = new HashSet<Vertex>();
            ControlFlow conn1 = Connect(v, thenFlow);
            ControlFlow conn2 = Connect(v, elseFlow);
            outSet.UnionWith(conn1.GetOut());
            outSet.UnionWith(conn2.GetOut());
            ControlFlow result = new ControlFlow(v, outSet);
            result.GetBreaks().UnionWith(conn1.GetBreaks());
            result.GetBreaks().UnionWith(conn2.GetBreaks());
            return result;
        }

        public ControlFlow ContinueStmt(Vertex v, Vertex loop)
        {
            deferred.Add(() => {
                AddEdge(v, loop);
            });
            AddVertex(v);
            return new ControlFlow(v, CFGBuilder.EXIT);
        }

        public ControlFlow Seq(params ControlFlow[] seq)
        {
            return Seq(seq.ToList());
        }

        public ControlFlow Seq(List<ControlFlow> seq)
        {
            ControlFlow result = null;
            for (int i = 0; i < seq.Count; i++)
            {
                if (i == 0)
                    result = seq[0];
                else
                {
                    ControlFlow next = seq[i];
                    // If we are leaving the sequence (return, break, continue), the subsequent
                    // nodes will be unreachable (unless it is the final exit node in the CFG).
                    if (result != null && result.GetOut().Count == 1 && result.GetOut().Contains(EXIT))
                    {
                        if (VertexType.EXIT.Equals(next.GetIn().GetType()))
                            result = Connect(result, next);
                        else
                        {
                            HandleFlowBreak(seq, next, i);
                            break;
                        }
                    }
                    else
                        result = Connect(result, next);
                }
            }
            return result;
        }

        private void HandleFlowBreak(List<ControlFlow> seq, ControlFlow next, int i)
        {
            if (WITH_UNREACHABLE_COMPONENTS)
            {
                Console.WriteLine("Unreachable code detected.");
                ControlFlow current = next;
                if (i + 1 < seq.Count)
                {
                    for (int j = i + 1; j < seq.Count; j++)
                    {
                        current = Connect(current, seq[j], true);
                    }
                }
                else
                {
                    cfg.AddVertexRange(current.GetOut());
                }
            }
        }

        private ControlFlow Connect(ControlFlow f, Vertex v)
        {
            ControlFlow fv = new ControlFlow(v, v);
            return Connect(f, fv);
        }

        private ControlFlow Connect(ControlFlow f1, ControlFlow f2)
        {
            return Connect(f1, f2, false);
        }

        private ControlFlow Connect(Vertex v, ControlFlow f)
        {
            ControlFlow fv = new ControlFlow(v, v);
            return Connect(fv, f);
        }

        private ControlFlow Connect(ControlFlow f1, ControlFlow f2, bool withUnreachableComponents)
        {
            if (f1 == null)
                return f2;
            if (f2 == null)
                return f1;
            foreach (Vertex o in f1.GetOut())
            {
                if (o == EXIT)
                {
                    if (withUnreachableComponents)
                        AddVertex(f2.GetIn());
                    continue;
                }
                AddVertex(o);
                if (VertexType.BREAK.Equals(o.GetType()))
                    f1.GetBreaks().Remove(o);
                Vertex i = f2.GetIn();
                AddVertex(i);
                AddEdge(o, i);
            }
            ControlFlow result = new ControlFlow(f1.GetIn(), f2.GetOut());
            result.GetBreaks().UnionWith(f1.GetBreaks());
            result.GetBreaks().UnionWith(f2.GetBreaks());
            return result;
        }

        public void AddVertex(Vertex v)
        {
            cfg.AddVertex(v);
        }

        public void AddEdge(Vertex source, Vertex target)
        {
            cfg.AddEdge(new Edge(source, target, EdgeType.CTRL_TRUE));
        }


        private void Deferred()
        {
            foreach (var action in deferred)
                action();
            deferred.Clear();
        }

        public void Put(Vertex k)
        {
            Deferred();
            m[k] = cfg;
            cfg = new CFG();
        }
    }
}
