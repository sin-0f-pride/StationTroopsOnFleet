using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.SaveSystem;

namespace StationTroopsWithFleet.Roster
{
    internal class TroopStationRoster
    {
        [SaveableProperty(1)]
        public TroopRoster MemberRoster { get; set; }

        [SaveableProperty(2)]
        public TroopRoster PrisonRoster { get; set; }

        public TroopStationRoster(TroopRoster memberRoster, TroopRoster prisonRoster)
        {
            MemberRoster = memberRoster;
            PrisonRoster = prisonRoster;
        }
    }
}
