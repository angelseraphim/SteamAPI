using System;
using CommandSystem;
using static SteamSusAcc.DataBase.Data;
using Extensions = SteamSusAcc.DataBase.Extensions;

namespace SteamSusAcc
{
    internal class Info : ICommand
    {
        public string Command { get; } = "info";

        public string[] Aliases { get; } = { "check", "show" };

        public string Description { get; } = "Get info about player";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = $"Usage: steamapi {Command} (target SteamID)";
                return false;
            }
            string SteamID = arguments.At(0);
            if (!Extensions.TryGetValue(SteamID, out PlayerInfo info))
            {
                response = "UserID not found...";
                return false;
            }
            string Nicks = string.Empty;
            string IPs = string.Empty;
            foreach (string item in info.Nicknames)
            {
                Nicks += item + "\n";
            }
            foreach (string item in info.IPs)
            {
                IPs += item + "\n";
            }

            response = $"Info about {SteamID}:\nIPs:\n{IPs}\nNicknames:\n{Nicks}";
            return true;
        }
    }
}
