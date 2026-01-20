using Helpers;
using StationTroopsWithFleet.Roster;
using StationTroopsWithFleet.Station;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace StationTroopsWithFleet.Behaviors
{
    internal sealed class StationTroopsBehavior : CampaignBehaviorBase
    {
        public int TroopsToStation { get; set; } = 0;

        internal StationedTroopsManager _stationedTroopsManager;

        internal TroopStationRosterManager _troopStationRosterManager;

        public StationTroopsBehavior()
        {
            _stationedTroopsManager ??= new();
            _troopStationRosterManager ??= new();
            TroopStationRoster troopStationRoster = _troopStationRosterManager.FindTroopStationRoster();
            if (troopStationRoster != null)
            {
                TroopsToStation = troopStationRoster.MemberRoster.TotalManCount;
            }
        }

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
            MobileParty leftParty = new MobileParty();
            leftParty.Party.SetCustomName(new TextObject("{=!!!!}Station Troops"));
            PartyScreenHelper.OpenScreenAsManageTroopsAndPrisoners(leftParty, onPartyScreenClosed);
            PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
            partyState.IsDonating = false;
            partyState.PartyScreenMode = PartyScreenHelper.PartyScreenMode.Normal;
            PartyScreenLogic partyScreenLogic = new PartyScreenLogic();
            IsTroopTransferableDelegate troopTransferableDelegate = PartyScreenHelper.ClanManageTroopAndPrisonerTransferableDelegate;
            PartyScreenHelper.PartyScreenMode partyScreenMode = partyState.PartyScreenMode;
            PartyPresentationDoneButtonDelegate partyPresentationDoneButtonDelegate = ManageTroopsAndPrisonersDoneHandler;
            PartyScreenLogicInitializationData initializationData = PartyScreenLogicInitializationData.CreateBasicInitDataWithMainPartyAndOther(leftParty, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.Transferable, troopTransferableDelegate, partyScreenMode, new TextObject("{=uQgNPJnc}Manage Troops"), partyPresentationDoneButtonDelegate, null, null, null, onPartyScreenClosed);
            initializationData.LeftPartyMembersSizeLimit = 0;
            initializationData.LeftPartyPrisonersSizeLimit = 0;
            partyScreenLogic.Initialize(initializationData);
            partyState.PartyScreenLogic = partyScreenLogic;
            Game.Current.GameStateManager.PushState(partyState);
            Campaign.Current.GameMenuManager.RefreshMenuOptions(args.MenuContext);
            SetMenuText(args);
        }

        private void onPartyScreenClosed(PartyBase leftOwnerParty, TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, PartyBase rightOwnerParty, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, bool fromCancel)
        {
            if (leftMemberRoster.TotalManCount > 0)
            {
                TroopsToStation = leftMemberRoster.TotalManCount;
                SaveTroopStationRoster(leftMemberRoster, leftPrisonRoster);
                foreach (TroopRosterElement element in leftMemberRoster.GetTroopRoster())
                {
                    rightOwnerParty.MemberRoster.AddToCounts(element.Character, element.Number, false, element.WoundedNumber);
                }
                foreach (TroopRosterElement element2 in leftPrisonRoster.GetTroopRoster())
                {
                    rightOwnerParty.PrisonRoster.AddToCounts(element2.Character, element2.Number, false, element2.WoundedNumber);
                }
            }
        }

        private static bool ManageTroopsAndPrisonersDoneHandler(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty = null, PartyBase rightParty = null)
        {
            return true;
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

        public StationedTroops StationTroops(TroopRoster memberRoster, TroopRoster prisonRoster) => _stationedTroopsManager.StationTroops(memberRoster, prisonRoster);

        public void RemoveStationedTroops() => _stationedTroopsManager.RemoveStationedTroops();

        public StationedTroops FindStationedTroops() => _stationedTroopsManager.FindStationedTroops();

        public TroopStationRoster SaveTroopStationRoster(TroopRoster memberRoster, TroopRoster prisonRoster) => _troopStationRosterManager.SaveTroopStationRoster(memberRoster, prisonRoster);

        public void RemoveTroopStationRoster() => _troopStationRosterManager.RemoveTroopStationRoster();

        public TroopStationRoster FindTroopStationRoster() => _troopStationRosterManager.FindTroopStationRoster();

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_stationedTroopsManager", ref _stationedTroopsManager);
            dataStore.SyncData("_troopStationRosterManager", ref _troopStationRosterManager);
            if (dataStore.IsLoading)
            {
                _stationedTroopsManager ??= new();
            }
        }
    }
}