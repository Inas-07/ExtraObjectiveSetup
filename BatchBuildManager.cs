using System;
using LevelGeneration;
using System.Collections.Generic;

namespace ExtraObjectiveSetup
{
    public sealed class BatchBuildManager
    {
        public static BatchBuildManager Current { get; private set; } = new();

        private Dictionary<LG_Factory.BatchName, Action> OnBatchStart = new();
        
        private Dictionary<LG_Factory.BatchName, Action> OnBatchDone = new();

        public void Init()
        {
            if (OnBatchStart.Count < 1)
            {
                foreach (var batchName in Enum.GetValues(typeof(LG_Factory.BatchName)))
                {
                    OnBatchStart[(LG_Factory.BatchName)batchName] = null;
                }
            }

            if (OnBatchDone.Count < 1)
            {
                foreach (var batchName in Enum.GetValues(typeof(LG_Factory.BatchName)))
                {
                    OnBatchDone[(LG_Factory.BatchName)batchName] = null;
                }
            }

        }

        public void Add_OnBatchDone(LG_Factory.BatchName batchName, Action action) => OnBatchDone[batchName] += action;
        
        public void Remove_OnBatchDone(LG_Factory.BatchName batchName, Action action) => OnBatchDone[batchName] -= action;
        
        public Action Get_OnBatchDone(LG_Factory.BatchName batchName) => OnBatchDone[batchName];

        public void Add_OnBatchStart(LG_Factory.BatchName batchName, Action action) => OnBatchStart[batchName] += action;

        public void Remove_OnBatchStart(LG_Factory.BatchName batchName, Action action) => OnBatchStart[batchName] -= action;

        public Action Get_OnBatchStart(LG_Factory.BatchName batchName) => OnBatchStart[batchName];

        private BatchBuildManager() {}

        static BatchBuildManager() { }
    }
}
