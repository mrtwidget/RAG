using System;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace RAG.Commands
{
    public class CommandArena : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public bool AllowFromConsole => false;

        public string Name => "arena";

        public string Help => "Vote for next Arena location";

        public List<string> Aliases => new List<string>() { };

        public string Syntax => "/arena";

        public List<string> Permissions => new List<string>() { "arena" };

        #endregion

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;


        }
    }
}
