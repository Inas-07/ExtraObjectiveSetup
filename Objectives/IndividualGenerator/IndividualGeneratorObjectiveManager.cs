using AmorLib.Utils;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Utils;
using GameData;
using GTFO.API;
using GTFO.API.Utilities;
using LevelGeneration;
using System.Collections.Generic;
using UnityEngine;

namespace ExtraObjectiveSetup.Objectives.IndividualGenerator
{
    internal sealed class IndividualGeneratorObjectiveManager : InstanceDefinitionManager<IndividualGeneratorDefinition>
    {
        public static IndividualGeneratorObjectiveManager Current { private set; get; } = new();

        protected override string DEFINITION_NAME { get; } = "IndividualGenerator";

        private IndividualGeneratorObjectiveManager() : base() { }

        static IndividualGeneratorObjectiveManager()
        {
            LevelAPI.OnBeforeBuildBatch += OnBeforeBuildBatch;
        }

        /// <summary>
        /// Generators built during the level build get their spawn node reassigned once course nodes exist,
        /// which is right before the FunctionMarkerFallback batch runs.
        /// </summary>
        private static void OnBeforeBuildBatch(LG_Factory.BatchName batch)
        {
            if (batch != LG_Factory.BatchName.FunctionMarkerFallback) return;

            foreach (var def in GetDefinitionsOfActiveLevel())
            {
                if (!def.ReassignSpawnNode) continue;

                var generator = PowerGeneratorInstanceManager.Current.GetInstance(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex);
                if (generator == null) continue;

                SetParentCourseNode(generator, def);
            }
        }

        protected override void FileChanged(LiveEditEventArgs e)
        {
            base.FileChanged(e);

            if (GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            foreach (var def in GetDefinitionsOfActiveLevel())
            {
                var generator = PowerGeneratorInstanceManager.Current.GetInstance(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex);
                if (generator == null) continue;
                if (def.Position.ToVector3() == Vector3.zero) continue;

                RepositionGenerator(generator, def);
                SetParentCourseNode(generator, def);
            }
        }

        internal static void ApplyDefinition(LG_PowerGenerator_Core generator, IndividualGeneratorDefinition def)
        {
            RepositionGenerator(generator, def);

            // During the build, spawn node reassignment happens in OnBeforeBuildBatch instead
            if (GameStateManager.CurrentStateName == eGameStateName.Generating
                && LG_Factory.Current.m_currentBatchName >= LG_Factory.BatchName.FunctionMarkerFallback)
            {
                SetParentCourseNode(generator, def);
            }
        }

        private static void RepositionGenerator(LG_PowerGenerator_Core generator, IndividualGeneratorDefinition def)
        {
            var position = def.Position.ToVector3();
            var rotation = def.Rotation.ToQuaternion();

            generator.m_sound.UpdatePosition(position);

            var markerProducer = generator.GetComponentInParent<LG_MarkerProducer>();
            if ((!def.RepositionCover && !def.HideCover) || markerProducer == null)
            {
                generator.transform.SetPositionAndRotation(position, rotation);
                return;
            }

            // The cover is a chain of single-child transforms that holds the generator itself
            var markerTransform = markerProducer.transform;
            while (markerTransform.childCount == 1 && markerTransform.GetChild(0).GetComponent<LG_PowerGenerator_Core>() == null)
            {
                markerTransform = markerTransform.GetChild(0);
            }

            for (int i = 0; i < markerTransform.childCount; i++)
            {
                var markerChild = markerTransform.GetChild(i);
                var childGenerator = markerChild.GetComponentInChildren<LG_PowerGenerator_Core>(true);

                if (def.HideCover && childGenerator == null)
                {
                    markerChild.gameObject.SetActive(false);
                    continue;
                }

                markerChild.SetPositionAndRotation(position, rotation);
            }
        }

        private static void SetParentCourseNode(LG_PowerGenerator_Core generator, IndividualGeneratorDefinition def)
        {
            if (!def.ReassignSpawnNode || generator.SpawnNode == null) return;

            var courseNode = CourseNodeUtil.GetCourseNode(def.Position.ToVector3());
            if (courseNode == null || courseNode.NodeID == generator.SpawnNode.NodeID) return;

            var sourceZoneID = generator.SpawnNode.m_zone.ID;
            EOSLogger.Debug($"{generator.PublicName} from Area_{generator.SpawnNode.m_area.m_navInfo.Suffix} was repositioned to Area_{courseNode.m_area.m_navInfo.Suffix}");

            generator.transform.SetParent(courseNode.m_area.transform, true);
            generator.m_terminalItem.SpawnNode = courseNode;

            if (sourceZoneID != courseNode.m_zone.ID)
            {
                EOSLogger.Warning($"{generator.PublicName} was repositioned to a different zone");
            }
        }

        private static List<IndividualGeneratorDefinition> GetDefinitionsOfActiveLevel()
        {
            uint layout = RundownManager.ActiveExpedition?.LevelLayoutData ?? 0u;
            return Current.GetDefinitionsForLevel(layout) ?? new List<IndividualGeneratorDefinition>();
        }
    }
}
