using System.IO;
using System;
using GTFO.API.Utilities;
using System.Collections.Generic;
using MTFO.API;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.JSON;
using GTFO.API;
using LevelGeneration;

namespace ExtraObjectiveSetup.Expedition
{
    public sealed class ExpeditionDefinitionManager
    {
        public static ExpeditionDefinitionManager Current { get; private set; } = new();

        public uint CurrentMainLevelLayout => RundownManager.ActiveExpedition.LevelLayoutData;

        private Dictionary<uint, ExpeditionDefinition> definitions = new();

        private readonly LiveEditListener liveEditListener;

        public const string DEFINITION_NAME = "ExtraExpeditionSettings";

        public string DEFINITION_PATH { get; private set; } = Path.Combine(MTFOPathAPI.CustomPath, "ExtraObjectiveSetup", DEFINITION_NAME);

        private void AddDefinitions(ExpeditionDefinition definitions)
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
                ExpeditionDefinition conf = EOSJson.Deserialize<ExpeditionDefinition>(content);
                AddDefinitions(conf);
            });
        }

        public ExpeditionDefinition GetDefinition(uint MainLevelLayout) => definitions.ContainsKey(MainLevelLayout) ? definitions[MainLevelLayout] : null;

        /// <summary>
        /// Finds the terminal definition whose global zone index matches the zone the terminal is in.
        /// </summary>
        public bool TryGetTerminalDefinitionFromInstance(LG_ComputerTerminal terminal, out ExpeditionTerminalsDefinition termDef)
        {
            termDef = null;
            if (terminal == null || !definitions.TryGetValue(CurrentMainLevelLayout, out var def)) return false;
            if (terminal.SpawnNode == null && terminal.ConnectedReactor == null) return false;

            var (dim, layer, zone) = TerminalInstanceManager.Current.GetGlobalZoneIndex(terminal);
            termDef = def.Terminals.Find(t => t.DimensionIndex == dim && t.LayerType == layer && t.LocalIndex == zone);

            return termDef != null;
        }

        public void Init() { }

        private ExpeditionDefinitionManager()
        {
            string MODULE_CUSTOM_FOLDER = Path.Combine(MTFOPathAPI.CustomPath, "ExtraObjectiveSetup");
            if (!Directory.Exists(MODULE_CUSTOM_FOLDER))
            {
                Directory.CreateDirectory(MODULE_CUSTOM_FOLDER);
            }

            if (!Directory.Exists(DEFINITION_PATH))
            {
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(EOSJson.Serialize(new ExpeditionDefinition()));
                file.Flush();
                file.Close();
            }

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    string content = File.ReadAllText(confFile);
                    ExpeditionDefinition conf = EOSJson.Deserialize<ExpeditionDefinition>(content);
                    AddDefinitions(conf);
                }
                catch (Exception ex)
                {
                    EOSLogger.Error($"ExpeditionDefinitionManager: an exception was thrown while reading files:\n{ex}");
                    continue;
                }
            }

            liveEditListener = LiveEdit.CreateListener(DEFINITION_PATH, "*.json", true);
            liveEditListener.FileChanged += FileChanged;
        }

        static ExpeditionDefinitionManager() { }
    }
}
