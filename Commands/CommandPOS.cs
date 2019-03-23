using System;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace NEXIS.TDM
{
    public class CommandPOS : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public bool AllowFromConsole => false;

        public string Name => "pos";

        public string Help => "Get your current position";

        public List<string> Aliases => new List<string>() { };

        public string Syntax => "/pos";

        public List<string> Permissions => new List<string>() { "pos" };

        #endregion

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            UnturnedChat.Say(caller, "Position: " + player.Position.ToString() + ", Rotation: " + player.Rotation.ToString(), Color.white);
        }
    }
}
