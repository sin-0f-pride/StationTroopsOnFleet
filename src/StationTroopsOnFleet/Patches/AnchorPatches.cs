using HarmonyLib;
using StationTroopsOnFleet.Behaviors;
using StationTroopsOnFleet.Station;
using System.Linq;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;

namespace StationTroopsOnFleet.Patches
{
    [HarmonyPatch(typeof(AnchorPoint), "SetPosition")]
    public class SetPositionPatch
    {
        public static void Postfix(AnchorPoint __instance)
        {
            if (__instance.Owner != null && __instance.Owner == MobileParty.MainParty && __instance.Owner.IsTransitionInProgress)
            {
                StationTroopsBehavior behavior = Campaign.Current.GetCampaignBehavior<StationTroopsBehavior>();
                if (behavior.TroopsToStation != 0)
                {
                    if (behavior.FindStationedTroops(__instance) == null)
                    {
                        behavior.StationTroops(__instance, StationTroops(__instance.Owner, behavior.TroopsToStation), StationPrisoners(__instance.Owner));
                    }
                }
            }
        }

        private static TroopRoster StationTroops(MobileParty party, int troopsToStation)
        {
            TroopRoster memberRoster = TroopRoster.CreateDummyTroopRoster();
            int totalManCountCached = party.MemberRoster.TotalManCount;
            if (totalManCountCached > 0)
            {
                int stationed = 0;
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
                        for (int i = 0; i < element.Number; i++)
                        {
                            memberRoster.AddToCounts(element.Character, 1, false, element.WoundedNumber);
                            party.MemberRoster.AddToCounts(element.Character, -1);
                            stationed++;
                            if (stationed >= troopsToStation)
                            {
                                SubModule.Log("stationed=" + stationed);
                                return memberRoster;
                            }
                        }
                    }
                }
            }
            return memberRoster;
        }

        private static TroopRoster StationPrisoners(MobileParty party)
        {
            TroopRoster prisonRoster = TroopRoster.CreateDummyTroopRoster();
            if (party.PrisonRoster.TotalManCount > 0)
            {
                foreach (TroopRosterElement troop in party.PrisonRoster.GetTroopRoster())
                {
                    if (!troop.Character.IsHero)
                    {
                        prisonRoster.AddToCounts(troop.Character, troop.Number, false, troop.WoundedNumber);
                        party.PrisonRoster.AddToCounts(troop.Character, -troop.Number);
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
                StationedTroops stationedTroops = behavior.FindStationedTroops(__instance);
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
                    behavior.RemoveStationedTroops(__instance);
                }
            }
        }
    }
}
