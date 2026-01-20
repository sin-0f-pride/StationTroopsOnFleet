using StationTroopsWithFleet.Roster;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.SaveSystem;

namespace StationTroopsWithFleet.Station
{
    internal sealed class StationedTroopsManager
    {
        [SaveableField(1)]
        private StationedTroops? _stationedTroops;

        internal StationedTroopsManager() { }

        public StationedTroops StationTroops(TroopRoster memberRoster, TroopRoster prisonRoster)
        {
            StationedTroops stationedtroops = FindStationedTroops();
            if (stationedtroops != null)
            {
                stationedtroops.AnchorPoint = null!;
                stationedtroops.MemberRoster = memberRoster;
                stationedtroops.PrisonRoster = prisonRoster;
                return stationedtroops;
            }
            stationedtroops = new StationedTroops(null!, memberRoster, prisonRoster);
            _stationedTroops = stationedtroops;
            return stationedtroops;
        }

        public void RemoveStationedTroops()
        {
            _stationedTroops = null;
        }

        public StationedTroops FindStationedTroops()
        {
            if (_stationedTroops != null)
            {
                return _stationedTroops;
            }
            return null!;
        }
    }
}
