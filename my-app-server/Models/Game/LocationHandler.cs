using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace my_app_server.Models
{
    public static class LocationHandler
    {
        public static AstarResult DistanceToMove(LocationDescription description, LocationState state, int LocalTarget)
        {
            LocationResult<MainNodeResult> res = description.GenLocalForm(state);
            Graph<Node> graph = new Graph<Node>(res.Nodes, res.Edges, Node.HeuristicDistance);
            return graph.Astar(res.CurrentLocation, LocalTarget);
        }
        public static AstarResult DistanceToMove(InstanceDescription description, InstanceState state, int LocalTarget)
        {
            LocationResult<InstanceNodeResult> res = description.GenLocalForm(state);
            Graph<Node> graph = new Graph<Node>(res.Nodes, res.Edges, Node.HeuristicDistance);
            return graph.Astar(res.CurrentLocation, LocalTarget);
        }
        public static double TimeTravel(double distance, int mapscale, double velocity = 18.0)
        {
            return (distance * mapscale / velocity / 100 * 3.6);
        }
        public static Dictionary<LOCATIONS, List<LOCATION_OPTIONS>> OptionsForLocation = new Dictionary<LOCATIONS, List<LOCATION_OPTIONS>>()
        {
            {LOCATIONS.UNKNOWN, new List<LOCATION_OPTIONS>() {} },
            {LOCATIONS.LANDLOCATION, new List<LOCATION_OPTIONS>() {LOCATION_OPTIONS.TOINSTANCE} },
            {LOCATIONS.GLOBALLOCATION, new List<LOCATION_OPTIONS>() {LOCATION_OPTIONS.TOGLOBAL, LOCATION_OPTIONS.TOREST} },
            {LOCATIONS.SAFELOCATION, new List<LOCATION_OPTIONS>() {LOCATION_OPTIONS.TOREST} },
            {LOCATIONS.LOCALLOCATION, new List<LOCATION_OPTIONS>() {LOCATION_OPTIONS.TOLOCAL} },
        };
        public static Dictionary<LOCATION_OPTIONS, Func<my_appContext, Heros, int, int>> LocationTypeFunctions = new Dictionary<LOCATION_OPTIONS, Func<my_appContext, Heros, int, int>>(){
            {LOCATION_OPTIONS.TOGLOBAL, MoveHeroToGlobal},
            {LOCATION_OPTIONS.TOLOCAL, MoveHeroToLocation},
            {LOCATION_OPTIONS.TOINSTANCE, MoveHeroToLocation},
        };
        public static int MoveHeroToGlobal (my_appContext _context, Heros hero, int locationID)
        {
            MoveHeroToLocation(_context, hero, 0);
            return 0;
        }
        public static int MoveHeroToLocation(my_appContext _context, Heros hero, int locationID)
        {
            if (hero.Status != 0)
            {
                throw new Exception("Hero is not able to move.");
            }
            var globalStatus = _context.HerosLocations.FirstOrDefault(e => e.HeroId == hero.HeroId && e.LocationIdentifier == locationID);
            if (globalStatus == null)
            {
                HerosLocations location = HerosLocations.GenInitialLocation(_context, hero.HeroId, locationID);
                _context.HerosLocations.Add(location);
            }
            hero.CurrentLocation = locationID;
            return 0;
        }
        public static Fighting StartFightAfterTravel(my_appContext _context, Heros hero, int enemyID)
        {
            var enemy = _context.Enemies.FirstOrDefault(e => e.EnemyId == enemyID);
            if (enemy == null)
            {
                throw new OperationException("fightErr", "Unknown emeny to enter battle.");
            }
            Fighting fight = new Fighting()
            {
                EnemyHp = enemy.MaxHp,
                EnemyId = enemy.EnemyId,
                HeroId = hero.HeroId,
                IsOver = false,
                Loot = null,
                Initiative = 0,
            };
            _context.Fighting.Add(fight);
            hero.Status = 3;

            return fight;
        }
        public static int StartFight(my_appContext _context,Heros hero, int enemyID)
        {
            if (hero.Status == 0)
            {
                var enemy = _context.Enemies.FirstOrDefault(e => e.EnemyId == enemyID);
                if(enemy == null)
                {
                    throw new OperationException("fightErr", "Unknown emeny to enter battle.");
                }
                Fighting fight = new Fighting()
                {
                    EnemyHp = enemy.MaxHp,
                    EnemyId = enemy.EnemyId,
                    HeroId = hero.HeroId,
                    IsOver = false,
                    Loot = null,
                    Initiative = 0,
                };
                _context.Fighting.Add(fight);
                hero.Status = 3;
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    throw new OperationException("databaseErr", "Failed to add healing.");
                }
            }
            else
            {
                throw new OperationException("LocationErr", "Hero is not able to change state.");
            }
            return 0;
        }
        public static Dictionary<INSTANCES, List<INSTANCE_OPTIONS>> OptionsForInstances = new Dictionary<INSTANCES, List<INSTANCE_OPTIONS>>()
        {
            {INSTANCES.ENTRANCE, new List<INSTANCE_OPTIONS>() { INSTANCE_OPTIONS.TOLOCAL} },
            {INSTANCES.ENEMY, new List<INSTANCE_OPTIONS>() { INSTANCE_OPTIONS.TOFIGHT} },
            {INSTANCES.BOSS, new List<INSTANCE_OPTIONS>() { INSTANCE_OPTIONS.TOFIGHT} },
        };
        public static Dictionary<INSTANCE_OPTIONS, Func<my_appContext, Heros, int, int>> InstanceTypeFunctions = new Dictionary<INSTANCE_OPTIONS, Func<my_appContext, Heros, int, int>>(){
            {INSTANCE_OPTIONS.TOLOCAL, MoveHeroToLocation},
            {INSTANCE_OPTIONS.TOFIGHT, StartFight},
        };
        public static LocationResult<InstanceNodeResult> InstanceClearCurrent(my_appContext _context, Heros hero, bool affectNeighbours)
        {
            var location = _context.HerosLocations.FirstOrDefault(e => (e.HeroId == hero.HeroId) && (e.LocationIdentifier == hero.CurrentLocation));
            if (location == null)
            {
                throw new Exception("Location is not available.");
            }
            var descr = _context.LocationsDb.FirstOrDefault(e => e.LocationIdentifier == location.LocationIdentifier);
            if (descr == null)
            {
                throw new Exception("LocationData is not available.");
            }

            int LocationType = descr.LocationGlobalType;
            if (LocationType != 2)
            {
                throw new OperationException("locationErr", "Fight outside the location");
            }
            else
            {
                InstanceDescription description = JsonConvert.DeserializeObject<InstanceDescription>(descr.Sketch);
                InstanceState state = JsonConvert.DeserializeObject<InstanceState>(location.Description);
                description.LocationGlobalType = descr.LocationGlobalType;

                if (hero.Hp > 0)
                {
                    state.IsCleared[state.CurrentLocation] = true;

                    state.IsVisited[state.CurrentLocation] = true;
                    if (affectNeighbours)
                    {
                        var paths = description.Paths.Where(e => (e.NodeFrom == state.CurrentLocation) || (e.NodeTo == state.CurrentLocation));
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

                    location.Description = JsonConvert.SerializeObject(state);
                }

                return description.GenLocalForm(state);
            }
        }

        public static GeneralStatus GetHeroGeneralStatus(my_appContext _context, Heros hero, DateTime now)
        {
            object statusData = null;
            var location = _context.HerosLocations.FirstOrDefault(e => (e.HeroId == hero.HeroId) && (e.LocationIdentifier == hero.CurrentLocation));
            if (location == null)
            {
                throw new Exception("Location is not available.");
            }
            var descr = _context.LocationsDb.FirstOrDefault(e => e.LocationIdentifier == location.LocationIdentifier);
            if (descr == null)
            {
                throw new Exception("LocationData is not available.");
            }
            // TODO Location Type
            object locationResult = new LocationResult<object>();

            int LocationType = descr.LocationGlobalType;
            if (LocationType != 2)
            {
                LocationDescription description = JsonConvert.DeserializeObject<LocationDescription>(descr.Sketch);
                LocationState state = JsonConvert.DeserializeObject<LocationState>(location.Description);
                description.LocationGlobalType = descr.LocationGlobalType;

                if (hero.Status == 1)
                {
                    Traveling travel = _context.Traveling.FirstOrDefault(e => e.HeroId == hero.HeroId);
                    if (travel == null)
                    {
                        throw new Exception("Traveling hero without travel in DB.");
                    }
                    if (travel.HasEnded(now))
                    {
                        state = description.MoveTo(travel.UpdatedLocationID(), state);
                        hero.Status = 0;
                        location.Description = JsonConvert.SerializeObject(state);
                        _context.Traveling.Remove(travel);
                        try
                        {
                            _context.SaveChanges();
                        }
                        catch (DbUpdateException)
                        {
                            throw new Exception("Failed to remove travel.");
                        }
                    }
                    else
                    {
                        statusData = travel.GenTravelResult(now);
                    }
                }
                locationResult = description.GenLocalForm(state);
            }
            else
            {
                // This part is about the location, where hero can enter fight
                InstanceDescription description = JsonConvert.DeserializeObject<InstanceDescription>(descr.Sketch);
                InstanceState state = JsonConvert.DeserializeObject<InstanceState>(location.Description);
                description.LocationGlobalType = descr.LocationGlobalType;

                if (hero.Status == 1)
                {
                    Traveling travel = _context.Traveling.FirstOrDefault(e => e.HeroId == hero.HeroId);
                    if (travel == null)
                    {
                        throw new Exception("Traveling hero without travel in DB.");
                    }
                    if (travel.HasEnded(now))
                    {
                        // get info about end of travel -> maybe other move to with BOOL?
                        var targetnode = description.MainNodes.FirstOrDefault(e => e.NodeID == travel.UpdatedLocationID());
                        if (targetnode == null)
                        {
                            throw new Exception("Move to notmain node!");
                        }
                        if((targetnode.InstanceType == INSTANCES.BOSS || targetnode.InstanceType == INSTANCES.ENEMY) && !state.IsCleared[targetnode.NodeID])
                        {
                            LocationHandler.StartFightAfterTravel(_context, hero, targetnode.Data);
                            //hero status will be read below, no need to double code
                            hero.Status = 3;
                            state = description.MoveTo(travel.UpdatedLocationID(), state,false);
                        }
                        else
                        {
                            hero.Status = 0;
                            state = description.MoveTo(travel.UpdatedLocationID(), state,true);
                        }
                        // TODO update with cleannode
                        
                        
                        location.Description = JsonConvert.SerializeObject(state);
                        _context.Traveling.Remove(travel);
                        try
                        {
                            _context.SaveChanges();
                        }
                        catch (DbUpdateException)
                        {
                            throw new Exception("Failed to remove travel.");
                        }
                    }
                    else
                    {
                        statusData = travel.GenTravelResult(now);
                    }
                }
                locationResult = description.GenLocalForm(state);
            }
                
            if(hero.Status == 2)
            {
                Healing heal = _context.Healing.FirstOrDefault(e => e.HeroId == hero.HeroId);
                if (heal == null)
                {
                    throw new Exception("Healing hero without heal in DB.");
                }
                if (heal.HasEnded(now))
                {
                    int newHP = heal.FinalHealth(now);
                    hero.Hp = newHP;
                    HeroCalculator.CheckHeroHP(hero, _context);

                    _context.Healing.Remove(heal);
                    hero.Status = 0;
                    try
                    {
                        _context.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                        throw new Exception("Failed to remove healing.");
                    }
                }
                else
                {
                    statusData = heal.GenHealingResult(now);
                }
            }
            if(hero.Status == 3)
            {
                Fighting fight = _context.Fighting.FirstOrDefault(e => e.HeroId == hero.HeroId);
                if (fight == null)
                {
                    throw new Exception("Healing hero without heal in DB.");
                }
                statusData = fight.GenResult(_context,hero);
            }
            return new GeneralStatus()
            {
                HeroStatus = hero.Status,
                Location = locationResult,
                StatusData = statusData,
            };
        }
        public class GeneralStatus
        {
            public object Location { get; set; }
            public int HeroStatus { get; set; }
            public object StatusData { get; set; }
        }
    }
    public class OperationException : Exception
    {
        public OperationException(string ErrorClass, string Message) : base(Message)
        {
            this.ErrorClass = ErrorClass;
        }
        public string ErrorClass { get; set; }
    }
}
