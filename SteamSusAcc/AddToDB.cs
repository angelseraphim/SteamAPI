using System;
using CommandSystem;
using static SteamSusAcc.DataBase.Data;
using Extensions = SteamSusAcc.DataBase.Extensions;

namespace SteamSusAcc
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    class AddToDB : ICommand
    {
        public string Command { get; } = "steamapi";

        public string[] Aliases { get; } = { };

        public string Description { get; } = "Add or remove a player from the database";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count != 2)
            {
                response = "Usage: steamapi add/remove (target SteamID)";
                return false;
            }
            switch (arguments.At(0))
            {
                case "add":
                    Plugin.plugin.AddToData(arguments.At(1));
                    response = $"Player with SteamID {arguments.At(1)} successfully added to the database!";
                    return true;
                case "remove":
                    if (!Extensions.GetPlayer(arguments.At(1), out PlayerInfo info))
                    {
                        response = $"Player with SteamID {arguments.At(1)} not found!";
                        return false;
                    }
                    Extensions.DeletePlayer(arguments.At(1));
                    response = $"Player with SteamID {arguments.At(1)} successfully removed from the database!";
                    return true;
                default:
                    response = "Usage: steamapi add/remove (target SteamID)";
                    return false;
            }
        }
    }
}
