using System.Collections.Generic;

using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.SaveSystem;

namespace StationTroopsOnFleet.Station
{
    internal sealed class StationedTroopsManager
    {
        [SaveableField(1)]
        private List<StationedTroops> _stationedTroops;

        internal StationedTroopsManager() => _stationedTroops ??= new();

        public StationedTroops StationTroops(AnchorPoint anchorPoint, TroopRoster memberRoster, TroopRoster prisonRoster)
        {
            StationedTroops stationedtroops = FindStationedTroops(anchorPoint);
            if (stationedtroops != null)
            {
                stationedtroops.MemberRoster = memberRoster;
                stationedtroops.PrisonRoster = prisonRoster;
                return stationedtroops;
            }
            stationedtroops = new StationedTroops(anchorPoint, memberRoster, prisonRoster);
            _stationedTroops.Add(stationedtroops);
            return stationedtroops;
        }

        public void RemoveStationedTroops(AnchorPoint anchorPoint)
        {
            StationedTroops stationedtroops = FindStationedTroops(anchorPoint);
            if (stationedtroops == null)
            {
                return;
            }
            _stationedTroops.Remove(stationedtroops);
        }

        public StationedTroops FindStationedTroops(AnchorPoint anchorPoint)
        {
            foreach (StationedTroops stationedTroops in _stationedTroops)
            {
                if (stationedTroops.AnchorPoint == anchorPoint)
                {
                    return stationedTroops;
                }
            }
            return null!;
        }
    }
}
