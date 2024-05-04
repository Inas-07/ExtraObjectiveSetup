using System.IO;
using GTFO.API.Utilities;
using System.Collections.Generic;
using MTFO.API;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.JSON;

namespace ExtraObjectiveSetup.BaseClasses
{
    public abstract class GenericDefinitionManager<T> where T: new()
    {
        public uint CurrentMainLevelLayout => RundownManager.ActiveExpedition.LevelLayoutData;

        protected Dictionary<uint, GenericDefinition<T>> definitions { get; set; } = new();

        private readonly LiveEditListener liveEditListener;

        protected abstract string DEFINITION_NAME { get; }

        public string DEFINITION_PATH { get; private set; }

        protected virtual void AddDefinitions(GenericDefinition<T> definition)
        {
            if (definition == null) return;

            if (definitions.ContainsKey(definition.ID))
            {
                EOSLogger.Log("Replaced ID {0}", definition.ID);
            }

            definitions[definition.ID] = definition;
        }

        protected virtual void FileChanged(LiveEditEventArgs e)
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                var conf = EOSJson.Deserialize<GenericDefinition<T>>(content);
                AddDefinitions(conf);
            });
        }

        public GenericDefinition<T> GetDefinition(uint ID) => definitions.ContainsKey(ID) ? definitions[ID] : null;

        public virtual void Init() { }

        public GenericDefinitionManager()
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
                file.WriteLine(EOSJson.Serialize(new GenericDefinition<T>()));
                file.Flush();
                file.Close();
            }

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                var conf = EOSJson.Deserialize<GenericDefinition<T>>(content);

                AddDefinitions(conf);
            }

            liveEditListener = LiveEdit.CreateListener(DEFINITION_PATH, "*.json", true);
            liveEditListener.FileChanged += FileChanged;
        }

        static GenericDefinitionManager() { }
    }
}
