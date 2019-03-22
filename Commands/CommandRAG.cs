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

            if (command.Length > 1)
            {
                if (command[0] == "start")
                {
                    RAG.Instance.LastStateChange = DateTime.Now;
                    RAG.Instance.MatchState = RAG.GameStates.Active;
                    UnturnedChat.Say(RAG.Instance.Translations.Instance.Translate("rag_game_state", "Match"), Color.cyan);
                }                    
                else if(command[0] == "stop")
                {
                    RAG.Instance.LastStateChange = DateTime.Now;
                    RAG.Instance.MatchState = RAG.GameStates.Intermission;
                    UnturnedChat.Say(RAG.Instance.Translations.Instance.Translate("rag_game_state", "Intermission"), Color.yellow);
                }                    
            }
            else
            {
                UnturnedChat.Say(caller, RAG.Instance.Translations.Instance.Translate("rag_player_status", RAG.Instance.Players[player.CSteamID].Kills, RAG.Instance.Players[player.CSteamID].Deaths), Color.white);
            }
            
        }
    }
}
