using CommandSystem;
using System;

namespace SteamSusAcc.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SteamAPICommad : ParentCommand
    {
        public SteamAPICommad() => LoadGeneratedCommands();

        public override string Command => "steamapi";

        public override string[] Aliases { get; } = { };

        public override string Description => "SteamAPI parent command";

        public sealed override void LoadGeneratedCommands()
        {
            RegisterCommand(new Add());
            RegisterCommand(new Info());
            RegisterCommand(new Remove());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = $"Usage: {Command} add/delete/show";
            return false;
        }
    }
}
