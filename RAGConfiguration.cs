using Rocket.API;

namespace RAG
{
    public class RAGConfiguration : IRocketPluginConfiguration
    {
        public bool Debug;
        public int MatchLength;
        public int IntermissionLength;

        public void LoadDefaults()
        {
            Debug = true;
            MatchLength = 600;
            IntermissionLength = 60;
        }
    }
}