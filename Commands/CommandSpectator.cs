using System;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace RAG.Commands
{
    public class CommandSpectator : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public bool AllowFromConsole => false;

        public string Name => "spectator";

        public string Help => "Enable/Disable Spectator Mode";

        public List<string> Aliases => new List<string>() { };

        public string Syntax => "/spectator";

        public List<string> Permissions => new List<string>() { "spectator", "spectate" };

        #endregion

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (RAG.Instance.Players[player.CSteamID].Spectator)
            {
                RAG.Instance.Players[player.CSteamID].Spectator = false; // spectator OFF
                UnturnedChat.Say(caller, RAG.Instance.Translations.Instance.Translate("rag_spectator_mode", "Disabled"), Color.gray);

                if (RAG.Instance.Configuration.Instance.Debug)
                    Console.WriteLine(player.CharacterName + " (" + player.CSteamID + ") DISABLED Spectator Mode");
            }                
            else
            {
                RAG.Instance.Players[player.CSteamID].Spectator = true; // spectator ON
                UnturnedChat.Say(caller, RAG.Instance.Translations.Instance.Translate("rag_spectator_mode", "Enabled"), Color.yellow);

                if (RAG.Instance.Configuration.Instance.Debug)
                    Console.WriteLine(player.CharacterName + " (" + player.CSteamID + ") ENABLED Spectator Mode");
            }
        }
    }
}