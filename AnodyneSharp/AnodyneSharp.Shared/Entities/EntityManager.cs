using AnodyneSharp.Entities.Enemy;
using AnodyneSharp.Entities.Gadget.Doors;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace AnodyneSharp.Entities
{
    public static class EntityManager
    {
        private const string EntityFilePath = "Content.Entities.xml";

        private static Dictionary<string, List<EntityPreset>> _entities = new();
        private static Dictionary<int, DoorPair> _doorPairs = new();
        private static Dictionary<int, List<EntityPreset>> _linkGroups = new();
        public static Dictionary<Guid, EntityState> State = new();

        /// <summary>
        /// Loads the raw XML entity file. Stateful entities will be edited after save loading.
        /// </summary>
        public static void Initialize()
        {
            var assembly = Assembly.GetEntryAssembly();

            string path = $"{assembly.GetName().Name}.{EntityFilePath}";
            string xml = "";

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                using StreamReader reader = new(stream);
                xml = reader.ReadToEnd();
            }

            ReadEntities(xml);

        }

        public static void SetAlive(Guid id,  bool isAlive)
        {
            if (State.TryGetValue(id, out EntityState s))
            {
                s.Alive = isAlive;
                if(s.Alive == true && s.Activated == false)
                {
                    State.Remove(id);
                }
            }
            else if(isAlive == false)
            {
                State.Add(id, new() { Alive = false });
            }
        }

        public static void SetActive(Guid id, bool isActive)
        {
            if (State.TryGetValue(id, out EntityState s))
            {
                s.Activated = isActive;
                if (s.Alive == true && s.Activated == false)
                {
                    State.Remove(id);
                }
            }
            else if (isActive == true)
            {
                State.Add(id, new() { Activated = true });
            }
        }

        public static List<EntityPreset> GetMapEntities(string mapName)
        {
            if (!_entities.ContainsKey(mapName))
            {
                return new List<EntityPreset>();
            }

            return _entities[mapName];
        }

        public static List<EntityPreset> GetGridEntities(string mapName, Point grid)
        {
            return GetMapEntities(mapName).Where(e => MapUtilities.GetRoomCoordinate(e.Position) == grid).ToList();
        }

        public static DoorMapPair GetLinkedDoor(EntityPreset door)
        {
            if (_doorPairs.TryGetValue(door.Frame, out DoorPair pair))
            {
                return pair.GetLinkedDoor(door);
            }
            else
            {
                DebugLogger.AddCritical($"Could not find door pair with id {door.Frame}");
                return null;
            }
        }

        public static List<EntityPreset> GetLinkGroup(int linkid)
        {
            return _linkGroups[linkid];
        }

        public static DoorMapPair GetNexusGateForCurrentMap()
        {
            try
            {
                var t = GetMapEntities(GlobalState.CURRENT_MAP_NAME).Where(p => p.Type == typeof(NexusPad)).Single();
                return GetLinkedDoor(t);
            }
            catch (Exception)
            {
                if (GlobalState.events.VisitedMaps.Contains("NEXUS"))
                {
                    var t = GetMapEntities("NEXUS").Where(p => p.Type == typeof(DungeonEntrance)).Single();
                    return new DoorMapPair(t, "NEXUS");
                }
                return null;
            }
        }

        private static void ReadEntities(string xml)
        {
            var type_lookup = (from t in Assembly.GetEntryAssembly().GetTypes()
                               where t.IsDefined(typeof(NamedEntity), false)
                               group new { type = t, check = t.GetCustomAttribute<NamedEntity>() } by t.GetCustomAttribute<NamedEntity>().GetName(t)
                               ).ToDictionary(t => t.Key, t => t.ToList());

            HashSet<string> missing = new();

            XmlDocument doc = new();

            doc.LoadXml(xml);

            XmlNode root = doc.FirstChild;

            List<DoorMapPair> doors = new();
            HashSet<string> missingInThisMap = new();

            if (root.HasChildNodes)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    var map = root.ChildNodes[i];

                    string mapName = map.Attributes.GetNamedItem("name").Value;

                    if (!_entities.ContainsKey(mapName))
                    {
                        _entities.Add(mapName, new List<EntityPreset>());
                        missingInThisMap = new();
                    }
                    List<EntityPreset> presets = _entities[mapName];

                    foreach (XmlNode child in map.ChildNodes)
                    {
                        if (!type_lookup.ContainsKey(child.Name))
                        {
                            if (!missingInThisMap.Contains(child.Name))
                            {
                                missing.Add(child.Name);
                                missingInThisMap.Add(child.Name);
                                DebugLogger.AddWarning($"Missing Entity {child.Name}-{mapName}!");
                            }
                            continue;
                        }
                        if (float.TryParse(child.Attributes.GetNamedItem("x").Value, out float x) &&
                            float.TryParse(child.Attributes.GetNamedItem("y").Value, out float y) &&
                            Guid.TryParse(child.Attributes.GetNamedItem("guid").Value, out Guid id) &&
                            int.TryParse(child.Attributes.GetNamedItem("frame").Value, out int frame))
                        {
                            Permanence p = Permanence.GRID_LOCAL;

                            if (child.Attributes.GetNamedItem("p") != null)
                            {
                                p = (Permanence)int.Parse(child.Attributes.GetNamedItem("p").Value);
                            }

                            string type = child.Attributes.GetNamedItem("type")?.Value ?? "";

                            var matching = type_lookup[child.Name]
                                .FindAll(t => t.check.Matches(frame, type, mapName))
                                .GroupBy(t=>t.check.SpecificityCount())
                                .OrderByDescending(g=>g.Key)
                                .ToList();
                            if (matching.Count == 0)
                            {
                                string missing_entity = $"{child.Name}-{frame}-'{type}'";
                                if (!missingInThisMap.Contains(missing_entity))
                                {
                                    missing.Add(missing_entity);
                                    missingInThisMap.Add(missing_entity);
                                    DebugLogger.AddWarning($"Missing Entity {missing_entity}-{mapName}!");
                                }
                            }
                            else if (matching[0].Count() > 1)
                            {
                                DebugLogger.AddWarning($"Conflict at {child.Name}-{frame}-'{type}': " + String.Join(", ", matching[0].Select(t => t.type.Name)));
                            }
                            else
                            {
                                EntityPreset preset = new(matching[0].First().type, new Vector2(x, y), id, frame, p, type);

                                if(int.TryParse(child.Attributes.GetNamedItem("linkid")?.Value,out int linkid))
                                {
                                    preset = new(matching[0].First().type, new Vector2(x, y), id, frame, p, type, linkid);

                                    List<EntityPreset> GetLinked(int link)
                                    {
                                        if(_linkGroups.TryGetValue(link, out var presets))
                                        {
                                            return presets;
                                        }
                                        List<EntityPreset> ret = new();
                                        _linkGroups.Add(link, ret);
                                        return ret;
                                    }
                                    GetLinked(linkid).Add(preset);
                                    string group = child.Attributes.GetNamedItem("linkgroup")?.Value;
                                    if(group != null)
                                    {
                                        int[] ids = group.Split(',').Select(int.Parse).ToArray();
                                        List<EntityPreset> all = ids.SelectMany(GetLinked).ToList();
                                        foreach(int l_id in ids)
                                        {
                                            _linkGroups[l_id] = all;
                                        }
                                    }
                                }

                                presets.Add(preset);

                                if (child.Name == "Door")
                                {
                                    DoorMapPair newDoor = new(preset, mapName);

                                    if (doors.Any(d => d.Door.Frame == preset.Frame))
                                    {
                                        DoorMapPair doorOne = doors.First(d => d.Door.Frame == preset.Frame);
                                        _doorPairs.Add(preset.Frame, new DoorPair(doorOne, newDoor));

                                        doors.Remove(doorOne);

                                        //DebugLogger.AddInfo($"DOOR PAIR {preset.Frame}\n{doorOne.Door.Position.X} {doorOne.Door.Position.Y} {doorOne.Map}\n{newDoor.Door.Position.X} {newDoor.Door.Position.Y} {newDoor.Map}");
                                    }
                                    else
                                    {
                                        doors.Add(newDoor);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var item in doors)
                {
                    DebugLogger.AddWarning($"Door {item.Door.EntityID} in map {item.Map} with link ID {item.Door.Frame} is missing a pair!");
                }
            }
        }
    }
}
