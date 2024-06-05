using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnodyneSharp.Resources
{
    public static class CardDataManager
    {
        private const string CardDataPath = "Content.CardLookup.card";
        private static Dictionary<string, Dictionary<Point, int>> _cardLocations = new();

        public static void ReadCardData()
        {
            var assembly = Assembly.GetEntryAssembly();

            string path = $"{Assembly.GetEntryAssembly().GetName().Name}.{CardDataPath}";

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string mapName = reader.ReadLine().Trim();
                        Dictionary<Point, int> mapCards = new();

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
                                    mapCards.Add(new(x, y), id);
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
            Point gridPos = GlobalState.CurrentMapGrid;

            if (mapName == "NEXUS")
            {
                return 48;
            }

            if (!_cardLocations.ContainsKey(mapName))
            {

                DebugLogger.AddError($"No card data found for map {mapName}!");
                return -1;
            }

            Dictionary<Point, int> mapCards = _cardLocations[mapName];

            if (!mapCards.ContainsKey(gridPos))
            {
                DebugLogger.AddError($"No card data at {gridPos.X}, {gridPos.Y} of map {mapName}!");
                return -1;
            }

            return mapCards[gridPos];
        }

        public static bool GotAllNormalCards(string mapName)
        {
            if (!_cardLocations.ContainsKey(mapName))
            {
                return GlobalState.events.ActivatedNexusPortals.Contains(mapName);
            }

            return _cardLocations[mapName].Values.All((c) => (c > 36 && c != 43) || GlobalState.inventory.CardStatus[c]);
        }

        public static bool GotAllNormalCardsOfAnyMap()
        {
            return _cardLocations.Where(map => map.Value.Values.Any(c => c < 36 || c == 43)).Any(map => GotAllNormalCards(map.Key));
        }
    }
}
