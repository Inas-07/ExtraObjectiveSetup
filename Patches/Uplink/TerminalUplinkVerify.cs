using HarmonyLib;
using LevelGeneration;
using Localization;
using GameData;
using ChainedPuzzles;
using ExtraObjectiveSetup.Objectives.TerminalUplink;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Utils;
using SNetwork;

namespace ExtraObjectiveSetup.Patches.Uplink
{
    [HarmonyPatch]
    internal class TerminalUplinkVerify
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.TerminalUplinkVerify))]
        private static bool Pre_LG_ComputerTerminalCommandInterpreter_TerminalUplinkVerify(LG_ComputerTerminalCommandInterpreter __instance, string param1, string param2, ref bool __result)
        {
            if (__instance.m_terminal.m_isWardenObjective) return true; // vanilla uplink
                                                                        // corr log is sent in TerminalUplinkPuzzle.
            var uplinkPuzzle = __instance.m_terminal.UplinkPuzzle;

            var globalIndex = TerminalInstanceManager.Current.GetGlobalZoneIndex(__instance.m_terminal);
            var instanceIndex = TerminalInstanceManager.Current.GetZoneInstanceIndex(__instance.m_terminal);
            var uplinkConfig = UplinkObjectiveManager.Current.GetDefinition(globalIndex, instanceIndex);

            int CurrentRoundIndex = uplinkPuzzle.m_roundIndex;
            int i = uplinkConfig.RoundOverrides.FindIndex(o => o.RoundIndex == CurrentRoundIndex);
            UplinkRound roundOverride = i != -1 ? uplinkConfig.RoundOverrides[i] : null;
            TimeSettings timeSettings = i != -1 ? roundOverride.OverrideTimeSettings : uplinkConfig.DefaultTimeSettings;

            float timeToStartVerify = timeSettings.TimeToStartVerify >= 0f ? timeSettings.TimeToStartVerify : uplinkConfig.DefaultTimeSettings.TimeToStartVerify;
            float timeToCompleteVerify = timeSettings.TimeToCompleteVerify >= 0f ? timeSettings.TimeToCompleteVerify : uplinkConfig.DefaultTimeSettings.TimeToCompleteVerify;
            float timeToRestoreFromFail = timeSettings.TimeToRestoreFromFail >= 0f ? timeSettings.TimeToRestoreFromFail : uplinkConfig.DefaultTimeSettings.TimeToRestoreFromFail;

            if (uplinkPuzzle.Connected)
            {
                // Attempting uplink verification
                __instance.AddOutput(TerminalLineType.SpinningWaitNoDone, Text.Get(2734004688), timeToStartVerify);

                // correct verification 
                if (!uplinkPuzzle.Solved && uplinkPuzzle.CurrentRound.CorrectCode.ToUpper() == param1.ToUpper())
                {
                    // Verification code {0} correct
                    __instance.AddOutput(string.Format(Text.Get(1221800228), uplinkPuzzle.CurrentProgress));
                    if (uplinkPuzzle.TryGoToNextRound()) // Goto next round
                    {
                        int newRoundIndex = uplinkPuzzle.m_roundIndex;
                        int j = uplinkConfig.RoundOverrides.FindIndex(o => o.RoundIndex == newRoundIndex);
                        UplinkRound newRoundOverride = j != -1 ? uplinkConfig.RoundOverrides[j] : null;

                        roundOverride?.EventsOnRound.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.OnMid, false));

                        if (roundOverride != null && roundOverride.ChainedPuzzleToEndRoundInstance != null)
                        {
                            TextDataBlock block = GameDataBlockBase<TextDataBlock>.GetBlock("InGame.UplinkTerminal.ScanRequiredToProgress");
                            if (block != null)
                            {
                                __instance.AddOutput(TerminalLineType.ProgressWait, Text.Get(block.persistentID));
                            }

                            roundOverride.ChainedPuzzleToEndRoundInstance.OnPuzzleSolved += new System.Action(() =>
                            {
                                __instance.AddOutput(TerminalLineType.ProgressWait, Text.Get(27959760), timeToCompleteVerify); // "Building uplink verification signature"
                                __instance.AddOutput("");
                                __instance.AddOutput(string.Format(Text.Get(4269617288), uplinkPuzzle.CurrentProgress, uplinkPuzzle.CurrentRound.CorrectPrefix));
                                __instance.OnEndOfQueue = new System.Action(() =>
                                {
                                    EOSLogger.Log("UPLINK VERIFICATION GO TO NEXT ROUND!");
                                    uplinkPuzzle.CurrentRound.ShowGui = true;
                                    newRoundOverride?.EventsOnRound.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.OnStart, false));
                                    UplinkObjectiveManager.Current.ChangeState(__instance.m_terminal, new() { Status = UplinkStatus.InProgress, CurrentRoundIndex = uplinkPuzzle.m_roundIndex });
                                });
                            });

                            if (SNet.IsMaster)
                            {
                                roundOverride.ChainedPuzzleToEndRoundInstance.AttemptInteract(eChainedPuzzleInteraction.Activate);
                            }
                        }
                        else
                        {
                            __instance.AddOutput(TerminalLineType.ProgressWait, Text.Get(27959760), timeToCompleteVerify); // "Building uplink verification signature"
                            __instance.AddOutput("");
                            __instance.AddOutput(string.Format(Text.Get(4269617288), uplinkPuzzle.CurrentProgress, uplinkPuzzle.CurrentRound.CorrectPrefix));

                            __instance.OnEndOfQueue = new System.Action(() =>
                            {
                                EOSLogger.Log("UPLINK VERIFICATION GO TO NEXT ROUND!");
                                uplinkPuzzle.CurrentRound.ShowGui = true;
                                newRoundOverride?.EventsOnRound.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.OnStart, false));
                                UplinkObjectiveManager.Current.ChangeState(__instance.m_terminal, new() { Status = UplinkStatus.InProgress, CurrentRoundIndex = uplinkPuzzle.m_roundIndex });
                            });
                        }
                    }
                    else // uplink done
                    {
                        __instance.AddOutput(TerminalLineType.SpinningWaitNoDone, Text.Get(1780488547), 3f);
                        __instance.AddOutput("");
                        __instance.OnEndOfQueue = new System.Action(() =>
                        {
                            roundOverride?.EventsOnRound.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.OnMid, false));
                            uplinkPuzzle.CurrentRound.ShowGui = false;

                            if (roundOverride != null && roundOverride.ChainedPuzzleToEndRoundInstance != null)
                            {
                                roundOverride.ChainedPuzzleToEndRoundInstance.OnPuzzleSolved += new System.Action(() =>
                                {
                                    __instance.AddOutput(TerminalLineType.Normal, string.Format(Text.Get(3928683780), uplinkPuzzle.TerminalUplinkIP), 2f); // establish succeed
                                    __instance.AddOutput("");

                                    __instance.OnEndOfQueue = new System.Action(() =>
                                    {
                                        EOSLogger.Error("UPLINK VERIFICATION SEQUENCE DONE!");
                                        LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId = 0U;
                                        uplinkPuzzle.Solved = true;

                                        // Tested, it's save to do this
                                        uplinkPuzzle.OnPuzzleSolved?.Invoke();
                                        UplinkObjectiveManager.Current.ChangeState(__instance.m_terminal, new() { Status = UplinkStatus.Finished, CurrentRoundIndex = uplinkPuzzle.m_roundIndex });
                                    });
                                });

                                if (SNet.IsMaster)
                                {
                                    roundOverride.ChainedPuzzleToEndRoundInstance.AttemptInteract(eChainedPuzzleInteraction.Activate);
                                }
                            }
                            else
                            {
                                __instance.AddOutput(TerminalLineType.Normal, string.Format(Text.Get(3928683780), uplinkPuzzle.TerminalUplinkIP), 2f); // establish succeed
                                __instance.AddOutput("");

                                EOSLogger.Error("UPLINK VERIFICATION SEQUENCE DONE!");
                                LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId = 0U;
                                uplinkPuzzle.Solved = true;

                                uplinkPuzzle.OnPuzzleSolved?.Invoke(); 
                                UplinkObjectiveManager.Current.ChangeState(__instance.m_terminal, new() { Status = UplinkStatus.Finished, CurrentRoundIndex = uplinkPuzzle.m_roundIndex });
                            }
                        });
                    }
                }
                else if (uplinkPuzzle.Solved) // already solved
                {
                    __instance.AddOutput("");
                    __instance.AddOutput(TerminalLineType.Fail, Text.Get(4080876165)); // failed, already done
                    __instance.AddOutput(TerminalLineType.Normal, Text.Get(4104839742), 6f); // "Returning to root.."
                }
                else // incorrect verification
                {
                    __instance.AddOutput("");
                    __instance.AddOutput(TerminalLineType.Fail, string.Format(Text.Get(507647514), uplinkPuzzle.CurrentRound.CorrectPrefix)); //"Verfication failed! Use public key <color=orange>" + + "</color> to obtain the code");
                    __instance.AddOutput(TerminalLineType.Normal, Text.Get(4104839742), timeToRestoreFromFail);
                }
            }
            else // unconnected
            {
                __instance.AddOutput("");
                __instance.AddOutput(Text.Get(403360908));
            }

            __result = false;
            return false;
        }

    }
}
