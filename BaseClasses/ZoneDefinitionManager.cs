using System;
using System.IO;
using GTFO.API.Utilities;
using System.Collections.Generic;
using MTFO.API;
using ExtraObjectiveSetup.Utils;
using LevelGeneration;
using GameData;
using ExtraObjectiveSetup.JSON;

namespace ExtraObjectiveSetup.BaseClasses
{
    /// <summary>
    /// Zone tweaks holder.
    /// This is a copy of InstanceDefinitionManager, with generic type bound being modified
    /// </summary>
    /// <typeparam name="T"> Common base class of objective definition. </typeparam>
    public abstract class ZoneDefinitionManager<T> where T : GlobalZoneIndex, new()
    {
        /// <summary>
        /// Path to the common parent folder of ALL definitions 
        /// </summary>
        public static string MODULE_CUSTOM_FOLDER { get; private set; } = Path.Combine(MTFOPathAPI.CustomPath, "ExtraObjectiveSetup");

        /// <summary>
        /// Definitions holder. Hold all definitions of this type (the generic type T) for the loaded rundown(s) (or, to be more specific, profile).
        /// Subclasses should not modify its contents, except properies with [JsonIgnore] attribute.
        /// </summary>
        protected Dictionary<uint, ZoneDefinitionsForLevel<T>> definitions = new();

        /// <summary>
        /// LiveEditListener to enable live edit for this definition.
        /// Subclasses may add additional operations upon live-edit events via this listener.
        /// </summary>
        protected readonly LiveEditListener liveEditListener;

        /// <summary>
        /// The name of definition to hold. This defines the definition file path from which to fetch, live-edit definition data. 
        /// </summary>
        protected abstract string DEFINITION_NAME { get; }

        /// <summary>
        /// Path to all definition files of this type (generic type T) of definition. 
        /// </summary>
        protected string DEFINITION_PATH { get; private set; } 

        /// <summary>
        /// Utility method. Sort definitions by dimension index, layer type, local index and instance index.
        /// </summary>
        protected void Sort(ZoneDefinitionsForLevel<T> levelDefs)
        {
            levelDefs.Definitions.Sort((u1, u2) =>
            {
                if (u1.DimensionIndex != u2.DimensionIndex) return (int)u1.DimensionIndex < (int)u2.DimensionIndex ? -1 : 1;
                if (u1.LayerType != u2.LayerType) return (int)u1.LayerType < (int)u2.LayerType ? -1 : 1;
                if (u1.LocalIndex != u2.LocalIndex) return (int)u1.LocalIndex < (int)u2.LocalIndex ? -1 : 1;
                return 0;
            });
        }

        /// <summary>
        /// Add objective definitions for a level.
        /// </summary>
        /// <param name="definitions">definitions of a level to be added</param>
        protected virtual void AddDefinitions(ZoneDefinitionsForLevel<T> definitions)
        {
            if (definitions == null) return;

            if (this.definitions.ContainsKey(definitions.MainLevelLayout))
            {
                EOSLogger.Log("Replaced MainLevelLayout {0}", definitions.MainLevelLayout);
            }

            this.definitions[definitions.MainLevelLayout] = definitions;
        }

        /// <summary>
        /// Common callback function of live-edit, implements definition add / update.
        /// If additional operation is needed in subclasses, turn to `liveEditListener` instead.
        /// </summary>
        /// <param name="e">live edit args</param>
        protected virtual void FileChanged(LiveEditEventArgs e)
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                ZoneDefinitionsForLevel<T> conf = EOSJson.Deserialize<ZoneDefinitionsForLevel<T>>(content);
                AddDefinitions(conf);
            });
        }

        public virtual List<T> GetDefinitionsForLevel(uint MainLevelLayout) => definitions.ContainsKey(MainLevelLayout) ? definitions[MainLevelLayout].Definitions : null;

        /// <summary>
        /// Get definitni
        /// </summary>
        /// <param name="globalIndex"></param>
        /// <param name="instanceIndex"></param>
        /// <returns></returns>
        public virtual T GetDefinition((eDimensionIndex, LG_LayerType, eLocalZoneIndex) globalIndex) => GetDefinition(globalIndex.Item1, globalIndex.Item2, globalIndex.Item3);

        public virtual T GetDefinition(eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex)
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return null;

            return definitions[RundownManager.ActiveExpedition.LevelLayoutData]
                .Definitions.Find(def => def.DimensionIndex == dimensionIndex && def.LayerType == layerType && def.LocalIndex == localIndex);
        }

        /// <summary>
        /// Initialize this definition manager. Subclasses are not required to implement this method. 
        /// Invoke this method to eagerly load the definition manager.
        /// </summary>
        public virtual void Init() { }

        protected ZoneDefinitionManager()
        {
            if (!Directory.Exists(MODULE_CUSTOM_FOLDER))
            {
                Directory.CreateDirectory(MODULE_CUSTOM_FOLDER);
            }

            DEFINITION_PATH = Path.Combine(MODULE_CUSTOM_FOLDER, DEFINITION_NAME);

            if (!Directory.Exists(DEFINITION_PATH))
            {
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(EOSJson.Serialize(new ZoneDefinitionsForLevel<T>()));
                file.Flush();
                file.Close();
            }

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                ZoneDefinitionsForLevel<T> conf = EOSJson.Deserialize<ZoneDefinitionsForLevel<T>>(content);

                AddDefinitions(conf);
            }

            liveEditListener = LiveEdit.CreateListener(DEFINITION_PATH, "*.json", true);
            liveEditListener.FileChanged += FileChanged;
        }

        static ZoneDefinitionManager() { }
    }
}
