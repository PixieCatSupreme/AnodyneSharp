using AnodyneSharp.Logging;
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

        private static Dictionary<string, List<EntityPreset>> stateless;
        private static Dictionary<string, List<EntityPreset>> stateful;

        static EntityManager()
        {
            stateless = new Dictionary<string, List<EntityPreset>>();
            stateful = new Dictionary<string, List<EntityPreset>>();
        }

        /// <summary>
        /// Loads the raw XML entity file. Stateful entities will be edited after save loading.
        /// </summary>
        public static void Initialize()
        {
            var assembly = Assembly.GetCallingAssembly();

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


        public static bool GetMapEntities(string mapName, out List<EntityPreset> mapStateless, out List<EntityPreset> mapStateful)
        {
            mapStateless = new List<EntityPreset>();
            mapStateful = new List<EntityPreset>();

            if (!stateless.ContainsKey(mapName) || !stateful.ContainsKey(mapName))
            {
                return false;
            }

            mapStateless = stateless[mapName];
            mapStateful = stateful[mapName];

            return true;
        }

        public static bool GetGridEntities(string mapName, Vector2 grid, out List<EntityPreset> gridStateless, out List<EntityPreset> gridStateful)
        {
            gridStateless = new List<EntityPreset>();
            gridStateful = new List<EntityPreset>();

            if (!GetMapEntities(mapName, out gridStateless, out gridStateful))
            {
                return false;
            }

            gridStateless = gridStateless.Where(e => e.GridPosition == grid).ToList();
            gridStateful = gridStateful.Where(e => e.GridPosition == grid).ToList();

            return true;
        }

        private static void ReadEntities(string xml)
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(xml);

            XmlNode root = doc.FirstChild;

            DebugLogger.AddInfo(root.Name);

            if (root.HasChildNodes)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    var map = root.ChildNodes[i];

                    string mapName = map.Attributes.GetNamedItem("name").Value;
                    bool stateLess = map.Attributes.GetNamedItem("type").Value == "Stateless";

                    List<EntityPreset> presets = new List<EntityPreset>();

                    foreach (XmlNode child in map.ChildNodes)
                    {
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

                            presets.Add(new EntityPreset(child.Name, new Vector2(x, y), id,frame, p, type));
                        }
                    }

                    if (map.Attributes.GetNamedItem("type").Value == "Stateless")
                    {
                        stateless.Add(mapName, presets);
                    }
                    else
                    {
                        stateful.Add(mapName, presets);
                    }
                }
            }
        }
    }
}
