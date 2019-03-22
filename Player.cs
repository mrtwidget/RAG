using System;
using Steamworks;

namespace RAG
{
    public class Player
    {
        public CSteamID SteamID { get; set; }
        public string CharacterName { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public bool Spectator { get; set; }
    }
}