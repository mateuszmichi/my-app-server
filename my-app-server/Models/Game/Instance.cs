using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_app_server.Models
{
    public class InstanceDescription
    {
        public string LocationName { get; set; }
        public Node[] Nodes { get; set; }
        public InstanceNode[] MainNodes { get; set; }
        public Edge[] Edges { get; set; }
        public ExtensionPath[] Paths { get; set; }
        public int InitialNode { get; set; }
        public int TravelScale { get; set; }
        public int LocationID { get; set; }
        public int LocationGlobalType { get; set; }

        public LocationResult<InstanceNodeResult> GenLocalForm(InstanceState state)
        {
            //check loaded data
            if (state.IsDiscovered.Length != this.Nodes.Length)
            {
                throw new ArgumentException("Bad state desc");
            }

            int counter = 0;
            Dictionary<int, int> dict = new Dictionary<int, int>();
            List<Node> seenNodes = new List<Node>();
            List<InstanceNodeResult> seenMains = new List<InstanceNodeResult>();
            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (state.IsDiscovered[i])
                {
                    seenNodes.Add(this.Nodes[i]);
                    dict.Add(i, counter++);
                }
            }
            foreach (InstanceNode main in this.MainNodes)
            {
                if (state.IsDiscovered[main.NodeID])
                {
                    InstanceNodeResult nowa = new InstanceNodeResult()
                    {
                        InstanceType = main.InstanceType,
                        Level = main.Level,
                        NodeID = dict[main.NodeID],
                        IsCleared = state.IsCleared[main.NodeID],
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
            return new LocationResult<InstanceNodeResult>()
            {
                LocationGlobalType = this.LocationGlobalType,
                CurrentLocation = dict[state.CurrentLocation],
                Edges = seenEdges.ToArray(),
                MainNodes = seenMains.ToArray(),
                Nodes = seenNodes.ToArray(),
                LocationName = this.LocationName,
                TravelScale = this.TravelScale,
                LocationID = this.LocationID,
            };
        }
        public InstanceState MoveTo(int newNode, InstanceState initialState)
        {
            List<int> mapper = GenerateDictionary(initialState);
            int globalNewNode = mapper[newNode];
            InstanceState state = new InstanceState()
            {
                CurrentLocation = globalNewNode,
                IsDiscovered = initialState.IsDiscovered,
                IsVisited = initialState.IsVisited,
                IsCleared = initialState.IsCleared,
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
        private List<int> GenerateDictionary(InstanceState initialstate)
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
        public int GlobalMainNodeID(int localID, InstanceState localstate)
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

        public InstanceState InitializeLocation()
        {
            InstanceState state = new InstanceState
            {
                CurrentLocation = this.InitialNode,
                IsDiscovered = new bool[this.Nodes.Length],
                IsVisited = new bool[this.Nodes.Length],
                IsCleared = new bool[this.Nodes.Length],
            };
            state.IsDiscovered[state.CurrentLocation] = true;
            return MoveTo(state.CurrentLocation, state);
        }
    }
    public class InstanceState
    {
        public int CurrentLocation { get; set; }
        public bool[] IsDiscovered { get; set; }
        public bool[] IsVisited { get; set; }
        public bool[] IsCleared { get; set; }
    }
    public class InstanceNode
    {
        public int NodeID { get; set; }
        public INSTANCES InstanceType { get; set; }
        public int Level { get; set; }
        public int Data { get; set; }
    }
    public class InstanceNodeResult
    {
        public int NodeID { get; set; }
        public INSTANCES InstanceType { get; set; }
        public int Level { get; set; }
        public bool IsCleared { get; set; }
    }
    public enum INSTANCES
    {
        ENTRANCE,
        ENEMY,
        BOSS,
        TREASURE,
    }
    public enum INSTANCE_OPTIONS
    {
        TOLOCAL,
        TOFIGHT,
        TOTREASURE,
        TOEVENT,
    }
}
