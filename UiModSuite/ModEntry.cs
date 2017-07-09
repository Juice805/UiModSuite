using UiModSuite.UiMods;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using UiModSuite.Options;
using StardewConfigFramework;

namespace UiModSuite {
    public class ModEntry : Mod {

		public static ModOptions Options;
		public static new IModHelper Helper;
		public static string modDirectory;
		public static ModConfig ModConfig;
        public static Boolean isTesting = false;
		public static new IMonitor Monitor;

		public FeatureController controller;
            
        public override void Entry(IModHelper helper) {
			var Settings = IModSettingsFramework.Instance;
			ModEntry.Options = ModOptions.LoadUserSettings(this);
			Settings.AddModOptions(ModEntry.Options);

			ModEntry.Helper = helper;
			ModEntry.Monitor = base.Monitor;


			// Load Modconfig
			ModConfig = Helper.ReadConfig<ModConfig>();

            // Loads the correct settings on character load
			SaveEvents.AfterLoad += LoadFeatures;
			SaveEvents.AfterReturnToTitle += RemoveListener;
			SaveEvents.AfterSave += SaveEvents_AfterSave;

			var skipIntro = Options.GetOptionWithIdentifier<ModOptionToggle>("skipIntro") ?? new ModOptionToggle("skipIntro", "Skip Intro");
			Options.AddModOption(skipIntro);
			// Skip Intro
			if (skipIntro.IsOn)
				MenuEvents.MenuChanged += SkipIntro.onMenuChange;
        }

		void SaveEvents_AfterSave(object sender, EventArgs e)
		{
			ModEntry.Helper.WriteConfig(ModEntry.ModConfig);
			Options.SaveUserSettings();
		}

		void LoadFeatures(object sender, EventArgs e)
		{
			RemoveListener(sender, e);
			GraphicsEvents.OnPreRenderEvent += IconHandler.reset;
			controller = new FeatureController();
		}

		void RemoveListener(object sender, EventArgs e)
		{
			GraphicsEvents.OnPreRenderEvent -= IconHandler.reset;
		}
	}
}
