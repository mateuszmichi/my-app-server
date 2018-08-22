using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace my_app_server.Models
{
    public static class LocationHandler
    {
        public static AstarResult DistanceToMove(LocationDescription description, LocationState state, int LocalTarget)
        {
            LocationResult res = description.GenLocalForm(state);
            Graph<Node> graph = new Graph<Node>(res.Nodes, res.Edges, Node.HeuristicDistance);
            return graph.Astar(res.CurrentLocation, LocalTarget);
        }
        public static double TimeTravel(double distance,int mapscale,double velocity = 18.0)
        {
            return (distance * mapscale / velocity / 100 * 3.6);
        }
    }
}
