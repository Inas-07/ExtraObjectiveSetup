using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using AmorLib.Dependencies;
using ExtraObjectiveSetup.Expedition;
using ExtraObjectiveSetup.Expedition.Gears;
using ExtraObjectiveSetup.Expedition.IndividualGeneratorGroup;
using ExtraObjectiveSetup.Utils;

namespace ExtraObjectiveSetup
{
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("Amor.AmorLib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.dak.MTFO", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(PartialData_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(InjectLib_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.sinai.unityexplorer", BepInDependency.DependencyFlags.SoftDependency)] // try prevent CTD
    [BepInPlugin(AUTHOR + "." + PLUGIN_NAME, PLUGIN_NAME, VERSION)]
    [BepInIncompatibility("Amor.ExcellentObjectiveSetup")]
    public class EntryPoint: BasePlugin
    {
        public const string AUTHOR = "Inas";
        public const string PLUGIN_NAME = "ExtraObjectiveSetup";
        public const string VERSION = "1.8.0";

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
            LocalPlayer.Init();
            BatchBuildManager.Current.Init();
            Tweaks.TerminalTweak.SerialGeneratorManager.Init();

            Objectives.IndividualGenerator.IndividualGeneratorObjectiveManager.Current.Init();
            Objectives.GeneratorCluster.GeneratorClusterObjectiveManager.Current.Init();
            Objectives.ActivateSmallHSU.HSUActivatorObjectiveManager.Current.Init();
            Objectives.TerminalUplink.UplinkObjectiveManager.Current.Init();

            Tweaks.TerminalPosition.TerminalPositionOverrideManager.Current.Init();
            Tweaks.Scout.ScoutScreamEventManager.Current.Init();
            Tweaks.BossEvents.BossDeathEventManager.Current.Init();
            //Tweaks.TerminalTweak.TerminalTweakManager.Current.Init();

            ExpeditionDefinitionManager.Current.Init();
            ExpeditionGearManager.Current.Init();
            ExpeditionIGGroupManager.Current.Init();

            Instances.GeneratorClusterInstanceManager.Current.Init();
            Instances.HSUActivatorInstanceManager.Current.Init();
            Instances.PowerGeneratorInstanceManager.Current.Init();
            Instances.TerminalInstanceManager.Current.Init();

            Objectives.ObjectiveCounter.ObjectiveCounterManager.Current.Init();
        }
    }
}

