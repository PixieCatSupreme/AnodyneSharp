using AnodyneSharp.Entities.Enemy;
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

        static EntityManager()
        {
            _entities = new Dictionary<string, List<EntityPreset>>();
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
                using (StreamReader reader = new StreamReader(stream))
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

        private static void ReadEntities(string xml)
        {
            var type_lookup = (from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.IsDefined(typeof(NamedEntity), false)
                               select t).ToDictionary(t => t.GetCustomAttribute<NamedEntity>().GetName(t), t => t);

            HashSet<string> missing = new HashSet<string>();

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(xml);

            XmlNode root = doc.FirstChild;

            if (root.HasChildNodes)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    var map = root.ChildNodes[i];

                    string mapName = map.Attributes.GetNamedItem("name").Value;
                    
                    if(!_entities.ContainsKey(mapName))
                    {
                        _entities.Add(mapName, new List<EntityPreset>());
                    }
                    List<EntityPreset> presets = _entities[mapName];

                    foreach (XmlNode child in map.ChildNodes)
                    {
                        if(!type_lookup.ContainsKey(child.Name))
                        {
                            if (!missing.Contains(child.Name))
                            {
                                DebugLogger.AddWarning($"Missing Enitity {child.Name}!");
                                missing.Add(child.Name);
                            }
                            continue;
                        }
                        if (int.TryParse(child.Attributes.GetNamedItem("x").Value, out int x) &&
                            int.TryParse(child.Attributes.GetNamedItem("y").Value, out int y) &&
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

                            bool alive = true;

                            if (child.Attributes.GetNamedItem("alive") != null)
                            {
                                alive = bool.Parse(child.Attributes.GetNamedItem("alive").Value);
                            }

                            presets.Add(new EntityPreset(type_lookup[child.Name], new Vector2(x, y), id,frame, p, type, alive));
                        }
                    }
                }
            }
        }
    }
}
