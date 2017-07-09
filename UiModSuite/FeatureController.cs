using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using UiModSuite.Options;
using UiModSuite.UiMods;
using StardewConfigFramework;

namespace UiModSuite
{
	public class FeatureController
	{

		// Load all mods, mods decide if they are loaded in their respective toggle method
		// [Juice] Due to original mod, load order matters for options page order. Maintained same order.
		Experience uiModExperience;
		LuckOfDay uiModluckOfDay;
		AccurateHearts uiModAccurateHearts;
		ItemRolloverInformation uiModItemrolloverInformation;
		LocationOfTownsfolk uiModLocationOfTownsfolk;
		ShowTravelingMerchant uiModShowTravelingMerchant;
		DisplayCropAndBarrelTime uiModDisplayCropAndBarrelTime;
		DisplayBirthdayIcon uiModDisplayBirthdayIcon;
		DisplayCalendarAndBillboardOnGameMenuButton uiModDisplayCalendarAndBillboardOnGameMenuButton;
		DisplayAnimalNeedsPet uiModDisplayAnimalNeedsPet;
		DisplayScarecrowAndSprinklerRange uiModDisplayScarecrowAndSprinklerRange;
		ShopHarvestPrices shopHarvestPrices;

		public FeatureController() {
			uiModExperience = new Experience();
			uiModluckOfDay = new LuckOfDay();
			uiModAccurateHearts = new AccurateHearts();
			uiModItemrolloverInformation = new ItemRolloverInformation();
			uiModLocationOfTownsfolk = new LocationOfTownsfolk();
			uiModShowTravelingMerchant = new ShowTravelingMerchant();
			uiModDisplayCropAndBarrelTime = new DisplayCropAndBarrelTime();
			uiModDisplayBirthdayIcon = new DisplayBirthdayIcon();
			uiModDisplayAnimalNeedsPet = new DisplayAnimalNeedsPet();
			uiModDisplayScarecrowAndSprinklerRange = new DisplayScarecrowAndSprinklerRange();
			shopHarvestPrices = new ShopHarvestPrices();
			uiModDisplayCalendarAndBillboardOnGameMenuButton = new DisplayCalendarAndBillboardOnGameMenuButton();

			var saveButton = ModEntry.Options.GetOptionWithIdentifier<ModOptionTrigger>("saveButton") ?? new ModOptionTrigger("saveButton", "Save Settings", OptionActionType.OK);
			ModEntry.Options.AddModOption(saveButton);

			saveButton.ActionTriggered += (identifier) => {
				ModEntry.Options.SaveUserSettings();
				ModEntry.Helper.WriteConfig(ModEntry.ModConfig);
			};
		}
	}
}
