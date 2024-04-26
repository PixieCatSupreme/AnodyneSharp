using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Entities.Gadget.Treasures;
using AnodyneSharp.Registry;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using static System.Collections.Specialized.BitVector32;

namespace AnodyneSharp.Archipelago
{
    public static class ArchipelagoSessionHandler
    {
        public static bool IsConnected { get; private set; }

        private static ArchipelagoSession _session;

        public static bool Initialize(string hostName, int port, string user, string pass)
        {
            _session = ArchipelagoSessionFactory.CreateSession(hostName, port);

            LoginResult result;

            try
            {
                result = _session.TryConnectAndLogin("Anodyne Fan Remake", user, ItemsHandlingFlags.AllItems, password: string.IsNullOrEmpty(pass) ? null : pass);
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

            IsConnected = true;

            return loginSuccess.Successful;
        }

        public static Item GetItemAtLocation(int locationID)
        {
            LocationInfoPacket locInfo = _session.Locations.ScoutLocationsAsync(locationID).Result;

            var itemID = locInfo.Locations[locationID].Item;

            Item? item = GlobalState.GetItemValues(itemID);

            if (item != null)
            {
                return item.Value;
            }
            else
            {
                return new Item((int)TreasureChest.TreasureType.ARCHIPELAGO, $"{locationID}");
            }
        }

        public static ArchipelagoItem GetOutsideItemInfo(int locationID)
        {
            LocationInfoPacket locInfo = _session.Locations.ScoutLocationsAsync(locationID).Result;

            var itemID = locInfo.Locations[locationID].Item;

            var playerID = locInfo.Locations[0].Player;
            var flag = locInfo.Locations[0].Flags;

            string playerName = _session.Players.GetPlayerName(playerID);
            string itemName = _session.Items.GetItemName(itemID);

            return new ArchipelagoItem(playerName, itemName, locationID, flag.HasFlag(ItemFlags.Advancement));
        }

        public static void ReportCollected(int locationID)
        {
            _session.Locations.CompleteLocationChecks(3);
        }
    }
}
