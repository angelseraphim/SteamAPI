using System;
using CommandSystem;
using static SteamSusAcc.DataBase.Data;
using Extensions = SteamSusAcc.DataBase.Extensions;

namespace SteamSusAcc
{
    internal class Add : ICommand
    {
        public string Command { get; } = "add";

        public string[] Aliases { get; } = { };

        public string Description { get; } = "Add player to the database";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: steamapi add (target SteamID)";
                return false;
            }
            string SteamID = arguments.At(0);
            if (Extensions.TryGetValue(SteamID, out PlayerInfo info))
            {
                response = $"Player with SteamID {SteamID} already in the database";
                return false;
            }
            Extensions.InsertPlayer(SteamID, "none", "none");
            response = $"Player with SteamID {SteamID} successfully added to the database!";
            return true;
        }
    }
}
