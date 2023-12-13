using System.IO;
using GTFO.API.Utilities;
using System.Collections.Generic;
using MTFO.API;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.JSON;

namespace ExtraObjectiveSetup.BaseClasses
{
    public abstract class GenericExpeditionDefinitionManager<T> where T: new()
    {
        public uint CurrentMainLevelLayout => RundownManager.ActiveExpedition.LevelLayoutData;

        protected Dictionary<uint, GenericExpeditionDefinition<T>> definitions { get; set; } = new();

        private readonly LiveEditListener liveEditListener;

        protected abstract string DEFINITION_NAME { get; }

        public string DEFINITION_PATH { get; private set; }

        private void AddDefinitions(GenericExpeditionDefinition<T> definitions)
        {
            if (definitions == null) return;

            if (this.definitions.ContainsKey(definitions.MainLevelLayout))
            {
                EOSLogger.Log("Replaced MainLevelLayout {0}", definitions.MainLevelLayout);
            }

            this.definitions[definitions.MainLevelLayout] = definitions;
        }

        private void FileChanged(LiveEditEventArgs e)
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                GenericExpeditionDefinition<T> conf = EOSJson.Deserialize<GenericExpeditionDefinition<T>>(content);
                AddDefinitions(conf);
            });
        }

        public GenericExpeditionDefinition<T> GetDefinition(uint MainLevelLayout) => definitions.ContainsKey(MainLevelLayout) ? definitions[MainLevelLayout] : null;

        public void Init() { }

        public GenericExpeditionDefinitionManager()
        {
            string MODULE_CUSTOM_FOLDER = Path.Combine(MTFOPathAPI.CustomPath, "ExtraObjectiveSetup");
            if (!Directory.Exists(MODULE_CUSTOM_FOLDER))
            {
                Directory.CreateDirectory(MODULE_CUSTOM_FOLDER);
            }

            DEFINITION_PATH = Path.Combine(MODULE_CUSTOM_FOLDER, DEFINITION_NAME);
            if (!Directory.Exists(DEFINITION_PATH))
            {
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(EOSJson.Serialize(new GenericExpeditionDefinition<T>()));
                file.Flush();
                file.Close();
            }

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                var conf = EOSJson.Deserialize<GenericExpeditionDefinition<T>>(content);

                AddDefinitions(conf);
            }

            liveEditListener = LiveEdit.CreateListener(DEFINITION_PATH, "*.json", true);
            liveEditListener.FileChanged += FileChanged;
        }

        static GenericExpeditionDefinitionManager() { }
    }
}
