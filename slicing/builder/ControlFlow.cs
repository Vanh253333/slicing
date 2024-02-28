using slicing.graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slicing.builder
{
    /* 
     * Model control flow between statements
     */
    class ControlFlow
    {
        private readonly Vertex inVertex;
        private readonly HashSet<Vertex> outVertices;
        private readonly HashSet<Vertex> breaks;

        public ControlFlow()
        {
            inVertex = null;
            outVertices = new HashSet<Vertex>();
            breaks = new HashSet<Vertex>();
        }

        public ControlFlow(Vertex inVertex, Vertex outVertex)
        {
            this.inVertex = inVertex;
            outVertices = new HashSet<Vertex>();
            breaks = new HashSet<Vertex>();
            outVertices.Add(outVertex);
        }

        public ControlFlow(Vertex inVertex, HashSet<Vertex> outVertices)
        {
            this.inVertex = inVertex;
            this.outVertices = new HashSet<Vertex>(outVertices);
            breaks = new HashSet<Vertex>();
        }

        public Vertex GetIn()
        {
            return inVertex;
        }

        public HashSet<Vertex> GetOut()
        {
            return outVertices;
        }

        public HashSet<Vertex> GetBreaks()
        {
            return breaks;
        }

        public override string ToString()
        {
            return string.Format("<{0}, {1}>", inVertex, outVertices);
        }
    }
}
