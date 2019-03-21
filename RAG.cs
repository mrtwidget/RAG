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

        #endregion

        #region Overrides

        protected override void Load()
        {
            Instance = this;
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
                    {"rag_disabled", "RAG is currently unavailable"}
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
        }

        public void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
        }

        public void Events_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
        }

        public void Events_OnPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        {
        }

        #endregion

        public void FixedUpdate()
        {
            if (Instance.State != PluginState.Loaded) return;
            
        }
    }
}
