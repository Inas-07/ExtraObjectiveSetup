using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ExtraObjectiveSetup.JSON;
using ExtraObjectiveSetup.Expedition;
using ExtraObjectiveSetup.Expedition.Gears;
using ExtraObjectiveSetup.Expedition.IndividualGeneratorGroup;

namespace ExtraObjectiveSetup
{
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("GTFO.FloLib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(InjectLibUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(AUTHOR + "." + PLUGIN_NAME, PLUGIN_NAME, VERSION)]
    
    public class EntryPoint: BasePlugin
    {
        public const string AUTHOR = "Inas";
        public const string PLUGIN_NAME = "ExtraObjectiveSetup";
        public const string VERSION = "1.4.5";

        private Harmony m_Harmony;
        
        public override void Load()
        {
            m_Harmony = new Harmony("ExtraObjectiveSetup");
            m_Harmony.PatchAll();

            SetupManagers();
        }

        /// <summary>
        /// Explicitly invoke Init() to all managers to eager-load, which in the meantime defines chained puzzle creation order if any
        /// </summary>
        private void SetupManagers()
        {
            BatchBuildManager.Current.Init();

            Objectives.IndividualGenerator.IndividualGeneratorObjectiveManager.Current.Init();
            Objectives.GeneratorCluster.GeneratorClusterObjectiveManager.Current.Init();
            Objectives.ActivateSmallHSU.HSUActivatorObjectiveManager.Current.Init();
            Objectives.TerminalUplink.UplinkObjectiveManager.Current.Init();

            Tweaks.TerminalPosition.TerminalPositionOverrideManager.Current.Init();
            Tweaks.Scout.ScoutScreamEventManager.Current.Init();
            Tweaks.BossEvents.BossDeathEventManager.Current.Init();

            ExpeditionDefinitionManager.Current.Init();
            ExpeditionGearManager.Current.Init();
            ExpeditionIGGroupManager.Current.Init();

            Instances.GeneratorClusterInstanceManager.Current.Init();
            Instances.HSUActivatorInstanceManager.Current.Init();
            Instances.PowerGeneratorInstanceManager.Current.Init();
            Instances.TerminalInstanceManager.Current.Init();
        }
    }
}

