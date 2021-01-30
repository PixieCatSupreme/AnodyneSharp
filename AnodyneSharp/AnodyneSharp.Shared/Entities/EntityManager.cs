using AnodyneSharp.Entities.Enemy;
using AnodyneSharp.Entities.Gadget.Doors;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
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

        private static Dictionary<string, List<EntityPreset>> _entities;
        private static Dictionary<int, DoorPair> _doorPairs;

        static EntityManager()
        {
            _entities = new Dictionary<string, List<EntityPreset>>();
            _doorPairs = new Dictionary<int, DoorPair>();
        }

        /// <summary>
        /// Loads the raw XML entity file. Stateful entities will be edited after save loading.
        /// </summary>
        public static void Initialize()
        {
            var assembly = Assembly.GetExecutingAssembly();

            string path = $"{assembly.GetName().Name}.{EntityFilePath}";
            string xml = "";

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                using (StreamReader reader = new(stream))
                {
                    xml = reader.ReadToEnd();
                }
            }

            ReadEntities(xml);

        }

        public static List<EntityPreset> GetMapEntities(string mapName)
        {
            if (!_entities.ContainsKey(mapName))
            {
                return new List<EntityPreset>();
            }

            return _entities[mapName];
        }

        public static List<EntityPreset> GetGridEntities(string mapName, Vector2 grid)
        {
            return GetMapEntities(mapName).Where(e => e.GridPosition == grid).ToList();
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

        public static DoorMapPair GetNexusGateForCurrentMap()
        {
            try
            {
                var t = GetMapEntities(GlobalState.CURRENT_MAP_NAME).Where(p => p.Type == typeof(NexusPad)).Single();
                return GetLinkedDoor(t);
            }
            catch (Exception e)
            {
                if(GlobalState.events.VisitedMaps.Contains("NEXUS"))
                {
                    var t = GetMapEntities("NEXUS").Where(p => p.Type == typeof(DungeonEntrance)).Single();
                    return new DoorMapPair(t, "NEXUS");
                }
                return null;
            }
        }

        private static void ReadEntities(string xml)
        {
            var type_lookup = (from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.IsDefined(typeof(NamedEntity), false)
                               group new { type = t, check = t.GetCustomAttribute<NamedEntity>() } by t.GetCustomAttribute<NamedEntity>().GetName(t)
                               ).ToDictionary(t => t.Key, t => t.ToList());

            HashSet<string> missing = new();

            XmlDocument doc = new();

            doc.LoadXml(xml);

            XmlNode root = doc.FirstChild;

            List<DoorMapPair> doors = new();

            if (root.HasChildNodes)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    var map = root.ChildNodes[i];

                    string mapName = map.Attributes.GetNamedItem("name").Value;

                    if (!_entities.ContainsKey(mapName))
                    {
                        _entities.Add(mapName, new List<EntityPreset>());
                    }
                    List<EntityPreset> presets = _entities[mapName];

                    foreach (XmlNode child in map.ChildNodes)
                    {
                        if (!type_lookup.ContainsKey(child.Name))
                        {
                            if (!missing.Contains(child.Name))
                            {
                                DebugLogger.AddWarning($"Missing Entity {child.Name}!");
                                missing.Add(child.Name);
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

                            string type = "";

                            if (child.Attributes.GetNamedItem("type") != null)
                            {
                                type = child.Attributes.GetNamedItem("type").Value;
                            }

                            var matching = type_lookup[child.Name].FindAll(t => t.check.Matches(frame, type, mapName)).ToList();
                            if (matching.Count == 0)
                            {
                                string missing_entity = $"{child.Name}-{frame}-'{type}'";
                                if (!missing.Contains(missing_entity))
                                {
                                    missing.Add(missing_entity);
                                    DebugLogger.AddWarning($"Missing Entity {missing_entity}!");
                                }
                            }
                            else if (matching.Count > 1)
                            {
                                DebugLogger.AddWarning($"Conflict at {child.Name}-{frame}-'{type}': " + String.Join(", ", matching.Select(t => t.type.Name)));
                            }
                            else
                            {
                                EntityPreset preset = new(matching[0].type, new Vector2(x, y), id, frame, p, type);

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
