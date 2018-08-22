using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public class LocationDescription
    {
        public string LocationName { get; set; }
        public Node[] Nodes { get; set; }
        public MainNode[] MainNodes { get; set; }
        public Edge[] Edges { get; set; }
        public ExtensionPath[] Paths { get; set; }
        public int InitialNode { get; set; }
        public int TravelScale { get; set; }
        public int LocationID { get; set; }

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
                CurrentLocation = dict[state.CurrentLocation],
                Edges = seenEdges.ToArray(),
                MainNodes = seenMains.ToArray(),
                Nodes = seenNodes.ToArray(),
                LocationName = this.LocationName,
                TravelScale = this.TravelScale,
                LocationID = this.LocationID,
            };
        }
        
        public LocationState MoveTo(int globalNewNode, LocationState initialState)
        {
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
        public int GlobalMainNodeID(int localID, LocationState localstate)
        {
            List<int> mapper = GenerateDictionary(localstate);
            int globalNewNode = mapper[localID];
            var mainnode = this.MainNodes.FirstOrDefault(e => e.NodeID == globalNewNode);
            if (mainnode == null)
            {
                throw new Exception("Move to notmain node!");
            }
            if (!localstate.IsDiscovered[globalNewNode])
            {
                throw new Exception("Move to undiscovered node!");
            }
            return globalNewNode;
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
    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public static Func<Node,Node,double> HeuristicDistance = (Node n1, Node n2) =>
        {
            return Math.Sqrt((n1.X - n2.X) * (n1.X - n2.X) + (n1.Y - n2.Y) * (n1.Y - n2.Y));
        };
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
        public int TravelScale { get; set; }
        public Node[] Nodes { get; set; }
        public MainNode[] MainNodes { get; set; }
        public Edge[] Edges { get; set; }
        public int LocationID { get; set; }
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
}
