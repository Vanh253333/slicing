﻿using QuickGraph;
using slicing.graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slicing
{
    public enum EdgeType
    {
        DATA, OUTPUT, CTRL_TRUE, CTRL_FALSE, CALL, PARAM_IN, PARAM_OUT, MEMBER_OF
    }
    class Edge : IEdge<Vertex>
    {
        private Vertex source;
        private Vertex target;
        private EdgeType type;

        public Edge()
        {
        }
        public Edge(Vertex source, Vertex target)
        {
            this.source = source;
            this.target = target;
        }

        public Edge(Vertex source, Vertex target, EdgeType type)
        {
            this.source = source;
            this.target = target;
            this.type = type;
        }

        public Vertex Source => source;

        public Vertex Target => target;

        public Vertex GetSource()
        {
            return source;
        }

        public Vertex GetTarget()
        {
            return target;
        }

        public EdgeType GetType()
        {
            return type;
        }

        public Boolean IsControl()
        {
            return type == EdgeType.CTRL_TRUE || type == EdgeType.CTRL_FALSE;
        }
    }
}
