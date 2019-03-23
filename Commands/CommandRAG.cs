using System;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace RAG.Commands
{
    public class CommandRAG : IRocketCommand
    {
        #region Properties

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public bool AllowFromConsole => false;

        public string Name => "rag";

        public string Help => "Run and Gun tools";

        public List<string> Aliases => new List<string>() { };

        public string Syntax => "/rag";

        public List<string> Permissions => new List<string>() { "rag" };

        #endregion

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, RAG.Instance.Translations.Instance.Translate("rag_player_status", RAG.Instance.Players[player.CSteamID].Kills, RAG.Instance.Players[player.CSteamID].Deaths), Color.white);
                return;
            }

            switch (command[0])
            {
                case "start":
                    // start the match
                    RAG.Instance.LastStateChange = DateTime.Now;
                    RAG.Instance.MatchState = RAG.GameStates.Active;
                    RAG.Instance.Loadout(); // teleport and arm players
                    UnturnedChat.Say(RAG.Instance.Translations.Instance.Translate("rag_game_state", "Match"), Color.cyan);
                    break;
                case "stop":
                    // stop the match
                    RAG.Instance.LastStateChange = DateTime.Now;
                    RAG.Instance.MatchState = RAG.GameStates.Intermission;
                    RAG.Instance.Intermission(); // return players to lobby
                    UnturnedChat.Say(RAG.Instance.Translations.Instance.Translate("rag_game_state", "Intermission"), Color.yellow);
                    break;
            }

        }
    }
}
