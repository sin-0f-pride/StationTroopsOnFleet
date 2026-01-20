using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.SaveSystem;

namespace StationTroopsWithFleet.Station
{
    internal class StationedTroops
    {
        [SaveableProperty(1)]
        public AnchorPoint AnchorPoint { get; set; }

        [SaveableProperty(2)]
        public TroopRoster MemberRoster { get; set; }

        [SaveableProperty(3)]
        public TroopRoster PrisonRoster { get; set; }

        public StationedTroops(AnchorPoint anchorPoint, TroopRoster memberRoster, TroopRoster prisonRoster)
        {
            AnchorPoint = anchorPoint;
            MemberRoster = memberRoster;
            PrisonRoster = prisonRoster;
        }
    }
}
