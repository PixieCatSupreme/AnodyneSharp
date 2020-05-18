using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AnodyneSharp.Resources
{
    public static class CardDataManager
    {
        private const string CardDataPath = "Content.CardLookup.card";
        private static Dictionary<string, Dictionary<Vector2, int>> _cardLocations;

        static CardDataManager()
        {
            _cardLocations = new Dictionary<string, Dictionary<Vector2, int>>();
        }

        public static void ReadCardData()
        {
            var assembly = Assembly.GetExecutingAssembly();

            string path = $"{Assembly.GetExecutingAssembly().GetName().Name}.{CardDataPath}";

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string mapName = reader.ReadLine().Trim();
                        Dictionary<Vector2, int> mapCards = new Dictionary<Vector2, int>();

                        if (reader.ReadLine().Trim() == "{")
                        {
                            string line = reader.ReadLine().Trim();
                            while (line != "}")
                            {
                                string[] cardLine = line.Split('\t');

                                if (int.TryParse(cardLine[0], out int id) &&
                                    int.TryParse(cardLine[1], out int x) &&
                                    int.TryParse(cardLine[2], out int y))
                                {
                                    mapCards.Add(new Vector2(x, y), id);
                                }

                                line = reader.ReadLine().Trim();
                            }
                        }

                        _cardLocations.Add(mapName, mapCards);
                    }
                }
            }
        }

        public static int GetCardId()
        {
            string mapName = GlobalState.CURRENT_MAP_NAME;
            Vector2 gridPos = GlobalState.CurrentMapGrid;

            if (!_cardLocations.ContainsKey(mapName))
            {
                DebugLogger.AddError($"No card data found for map {mapName}!");
                return -1;
            }

            Dictionary<Vector2, int> mapCards = _cardLocations[mapName];

            if (!mapCards.ContainsKey(gridPos))
            {
                DebugLogger.AddError($"No card data at {gridPos.X}, {gridPos.Y} of map {mapName}!");
                return -1;
            }

            return mapCards[gridPos];
        }
    }
}
