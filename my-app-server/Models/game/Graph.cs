using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Priority_Queue;

namespace my_app_server.Models
{
    public class Graph<T>
    {
        public T[] Nodes;
        public Edge[] PassedEdges;
        private List<Edge>[] Edges;
        private Func<T, T, double> HeuristicDistance;
        public Graph(T[] nodes, Edge[] edges, Func<T, T, double> HeuristicDistance)
        {
            this.Nodes = nodes;
            this.PassedEdges = edges;
            this.HeuristicDistance = HeuristicDistance;
            this.Edges = new List<Edge>[this.NodesCount()];
            for (int i = 0; i < this.NodesCount(); i++)
            {
                this.Edges[i] = new List<Edge>();
            }
            foreach (Edge e in this.PassedEdges)
            {
                this.AddEdge(e);
            }
        }
        private void AddEdge(Edge e)
        {
            if (e.To >= this.NodesCount())
            {
                throw new Exception("Bad Edge");
            }
            this.Edges[e.From].Add(e);
            this.Edges[e.To].Add(new Edge(e.To, e.From, e.Value));
        }
        private List<Edge> OutEdges(int index)
        {
            return this.Edges[index];
        }
        public int NodesCount()
        {
            return this.Nodes.Length;
        }
        public AstarResult Astar(int start, int end)
        {
            if (start >= this.NodesCount() || end >= this.NodesCount())
            {
                throw new ArgumentException("Nodes not in graph");
            }
            var gScore = new Dictionary<int, (double distance, int prev)>
            {
                { start, (0, -1) }
            };

            HashSet<int> closedset = new HashSet<int>();
            var openset = new SimplePriorityQueue<int, double>();
            openset.Enqueue(start, 0 + this.HeuristicDistance(this.Nodes[start], this.Nodes[end]));

            while (openset.Count > 0)
            {
                int u = openset.Dequeue();
                closedset.Add(u);
                if (u == end)
                {
                    return GenAstarResult(end, start, gScore);
                }
                foreach (Edge e in this.OutEdges(u))
                {
                    if (closedset.Contains(e.To)) continue;
                    if (!openset.Contains(e.To))
                    {
                        gScore.Add(e.To, (double.MaxValue, -1));
                        openset.Enqueue(e.To, double.MaxValue);
                    }
                    double ndist = gScore[u].distance + e.Value * this.HeuristicDistance(this.Nodes[u], this.Nodes[e.To]);
                    if (gScore[e.To].distance > ndist)
                    {
                        gScore[e.To] = (ndist, u);
                        openset.UpdatePriority(e.To, ndist + this.HeuristicDistance(this.Nodes[e.To], this.Nodes[end]));
                    }
                }
            }
            throw new Exception("Not Connected Nodes");
        }
        private AstarResult GenAstarResult(int end, int start, Dictionary<int, (double distance, int prev)> gScore)
        {
            if (!gScore.ContainsKey(end))
            {
                throw new Exception("Not Connected Nodes");
            }
            int iter = end;
            List<int> result = new List<int>();
            while (gScore[iter].prev != -1)
            {
                result.Add(iter);
                iter = gScore[iter].prev;
            }
            if (iter != start)
            {
                throw new Exception("Not Connected Nodes");
            }
            else
            {
                result.Add(iter);
            }
            result.Reverse();
            return new AstarResult(gScore[end].distance, result.ToArray());

        }
    }
    public class Edge
    {
        public int From { get; set; }
        public int To { get; set; }
        public double Value { get; set; }
        public Edge(int _from, int _to, double _value = 1)
        {
            this.From = _from;
            this.To = _to;
            this.Value = _value;
        }
    }
    public class AstarResult
    {
        public double Distance { get; }
        public int[] Nodes { get; }
        public AstarResult(double dist, int[] nodes)
        {
            this.Distance = dist;
            this.Nodes = nodes;
        }
    }
}
