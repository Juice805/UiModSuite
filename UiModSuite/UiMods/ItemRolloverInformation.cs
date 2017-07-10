using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StardewConfigFramework;
using UiModSuite.Options;

namespace UiModSuite.UiMods
{
	class ItemRolloverInformation
	{

		static Rectangle springIconSourceRect = new Rectangle(406, 441, 12, 8);
		static Rectangle summerIconSourceRect = new Rectangle(406, 449, 12, 8);
		static Rectangle fallIconSourceRect = new Rectangle(406, 457, 12, 8);
		static Rectangle winterIconSourceRect = new Rectangle(406, 465, 12, 8);
		static Rectangle sunnyIconSourceRect = new Rectangle(452, 333, 13, 13);
		static Rectangle nightIconSourceRect = new Rectangle(465, 344, 13, 13);
		static Rectangle rainIconSourceRect = new Rectangle(465, 333, 13, 13);

		static Rectangle fishIconSourceRect = new Rectangle(20, 428, 10, 10);
		static Rectangle cropIconSourceRect = new Rectangle(10, 428, 10, 10);
		static Rectangle bundleIconSourceRect = new Rectangle(331, 374, 15, 14);

		Dictionary<int, string> fishData = ModEntry.Helper.Content.Load<Dictionary<int, string>>(Path.Combine("Data", "Fish.xnb"), StardewModdingAPI.ContentSource.GameContent);
		List<string> cropData = ModEntry.Helper.Content.Load<Dictionary<int, string>>(Path.Combine("Data", "Crops.xnb"), StardewModdingAPI.ContentSource.GameContent).Values.ToList();
		Dictionary<int, string> treeData = ModEntry.Helper.Content.Load<Dictionary<int, string>>(Path.Combine("Data", "fruitTrees.xnb"), StardewModdingAPI.ContentSource.GameContent);
		Dictionary<string, string> bundleData = ModEntry.Helper.Content.Load<Dictionary<string, string>>(Path.Combine("Data", "Bundles.xnb"), StardewModdingAPI.ContentSource.GameContent);

		Item hoverItem;
		CommunityCenter communityCenter = (CommunityCenter)Game1.getLocationFromName("CommunityCenter");
		Dictionary<string, List<int>> prunedRequiredBundles = new Dictionary<string, List<int>>();
		ClickableTextureComponent bundleIcon = new ClickableTextureComponent("", new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), (float)Game1.pixelZoom, false);

		private ModOptionToggle option;

		public ItemRolloverInformation()
		{
			this.option = ModEntry.Options.GetOptionWithIdentifier<ModOptionToggle>("displayExtraItemInfo") ?? new ModOptionToggle("displayExtraItemInfo", "Show extra item information");
			ModEntry.Options.AddModOption(this.option);

			this.option.ValueChanged += toggleOption;
			toggleOption(this.option.identifier, this.option.IsOn);
		}

		/// <summary>
		/// This mod displays an improved tooltip
		/// </summary>
		public void toggleOption(string identifier, bool IsOn)
		{
			PlayerEvents.InventoryChanged -= populateRequiredBundled;
			GraphicsEvents.OnPostRenderEvent -= drawAdvancedToolipForMenu;
			GraphicsEvents.OnPostRenderHudEvent -= drawAdvancedToolipForToolbar;
			GraphicsEvents.OnPreRenderEvent -= getHoverItem;

			if (IsOn)
			{

				// Load bundle data
				communityCenter = (CommunityCenter)Game1.getLocationFromName("CommunityCenter");

				// Parse data to easily work with bundle data
				populateRequiredBundled(null, null);

				PlayerEvents.InventoryChanged += populateRequiredBundled;
				GraphicsEvents.OnPostRenderEvent += drawAdvancedToolipForMenu;
				GraphicsEvents.OnPostRenderHudEvent += drawAdvancedToolipForToolbar;
				GraphicsEvents.OnPreRenderEvent += getHoverItem;
			}
		}

		private void drawAdvancedToolipForMenu(object sender, EventArgs e)
		{
			if (Game1.activeClickableMenu != null)
			{
				drawAdvancedToolip(sender, e);
			}
		}

		private void drawAdvancedToolipForToolbar(object sender, EventArgs e)
		{
			if (Game1.activeClickableMenu == null)
			{
				drawAdvancedToolip(sender, e);
			}
		}

		/// <summary>
		/// Finds all the bundles still needing resources
		/// </summary>
		private void populateRequiredBundled(object sender, EventArgs e)
		{
			bundleData = ModEntry.Helper.Content.Load<Dictionary<string, string>>(Path.Combine("Data", "Bundles.xnb"), StardewModdingAPI.ContentSource.GameContent);
		}

		/// <summary>
		/// Draw it!
		/// </summary>
		private void drawAdvancedToolip(object sender, EventArgs e)
		{

			if (hoverItem != null)
				drawToolTip(Game1.spriteBatch, hoverItem);
		}

		/// <summary>
		/// This restores the hover item so it will be usable for lookup anything or any other mod
		/// </summary>
		private void restoreMenuState()
		{
			if (Game1.activeClickableMenu is ItemGrabMenu)
			{
				var itemGrabMenu = (ItemGrabMenu)Game1.activeClickableMenu;
				itemGrabMenu.hoveredItem = hoverItem;
			}
		}

		/// <summary>
		/// Gets the correct item price as ore prices use the price property
		/// </summary>
		/// <param name="hoverItem">The item</param>
		/// <returns>The correct sell price</returns>
		public static int getTruePrice(Item hoverItem)
		{
			if (hoverItem is StardewValley.Object)
			{

				// No clue why selToStorePrice needs to be multiplied for the correct value...???
				return (hoverItem as StardewValley.Object).sellToStorePrice() * 2;
			}

			// Overwrite ores cause salePrice() is not accurate for some reason...???
			switch (hoverItem.parentSheetIndex)
			{
				case 378:
				case 380:
				case 382:
				case 384:
				case 388:
				case 390:
					return (int)((double)((hoverItem as StardewValley.Object).price * 2) * (1.0 + (double)(hoverItem as StardewValley.Object).quality * 0.25));
				default:
					return hoverItem.salePrice();
			}
		}

		/// <summary>
		/// Gets the hover item and removes the vanilla tooltip displays **HACKY
		/// </summary>
		private void getHoverItem(object sender, EventArgs e)
		{
			var test = Game1.player.items;

			// Remove hovers from toolbar
			for (int j = 0; j < Game1.onScreenMenus.Count; j++)
			{
				if (Game1.onScreenMenus[j] is Toolbar)
				{
					var menu = Game1.onScreenMenus[j] as Toolbar;

					hoverItem = (Item)typeof(Toolbar).GetField("hoverItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);
					typeof(Toolbar).GetField("hoverItem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(menu, null);
				}
			}

			// Remove hovers from inventory
			if (Game1.activeClickableMenu is GameMenu)
			{

				// Get pages from GameMenu            
				var pages = (List<IClickableMenu>)typeof(GameMenu).GetField("pages", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Game1.activeClickableMenu);

				// Overwrite Inventory Menu
				for (int i = 0; i < pages.Count; i++)
				{
					if (pages[i] is InventoryPage)
					{
						var inventoryPage = (InventoryPage)pages[i];
						hoverItem = (Item)typeof(InventoryPage).GetField("hoveredItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(inventoryPage);
						typeof(InventoryPage).GetField("hoverText", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(inventoryPage, "");
					}
				}
			}

			// Remove hovers from chests and shipping bin
			if (Game1.activeClickableMenu is ItemGrabMenu)
			{
				var itemGrabMenu = (ItemGrabMenu)Game1.activeClickableMenu;
				hoverItem = itemGrabMenu.hoveredItem;
				itemGrabMenu.hoveredItem = null;
			}
		}

		// --------------------Below is the copied tooltip modified to my own specifications--------------------

		//(SpriteBatch b, SpriteFont font, Item hoveredItem, string title, string description, int moneyAmount, int healAmount, int currencySymbol = 0, string[] buffTitles = null, CraftingRecipe craftingIngredients = null, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, float alpha = 1f, int xOffset = 0, int yOffset = 0, int overrideX = -1, int overrideY = -1)
		public void drawToolTip(SpriteBatch b, Item hoveredItem)
		{

			string title = hoveredItem.DisplayName;
			string description = hoveredItem.getDescription();

			// standardize empty titles to null
			if (title != null && title == String.Empty)
			{
				title = null;
			}

			StardewValley.Object hoveredObject = (hoveredItem as StardewValley.Object);
			bool EdibleItem = hoveredObject != null && hoveredObject.edibility != -300;

			string[] buffIcons = (string[])null;

			int healAmount = -1;
			int currencySymbol = 0;
			int price = -1;

			string[] fields;

			if (hoveredObject == null)
			{
				if (hoveredItem.Name != "Scythe" && !(hoveredItem is FishingRod)) // weird exceptions
					price = hoveredItem.salePrice();
			}
			else
			{
				price = hoveredObject.sellToStorePrice();
				fields = Game1.objectInformation[hoveredItem.parentSheetIndex].Split('/');

				if (fields.Length >= 8)
				{
					buffIcons = fields[7].Split(' ');
				}

				healAmount = hoveredObject.Edibility;
			}

			price = (price == 0) ? -1 : price;
			drawHoverText(b, Game1.smallFont, hoveredItem, title, description, price, healAmount, currencySymbol, buffIcons);

		}

		// Original Modified
		public void drawHoverText(SpriteBatch b, SpriteFont font, Item hoveredItem, string title, string description, int moneyAmount, int healAmount, int currencySymbol = 0, string[] buffTitles = null, CraftingRecipe craftingIngredients = null, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, float alpha = 1f)
		{

			StardewValley.Object hoveredObject = (hoveredItem as StardewValley.Object);
			bool EdibleItem = hoveredObject != null && hoveredObject.edibility != -300;

			int heightBase = 20;
			int baseFontHeight = (int)font.MeasureString("T").Y;
			int titleFontHeight = (int)Game1.dialogueFont.MeasureString("T").Y;

			Vector2 healTextSize = (healAmount != -1) ? font.MeasureString(healAmount.ToString() + "+ Energy" + (object)(Game1.tileSize / 2)) : Vector2.Zero;
			Vector2 descriptionTextSize = font.MeasureString(description);
			Vector2 titleTextSize = title != null ? Game1.dialogueFont.MeasureString(title) : Vector2.Zero;

			string moneyAmountDisplay = (hoveredItem.Stack > 1) ? $"{moneyAmount} ({moneyAmount * hoveredItem.Stack})" : moneyAmount + string.Empty;

			// If Seed, show final crop price
			if (hoveredObject != null && hoveredObject.type == "Seeds" && moneyAmount != -1)
			{

				if (hoveredObject.Name != "Mixed Seeds" && hoveredObject.Name != "Winter Seeds" && hoveredObject.Name != "Summer Seeds" && hoveredObject.Name != "Fall Seeds" && hoveredObject.Name != "Spring Seeds")
				{
					var crop = new Crop(hoveredObject.parentSheetIndex, 0, 0);
					var debris = new Debris(crop.indexOfHarvest, Game1.player.position, Game1.player.position);
					var item = new StardewValley.Object(debris.chunkType, 1);
					moneyAmountDisplay += (hoveredItem.Stack > 1) ? $"> {item.Price} ({item.Price * hoveredItem.Stack})" : $"> {item.Price}";
				}
			}

			Vector2 moneyTextSize = (moneyAmount > -1) ? Game1.dialogueFont.MeasureString(moneyAmountDisplay + String.Empty) : Vector2.Zero;
			float moneyScale = 0.75f;
			moneyTextSize.X *= moneyScale;
			moneyTextSize.Y *= moneyScale;

			// MARK: Measuring Required box size
			int width = Math.Max((int)healTextSize.X, Math.Max((int)descriptionTextSize.X, (int)titleTextSize.X)) + Game1.tileSize / 2;
			int height = Math.Max(heightBase * 3, (int)(descriptionTextSize.Y + Game1.tileSize / 1.5) + (moneyAmount > -1 ? (int)(moneyTextSize.Y + 4.0) : 0) + (title != null ? (int)(titleTextSize.Y + (Game1.tileSize / 4)) : 0) + (healAmount >= 0 ? 38 : 0));

			// Draw bundle info
			bool inBundle = false;
			string bundleName = null;
			int bundleIndex = 0;
			int bundleItemIndex = 0;
			//communityCenter;
			if (bundleData.Values.ToList().Exists(x =>
			{
				var valueData = x.Split('/');
				var items = valueData[2].Split(' ');

				for (int i = 0; i < items.Count(); i += 3)
				{
					if (items[i] == $"{hoverItem.parentSheetIndex}" && items[i + 1] != items[i + 2])
					{
						bundleItemIndex = i;
						bundleName = valueData[0];
						bundleIndex = bundleData.Values.ToList().IndexOf(x);
						return true;
					}
				}
				return false;
			}))
			{
				var bundleNum = int.Parse(bundleData.Keys.ToList()[bundleIndex].Split('/')[1]);
				if (!communityCenter.bundles[bundleNum][bundleItemIndex])
				{
					inBundle = true;
					height += bundleIconSourceRect.Height * Game1.pixelZoom;
					width = Math.Max(width, bundleIconSourceRect.Width * Game1.pixelZoom + (int)font.MeasureString(bundleName).X + Game1.pixelZoom * 6);

				}
			}

			// If extra items need to be shown
			if (extraItemToShowIndex != -1)
			{
				string[] extraItemData = Game1.objectInformation[extraItemToShowIndex].Split(new char[] {
						'/'
				});
				string extraItemName = extraItemData[0];
				if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
				{
					extraItemName = extraItemData[extraItemData.Length - 1];
				}
				string text3 = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", new object[] {
						extraItemToShowAmount,
						extraItemName
				});
				int extraWidth = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16).Width * 2 * Game1.pixelZoom;
				width = Math.Max(width, extraWidth + (int)font.MeasureString(text3).X);
			}

			// edible items add height
			if (buffTitles != null)
			{
				foreach (string str in buffTitles)
				{
					if (!str.Equals("0"))
						height += 34;
				}
				height += 4;
			}


			// Fix Height and Widths for special cases
			string category = null;
			if (hoveredItem != null)
			{
				height += (Game1.tileSize + 4) * hoveredItem.attachmentSlots();
				category = hoveredItem.getCategoryName();
				if (category.Length > 0)
				{
					width = Math.Max(width, (int)font.MeasureString(category).X + Game1.tileSize / 2);
					height += baseFontHeight;
				}
				int maxDmg = 9999;
				int padding = 15 * Game1.pixelZoom + Game1.tileSize / 2;
				if (hoveredItem is MeleeWeapon)
				{
					MeleeWeapon meleeWeapon = hoveredItem as MeleeWeapon;
					height = Math.Max(heightBase * 3, (int)((title == null) ? 0f : (Game1.dialogueFont.MeasureString(title).Y + (float)(Game1.tileSize / 4))) + Game1.tileSize / 2) + baseFontHeight + (int)((moneyAmount <= -1) ? 0f : (font.MeasureString(moneyAmount + string.Empty).Y + 4f));
					height += ((!(hoveredItem.Name == "Scythe")) ? ((hoveredItem as MeleeWeapon).getNumberOfDescriptionCategories() * Game1.pixelZoom * 12) + 4 : 10);
					height += (int)font.MeasureString(Game1.parseText((hoveredItem as MeleeWeapon).description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y;
					width = (int)Math.Max((float)width, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Damage", new object[] {
								maxDmg,
								maxDmg
						})).X + (float)padding, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Speed", new object[] {
								maxDmg
						})).X + (float)padding, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] {
								maxDmg
						})).X + (float)padding, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", new object[] {
								maxDmg
						})).X + (float)padding, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", new object[] {
								maxDmg
						})).X + (float)padding, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Weight", new object[] {
								maxDmg
						})).X + (float)padding))))));
				}
				else if (hoveredItem is Boots)
				{
					height -= (int)descriptionTextSize.Y;
					height += (int)((float)((hoveredItem as Boots).getNumberOfDescriptionCategories() * Game1.pixelZoom * 12) + font.MeasureString(Game1.parseText((hoveredItem as Boots).description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y);
					width = (int)Math.Max((float)width, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] {
								maxDmg
						})).X + (float)padding, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", new object[] {
								maxDmg
						})).X + (float)padding));
				}
				else if (EdibleItem)
				{
					if (healAmount == -1)
					{
						height += (Game1.tileSize / 2 + Game1.pixelZoom * 1) * ((healAmount <= 0) ? 1 : 2);
					}
					else
					{
						height += Game1.tileSize / 2 + Game1.pixelZoom * 1;
					}
					healAmount = (int)Math.Ceiling((double)hoveredObject.Edibility * 2.5) + hoveredObject.quality * hoveredObject.Edibility;
					width = (int)Math.Max((float)width, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Energy", new object[] {
								maxDmg
						})).X + (float)padding, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Health", new object[] {
								maxDmg
						})).X + (float)padding));
				}

				// Calculate required height for fish/crop/tree info
				if (hoveredObject != null && fishData.ContainsKey(hoveredObject.ParentSheetIndex) && !hoveredObject.Name.Contains("Algae") && !hoveredObject.Name.Contains("Seaweed"))
				{
					// Add the height of the seasons icons 
					var data = fishData[hoveredObject.ParentSheetIndex].Split('/');
					if (data[1] != "trap")
					{

						height += (fishIconSourceRect.Height + 2) * Game1.pixelZoom;
						var times = data[5].Split(' ');
						string timesString = "";
						for (int i = 0; i < times.Length; i++)
						{
							int time = (int.Parse(times[i]) / 100);
							timesString += time - (time > 12 ? 12 : 0);
							if (time > 12)
								timesString += "p";
							else
								timesString += "a";

							if (i % 2 == 1 && i != times.Length - 1)
							{
								timesString += ", ";
							}
							else if (i % 2 == 0)
							{
								timesString += "-";
							}
						}

						width = Math.Max(width, (int)font.MeasureString(timesString).X + Game1.tileSize / 2);

						var seasons = data[6].Split(' ');
						if (seasons.Count() > 0 && seasons.Count() < 4)
						{ // if all seasons don't draw any
							height += (summerIconSourceRect.Height + 2) * Game1.pixelZoom;
							width = Math.Max(width, ((summerIconSourceRect.Width + 2) * Game1.pixelZoom * seasons.Count()));
						}

						var weather = data[7].Split(' ');
						if (!weather.Contains("both"))
						{ // if all weather don't draw any
							width = Math.Max(width, ((rainIconSourceRect.Width * 3) + (2 * Game1.pixelZoom)) * weather.Count() + fishIconSourceRect.Width * Game1.pixelZoom + Game1.pixelZoom + (int)font.MeasureString(timesString).X + Game1.tileSize / 2);
						}

					}
				}
				else if (hoveredObject != null && treeData.ContainsKey(hoveredObject.ParentSheetIndex))
				{
					var data = treeData[hoveredObject.ParentSheetIndex].Split('/');

					var seasons = data[1].Split(' ');
					if (seasons.Count() > 0 && seasons.Count() < 4)
					{
						height += (summerIconSourceRect.Height + 2) * Game1.pixelZoom;
						width = Math.Max(width, ((summerIconSourceRect.Width + 2) * Game1.pixelZoom * seasons.Count()));
					}

				}
				else if (hoveredObject != null && cropData.Exists(x => { return x.Split('/')[3] == $"{hoveredObject.ParentSheetIndex}"; }))
				{
					// TODO 

					var data = cropData.Find(x => { return x.Split('/')[3] == $"{hoveredObject.ParentSheetIndex}"; }).Split('/');
					var seasons = data[1].Split(' ');
					if (seasons.Count() > 0 && seasons.Count() < 4)
					{
						height += (summerIconSourceRect.Height + 2) * Game1.pixelZoom;
						width = Math.Max(width, ((summerIconSourceRect.Width + 2) * Game1.pixelZoom * seasons.Count()));
					}
				}

				// edibles add Width
				if (buffTitles != null)
				{
					for (int j = 0; j < buffTitles.Length; j++)
					{
						if (!buffTitles[j].Equals("0") && j <= 11)
						{
							width = (int)Math.Max((float)width, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + j, new object[] {
												maxDmg
										})).X + (float)padding);
						}
					}
				}
			}

			// crafting ingredients change dimensions
			if (craftingIngredients != null)
			{
				width = Math.Max((int)titleTextSize.X + Game1.pixelZoom * 3, Game1.tileSize * 6);
				height += craftingIngredients.getDescriptionHeight(width - Game1.pixelZoom * 2) + ((healAmount != -1) ? 0 : (-Game1.tileSize / 2)) + Game1.pixelZoom * 3;
			}

			// add space for fishing rod if has price
			if (hoveredItem is FishingRod && moneyAmount > -1)
			{
				//height += (int) (titleFontHeight * 0.75);
			}

			int prevMouseXOffset = Game1.getOldMouseX() + Game1.tileSize / 2;
			int prevMouseYOffset = Game1.getOldMouseY() + Game1.tileSize / 2;

			// ensure hover text does not go offscreen right
			if (prevMouseXOffset + width > Utility.getSafeArea().Right)
			{
				prevMouseXOffset = Utility.getSafeArea().Right - width;
				prevMouseYOffset += Game1.tileSize / 4;
			}

			// ensure hover text does not go offscreen bottom
			if (prevMouseYOffset + height > Utility.getSafeArea().Bottom)
			{
				prevMouseXOffset += Game1.tileSize / 4;
				if (prevMouseXOffset + width > Utility.getSafeArea().Right)
				{
					prevMouseXOffset = Utility.getSafeArea().Right - width;
				}
				prevMouseYOffset = Utility.getSafeArea().Bottom - height;
			}

			// MARK: Drawing Content
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), prevMouseXOffset, prevMouseYOffset, width + ((craftingIngredients == null) ? 0 : (Game1.tileSize / 3)), height, Color.White * alpha, 1f, true);



			// Items with no title
			if (title != null)
			{
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), prevMouseXOffset, prevMouseYOffset, width + ((craftingIngredients == null) ? 0 : (Game1.tileSize / 3)), (int)titleFontHeight + Game1.tileSize / 2 + (int)((hoveredItem == null || category.Length <= 0) ? 0f : baseFontHeight) + (int)((moneyAmount > -1) ? moneyTextSize.Y : 0) + (inBundle ? bundleIconSourceRect.Height * Game1.pixelZoom : 0) - Game1.pixelZoom, Color.White * alpha, 1f, false);
				b.Draw(Game1.menuTexture, new Rectangle(prevMouseXOffset + Game1.pixelZoom * 3, prevMouseYOffset + (int)Game1.dialogueFont.MeasureString(title).Y + Game1.tileSize / 2 + (int)((hoveredItem == null || category.Length <= 0) ? 0f : baseFontHeight) + (int)((moneyAmount > -1) ? moneyTextSize.Y : 0) + (inBundle ? bundleIconSourceRect.Height * Game1.pixelZoom : 0) - Game1.pixelZoom, width - Game1.pixelZoom * ((craftingIngredients != null) ? 1 : 6), Game1.pixelZoom), new Rectangle?(new Rectangle(44, 300, 4, 4)), Color.White);

				// Draw bundle info
				if (inBundle)
				{

					int amountOfSectionsWithoutAlpha = 10;
					int amountOfSections = 36;
					int sectionWidth = (width - bundleIconSourceRect.Width * Game1.pixelZoom) / amountOfSections;

					for (int i = 0; i < amountOfSections; i++)
					{
						float sectionAlpha;
						if (i < amountOfSectionsWithoutAlpha)
						{
							sectionAlpha = 0.92f;
						}
						else
						{
							sectionAlpha = 0.92f - (i - amountOfSectionsWithoutAlpha) * (1f / (amountOfSections - amountOfSectionsWithoutAlpha));
						}
						Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(prevMouseXOffset + bundleIconSourceRect.Width * Game1.pixelZoom + (sectionWidth * i), prevMouseYOffset, sectionWidth, bundleIconSourceRect.Height * Game1.pixelZoom), Color.Crimson * sectionAlpha);
					}


					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(prevMouseXOffset, prevMouseYOffset), bundleIconSourceRect, Color.White, 0f, Vector2.Zero);
					b.DrawString(font, bundleName, new Vector2(prevMouseXOffset + bundleIconSourceRect.Width * Game1.pixelZoom + Game1.pixelZoom, prevMouseYOffset + 3 * Game1.pixelZoom), Color.White);
					prevMouseYOffset += bundleIconSourceRect.Height * Game1.pixelZoom;
				}

				b.DrawString(Game1.dialogueFont, title, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(Game1.dialogueFont, title, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(Game1.dialogueFont, title, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor);
				prevMouseYOffset += titleFontHeight;
			}

			prevMouseYOffset -= 4;

			if (moneyAmount > -1)
			{

				b.DrawString(Game1.dialogueFont, moneyAmountDisplay, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 2f), Game1.textShadowColor, 0f, Vector2.Zero, moneyScale, SpriteEffects.None, 0f);
				b.DrawString(Game1.dialogueFont, moneyAmountDisplay, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(0f, 2f), Game1.textShadowColor, 0f, Vector2.Zero, moneyScale, SpriteEffects.None, 0f);
				b.DrawString(Game1.dialogueFont, moneyAmountDisplay, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 0f), Game1.textShadowColor, 0f, Vector2.Zero, moneyScale, SpriteEffects.None, 0f);
				b.DrawString(Game1.dialogueFont, moneyAmountDisplay, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor, 0f, Vector2.Zero, moneyScale, SpriteEffects.None, 0f);

				if (currencySymbol == 0)
				{
					b.Draw(Game1.debrisSpriteSheet, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4) + moneyTextSize.X + 20f, (float)(prevMouseYOffset + Game1.tileSize / 4 + 22)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), (float)Game1.pixelZoom, SpriteEffects.None, 0.95f);
				}
				else if (currencySymbol == 1)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 8) + moneyTextSize.X + 20f, (float)(prevMouseYOffset + Game1.tileSize / 4 - 5)), new Rectangle?(new Rectangle(338, 400, 8, 8)), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1f);
				}
				else if (currencySymbol == 2)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 8) + moneyTextSize.X + 20f, (float)(prevMouseYOffset + Game1.tileSize / 4 - 7)), new Rectangle?(new Rectangle(211, 373, 9, 10)), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1f);
				}
				prevMouseYOffset += (int)moneyTextSize.Y;
			}

			if (hoveredItem != null && category.Length > 0)
			{

				Utility.drawTextWithShadow(b, category, font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), hoveredItem.getCategoryColor(), 1f, -1f, 2, 2, 1f, 3);
				prevMouseYOffset += baseFontHeight + ((title == null) ? 0 : (Game1.tileSize / 4)) + Game1.pixelZoom;
			}
			else
			{
				prevMouseYOffset += ((title == null) ? 0 : (Game1.tileSize / 4));
			}

			prevMouseYOffset += 4;

			if (hoveredItem != null && hoveredItem is Boots)
			{
				Boots boots = hoveredItem as Boots;
				Utility.drawTextWithShadow(b, Game1.parseText(boots.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				prevMouseYOffset += (int)font.MeasureString(Game1.parseText(boots.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y;
				if (boots.defenseBonus > 0)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] {
								boots.defenseBonus
						}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += (int)Math.Max(font.MeasureString("TT").Y, (float)(12 * Game1.pixelZoom));
				}
				if (boots.immunityBonus > 0)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", new object[] {
								boots.immunityBonus
						}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += (int)Math.Max(font.MeasureString("TT").Y, (float)(12 * Game1.pixelZoom));
				}
			}
			else if (hoveredItem != null && hoveredItem is MeleeWeapon)
			{
				MeleeWeapon meleeWeapon2 = hoveredItem as MeleeWeapon;
				Utility.drawTextWithShadow(b, Game1.parseText(meleeWeapon2.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				prevMouseYOffset += (int)font.MeasureString(Game1.parseText(meleeWeapon2.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y;
				if (meleeWeapon2.indexOfMenuItemView != 47)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(120, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Damage", new object[] {
								meleeWeapon2.minDamage,
								meleeWeapon2.maxDamage
						}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += (int)Math.Max(font.MeasureString("TT").Y, (float)(12 * Game1.pixelZoom));
					if (meleeWeapon2.speed != ((meleeWeapon2.type != 2) ? 0 : -8))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(130, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						bool flag = (meleeWeapon2.type == 2 && meleeWeapon2.speed < -8) || (meleeWeapon2.type != 2 && meleeWeapon2.speed < 0);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Speed", new object[] {
										((((meleeWeapon2.type != 2) ? meleeWeapon2.speed : (meleeWeapon2.speed - -8)) <= 0) ? string.Empty : "+") + ((meleeWeapon2.type != 2) ? meleeWeapon2.speed : (meleeWeapon2.speed - -8)) / 2
								}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), (!flag) ? (Game1.textColor * 0.9f * alpha) : Color.DarkRed, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int)Math.Max(font.MeasureString("TT").Y, (float)(12 * Game1.pixelZoom));
					}
					if (meleeWeapon2.addedDefense > 0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] {
										meleeWeapon2.addedDefense
								}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int)Math.Max(font.MeasureString("TT").Y, (float)(12 * Game1.pixelZoom));
					}
					if ((double)meleeWeapon2.critChance / 0.02 >= 2.0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(40, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", new object[] {
										(int)((double)meleeWeapon2.critChance / 0.02)
								}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int)Math.Max(font.MeasureString("TT").Y, (float)(12 * Game1.pixelZoom));
					}
					if ((double)(meleeWeapon2.critMultiplier - 3f) / 0.02 >= 1.0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(160, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", new object[] {
										(int)((double)(meleeWeapon2.critMultiplier - 3f) / 0.02)
								}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 11), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int)Math.Max(font.MeasureString("TT").Y, (float)(12 * Game1.pixelZoom));
					}
					if (meleeWeapon2.knockback != meleeWeapon2.defaultKnockBackForThisType(meleeWeapon2.type))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(70, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Weight", new object[] {
										(((float)((int)Math.Ceiling ((double)(Math.Abs (meleeWeapon2.knockback - meleeWeapon2.defaultKnockBackForThisType (meleeWeapon2.type)) * 10f))) <= meleeWeapon2.defaultKnockBackForThisType (meleeWeapon2.type)) ? string.Empty : "+") + (int)Math.Ceiling ((double)(Math.Abs (meleeWeapon2.knockback - meleeWeapon2.defaultKnockBackForThisType (meleeWeapon2.type)) * 10f))
								}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int)Math.Max(font.MeasureString("TT").Y, (float)(12 * Game1.pixelZoom));
					}
				}
			}
			else if (!string.IsNullOrEmpty(description) && description != " ")
			{
				b.DrawString(font, description, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 2f), Game1.textShadowColor * alpha);
				b.DrawString(font, description, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(0f, 2f), Game1.textShadowColor * alpha);
				b.DrawString(font, description, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 0f), Game1.textShadowColor * alpha);
				b.DrawString(font, description, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor * 0.9f * alpha);
				prevMouseYOffset += (int)font.MeasureString(description).Y + 4;
			}

			if (craftingIngredients != null)
			{
				craftingIngredients.drawRecipeDescription(b, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset - Game1.pixelZoom * 2)), width);
				prevMouseYOffset += craftingIngredients.getDescriptionHeight(width);
			}

			if (healAmount != -1)
			{
				if (healAmount > 0)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4)), new Rectangle((healAmount >= 0) ? 0 : 140, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", new object[] {
								((healAmount <= 0) ? string.Empty : "+") + healAmount
						}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + 34 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += 34;
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4)), new Rectangle(0, 438, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Health", new object[] {
								((healAmount <= 0) ? string.Empty : "+") + (int)((float)healAmount * 0.4f)
						}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + 34 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += 34;
				}
				else if (healAmount != -300)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4)), new Rectangle(140, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", new object[] {
								string.Empty + healAmount
						}), font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + 34 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += 34;
				}
			}

			if (buffTitles != null)
			{
				for (int k = 0; k < buffTitles.Length; k++)
				{
					if (!buffTitles[k].Equals("0"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4)), new Rectangle(10 + k * 10, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
						string text6 = ((Convert.ToInt32(buffTitles[k]) <= 0) ? string.Empty : "+") + buffTitles[k] + " ";
						if (k <= 11)
						{
							text6 = Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + k, new object[] {
												text6
										});
						}
						Utility.drawTextWithShadow(b, text6, font, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + 34 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += 34;
					}
				}
			}

			if (hoveredObject != null && fishData.ContainsKey(hoveredObject.ParentSheetIndex) && !hoveredObject.Name.Contains("Algae") && !hoveredObject.Name.Contains("Seaweed"))
			{
				// Add the height of the seasons icons 
				var data = fishData[hoveredObject.ParentSheetIndex].Split('/');
				if (data[1] != "trap")
				{

					float curXOffset = (float)(prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(curXOffset, (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), fishIconSourceRect, Color.White, 0f, Vector2.Zero);
					curXOffset += (fishIconSourceRect.Width + 2) * Game1.pixelZoom;
					var weather = data[7].Split(' ');
					if (!weather.Contains("both"))
					{ // if all weather don't draw any
						if (weather.Contains("rainy"))
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(curXOffset, (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom + (fishIconSourceRect.Height * Game1.pixelZoom) - rainIconSourceRect.Height * 2.5f)), rainIconSourceRect, Color.White, 0f, Vector2.Zero, 2.5f);
						}
						else
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(curXOffset, (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom + (fishIconSourceRect.Height * Game1.pixelZoom) - sunnyIconSourceRect.Height * 2.5f)), sunnyIconSourceRect, Color.White, 0f, Vector2.Zero, 2.5f);
						}
					}

					var times = data[5].Split(' ');
					string timesString = "";
					for (int i = 0; i < times.Length; i++)
					{
						int time = (int.Parse(times[i]) / 100);
						timesString += time - (time > 12 ? 12 * (int)(time / 12) : 0);
						if (time >= 12 && time < 24)
							timesString += "pm";
						else
							timesString += "am";

						if (i % 2 == 1 && i != times.Length - 1)
						{
							timesString += ", ";
						}
						else if (i % 2 == 0)
						{
							timesString += "-";
						}
					}
					curXOffset += (!weather.Contains("both") ? (rainIconSourceRect.Width * 3f) + 2 * Game1.pixelZoom : 0);
					Utility.drawTextWithShadow(b, timesString, font, new Vector2(curXOffset, (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom + (fishIconSourceRect.Height * Game1.pixelZoom) - font.MeasureString("T").Y + (1 * Game1.pixelZoom))), Game1.textColor);

					prevMouseYOffset += (fishIconSourceRect.Height + 2) * Game1.pixelZoom;
					// show times
					var seasonIconSize = (summerIconSourceRect.Width + 2) * Game1.pixelZoom;
					var multiplier = 0;
					var seasons = data[6].Split(' ');
					if (seasons.Count() > 0 && seasons.Count() < 4)
					{ // if all seasons don't draw any

						if (seasons.Contains("spring"))
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), springIconSourceRect, Color.White, 0f, Vector2.Zero);
							multiplier++;
						}
						if (seasons.Contains("summer"))
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), summerIconSourceRect, Color.White, 0f, Vector2.Zero);
							multiplier++;
						}
						if (seasons.Contains("fall"))
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), fallIconSourceRect, Color.White, 0f, Vector2.Zero);
							multiplier++;
						}
						if (seasons.Contains("winter"))
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), winterIconSourceRect, Color.White, 0f, Vector2.Zero);
						prevMouseYOffset += (summerIconSourceRect.Height + 2) * Game1.pixelZoom;
					}
				}
			}
			else if (hoveredObject != null && treeData.ContainsKey(hoveredObject.ParentSheetIndex))
			{
				var data = treeData[hoveredObject.ParentSheetIndex].Split('/');

				var seasonIconSize = (summerIconSourceRect.Width + 2) * Game1.pixelZoom;
				var multiplier = 0;

				var seasons = data[1].Split(' ');
				if (seasons.Count() > 0 && seasons.Count() < 4)
				{
					if (seasons.Contains("spring"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), springIconSourceRect, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("summer"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), summerIconSourceRect, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("fall"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), fallIconSourceRect, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("winter"))
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), winterIconSourceRect, Color.White, 0f, Vector2.Zero);
					prevMouseYOffset += (summerIconSourceRect.Height + 2) * Game1.pixelZoom;
				}

			}
			else if (hoveredObject != null && cropData.Exists(x => { return x.Split('/')[3] == $"{hoveredObject.ParentSheetIndex}"; }))
			{
				// TODO 
				var data = cropData.Find(x => { return x.Split('/')[3] == $"{hoveredObject.ParentSheetIndex}"; }).Split('/');

				var seasonIconSize = (summerIconSourceRect.Width + 2) * Game1.pixelZoom;
				var multiplier = 0;
				var seasons = data[1].Split(' ');
				if (seasons.Count() > 0 && seasons.Count() < 4)
				{
					if (seasons.Contains("spring"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), springIconSourceRect, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("summer"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), summerIconSourceRect, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("fall"))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), fallIconSourceRect, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("winter"))
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float)(prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), winterIconSourceRect, Color.White, 0f, Vector2.Zero);
					prevMouseYOffset += (summerIconSourceRect.Height + 2) * Game1.pixelZoom;
				}
			}

			if (hoveredItem != null && hoveredItem.attachmentSlots() > 0)
			{
				prevMouseYOffset += 16;
				hoveredItem.drawAttachments(b, prevMouseXOffset + Game1.tileSize / 4, prevMouseYOffset);
				if (moneyAmount > -1)
				{
					//if (extraItemToShowIndex > -1) {
					prevMouseYOffset += Game1.tileSize * hoveredItem.attachmentSlots();
				}
			}

			/*
			if (moneyAmount > -1) {
				b.DrawString(font, moneyAmount + string.Empty, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(font, moneyAmount + string.Empty, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(font, moneyAmount + string.Empty, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 0f), Game1.textShadowColor);
				b.DrawString(font, moneyAmount + string.Empty, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor);
				if (currencySymbol == 0) {
					b.Draw(Game1.debrisSpriteSheet, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4) + font.MeasureString(moneyAmount + string.Empty).X + 20f, (float) (prevMouseYOffset + Game1.tileSize / 4 + 16)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), (float) Game1.pixelZoom, SpriteEffects.None, 0.95f);
				} else if (currencySymbol == 1) {
					b.Draw(Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 8) + font.MeasureString(moneyAmount + string.Empty).X + 20f, (float) (prevMouseYOffset + Game1.tileSize / 4 - 5)), new Rectangle?(new Rectangle(338, 400, 8, 8)), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, SpriteEffects.None, 1f);
				} else if (currencySymbol == 2) {
					b.Draw(Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 8) + font.MeasureString(moneyAmount + string.Empty).X + 20f, (float) (prevMouseYOffset + Game1.tileSize / 4 - 7)), new Rectangle?(new Rectangle(211, 373, 9, 10)), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, SpriteEffects.None, 1f);
				}
				prevMouseYOffset += Game1.tileSize * 3 / 4;
			}*/

			if (extraItemToShowIndex != -1)
			{
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), prevMouseXOffset, prevMouseYOffset + Game1.pixelZoom, width, Game1.tileSize * 3 / 2, Color.White, 1f, true);
				prevMouseYOffset += Game1.pixelZoom * 5;
				string[] array2 = Game1.objectInformation[extraItemToShowIndex].Split(new char[] {
						'/'
				});
				string text7 = array2[4];
				string text8 = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", new object[] {
						extraItemToShowAmount,
						text7
				});
				b.DrawString(font, text8, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.pixelZoom)) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(font, text8, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.pixelZoom)) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(font, text8, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.pixelZoom)) + new Vector2(2f, 0f), Game1.textShadowColor);
				b.DrawString(Game1.smallFont, text8, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4), (float)(prevMouseYOffset + Game1.pixelZoom)), Game1.textColor);
				b.Draw(Game1.objectSpriteSheet, new Vector2((float)(prevMouseXOffset + Game1.tileSize / 4 + (int)font.MeasureString(text8).X + Game1.tileSize / 3), (float)prevMouseYOffset), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16)), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1f);
			}
		}

		// End of Class
	}
}
