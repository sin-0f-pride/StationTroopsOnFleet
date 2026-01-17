using HarmonyLib;
using StationTroopsOnFleet.Behaviors;
using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace StationTroopsOnFleet
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("mod.bannerlord.StationTroopsOnFleet").PatchAll();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (gameStarterObject is CampaignGameStarter starter)
            {
                starter.AddBehavior(new StationTroopsBehavior());
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!!!!}Leave Troops And Ships Behind loaded").ToString(), Color.FromUint(0x00E67E22)));
        }

        protected override void OnApplicationTick(float dt)
        {
            if (Campaign.Current != null && Mission.Current == null)
            {
                if (MobileParty.MainParty.IsCurrentlyAtSea && Input.InputManager.IsKeyPressed(InputKey.Equals))
                {
                    PlayerEncounter.Start();
                    GameMenu.ActivateGameMenu("station_troops");
                }
            }
        }

        public static void Log(string message)
        {
            string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Mount and Blade II Bannerlord", "Logs");
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            string path = Path.Combine(text, "StationTroopsOnFleet.txt");
            using (StreamWriter streamWriter = new StreamWriter(path, true))
            {
                streamWriter.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message);
            }
        }
    }
}