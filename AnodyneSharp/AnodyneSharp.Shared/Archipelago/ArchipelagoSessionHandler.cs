using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Entities.Gadget.Treasures;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;

namespace AnodyneSharp.Archipelago
{
    public static class ArchipelagoSessionHandler
    {
        public static bool IsConnected { get; private set; }

        private static ArchipelagoSession _session;
        private static DeathLinkService _deathLink;

        private static string _user;

        private static int _lastCollectionSize;

        private static Dictionary<long, Item> _locations;
        private static Queue<string> _messages;

        public static bool Initialize(string hostName, int port, string user, string pass)
        {
            _messages = new();

            _session = ArchipelagoSessionFactory.CreateSession(hostName, port);

            LoginResult result;

            try
            {
                result = _session.TryConnectAndLogin("Anodyne", user, ItemsHandlingFlags.AllItems, password: string.IsNullOrEmpty(pass) ? null : pass);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"Failed to Connect to {hostName}:{port} as {user}:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }

                return false; // Did not connect, show the user the contents of `errorMessage`
            }

            // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be used to interact with the server and the returned `LoginSuccessful` contains some useful information about the initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
            var loginSuccess = (LoginSuccessful)result;

            _user = user;

            if (loginSuccess.SlotData.TryGetValue("death_link", out object dl) &&
                dl.ToString() == "true")
            {
                EnableDeathLink();
            }

            if (loginSuccess.SlotData.TryGetValue("unlock_gates", out object ug) &&
                ug.ToString() == "true")
            {

            }

            if (loginSuccess.SlotData.TryGetValue("nexus_gates_unlocked", out object ngu) &&
                ngu is JArray gates)
            {

            }

            IsConnected = true;

            bool loadedLocations;

            do
            {
                loadedLocations = GetLocations();
            } while (!loadedLocations);

            return loginSuccess.Successful;
        }

        public static bool GetLocations()
        {
            LocationInfoPacket locInfo;

            _locations = new();

            try
            {
                locInfo = _session.Locations.ScoutLocationsAsync(false, GlobalState.GetLocationIDs()).Result;
            }
            catch (Exception)
            {
                DebugLogger.AddWarning($"Unable to get locations. Unable to connect.");
                return false;
            }

            foreach (var networkItem in locInfo?.Locations)
            {
                long locationId = networkItem.Location;

                Item? item = GlobalState.GetItemValues(networkItem.Item);

                if (item == null)
                {
                    var playerID = networkItem.Player;
                    var flag = networkItem.Flags;

                    string playerName = _session.Players.GetPlayerName(playerID);
                    string itemName = _session.Items.GetItemName(networkItem.Item);

                    item = new ArchipelagoItem((int)TreasureChest.TreasureType.ARCHIPELAGO, "", playerName, itemName, locationId, flag.HasFlag(ItemFlags.Advancement));
                }

                _locations.Add(locationId, item);
            }

            return true;
        }

        public static Item? GetItemAtLocation(long locationID)
        {
            if (_locations.TryGetValue(locationID, out Item item))
            {
                return item;
            }

            return null;
        }

        public static void Update()
        {
            CheckItemCollection();

            if (_messages.Count > 0 && string.IsNullOrEmpty(GlobalState.Dialogue))
            {
                SoundManager.PlaySoundEffect("gettreasure");
                GlobalState.Dialogue = _messages.Dequeue();
            }
        }

        public static void ReportCollected(long locationID)
        {
            _session.Locations.CompleteLocationChecks(locationID);
        }

        public static void SendDeathLink(string message)
        {
            _deathLink?.SendDeathLink(new DeathLink(_user, message));
        }

        private static void CheckItemCollection()
        {
            try
            {
                var allItemsRecieved = _session.Items.AllItemsReceived;

                if (_lastCollectionSize == allItemsRecieved.Count)
                {
                    return;
                }

                for (int i = _lastCollectionSize; i < allItemsRecieved.Count; i++)
                {
                    NetworkItem networkItem = allItemsRecieved[i];

                    Item item = GlobalState.GetItemValues(networkItem.Item);

                    int dialogueID = -1;

                    switch (TreasureUtilities.GetTreasureType(item.Frame))
                    {
                        case TreasureChest.TreasureType.BROOM:
                            dialogueID = 1;
                            GlobalState.inventory.UnlockBroom(BroomType.Normal);
                            break;
                        case TreasureChest.TreasureType.KEY:
                            string mapName = item.TypeValue[0] + item.TypeValue[1..].ToLower();

                            _messages.Enqueue(string.Format(Dialogue.DialogueManager.GetDialogue("misc", "any", "treasure", 10), mapName));
                            GlobalState.inventory.AddMapKey(item.TypeValue, 1);
                            break;
                        case TreasureChest.TreasureType.GROWTH:
                            if (int.TryParse(item.TypeValue, out int cardID))
                            {
                                GlobalState.inventory.CardStatus[cardID] = true;
                            }
                            else
                            {
                                DebugLogger.AddWarning("Unable to collect card!");
                            }
                            break;
                        case TreasureChest.TreasureType.JUMP:
                            dialogueID = 4;
                            GlobalState.inventory.CanJump = true;
                            break;
                        case TreasureChest.TreasureType.WIDE:
                            dialogueID = 4;
                            GlobalState.inventory.UnlockBroom(BroomType.Wide);
                            break;
                        case TreasureChest.TreasureType.LONG:
                            dialogueID = 5;
                            GlobalState.inventory.UnlockBroom(BroomType.Long);
                            break;
                        case TreasureChest.TreasureType.SWAP:
                            dialogueID = 6;
                            GlobalState.inventory.UnlockBroom(BroomType.Transformer);
                            break;
                        case TreasureChest.TreasureType.SECRET:
                            break;
                        default:
                            _messages.Enqueue($"Unable to collect treasure {item.Frame} of type '{item.TypeValue}'");
                            break;
                    }

                    if (dialogueID == -1)
                    {
                        return;
                    }
                    else
                    {
                        _messages.Enqueue(Dialogue.DialogueManager.GetDialogue("misc", "any", "treasure", dialogueID));
                    }
                }

                _lastCollectionSize = allItemsRecieved.Count;

            }
            catch (Exception)
            {
                DebugLogger.AddWarning("Unable to check collected items!");
            }
        }

        private static void EnableDeathLink()
        {
            _deathLink = _session.CreateDeathLinkService();
            _deathLink.EnableDeathLink();
            _deathLink.OnDeathLinkReceived += (deathLinkObject) =>
            {
                GlobalState.CUR_HEALTH = 0;
            };
        }
    }
}
