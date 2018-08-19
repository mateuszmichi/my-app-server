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
    public class LocationData
    {
        private string LocationName;
        private Node[] Nodes;
        private MainNode[] MainNodes;
        private Edge[] Edges;
        private ExtensionPath[] Paths;
        private int InitialNode;

        public LocationData(LocationDescription desc)
        {
            this.Nodes = desc.Nodes;
            this.MainNodes = desc.MainNodes;
            this.Edges = desc.Edges;
            this.Paths = desc.Paths;
            this.InitialNode = desc.InitialNode;
            this.LocationName = desc.LocationName;
        }
        public LocationResult GenLocalForm(LocationState state)
        {
            //check loaded data
            if (state.IsDiscovered.Length != this.Nodes.Length)
            {
                throw new ArgumentException("Bad state desc");
            }

            int counter = 0;
            Dictionary<int, int> dict = new Dictionary<int, int>();
            List<Node> seenNodes = new List<Node>();
            List<MainNode> seenMains = new List<MainNode>();
            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (state.IsDiscovered[i])
                {
                    seenNodes.Add(this.Nodes[i]);
                    dict.Add(i, counter++);
                }
            }
            foreach (MainNode main in this.MainNodes)
            {
                if (state.IsDiscovered[main.NodeID])
                {
                    MainNode nowa = new MainNode()
                    {
                        LocationType = (state.IsVisited[main.NodeID]) ? main.LocationType : LOCATIONS.UNKNOWN,
                        Name = main.Name,
                        NodeID = dict[main.NodeID]
                    };
                    seenMains.Add(nowa);
                }
            }
            List<Edge> seenEdges = new List<Edge>();
            foreach (Edge e in this.Edges)
            {
                if (state.IsDiscovered[e.From] && state.IsDiscovered[e.To])
                {
                    seenEdges.Add(new Edge(dict[e.From], dict[e.To], e.Value));
                }
            }
            return new LocationResult()
            {
                CurrentLocation = state.CurrentLocation,
                Edges = seenEdges.ToArray(),
                MainNodes = seenMains.ToArray(),
                Nodes = seenNodes.ToArray(),
                LocationName = this.LocationName,
            };
        }
        public LocationState MoveTo(int newNode, LocationState initialState)
        {
            List<int> mapper = GenerateDictionary(initialState);
            int globalNewNode = mapper[newNode];
            LocationState state = new LocationState()
            {
                CurrentLocation = globalNewNode,
                IsDiscovered = initialState.IsDiscovered,
                IsVisited = initialState.IsVisited,
            };
            var mainnode = this.MainNodes.FirstOrDefault(e => e.NodeID == globalNewNode);
            if (mainnode == null)
            {
                throw new Exception("Move to notmain node!");
            }
            if (!initialState.IsDiscovered[globalNewNode])
            {
                throw new Exception("Move to undiscovered node!");
            }
            if (!initialState.IsVisited[globalNewNode])
            {
                state.IsVisited[globalNewNode] = true;
                var paths = this.Paths.Where(e => (e.NodeFrom == globalNewNode) || (e.NodeTo == globalNewNode));
                foreach (ExtensionPath path in paths)
                {
                    state.IsDiscovered[path.NodeFrom] = true;
                    state.IsDiscovered[path.NodeTo] = true;
                    foreach (int node in path.Path)
                    {
                        state.IsDiscovered[node] = true;
                    }
                }
            }
            return state;
        }
        private List<int> GenerateDictionary(LocationState initialstate)
        {
            List<int> dict = new List<int>();
            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (initialstate.IsDiscovered[i])
                {
                    dict.Add(i);
                }
            }
            return dict;
        }

        public LocationState InitializeLocation()
        {
            LocationState state = new LocationState
            {
                CurrentLocation = this.InitialNode,
                IsDiscovered = new bool[this.Nodes.Length],
                IsVisited = new bool[this.Nodes.Length]
            };
            state.IsDiscovered[state.CurrentLocation] = true;
            return MoveTo(state.CurrentLocation, state);
        }
    }
    public class MainNode
    {
        public int NodeID { get; set; }
        public LOCATIONS LocationType { get; set; }
        public string Name { get; set; }


    }
    public class LocationDescription
    {
        public string LocationName { get; set; }
        public Node[] Nodes { get; set; }
        public MainNode[] MainNodes { get; set; }
        public Edge[] Edges { get; set; }
        public ExtensionPath[] Paths { get; set; }
        public int InitialNode { get; set; }
    }
    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    public class ExtensionPath
    {
        public int[] Path { get; set; }
        public int NodeFrom { get; set; }
        public int NodeTo { get; set; }
    }
    public class LocationState
    {
        public int CurrentLocation { get; set; }
        public bool[] IsDiscovered { get; set; }
        public bool[] IsVisited { get; set; }
    }
    public class LocationResult
    {
        public int CurrentLocation { get; set; }
        public string LocationName { get; set; }
        public Node[] Nodes { get; set; }
        public MainNode[] MainNodes { get; set; }
        public Edge[] Edges { get; set; }
    }
    public enum LOCATIONS
    {
        UNKNOWN,
        LANDLOCATION,
        GLOBALLOCATION,
    }
    public enum LOCATION_OPTIONS
    {
        TOGLOBAL,
        TOLOCAL,
        TOINSTANCE,
        TOSHOP,
        TOREST,
    }

    public static class LocationHandler
    {
        public static LocationResult LoadLocalLocation(string Location_Sketch, string Location_State)
        {
            LocationDescription description = JsonConvert.DeserializeObject<LocationDescription>(Location_Sketch);
            LocationData data = new LocationData(description);
            LocationState state = JsonConvert.DeserializeObject<LocationState>(Location_State);
            return data.GenLocalForm(state);
        }
    }
}
