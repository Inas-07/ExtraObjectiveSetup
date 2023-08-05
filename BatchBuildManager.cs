using System;
using LevelGeneration;
using System.Collections.Generic;

namespace ExtraObjectiveSetup
{
    public sealed class BatchBuildManager
    {
        public static BatchBuildManager Current { get; private set; } = new();

        private Dictionary<LG_Factory.BatchName, Action> OnBatchDone = new();

        public void Init()
        {
            if (OnBatchDone.Count > 0) return;

            foreach (var batchName in Enum.GetValues(typeof(LG_Factory.BatchName)))
            {
                OnBatchDone[(LG_Factory.BatchName)batchName] = null;
            }
        }

        public void Add_OnBatchDone(LG_Factory.BatchName batchName, Action action) 
        {
            OnBatchDone[batchName] += action;
        }

        public void Remove_OnBatchDone(LG_Factory.BatchName batchName, Action action)
        {
            OnBatchDone[batchName] -= action;
        }

        public Action Get_OnBatchDone(LG_Factory.BatchName batchName)
        {
            return OnBatchDone[batchName];
        }

        private BatchBuildManager() {}

        static BatchBuildManager() { }
    }
}
