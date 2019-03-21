using Rocket.API;

namespace RAG
{
    public class RAGConfiguration : IRocketPluginConfiguration
    {
        public bool Debug;

        public void LoadDefaults()
        {
            Debug = true;
        }
    }
}