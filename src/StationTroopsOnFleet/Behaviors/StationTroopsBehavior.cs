using Helpers;
using StationTroopsOnFleet.Station;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace StationTroopsOnFleet.Behaviors
{
    internal sealed class StationTroopsBehavior : CampaignBehaviorBase
    {
        public int TroopsToStation { get; set; } = 0;

        internal StationedTroopsManager _stationedTroopsManager;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddGameMenus(starter);
        }

        private void AddGameMenus(CampaignGameStarter starter)
        {
            starter.AddGameMenu("station_troops", "{MENU_TEXT}", SetMenuText);
            starter.AddGameMenuOption("station_troops", "station_troops_all", "{=!!!!}The entire crew should remain with the fleet.", station_troops_all_on_condition, station_troops_all_on_consequence);
            starter.AddGameMenuOption("station_troops", "station_troops_half", "{=!!!!}Part of the crew should remain with the fleet.", station_troops_input_on_condition, station_troops_input_on_consequence);
            starter.AddGameMenuOption("station_troops", "station_troops_none", "{=!!!!}None of the crew should remain with the fleet.", station_troops_none_on_condition, station_troops_none_on_consequence);
            starter.AddGameMenuOption("station_troops", "leave", "Leave", leave_on_condition, leave_on_consequence);
        }

        private bool station_troops_all_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.CallFleet;
            return true;
        }

        private void station_troops_all_on_consequence(MenuCallbackArgs args)
        {
            TroopsToStation = -1;
            Campaign.Current.GameMenuManager.RefreshMenuOptions(args.MenuContext);
            SetMenuText(args);
        }

        private bool station_troops_input_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.ManageGarrison;
            return true;
        }

        private void station_troops_input_on_consequence(MenuCallbackArgs args)
        {
            InformationManager.ShowTextInquiry(new TextInquiryData("Input amount", "", true, false, GameTexts.FindText("str_done", null).ToString(), null, delegate (string input)
            {
                int.TryParse(input, out int result);
                if (result == 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!!!!}Amount must be a valid integer.").ToString(), Color.FromUint(0x00FF0000)));
                }
                else
                {
                    TroopsToStation = result;
                    Campaign.Current.GameMenuManager.RefreshMenuOptions(args.MenuContext);
                    SetMenuText(args);
                }

            }, null));
        }

        private bool station_troops_none_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.SetSail;
            return true;
        }

        private void station_troops_none_on_consequence(MenuCallbackArgs args)
        {
            TroopsToStation = 0;
            Campaign.Current.GameMenuManager.RefreshMenuOptions(args.MenuContext);
            SetMenuText(args);
        }

        private bool leave_on_condition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private void leave_on_consequence(MenuCallbackArgs args)
        {
            PlayerEncounter.Finish(true);
        }

        public void SetMenuText(MenuCallbackArgs args)
        {
            TextObject menuText = new TextObject("{=!!!!}You open your captains log to a page titled Fleet Management. You've used this in the past to remind yourself to announce to the crew your intentions to disembark alone or with the full crew. What are your orders?");
            TextObject orderText = new TextObject("{=!!!!}Current Orders: {ORDERS");
            if (TroopsToStation == 0)
            {
                orderText.SetTextVariable("ORDERS", new TextObject("{=!!!!}None of the crew should remain with the fleet."));
            }
            else if (TroopsToStation == -1)
            {
                orderText.SetTextVariable("ORDERS", new TextObject("{=!!!!}The entire crew should remain with the fleet."));
            }
            else
            {
                TextObject stringParenthesesText = new TextObject("{=!!!!}{STR1} ({STR2})");
                stringParenthesesText.SetTextVariable("STR1", new TextObject("{=!!!!}Part of the crew should remain with the fleet."));
                stringParenthesesText.SetTextVariable("STR2", new TextObject(TroopsToStation));
                orderText.SetTextVariable("ORDERS", stringParenthesesText);
            }
            GameTexts.SetVariable("MENU_TEXT", GameTexts.FindText("str_string_newline_newline_string").SetTextVariable("STR1", menuText).SetTextVariable("STR2", orderText).ToString());
        }

        [GameMenuInitializationHandler("station_troops")]
        public static void station_troops_on_init(MenuCallbackArgs args)
        {
            MobileParty party = MobileParty.MainParty;
            string backgroundMeshName = (SettlementHelper.FindNearestSettlementToMobileParty(party, party.NavigationCapability) ?? MobileParty.MainParty.LastVisitedSettlement).Culture.StringId + "_port";
            args.MenuContext.SetBackgroundMeshName(backgroundMeshName);
            args.MenuContext.SetAmbientSound("event:/map/ambient/node/settlements/2d/port");
        }

        public StationTroopsBehavior() => _stationedTroopsManager ??= new();

        public StationedTroops StationTroops(AnchorPoint anchorPoint, TroopRoster memberRoster, TroopRoster prisonRoster) => _stationedTroopsManager.StationTroops(anchorPoint, memberRoster, prisonRoster);

        public void RemoveStationedTroops(AnchorPoint anchorPoint) => _stationedTroopsManager.RemoveStationedTroops(anchorPoint);

        public StationedTroops FindStationedTroops(AnchorPoint anchorPoint) => _stationedTroopsManager.FindStationedTroops(anchorPoint);

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_stationedTroopsManager", ref _stationedTroopsManager);
            if (dataStore.IsLoading)
            {
                _stationedTroopsManager ??= new();
            }
        }
    }
}