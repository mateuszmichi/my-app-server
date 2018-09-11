﻿using System;
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
            LocationResult res = description.GenLocalForm(state);
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
            LocationResult locationResult = description.GenLocalForm(state);

            return new GeneralStatus()
            {
                HeroStatus = hero.Status,
                Location = locationResult,
                StatusData = statusData,
            };
        }
        public class GeneralStatus
        {
            public LocationResult Location { get; set; }
            public int HeroStatus { get; set; }
            public object StatusData { get; set; }
        }
    }
}
