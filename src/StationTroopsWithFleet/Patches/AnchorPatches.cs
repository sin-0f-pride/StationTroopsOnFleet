using HarmonyLib;
using StationTroopsWithFleet.Behaviors;
using StationTroopsWithFleet.Roster;
using StationTroopsWithFleet.Station;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace StationTroopsWithFleet.Patches
{
    [HarmonyPatch(typeof(AnchorPoint), "SetPosition")]
    public class SetPositionPatch
    {
        private static StationTroopsBehavior _behavior;

        public static void Postfix(AnchorPoint __instance)
        {
            if (__instance.Owner != null && __instance.Owner == MobileParty.MainParty && __instance.Owner.IsTransitionInProgress)
            {
                _behavior = Campaign.Current.GetCampaignBehavior<StationTroopsBehavior>();
                if (_behavior.TroopsToStation != 0)
                {
                    if (_behavior.FindStationedTroops() == null)
                    {
                        TroopStationRoster troopStationRoster = _behavior.FindTroopStationRoster();
                        _behavior.StationTroops(StationTroops(__instance.Owner, troopStationRoster.MemberRoster, _behavior.TroopsToStation), StationPrisoners(__instance.Owner, troopStationRoster.PrisonRoster));
                    }
                }
            }
        }

        private static TroopRoster StationTroops(MobileParty party, TroopRoster troopStationRoster, int troopsToStation)
        {
            TroopRoster memberRoster = TroopRoster.CreateDummyTroopRoster();
            if (party.MemberRoster.TotalManCount > 0)
            {
                foreach (TroopRosterElement element in party.MemberRoster.GetTroopRoster())
                {
                    if (!element.Character.IsHero)
                    {
                        if (troopsToStation == -1)
                        {
                            memberRoster.AddToCounts(element.Character, element.Number, false, element.WoundedNumber);
                            party.MemberRoster.AddToCounts(element.Character, -element.Number);
                            continue;
                        }
                        foreach (TroopRosterElement element2 in troopStationRoster.GetTroopRoster())
                        {
                            if (element.Character == element2.Character)
                            {
                                memberRoster.AddToCounts(element2.Character, element2.Number, false, element2.WoundedNumber);
                                party.MemberRoster.AddToCounts(element2.Character, -element2.Number);
                                continue;
                            }
                        }
                    }
                }
            }
            return memberRoster;
        }

        private static TroopRoster StationPrisoners(MobileParty party, TroopRoster troopStationRoster)
        {
            TroopRoster prisonRoster = TroopRoster.CreateDummyTroopRoster();
            if (_behavior.TroopsToStation > 0 && party.PrisonRoster.TotalManCount > 0)
            {
                foreach (TroopRosterElement element in party.PrisonRoster.GetTroopRoster())
                {
                    foreach (TroopRosterElement element2 in troopStationRoster.GetTroopRoster())
                    {
                        if (element.Character == element2.Character)
                        {
                            prisonRoster.AddToCounts(element2.Character, element2.Number, false, element2.WoundedNumber);
                            party.PrisonRoster.AddToCounts(element2.Character, -element2.Number);
                            continue;
                        }
                    }
                }
            }
            return prisonRoster;
        }
    }
    [HarmonyPatch(typeof(AnchorPoint), "ResetPosition")]
    public class ResetPositionPatch
    {
        public static void Postfix(AnchorPoint __instance)
        {
            if (__instance.Owner != null && __instance.Owner == MobileParty.MainParty && __instance.Owner.IsCurrentlyAtSea && !__instance.Owner.IsTransitionInProgress)
            {
                StationTroopsBehavior behavior = Campaign.Current.GetCampaignBehavior<StationTroopsBehavior>();
                StationedTroops stationedTroops = behavior.FindStationedTroops();
                if (stationedTroops != null)
                {
                    if (stationedTroops.MemberRoster.TotalManCount > 0)
                    {
                        TroopRoster memberRoster = stationedTroops.MemberRoster;
                        foreach (TroopRosterElement troop in memberRoster.GetTroopRoster())
                        {
                            __instance.Owner.MemberRoster.AddToCounts(troop.Character, troop.Number, false, troop.WoundedNumber);
                            memberRoster.AddToCounts(troop.Character, -troop.Number);
                        }
                    }
                    if (stationedTroops.PrisonRoster.TotalManCount > 0)
                    {
                        TroopRoster prisonRoster = stationedTroops.PrisonRoster;
                        foreach (TroopRosterElement troop in prisonRoster.GetTroopRoster())
                        {
                            __instance.Owner.PrisonRoster.AddToCounts(troop.Character, troop.Number, false, troop.WoundedNumber);
                            prisonRoster.AddToCounts(troop.Character, -troop.Number);
                        }
                    }
                    behavior.RemoveStationedTroops();
                }
            }
        }
    }
}
