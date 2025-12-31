using System.IO;
using GTFO.API.Utilities;
using System.Collections.Generic;
using MTFO.API;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.JSON;

namespace ExtraObjectiveSetup.BaseClasses
{
    public abstract class RundownWiseDefinitionManager<T> where T: new()
    {
        public const int INVALID_RUNDOWN_ID = -1;
        
        public const int APPLY_TO_ALL_RUNDOWN_ID = 0;

        protected Dictionary<int, RundownWiseDefinition<T>> definitions { get; set; } = new();

        private readonly LiveEditListener liveEditListener;

        protected abstract string DEFINITION_NAME { get; }

        public string DEFINITION_PATH { get; private set; }

        protected virtual void AddDefinitions(RundownWiseDefinition<T> definitions)
        {
            if (definitions == null || definitions.RundownID == INVALID_RUNDOWN_ID) return;

            if (this.definitions.ContainsKey(definitions.RundownID))
            {
                EOSLogger.Log($"Replaced RundownID: '{definitions.RundownID}' ({APPLY_TO_ALL_RUNDOWN_ID} means 'apply to all rundowns')");
            }

            this.definitions[definitions.RundownID] = definitions;
        }

        protected virtual void FileChanged(LiveEditEventArgs e)
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                RundownWiseDefinition<T> conf = EOSJson.Deserialize<RundownWiseDefinition<T>>(content);
                AddDefinitions(conf);
            });
        }

        public RundownWiseDefinition<T> GetDefinition(uint RundownID) => definitions.TryGetValue((int)RundownID, out var def) ? def : null; 

        public virtual void Init() { }

        public RundownWiseDefinitionManager()
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
                try
                {
                    string content = File.ReadAllText(confFile);
                    var conf = EOSJson.Deserialize<RundownWiseDefinition<T>>(content);
                    AddDefinitions(conf);
                }
                catch (Exception ex)
                {
                    EOSLogger.Error($"RundownWiseDefinitionManager: an exception was thrown while reading files:\n{ex}");
                    continue;
                }
            }

            liveEditListener = LiveEdit.CreateListener(DEFINITION_PATH, "*.json", true);
            liveEditListener.FileChanged += FileChanged;
        }

        static RundownWiseDefinitionManager() { }
    }
}
