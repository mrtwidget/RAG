using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Rocket.Unturned;
using Rocket.Core.Plugins;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.API;
using Rocket.API.Collections;
using SDG.Unturned;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.Core.Steam;
using Steamworks;
using System.Text.RegularExpressions;
using System.Net;
using System.Text;
using System.Xml;
using System.Threading;

namespace RAG
{
    public class RAG : RocketPlugin<RAGConfiguration>
    {
        #region Fields

        public static RAG Instance;
        public Dictionary<CSteamID, Player> Players;
        public enum GameStates { Waiting = 1, Intermission = 2, Active = 3 }
        public GameStates MatchState;
        public DateTime? LastStateChange;
        #endregion

        #region Overrides

        protected override void Load()
        {
            Instance = this;
            Players = new Dictionary<CSteamID, Player>();
            MatchState = GameStates.Waiting;

            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerDeath += Events_OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerChatted += Events_OnPlayerChatted;
            UnturnedPlayerEvents.OnPlayerUpdateStat += Events_OnPlayerUpdateStat;

            Logger.Log("Loaded RAG!", ConsoleColor.DarkGreen);
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerDeath -= Events_OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerChatted -= Events_OnPlayerChatted;

            Logger.Log("Unloaded RAG", ConsoleColor.Green);
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList() {
                    {"rag_disabled", "RAG is currently unavailable"},
                    {"rag_spectator_mode", "Spectator Mode {0}!"},
                    {"rag_game_state", "{0} has started!"},
                    {"rag_player_status", "You have {0} Kills and {1} Deaths"}
                };
            }
        }

        #endregion

        #region Events

        public void Events_OnPlayerUpdateStat(UnturnedPlayer player, EPlayerStat stat)
        {
        }

        public void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            // add new player
            Player plr = new Player();
            plr.CharacterName = player.CharacterName;
            plr.SteamID = player.CSteamID;
            plr.Kills = 0;
            plr.Deaths = 0;
            plr.Spectator = false;

            Players.Add(player.CSteamID, plr);

            if (Configuration.Instance.Debug)
                Console.WriteLine(player.CharacterName + " (" + player.CSteamID + ") Connected!");
        }

        public void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
            // remove player
            Players.Remove(player.CSteamID);

            if (Configuration.Instance.Debug)
                Console.WriteLine(player.CharacterName + " (" + player.CSteamID + ") Disconnected!");
        }

        public void Events_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (murderer.ToString().StartsWith("7656"))
            {
                if (player.CSteamID == murderer)
                    return;

                // death effect for player
                player.TriggerEffect(123); // explode
                player.TriggerEffect(128); // expanding white circles

                // effect and payout for murderer
                UnturnedPlayer.FromCSteamID(murderer).TriggerEffect(131); // yellow aura scream

                // add stats
                Players[player.CSteamID].Deaths++;
                Players[murderer].Kills++;
            }
        }

        public void Events_OnPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        {
        }

        #endregion

        public void FixedUpdate()
        {
            if (Instance.State != PluginState.Loaded) return;

            // if minimum players join, start intermission
            if (MatchState == GameStates.Waiting && Players.Count() >= Configuration.Instance.MinPlayers)
            {
                MatchState = GameStates.Intermission;
                UnturnedChat.Say(Translations.Instance.Translate("rag_game_state", "Intermission"), Color.yellow);

                if (Configuration.Instance.Debug)
                    Console.WriteLine("Minimum players have joined. Starting Intermission!");
            }

            // if intermission finshes, start match
            if (MatchState == GameStates.Intermission)
            {
                if (LastStateChange == null)
                    LastStateChange = DateTime.Now;

                if ((DateTime.Now - LastStateChange.Value).TotalSeconds > Configuration.Instance.IntermissionLength)
                {
                    LastStateChange = DateTime.Now;

                    MatchState = GameStates.Active;
                    UnturnedChat.Say(Translations.Instance.Translate("rag_game_state", "Match"), Color.cyan);
                }
            }

            // if match finishes, start intermission
            if (MatchState == GameStates.Active)
            {
                if (LastStateChange == null)
                    LastStateChange = DateTime.Now;

                if ((DateTime.Now - LastStateChange.Value).TotalSeconds > Configuration.Instance.MatchLength)
                {
                    LastStateChange = DateTime.Now;

                    MatchState = GameStates.Intermission;
                    UnturnedChat.Say(Translations.Instance.Translate("rag_game_state", "Intermission"), Color.yellow);
                }
            }

        }
    }
}
