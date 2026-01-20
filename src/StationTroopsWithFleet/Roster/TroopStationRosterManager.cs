using StationTroopsWithFleet.Roster;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.SaveSystem;

namespace StationTroopsWithFleet.Station
{
    internal sealed class TroopStationRosterManager
    {
        [SaveableField(1)]
        private TroopStationRoster? _troopStationRoster;

        internal TroopStationRosterManager() { }

        public TroopStationRoster SaveTroopStationRoster(TroopRoster memberRoster, TroopRoster prisonRoster)
        {
            if (_troopStationRoster == null)
            {
                return _troopStationRoster = new TroopStationRoster(memberRoster, prisonRoster);
            }
            _troopStationRoster.MemberRoster = memberRoster;
            _troopStationRoster.PrisonRoster = prisonRoster;
            return _troopStationRoster;
        }

        public void RemoveTroopStationRoster()
        {
            _troopStationRoster = null;
        }

        public TroopStationRoster FindTroopStationRoster()
        {
            if (_troopStationRoster != null)
            {
                return _troopStationRoster;
            }
            return null!;
        }
    }
}
