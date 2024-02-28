using QuickGraph;
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
            return Vertices.FirstOrDefault(v => v.GetType() == VertexType.ENTRY || v.GetType() == VertexType.INIT);
        }

        public Vertex GetExit()
        {
            return Vertices.OrderByDescending(v => v.GetId()).FirstOrDefault();
        }

        public Vertex GetVertexWithId(int id)
        {
            return Vertices.FirstOrDefault(v => v.GetId() == id);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public int CyclomaticComplexity()
        {
            return EdgeCount - VertexCount + 2;
        }
    }

}
