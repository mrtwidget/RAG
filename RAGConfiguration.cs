using Rocket.API;

namespace RAG
{
    public class RAGConfiguration : IRocketPluginConfiguration
    {
        public bool Debug;
        public int MinPlayers;
        public int MatchLength;
        public int IntermissionLength;
        public int RespawnLength;

        public void LoadDefaults()
        {
            Debug = true;
            MinPlayers = 2;
            MatchLength = 300;
            IntermissionLength = 60;
            RespawnLength = 10;
        }
    }
}