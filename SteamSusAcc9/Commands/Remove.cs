using System;
using CommandSystem;
using Extensions = SteamSusAcc.DataBase.Extensions;

namespace SteamSusAcc
{
    internal class Remove : ICommand
    {
        public string Command { get; } = "remove";

        public string[] Aliases { get; } = { };

        public string Description { get; } = "Remove a player from the database";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: steamapi remove (target SteamID)";
                return false;
            }
            string SteamId = arguments.At(0);
            if (!Extensions.TryGetValue(SteamId, out var player))
            {
                response = "UserID not found...";
                return false;
            }
            Extensions.DeletePlayer(SteamId);
            response = $"Player with SteamID {arguments.At(0)} successfully removed from the database!";
            return true;
        }
    }
}
