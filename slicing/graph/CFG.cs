using QuickGraph;
using System.Collections.Generic;
using System.Linq;

namespace slicing.graph
{
    class CFG : BidirectionalGraph<Vertex, Edge>
    {
        public CFG() : base(allowParallelEdges: true)
        {
        }

        public Vertex GetEntry()
        {
            return Vertices.FirstOrDefault(v => v.GetTypeVertex() == VertexType.ENTRY || v.GetTypeVertex() == VertexType.INIT);
        }

        public Vertex GetExit()
        {
            List<Vertex> vtcs = new List<Vertex>(Vertices);
            vtcs.Sort((v1, v2) => v2.GetId().CompareTo(v1.GetId()));
            return vtcs[0];
        }

        public Vertex GetVertexWithId(int id)
        {
            return Vertices.FirstOrDefault(v => v.GetId() == id);
        }

        public int CyclomaticComplexity()
        {
            return EdgeCount - VertexCount + 2;
        }
    }

}
