using GameData;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Tweaks.BossEvents
{
    public class EventsOnZoneBossDeath: BaseClasses.GlobalZoneIndex
    {
        public bool ApplyToHibernate { get; set; } = true;
        
        public int ApplyToHibernateCount { get; set; } = int.MaxValue;
        
        public bool ApplyToWave { get; set; } = false;

        public int ApplyToWaveCount { get; set; } = int.MaxValue;

        public List<uint> BossIDs { set; get; } = new() { 29, 36, 37 };

        public List<WardenObjectiveEventData> EventsOnBossDeath { set; get; } = new(); 
    }
}
