using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slicing.graph
{
    class PDG : BidirectionalGraph<Vertex, Edge>
    {
        private static HashSet<VertexType> dontCareTypes;
        static PDG()
        {
            dontCareTypes = new HashSet<VertexType>();
            dontCareTypes.Add(VertexType.ACTUAL_IN);
            dontCareTypes.Add(VertexType.ACTUAL_OUT);
            dontCareTypes.Add(VertexType.ARRAY_IDX);
            dontCareTypes.Add(VertexType.CLASS);
            dontCareTypes.Add(VertexType.FORMAL_IN);
            dontCareTypes.Add(VertexType.FORMAL_OUT);
            dontCareTypes.Add(VertexType.ENTRY);
            dontCareTypes.Add(VertexType.INIT);
            dontCareTypes.Add(VertexType.TRY);
            dontCareTypes.Add(VertexType.CATCH);
            dontCareTypes.Add(VertexType.FINALLY);
        }

        private string pathToProgram;

        public PDG()
        {
        }


    }
}
