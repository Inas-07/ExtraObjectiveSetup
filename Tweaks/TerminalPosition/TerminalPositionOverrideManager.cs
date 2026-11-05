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

namespace ExtraObjectiveSetup.Tweaks.TerminalPosition
{
    internal class TerminalPositionOverrideManager: InstanceDefinitionManager<TerminalPosition>
    {
        public static TerminalPositionOverrideManager Current = new();

        protected override string DEFINITION_NAME => "TerminalPosition";

        private TerminalPositionOverrideManager() { }

        static TerminalPositionOverrideManager()
        {
            LevelAPI.OnBeforeBuildBatch += OnBeforeBuildBatch;
        }

        /// <summary>
        /// Terminals built during the level build get their spawn node reassigned once course nodes exist,
        /// which is right before the FunctionMarkerFallback batch runs.
        /// </summary>
        private static void OnBeforeBuildBatch(LG_Factory.BatchName batch)
        {
            if (batch != LG_Factory.BatchName.FunctionMarkerFallback) return;

            foreach (var def in GetDefinitionsOfActiveLevel())
            {
                if (!def.ReassignSpawnNode) continue;

                var terminal = TerminalInstanceManager.Current.GetInstance(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex);
                if (terminal == null || terminal.ConnectedReactor != null) continue;

                SetParentCourseNode(terminal, def);
            }
        }

        protected override void FileChanged(LiveEditEventArgs e)
        {
            base.FileChanged(e);

            if (GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            foreach (var def in GetDefinitionsOfActiveLevel())
            {
                var terminal = TerminalInstanceManager.Current.GetInstance(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex);
                if (terminal == null || terminal.ConnectedReactor != null) continue;
                if (def.Position.ToVector3() == Vector3.zero) continue;

                RepositionTerminal(terminal, def);
                SetParentCourseNode(terminal, def);
            }
        }

        internal void Setup(LG_ComputerTerminal terminal, TerminalPosition def)
        {
            if (terminal.ConnectedReactor != null || def.Position.ToVector3() == Vector3.zero) return;

            RepositionTerminal(terminal, def);

            // During the build, spawn node reassignment happens in OnBeforeBuildBatch instead
            if (GameStateManager.CurrentStateName == eGameStateName.Generating
                && LG_Factory.Current.m_currentBatchName >= LG_Factory.BatchName.FunctionMarkerFallback)
            {
                SetParentCourseNode(terminal, def);
            }
        }

        private static void RepositionTerminal(LG_ComputerTerminal terminal, TerminalPosition def)
        {
            var position = def.Position.ToVector3();
            var rotation = def.Rotation.ToQuaternion();

            terminal.m_sound.UpdatePosition(position);

            var markerProducer = terminal.GetComponentInParent<LG_MarkerProducer>();
            if ((!def.RepositionCover && !def.HideCover) || markerProducer == null)
            {
                terminal.transform.SetPositionAndRotation(position, rotation);
                return;
            }

            // The cover is a chain of single-child transforms that holds the terminal itself
            var markerTransform = markerProducer.transform;
            while (markerTransform.childCount == 1 && markerTransform.GetChild(0).GetComponent<LG_ComputerTerminal>() == null)
            {
                markerTransform = markerTransform.GetChild(0);
            }

            for (int i = 0; i < markerTransform.childCount; i++)
            {
                var markerChild = markerTransform.GetChild(i);
                var childTerminal = markerChild.GetComponentInChildren<LG_ComputerTerminal>(true);

                if (def.HideCover && childTerminal == null)
                {
                    markerChild.gameObject.SetActive(false);
                    continue;
                }

                markerChild.SetPositionAndRotation(position, rotation);
            }
        }

        private static void SetParentCourseNode(LG_ComputerTerminal terminal, TerminalPosition def)
        {
            if (!def.ReassignSpawnNode || terminal.SpawnNode == null) return;

            var courseNode = CourseNodeUtil.GetCourseNode(def.Position.ToVector3());
            if (courseNode == null || courseNode.NodeID == terminal.SpawnNode.NodeID) return;

            var sourceZoneID = terminal.SpawnNode.m_zone.ID;
            EOSLogger.Debug($"{terminal.PublicName} from Area_{terminal.SpawnNode.m_area.m_navInfo.Suffix} was repositioned to Area_{courseNode.m_area.m_navInfo.Suffix}");

            terminal.transform.SetParent(courseNode.m_area.transform, true);
            terminal.m_terminalItem.SpawnNode = courseNode;

            if (sourceZoneID != courseNode.m_zone.ID)
            {
                EOSLogger.Warning($"{terminal.PublicName} was repositioned to a different zone");
            }
        }

        private static List<TerminalPosition> GetDefinitionsOfActiveLevel()
        {
            uint layout = RundownManager.ActiveExpedition?.LevelLayoutData ?? 0u;
            return Current.GetDefinitionsForLevel(layout) ?? new List<TerminalPosition>();
        }
    }
}
