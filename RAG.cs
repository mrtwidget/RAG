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

        Vector3 ArenaSpawn = new Vector3((float)-371.2, (float)39.0, (float)-161.2);
        Vector3 IntermissionSpawn = new Vector3((float)-435.6, (float)80.3, (float)-435.5);

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
            UnturnedPlayerEvents.OnPlayerRevive += Events_OnPlayerRevive;
            UnturnedPlayerEvents.OnPlayerChatted += Events_OnPlayerChatted;
            UnturnedPlayerEvents.OnPlayerUpdateStat += Events_OnPlayerUpdateStat;

            Logger.Log("Loaded RAG!", ConsoleColor.DarkGreen);
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            UnturnedPlayerEvents.OnPlayerDeath -= Events_OnPlayerDeath;
            UnturnedPlayerEvents.OnPlayerRevive += Events_OnPlayerRevive;
            UnturnedPlayerEvents.OnPlayerChatted -= Events_OnPlayerChatted;
            UnturnedPlayerEvents.OnPlayerUpdateStat -= Events_OnPlayerUpdateStat;

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

            if (MatchState == GameStates.Active)
            {
                UnturnedChat.Say("Spawning into Arena in 3 seconds...", Color.gray);

                new Thread(() =>
                {
                    Thread.Sleep(3000);
                    Respawn(player);
                    UnturnedChat.Say("Spawned!", Color.yellow);
                }).Start();
            }
            else if (MatchState == GameStates.Intermission || MatchState == GameStates.Waiting)
            {
                player.Heal(255, false, false);
                player.MaxSkills();

                StripPlayerItems(player);
                player.Teleport(IntermissionSpawn, player.Rotation);
            }

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

        public void Events_OnPlayerRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            if (MatchState == GameStates.Active)
            {
                UnturnedChat.Say("Respawning in 3 seconds...", Color.gray);

                new Thread(() =>
                {
                    Thread.Sleep(3000);
                    Respawn(player);
                    UnturnedChat.Say("Respawned!", Color.yellow);
                }).Start();
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

                    Loadout(); // teleport and arm players

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

                    Intermission(); // return players to lobby

                    MatchState = GameStates.Intermission;
                    UnturnedChat.Say(Translations.Instance.Translate("rag_game_state", "Intermission"), Color.yellow);
                }
            }
        }

        public void Intermission()
        {
            for (int i = 0; i < Provider.clients.Count; i++)
            {
                SteamPlayer sp = Provider.clients[i];
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(sp);

                // heal player
                player.Heal(255, false, false);

                // maxskills
                player.MaxSkills();

                StripPlayerItems(player);

                // teleport to intermission area
                player.Teleport(IntermissionSpawn, player.Rotation);
            }
        }

        public void Loadout()
        {
            for (int i = 0; i < Provider.clients.Count; i++)
            {
                SteamPlayer sp = Provider.clients[i];
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(sp);

                // heal player
                player.Heal(255, false, false);

                // maxskills
                player.MaxSkills();

                StripPlayerItems(player);

                player.GiveItem(246, 1); // Blue Travelpack (246)
                player.GiveItem(1134, 1); // Blue Scarf (1134)
                player.GiveItem(173, 1); // Blue Hoodie (173)
                player.GiveItem(426, 1); // Blue Cap (426)
                player.GiveItem(435, 1); // Blue Balaclava (435)
                player.GiveItem(1455, 1); // Blue Trunks (1455)

                player.GiveItem(116, 1); // gun
                player.GiveItem(17, 3); // mag

                player.GiveItem(394, 3); // dressings

                // teleport to arena
                player.Teleport(ArenaSpawn, player.Rotation);
            }
        }

        public void Respawn(UnturnedPlayer player)
        {
            // heal player
            player.Heal(255, false, false);

            // maxskills
            player.MaxSkills();

            StripPlayerItems(player);

            player.GiveItem(246, 1); // Blue Travelpack (246)
            player.GiveItem(1134, 1); // Blue Scarf (1134)
            player.GiveItem(173, 1); // Blue Hoodie (173)
            player.GiveItem(426, 1); // Blue Cap (426)
            player.GiveItem(435, 1); // Blue Balaclava (435)
            player.GiveItem(1455, 1); // Blue Trunks (1455)

            player.GiveItem(116, 1); // gun
            player.GiveItem(17, 3); // mag

            player.GiveItem(394, 3); // dressings

            // teleport to blue start
            player.Teleport(ArenaSpawn, player.Rotation);
        }

        public void StripPlayerItems(UnturnedPlayer player)
        {
            try
            {
                for (byte page = 0; page < 8; page++)
                {
                    var count = player.Inventory.getItemCount(page);

                    for (byte index = 0; index < count; index++)
                        player.Inventory.removeItem(page, index);
                }

                // glasses
                player.Player.clothing.askWearGlasses(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // hat
                player.Player.clothing.askWearHat(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // mask
                player.Player.clothing.askWearMask(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // pants
                player.Player.clothing.askWearPants(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // shirt
                player.Player.clothing.askWearShirt(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // vest
                player.Player.clothing.askWearVest(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
                // backpack
                player.Player.clothing.askWearBackpack(0, 0, new byte[0], true);
                for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                {
                    player.Player.inventory.removeItem(2, 0);
                }
            }
            catch (Exception e) { Logger.Log("[ERROR] Clearing Player Inventory: " + e.ToString(), ConsoleColor.DarkRed); }
        }
    }
}
