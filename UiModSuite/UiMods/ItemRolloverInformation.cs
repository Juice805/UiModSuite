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
using UiModSuite.Utilities;

namespace UiModSuite.UiMods {
	class ItemRolloverInformation {

		Dictionary<int, string> fishData = ModEntry.Helper.Content.Load<Dictionary<int, string>>(Path.Combine("Data", "Fish.xnb"), StardewModdingAPI.ContentSource.GameContent);
		List<string> cropData = ModEntry.Helper.Content.Load<Dictionary<int, string>>(Path.Combine("Data", "Crops.xnb"), StardewModdingAPI.ContentSource.GameContent).Values.ToList();
		Dictionary<int, string> treeData = ModEntry.Helper.Content.Load<Dictionary<int, string>>(Path.Combine("Data", "fruitTrees.xnb"), StardewModdingAPI.ContentSource.GameContent);
		Dictionary<string, string> bundleData = ModEntry.Helper.Content.Load<Dictionary<string, string>>(Path.Combine("Data", "Bundles.xnb"), StardewModdingAPI.ContentSource.GameContent);

		List<int> springForage = new List<int> { 16, 18, 20, 22, 399, 257, 404, 296 };
		List<int> summerForage = new List<int> { 396, 402, 420, 259 };
		List<int> fallForage = new List<int> { 406, 408, 410, 281, 404, 420 };
		List<int> winterForage = new List<int> { 412, 414, 416, 418, 283 };

		Components components = new Components();
		Item hoverItem;
		CommunityCenter communityCenter = (CommunityCenter)Game1.getLocationFromName("CommunityCenter");
		Dictionary<string, List<int>> prunedRequiredBundles = new Dictionary<string, List<int>>();
		ClickableTextureComponent bundleIcon = new ClickableTextureComponent("", new Rectangle(0, 0, Game1.tileSize, Game1.tileSize), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), (float)Game1.pixelZoom, false);

		private ModOptionToggle option;

		public ItemRolloverInformation() {
			this.option = ModEntry.Options.GetOptionWithIdentifier<ModOptionToggle>("displayExtraItemInfo") ?? new ModOptionToggle("displayExtraItemInfo", "Show extra item information");
			ModEntry.Options.AddModOption(this.option);

			this.option.ValueChanged += toggleOption;
			toggleOption(this.option.identifier, this.option.IsOn);
		}

		/// <summary>
		/// This mod displays an improved tooltip
		/// </summary>
		public void toggleOption(string identifier, bool IsOn) {
			PlayerEvents.InventoryChanged -= populateRequiredBundled;
			GraphicsEvents.OnPostRenderEvent -= drawAdvancedToolipForMenu;
			GraphicsEvents.OnPostRenderHudEvent -= drawAdvancedToolipForToolbar;
			GraphicsEvents.OnPreRenderEvent -= getHoverItem;

			if (IsOn) {

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

		private void drawAdvancedToolipForMenu(object sender, EventArgs e) {
			if (Game1.activeClickableMenu != null) {
				drawAdvancedToolip(sender, e);
			}
		}

		private void drawAdvancedToolipForToolbar(object sender, EventArgs e) {
			if (Game1.activeClickableMenu == null) {
				drawAdvancedToolip(sender, e);
			}
		}

		/// <summary>
		/// Finds all the bundles still needing resources
		/// </summary>
		private void populateRequiredBundled(object sender, EventArgs e) {
			bundleData = ModEntry.Helper.Content.Load<Dictionary<string, string>>(Path.Combine("Data", "Bundles.xnb"), StardewModdingAPI.ContentSource.GameContent);
		}

		/// <summary>
		/// Draw it!
		/// </summary>
		private void drawAdvancedToolip(object sender, EventArgs e) {

			if (hoverItem != null)
				drawToolTip(Game1.spriteBatch, hoverItem);
		}

		/// <summary>
		/// This restores the hover item so it will be usable for lookup anything or any other mod
		/// </summary>
		private void restoreMenuState() {
			if (Game1.activeClickableMenu is ItemGrabMenu) {
				var itemGrabMenu = (ItemGrabMenu)Game1.activeClickableMenu;
				itemGrabMenu.hoveredItem = hoverItem;
			}
		}

		/// <summary>
		/// Gets the correct item price as ore prices use the price property
		/// </summary>
		/// <param name="hoverItem">The item</param>
		/// <returns>The correct sell price</returns>
		public static int getTruePrice(Item hoverItem) {
			if (hoverItem is StardewValley.Object) {

				// No clue why selToStorePrice needs to be multiplied for the correct value...???
				return (hoverItem as StardewValley.Object).sellToStorePrice() * 2;
			}

			// Overwrite ores cause salePrice() is not accurate for some reason...???
			switch (hoverItem.parentSheetIndex) {
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
		private void getHoverItem(object sender, EventArgs e) {
			var test = Game1.player.items;

			// Remove hovers from toolbar
			for (int j = 0; j < Game1.onScreenMenus.Count; j++) {
				if (Game1.onScreenMenus[j] is Toolbar) {
					var menu = Game1.onScreenMenus[j] as Toolbar;

					hoverItem = (Item)typeof(Toolbar).GetField("hoverItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);
					typeof(Toolbar).GetField("hoverItem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(menu, null);
				}
			}

			// Remove hovers from inventory
			if (Game1.activeClickableMenu is GameMenu) {

				// Get pages from GameMenu            
				var pages = (List<IClickableMenu>)typeof(GameMenu).GetField("pages", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Game1.activeClickableMenu);

				// Overwrite Inventory Menu
				for (int i = 0; i < pages.Count; i++) {
					if (pages[i] is InventoryPage) {
						var inventoryPage = (InventoryPage)pages[i];
						hoverItem = (Item)typeof(InventoryPage).GetField("hoveredItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(inventoryPage);
						typeof(InventoryPage).GetField("hoverText", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(inventoryPage, "");
					}
				}
			}

			// Remove hovers from chests and shipping bin
			if (Game1.activeClickableMenu is ItemGrabMenu) {
				var itemGrabMenu = (ItemGrabMenu)Game1.activeClickableMenu;
				hoverItem = itemGrabMenu.hoveredItem;
				itemGrabMenu.hoveredItem = null;
			}
		}

		// --------------------Below is the copied tooltip modified to my own specifications--------------------

		//(SpriteBatch b, SpriteFont font, Item hoveredItem, string title, string description, int moneyAmount, int healAmount, int currencySymbol = 0, string[] buffTitles = null, CraftingRecipe craftingIngredients = null, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, float alpha = 1f, int xOffset = 0, int yOffset = 0, int overrideX = -1, int overrideY = -1)
		public void drawToolTip(SpriteBatch b, Item hoveredItem) {

			string title = hoveredItem.DisplayName;
			string description = hoveredItem.getDescription();

			// standardize empty titles to null
			if (title != null && title == String.Empty) {
				title = null;
			}

			StardewValley.Object hoveredObject = (hoveredItem as StardewValley.Object);
			bool EdibleItem = hoveredObject != null && hoveredObject.edibility != -300;

			string[] buffIcons = (string[])null;

			int healAmount = -1;
			int currencySymbol = 0;
			int price = -1;

			string[] fields;

			if (hoveredObject == null) {
				if (hoveredItem.Name != "Scythe" && !(hoveredItem is FishingRod)) // weird exceptions
					price = hoveredItem.salePrice();
			} else {
				price = hoveredObject.sellToStorePrice();
				fields = Game1.objectInformation[hoveredItem.parentSheetIndex].Split('/');

				if (fields.Length >= 8) {
					buffIcons = fields[7].Split(' ');
				}

				healAmount = hoveredObject.Edibility;
			}

			price = (price == 0) ? -1 : price;
			drawHoverText(b, Game1.smallFont, hoveredItem, title, description, price, healAmount, currencySymbol, buffIcons);

		}

		// =============================================================================================================================================

		/// <summary>
		/// Draws the hover text.
		/// </summary>
		/// <param name="b">The blue component.</param>
		/// <param name="font">Font.</param>
		/// <param name="hoveredItem">Hovered item.</param>
		/// <param name="title">Title.</param>
		/// <param name="description">Description.</param>
		/// <param name="moneyAmount">Money amount.</param>
		/// <param name="healAmount">Heal amount.</param>
		/// <param name="currencySymbol">Currency symbol.</param>
		/// <param name="buffTitles">Buff titles.</param>
		/// <param name="craftingIngredients">Crafting ingredients.</param>
		/// <param name="extraItemToShowIndex">Extra item to show index.</param>
		/// <param name="extraItemToShowAmount">Extra item to show amount.</param>
		/// <param name="alpha">Alpha.</param>
		public void drawHoverText(SpriteBatch b, SpriteFont font, Item hoveredItem, string title, string description, int moneyAmount, int healAmount, int currencySymbol = 0, string[] buffTitles = null, CraftingRecipe craftingIngredients = null, int extraItemToShowIndex = -1, int extraItemToShowAmount = -1, float alpha = 1f) {

			StardewValley.Object hoveredObject = (hoveredItem as StardewValley.Object);
			bool EdibleItem = hoveredObject != null && hoveredObject.edibility != -300;

			components.Reset();

			int heightBase = 20;
			int baseFontHeight = (int)font.MeasureString("T").Y;
			int titleFontHeight = (int)Game1.dialogueFont.MeasureString("T").Y;

			if (healAmount != -1) {

				if (healAmount > 0) {
					components.energy.hidden = false;
					components.energyIcon.hidden = false;

					components.energy.text = Game1.content.LoadString("Strings\\UI:ItemHover_Energy", new object[] {
								((healAmount <= 0) ? string.Empty : "+") + healAmount
						});

					components.healing.hidden = false;
					components.healingIcon.hidden = false;
					components.healing.text = Game1.content.LoadString("Strings\\UI:ItemHover_Health", new object[] {
								((healAmount <= 0) ? string.Empty : "+") + (int)((float)healAmount * 0.4f)
						});

				} else if (healAmount != -300) {
					components.energy.hidden = false;
					components.energyIcon.hidden = false;
					components.energy.text = Game1.content.LoadString("Strings\\UI:ItemHover_Energy", new object[] {
								string.Empty + healAmount
						});
				}

				components.ExtendBackgroundWidth(components.energy.Width + components.energyIcon.Width + Game1.tileSize / 4);
			}

			if (moneyAmount != -1) {
				components.price.hidden = false;
				components.price.text = (hoveredItem.Stack > 1) ? $"{moneyAmount} ({moneyAmount * hoveredItem.Stack})" : moneyAmount + string.Empty;
				components.price.scale = 0.75f;
				if (currencySymbol == 0)
					components.currencyIcon.hidden = false;

				// If Seed, show final crop price
				if (hoveredObject != null && hoveredObject.type == "Seeds") {

					if (hoveredObject.Name != "Mixed Seeds" && hoveredObject.Name != "Winter Seeds" && hoveredObject.Name != "Summer Seeds" && hoveredObject.Name != "Fall Seeds" && hoveredObject.Name != "Spring Seeds") {
						var crop = new Crop(hoveredObject.parentSheetIndex, 0, 0);
						var debris = new Debris(crop.indexOfHarvest, Game1.player.position, Game1.player.position);
						var item = new StardewValley.Object(debris.chunkType, 1);
						components.price.text += (hoveredItem.Stack > 1) ? $"> {item.Price} ({item.Price * hoveredItem.Stack})" : $"> {item.Price}";
					}
				}
			}

			// MARK: Measuring Required box size
			//components.Background.Width = Math.Max((int)healTextSize.X, Math.Max((int)descriptionTextSize.X, (int)titleTextSize.X)) + Game1.tileSize / 2;
			//components.Background.Height = Math.Max(heightBase * 3, (int)(descriptionTextSize.Y + Game1.tileSize / 1.5) + (moneyAmount > -1 ? (int)(moneyTextSize.Y + 4.0) : 0) + (title != null ? (int)(titleTextSize.Y + (Game1.tileSize / 4)) : 0) + (healAmount >= 0 ? 38 : 0));

			// Determine bundle info
			int bundleIndex = 0;
			int bundleItemIndex = 0;

			// Check if item missing from bundle;
			if (bundleData.Values.ToList().Exists(x => {
				var valueData = x.Split('/');
				var items = valueData[2].Split(' ');

				for (int i = 0; i < items.Count(); i += 3) {
					if (items[i] == $"{hoverItem.parentSheetIndex}") {
						bundleItemIndex = i;
						components.bundleName.text = valueData[0];
						bundleIndex = bundleData.Values.ToList().IndexOf(x);
						return true;
					}
				}
				return false;
			})) {
				var bundleNum = int.Parse(bundleData.Keys.ToList()[bundleIndex].Split('/')[1]);

				if (!communityCenter.bundles[bundleNum][bundleItemIndex / 3]) {
					components.bundleIcon.hidden = false;
					components.Background.Height += components.bundleIcon.Height;
					components.ExtendBackgroundWidth(components.bundleIcon.Width + components.bundleName.Width + Game1.pixelZoom * 6);
				}
			}

			// If extra items need to be shown
			if (extraItemToShowIndex != -1) {
				string[] extraItemData = Game1.objectInformation[extraItemToShowIndex].Split(new char[] {
						'/'
				});
				string extraItemName = extraItemData[0];
				if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en) {
					extraItemName = extraItemData[extraItemData.Length - 1];
				}
				string text3 = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", new object[] {
						extraItemToShowAmount,
						extraItemName
				});
				int extraWidth = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16).Width * 2 * Game1.pixelZoom;
				components.ExtendBackgroundWidth(extraWidth + (int)font.MeasureString(text3).X);
			}

			// buff items add height
			if (buffTitles != null) {
				foreach (string str in buffTitles) {
					if (!str.Equals("0"))
						components.Background.Height += 34;
				}
				components.Background.Height += 4;
			}


			// Fix Height and Widths for special cases
			if (hoveredItem != null) {
				components.Background.Height += (Game1.tileSize + 4) * hoveredItem.attachmentSlots();
				components.category.text = hoveredItem.getCategoryName();
				if (components.category.text.Length > 0) {
					components.category.hidden = false;
					components.ExtendBackgroundWidth(components.category.Width + Game1.tileSize / 2);
					components.Background.Height += components.category.Height;
					components.category.color = hoveredItem.getCategoryColor();
				}
				int maxDmg = 9999;
				int padding = 15 * Game1.pixelZoom + Game1.tileSize / 2;
				if (hoveredItem is MeleeWeapon) {
					MeleeWeapon meleeWeapon = hoveredItem as MeleeWeapon;
					components.Background.Height = Math.Max(heightBase * 3, (int)((title == null) ? 0f : (Game1.dialogueFont.MeasureString(title).Y + (float)(Game1.tileSize / 4))) + Game1.tileSize / 2) + baseFontHeight + (int)((moneyAmount <= -1) ? 0f : (font.MeasureString(moneyAmount + string.Empty).Y + 4f));
					components.Background.Height += ((!(hoveredItem.Name == "Scythe")) ? ((hoveredItem as MeleeWeapon).getNumberOfDescriptionCategories() * Game1.pixelZoom * 12) + 4 : 10);
					components.Background.Height += (int)font.MeasureString(Game1.parseText((hoveredItem as MeleeWeapon).description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y;
					components.ExtendBackgroundWidth(
						(int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Damage", new object[] {
								maxDmg,
								maxDmg
						})).X + padding, (int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Speed", new object[] {
								maxDmg
						})).X + padding, (int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] {
								maxDmg
						})).X + padding, (int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", new object[] {
								maxDmg
						})).X + padding, (int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", new object[] {
								maxDmg
						})).X + padding, (int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Weight", new object[] {
								maxDmg
						})).X + padding);

				} else if (hoveredItem is Boots) {
					components.Background.Height -= (int)components.description.Height;
					components.Background.Height += (int)((float)((hoveredItem as Boots).getNumberOfDescriptionCategories() * Game1.pixelZoom * 12) + font.MeasureString(Game1.parseText((hoveredItem as Boots).description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y);
					components.ExtendBackgroundWidth((int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] {
								maxDmg
					})).X + padding, (int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", new object[] {
								maxDmg
						})).X + padding);
				} else if (EdibleItem) {
					if (healAmount == -1) {
						components.Background.Height += (Game1.tileSize / 2 + Game1.pixelZoom * 1) * ((healAmount <= 0) ? 1 : 2);
					} else if (healAmount == 0) {

					} else {
						// hide healing
						components.Background.Height += Game1.tileSize / 2 + Game1.pixelZoom * 1;
					}

					healAmount = (int)Math.Ceiling((double)hoveredObject.Edibility * 2.5) + hoveredObject.quality * hoveredObject.Edibility;
					components.ExtendBackgroundWidth((int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Energy", new object[] {
								maxDmg
					})).X + padding, (int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Health", new object[] {
								maxDmg
						})).X + padding);
				}


				if (hoveredObject != null && fishData.ContainsKey(hoveredObject.ParentSheetIndex) && !hoveredObject.Name.Contains("Algae") && !hoveredObject.Name.Contains("Seaweed")) {
					// draw the seasons icons 
					var data = fishData[hoveredObject.ParentSheetIndex].Split('/');
					if (data[1] != "trap") {

						components.fishIcon.hidden = false;

						var weather = data[7].Split(' ');
						if (!weather.Contains("both")) { // if all weather don't draw any
							if (weather.Contains("rainy")) {
								components.rainyIcon.hidden = false;
							} else {
								components.sunnyIcon.hidden = false;
							}
						}

						var times = data[5].Split(' ');

						components.fishTimes.text = "";
						if (!(times[0] == "600" && times[1] == "2600")) {
							for (int i = 0; i < times.Length; i++) {
								int time = (int.Parse(times[i]) / 100);
								components.fishTimes.text += time - (time > 12 ? 12 * (int) (time / 12) : 0);
								if (time >= 12 && time < 24)
									components.fishTimes.text += "pm";
								else
									components.fishTimes.text += "am";

								if (i % 2 == 1 && i != times.Length - 1) {
									components.fishTimes.text += ", ";
								} else if (i % 2 == 0) {
									components.fishTimes.text += "-";
								}
							}
						} else {
							components.fishTimes.text = "Any Time";
						}

						// Add height for fishIcon, weather and Times Line
						components.ExtendBackgroundWidth(components.fishIcon.Width + (components.sunnyIcon.Width + (2 * Game1.pixelZoom)) * ((!components.sunnyIcon.hidden || !components.rainyIcon.hidden) ? weather.Count() : 0) + Game1.pixelZoom + components.fishTimes.Width + Game1.tileSize / 2);
						components.Background.Height += components.fishIcon.Height + 2 * Game1.pixelZoom;


						// show seasons
						var seasons = data[6].Split(' ');
						if (seasons.Count() > 0 && seasons.Count() < 4) { // if not all seasons

							components.Background.Height += components.summerIcon.Height + 2 * Game1.pixelZoom;
							components.ExtendBackgroundWidth((components.summerIcon.Width + 2 * Game1.pixelZoom) * seasons.Count());

							if (seasons.Contains("spring"))
								components.springIcon.hidden = false;

							if (seasons.Contains("summer"))
								components.summerIcon.hidden = false;

							if (seasons.Contains("fall"))
								components.fallIcon.hidden = false;

							if (seasons.Contains("winter"))
								components.winterIcon.hidden = false;
						}
					}
				} else if (hoveredObject != null && treeData.Values.ToList().Exists(x => x.Split('/')[2] == $"{hoveredObject.ParentSheetIndex}")) {

					var data = treeData.Values.ToList().Find(x => x.Split('/')[2] == $"{hoveredObject.ParentSheetIndex}").Split('/');

					var seasons = data[1].Split(' ');
					if (seasons.Count() > 0 && seasons.Count() < 4) {

						components.Background.Height += components.summerIcon.Height + 2 * Game1.pixelZoom;
						components.ExtendBackgroundWidth((components.summerIcon.Width + 2 * Game1.pixelZoom) * seasons.Count());

						if (seasons.Contains("spring"))
							components.springIcon.hidden = false;

						if (seasons.Contains("summer"))
							components.summerIcon.hidden = false;

						if (seasons.Contains("fall"))
							components.fallIcon.hidden = false;

						if (seasons.Contains("winter"))
							components.winterIcon.hidden = false;
					}

					// TODO: CHECK BELOW AGAINST DRAW CODE
				} else if (hoveredObject != null && cropData.Exists(x => { return x.Split('/')[3] == $"{hoveredObject.ParentSheetIndex}"; })) {

					var data = cropData.Find(x => { return x.Split('/')[3] == $"{hoveredObject.ParentSheetIndex}"; }).Split('/');

					var seasons = data[1].Split(' ');
					if (seasons.Count() > 0 && seasons.Count() < 4) {

						components.Background.Height += components.summerIcon.Height + 2 * Game1.pixelZoom;
						components.ExtendBackgroundWidth((components.summerIcon.Width + 2 * Game1.pixelZoom) * seasons.Count());

						if (seasons.Contains("spring"))
							components.springIcon.hidden = false;

						if (seasons.Contains("summer"))
							components.summerIcon.hidden = false;

						if (seasons.Contains("fall"))
							components.fallIcon.hidden = false;

						if (seasons.Contains("winter"))
							components.winterIcon.hidden = false;
					}
				} else if (hoveredObject != null
					&& ((fallForage.Contains(hoveredObject.ParentSheetIndex))
					|| (springForage.Contains(hoveredObject.ParentSheetIndex))
					|| (winterForage.Contains(hoveredObject.ParentSheetIndex))
					|| (summerForage.Contains(hoveredObject.ParentSheetIndex))
					)) { // Foraged items

					components.Background.Height += components.summerIcon.Height + 2 * Game1.pixelZoom;
					// Not worrying about width, guaranteed 1 tile

					if (springForage.Contains(hoveredObject.ParentSheetIndex))
						components.springIcon.hidden = false;

					if (summerForage.Contains(hoveredObject.ParentSheetIndex))
						components.summerIcon.hidden = false;

					if (fallForage.Contains(hoveredObject.ParentSheetIndex))
						components.fallIcon.hidden = false;

					if (winterForage.Contains(hoveredObject.ParentSheetIndex))
						components.winterIcon.hidden = false;
				}



				// edibles add Width
				if (buffTitles != null) {
					for (int j = 0; j < buffTitles.Length; j++) {
						if (!buffTitles[j].Equals("0") && j <= 11) {
							components.ExtendBackgroundWidth((int)font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + j, new object[] {
												maxDmg
										})).X + padding);
						}
					}
				}
			}

			// crafting ingredients change dimensions
			if (craftingIngredients != null) {
				components.ExtendBackgroundWidth(components.title.Width + Game1.pixelZoom * 3, Game1.tileSize * 6);
				components.Background.Height += craftingIngredients.getDescriptionHeight(components.Background.Width - Game1.pixelZoom * 2) + ((healAmount != -1) ? 0 : (-Game1.tileSize / 2)) + Game1.pixelZoom * 3;
			}
		}



		// ======================================= DRAW COMPONENTS ====================================================== //

		void drawComponents(SpriteBatch b, Item hoveredItem) {
			int prevMouseXOffset = Game1.getOldMouseX() + Game1.tileSize / 2;
			int prevMouseYOffset = Game1.getOldMouseY() + Game1.tileSize / 2;

			// ensure hover text does not go offscreen right
			if (prevMouseXOffset + components.Background.Width > Utility.getSafeArea().Right) {
				prevMouseXOffset = Utility.getSafeArea().Right - components.Background.Width;
				prevMouseYOffset += Game1.tileSize / 4;
			}

			// ensure hover text does not go offscreen bottom
			if (prevMouseYOffset + components.Background.Height > Utility.getSafeArea().Bottom) {
				prevMouseXOffset += Game1.tileSize / 4;
				if (prevMouseXOffset + components.Background.Width > Utility.getSafeArea().Right) {
					prevMouseXOffset = Utility.getSafeArea().Right - components.Background.Width;
				}
				prevMouseYOffset = Utility.getSafeArea().Bottom - components.Background.Height;
			}

			// MARK: Drawing Content
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), prevMouseXOffset, prevMouseYOffset, components.Background.Width + ((craftingIngredients == null) ? 0 : (Game1.tileSize / 3)), components.Background.Height, Color.White * alpha, 1f, true);

			// Items with no title
			if (!components.title.hidden) {
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), prevMouseXOffset, prevMouseYOffset, components.Background.Width + ((craftingIngredients == null) ? 0 : (Game1.tileSize / 3)), (int) titleFontHeight + Game1.tileSize / 2 + (int) ((hoveredItem == null || components.category.Length <= 0) ? 0f : baseFontHeight) + (int) ((moneyAmount > -1) ? moneyTextSize.Y : 0) + (!components.bundleIcon.hidden ? SourceRects.bundleIcon.Height * Game1.pixelZoom : 0) - Game1.pixelZoom, Color.White * alpha, 1f, false);
				b.Draw(Game1.menuTexture, new Rectangle(prevMouseXOffset + Game1.pixelZoom * 3, prevMouseYOffset + components.title.Height + Game1.tileSize / 2 + (int) ((hoveredItem == null || components.category.Length <= 0) ? 0f : baseFontHeight) + (int) ((moneyAmount > -1) ? moneyTextSize.Y : 0) + (!components.bundleIcon.hidden ? SourceRects.bundleIcon.Height * Game1.pixelZoom : 0) - Game1.pixelZoom, components.Background.Width - Game1.pixelZoom * ((craftingIngredients != null) ? 1 : 6), Game1.pixelZoom), new Rectangle?(new Rectangle(44, 300, 4, 4)), Color.White);

				// Draw bundle info
				if (!components.bundleIcon.hidden) {

					int amountOfSectionsWithoutAlpha = 10;
					int amountOfSections = 36;
					int sectionWidth = (components.Background.Width - components.bundleIcon.Width) / amountOfSections;

					for (int i = 0; i < amountOfSections; i++) {
						float sectionAlpha;
						if (i < amountOfSectionsWithoutAlpha) {
							sectionAlpha = 0.92f;
						} else {
							sectionAlpha = 0.92f - (i - amountOfSectionsWithoutAlpha) * (1f / (amountOfSections - amountOfSectionsWithoutAlpha));
						}
						Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(prevMouseXOffset + SourceRects.bundleIcon.Width * Game1.pixelZoom + (sectionWidth * i), prevMouseYOffset, sectionWidth, SourceRects.bundleIcon.Height * Game1.pixelZoom), Color.Crimson * sectionAlpha);
					}

					components.bundleIcon.drawWithShadow(b, new Vector2(prevMouseXOffset, prevMouseYOffset));
					components.bundleName.draw(b, new Vector2(prevMouseXOffset + components.bundleIcon.Width + Game1.pixelZoom, prevMouseYOffset + 3 * Game1.pixelZoom));
					prevMouseYOffset += components.bundleIcon.Height;
				}

				b.DrawString(components.title.font, components.title.text, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(components.title.font, components.title.text, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(0f, 2f), Game1.textShadowColor);
				components.title.draw(b, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)));
				prevMouseYOffset += components.title.Height;
			}

			prevMouseYOffset -= 4;

			if (!components.price.hidden) {

				b.DrawString(Game1.dialogueFont, components.price.text, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 2f), Game1.textShadowColor, 0f, Vector2.Zero, components.price.scale, SpriteEffects.None, 0f);
				b.DrawString(Game1.dialogueFont, components.price.text, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(0f, 2f), Game1.textShadowColor, 0f, Vector2.Zero, components.price.scale, SpriteEffects.None, 0f);
				b.DrawString(Game1.dialogueFont, components.price.text, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 0f), Game1.textShadowColor, 0f, Vector2.Zero, components.price.scale, SpriteEffects.None, 0f);
				b.DrawString(Game1.dialogueFont, components.price.text, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor, 0f, Vector2.Zero, components.price.scale, SpriteEffects.None, 0f);

				if (currencySymbol == 0) {
					b.Draw(Game1.debrisSpriteSheet, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4) + components.price.Width + 20f, (float) (prevMouseYOffset + Game1.tileSize / 4 + 22)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), (float) Game1.pixelZoom, SpriteEffects.None, 0.95f);
				} else if (currencySymbol == 1) {
					b.Draw(Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 8) + components.price.Width + 20f, (float) (prevMouseYOffset + Game1.tileSize / 4 - 5)), new Rectangle?(new Rectangle(338, 400, 8, 8)), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, SpriteEffects.None, 1f);
				} else if (currencySymbol == 2) {
					b.Draw(Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 8) + components.price.Width + 20f, (float) (prevMouseYOffset + Game1.tileSize / 4 - 7)), new Rectangle?(new Rectangle(211, 373, 9, 10)), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, SpriteEffects.None, 1f);
				}
				prevMouseYOffset += components.price.Height;
			}

			if (hoveredItem != null && components.category.Length > 0) {
				Utility.drawTextWithShadow(b, components.category.text, font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), hoveredItem.getCategoryColor(), 1f, -1f, 2, 2, 1f, 3);
				prevMouseYOffset += baseFontHeight + ((title == null) ? 0 : (Game1.tileSize / 4)) + Game1.pixelZoom;
			} else {
				prevMouseYOffset += ((components.title.hidden) ? 0 : (Game1.tileSize / 4));
			}

			prevMouseYOffset += 4;

			if (hoveredItem != null && hoveredItem is Boots) {
				Boots boots = hoveredItem as Boots;
				Utility.drawTextWithShadow(b, Game1.parseText(boots.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				prevMouseYOffset += (int) font.MeasureString(Game1.parseText(boots.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y;
				if (boots.defenseBonus > 0) {
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] {
								boots.defenseBonus
						}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += (int) Math.Max(font.MeasureString("TT").Y, (float) (12 * Game1.pixelZoom));
				}
				if (boots.immunityBonus > 0) {
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", new object[] {
								boots.immunityBonus
						}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += (int) Math.Max(font.MeasureString("TT").Y, (float) (12 * Game1.pixelZoom));
				}
			} else if (hoveredItem != null && hoveredItem is MeleeWeapon) {
				MeleeWeapon meleeWeapon2 = hoveredItem as MeleeWeapon;
				Utility.drawTextWithShadow(b, Game1.parseText(meleeWeapon2.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				prevMouseYOffset += (int) font.MeasureString(Game1.parseText(meleeWeapon2.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y;
				if (meleeWeapon2.indexOfMenuItemView != 47) {
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(120, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Damage", new object[] {
								meleeWeapon2.minDamage,
								meleeWeapon2.maxDamage
						}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += (int) Math.Max(font.MeasureString("TT").Y, (float) (12 * Game1.pixelZoom));
					if (meleeWeapon2.speed != ((meleeWeapon2.type != 2) ? 0 : -8)) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(130, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						bool flag = (meleeWeapon2.type == 2 && meleeWeapon2.speed < -8) || (meleeWeapon2.type != 2 && meleeWeapon2.speed < 0);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Speed", new object[] {
										((((meleeWeapon2.type != 2) ? meleeWeapon2.speed : (meleeWeapon2.speed - -8)) <= 0) ? string.Empty : "+") + ((meleeWeapon2.type != 2) ? meleeWeapon2.speed : (meleeWeapon2.speed - -8)) / 2
								}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), (!flag) ? (Game1.textColor * 0.9f * alpha) : Color.DarkRed, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int) Math.Max(font.MeasureString("TT").Y, (float) (12 * Game1.pixelZoom));
					}
					if (meleeWeapon2.addedDefense > 0) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] {
										meleeWeapon2.addedDefense
								}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int) Math.Max(font.MeasureString("TT").Y, (float) (12 * Game1.pixelZoom));
					}
					if ((double) meleeWeapon2.critChance / 0.02 >= 2.0) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(40, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", new object[] {
										(int)((double)meleeWeapon2.critChance / 0.02)
								}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int) Math.Max(font.MeasureString("TT").Y, (float) (12 * Game1.pixelZoom));
					}
					if ((double) (meleeWeapon2.critMultiplier - 3f) / 0.02 >= 1.0) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(160, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", new object[] {
										(int)((double)(meleeWeapon2.critMultiplier - 3f) / 0.02)
								}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 11), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int) Math.Max(font.MeasureString("TT").Y, (float) (12 * Game1.pixelZoom));
					}
					if (meleeWeapon2.knockback != meleeWeapon2.defaultKnockBackForThisType(meleeWeapon2.type)) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), new Rectangle(70, 428, 10, 10), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, false, 1f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Weight", new object[] {
										(((float)((int)Math.Ceiling ((double)(Math.Abs (meleeWeapon2.knockback - meleeWeapon2.defaultKnockBackForThisType (meleeWeapon2.type)) * 10f))) <= meleeWeapon2.defaultKnockBackForThisType (meleeWeapon2.type)) ? string.Empty : "+") + (int)Math.Ceiling ((double)(Math.Abs (meleeWeapon2.knockback - meleeWeapon2.defaultKnockBackForThisType (meleeWeapon2.type)) * 10f))
								}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom * 13), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom * 3)), Game1.textColor * 0.9f * alpha, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += (int) Math.Max(font.MeasureString("TT").Y, (float) (12 * Game1.pixelZoom));
					}
				}
			} else if (!string.IsNullOrEmpty(description) && description != " ") {
				b.DrawString(font, description, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 2f), Game1.textShadowColor * alpha);
				b.DrawString(font, description, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(0f, 2f), Game1.textShadowColor * alpha);
				b.DrawString(font, description, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)) + new Vector2(2f, 0f), Game1.textShadowColor * alpha);
				b.DrawString(font, description, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.tileSize / 4 + 4)), Game1.textColor * 0.9f * alpha);
				prevMouseYOffset += (int) font.MeasureString(description).Y + 4;
			}

			if (craftingIngredients != null) {
				craftingIngredients.drawRecipeDescription(b, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset - Game1.pixelZoom * 2)), components.Background.Width);
				prevMouseYOffset += craftingIngredients.getDescriptionHeight(components.Background.Width);
			}

			if (!components.energy.hidden) {
				if (healAmount > 0) {
					
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4)), new Rectangle((healAmount >= 0) ? 0 : 140, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", new object[] {
								((healAmount <= 0) ? string.Empty : "+") + healAmount
						}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + 34 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += 34;
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4)), new Rectangle(0, 438, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Health", new object[] {
								((healAmount <= 0) ? string.Empty : "+") + (int)((float)healAmount * 0.4f)
						}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + 34 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += 34;
				} else if (healAmount != -300) {
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4)), new Rectangle(140, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", new object[] {
								string.Empty + healAmount
						}), font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + 34 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					prevMouseYOffset += 34;
				}
			}

			if (buffTitles != null) {
				for (int k = 0; k < buffTitles.Length; k++) {
					if (!buffTitles[k].Equals("0")) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4)), new Rectangle(10 + k * 10, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
						string text6 = ((Convert.ToInt32(buffTitles[k]) <= 0) ? string.Empty : "+") + buffTitles[k] + " ";
						if (k <= 11) {
							text6 = Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + k, new object[] {
												text6
										});
						}
						Utility.drawTextWithShadow(b, text6, font, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + 34 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + 8)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
						prevMouseYOffset += 34;
					}
				}
			}

			if (hoveredObject != null && fishData.ContainsKey(hoveredObject.ParentSheetIndex) && !hoveredObject.Name.Contains("Algae") && !hoveredObject.Name.Contains("Seaweed")) {
				// draw the seasons icons 
				var data = fishData[hoveredObject.ParentSheetIndex].Split('/');
				if (data[1] != "trap") {

					float curXOffset = (float) (prevMouseXOffset + Game1.tileSize / 4 + Game1.pixelZoom);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(curXOffset, (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.fishIcon, Color.White, 0f, Vector2.Zero);
					curXOffset += (SourceRects.fishIcon.Width + 2) * Game1.pixelZoom;
					var weather = data[7].Split(' ');
					if (!weather.Contains("both")) { // if all weather don't draw any
						if (weather.Contains("rainy")) {
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(curXOffset, (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom + (SourceRects.fishIcon.Height * Game1.pixelZoom) - SourceRects.rainIcon.Height * 2.5f)), SourceRects.rainIcon, Color.White, 0f, Vector2.Zero, 2.5f);
						} else {
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(curXOffset, (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom + (SourceRects.fishIcon.Height * Game1.pixelZoom) - SourceRects.sunnyIcon.Height * 2.5f)), SourceRects.sunnyIcon, Color.White, 0f, Vector2.Zero, 2.5f);
						}
					}

					var times = data[5].Split(' ');
					string timesString = "";
					if (!fishAllDay)
						for (int i = 0; i < times.Length; i++) {
							int time = (int.Parse(times[i]) / 100);
							timesString += time - (time > 12 ? 12 * (int) (time / 12) : 0);
							if (time >= 12 && time < 24)
								timesString += "pm";
							else
								timesString += "am";

							if (i % 2 == 1 && i != times.Length - 1) {
								timesString += ", ";
							} else if (i % 2 == 0) {
								timesString += "-";
							}
						} else {
						timesString = "Any Time";
					}
					curXOffset += (!weather.Contains("both") ? (SourceRects.rainIcon.Width * 3f) + 2 * Game1.pixelZoom : 0);
					Utility.drawTextWithShadow(b, timesString, font, new Vector2(curXOffset, (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom + (SourceRects.fishIcon.Height * Game1.pixelZoom) - font.MeasureString("T").Y + (1 * Game1.pixelZoom))), Game1.textColor);

					prevMouseYOffset += (SourceRects.fishIcon.Height + 2) * Game1.pixelZoom;



					// show seasons
					var seasonIconSize = (SourceRects.summerIcon.Width + 2) * Game1.pixelZoom;
					var multiplier = 0;
					var seasons = data[6].Split(' ');
					if (seasons.Count() > 0 && seasons.Count() < 4) { // if all seasons don't draw any

						if (seasons.Contains("spring")) {
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.springIcon, Color.White, 0f, Vector2.Zero);
							multiplier++;
						}
						if (seasons.Contains("summer")) {
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.summerIcon, Color.White, 0f, Vector2.Zero);
							multiplier++;
						}
						if (seasons.Contains("fall")) {
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.fallIcon, Color.White, 0f, Vector2.Zero);
							multiplier++;
						}
						if (seasons.Contains("winter"))
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.winterIcon, Color.White, 0f, Vector2.Zero);
						prevMouseYOffset += (SourceRects.summerIcon.Height + 2) * Game1.pixelZoom;
					}
				}
			} else if (hoveredObject != null && treeData.Values.ToList().Exists(x => x.Split('/')[2] == $"{hoveredObject.ParentSheetIndex}")) {
				var data = treeData.Values.ToList().Find(x => x.Split('/')[2] == $"{hoveredObject.ParentSheetIndex}").Split('/');

				var seasonIconSize = (SourceRects.summerIcon.Width + 2) * Game1.pixelZoom;
				var multiplier = 0;

				var seasons = data[1].Split(' ');
				if (seasons.Count() > 0 && seasons.Count() < 4) {
					if (seasons.Contains("spring")) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.springIcon, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("summer")) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.summerIcon, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("fall")) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.fallIcon, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("winter"))
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.winterIcon, Color.White, 0f, Vector2.Zero);
					prevMouseYOffset += (SourceRects.summerIcon.Height + 2) * Game1.pixelZoom;
				}

			} else if (hoveredObject != null && cropData.Exists(x => { return x.Split('/')[3] == $"{hoveredObject.ParentSheetIndex}"; })) {
				// TODO 
				var data = cropData.Find(x => { return x.Split('/')[3] == $"{hoveredObject.ParentSheetIndex}"; }).Split('/');

				var seasonIconSize = (SourceRects.summerIcon.Width + 2) * Game1.pixelZoom;
				var multiplier = 0;
				var seasons = data[1].Split(' ');
				if (seasons.Count() > 0 && seasons.Count() < 4) {
					if (seasons.Contains("spring")) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.springIcon, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("summer")) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.summerIcon, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("fall")) {
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.fallIcon, Color.White, 0f, Vector2.Zero);
						multiplier++;
					}
					if (seasons.Contains("winter"))
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.winterIcon, Color.White, 0f, Vector2.Zero);
					prevMouseYOffset += (SourceRects.summerIcon.Height + 2) * Game1.pixelZoom;
				}
			} else if (hoveredObject != null
					 && ((fallForage.Contains(hoveredObject.ParentSheetIndex))
					 || (springForage.Contains(hoveredObject.ParentSheetIndex))
					 || (winterForage.Contains(hoveredObject.ParentSheetIndex))
					 || (summerForage.Contains(hoveredObject.ParentSheetIndex)))
					 ) { // Foraged items
				var seasonIconSize = (SourceRects.summerIcon.Width + 2) * Game1.pixelZoom;
				var multiplier = 0;

				if (springForage.Contains(hoveredObject.ParentSheetIndex)) {
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.springIcon, Color.White, 0f, Vector2.Zero);
					multiplier++;
				}
				if (summerForage.Contains(hoveredObject.ParentSheetIndex)) {
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.summerIcon, Color.White, 0f, Vector2.Zero);
					multiplier++;
				}
				if (fallForage.Contains(hoveredObject.ParentSheetIndex)) {
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.fallIcon, Color.White, 0f, Vector2.Zero);
					multiplier++;
				}
				if (winterForage.Contains(hoveredObject.ParentSheetIndex))
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float) (prevMouseXOffset + seasonIconSize * multiplier + Game1.tileSize / 4 + Game1.pixelZoom), (float) (prevMouseYOffset + Game1.tileSize / 4 + Game1.pixelZoom)), SourceRects.winterIcon, Color.White, 0f, Vector2.Zero);
				prevMouseYOffset += (SourceRects.summerIcon.Height + 2) * Game1.pixelZoom;

				components.Background.Height += (SourceRects.summerIcon.Height + 2) * Game1.pixelZoom;
			}

			if (hoveredItem != null && hoveredItem.attachmentSlots() > 0) {
				prevMouseYOffset += 16;
				hoveredItem.drawAttachments(b, prevMouseXOffset + Game1.tileSize / 4, prevMouseYOffset);
				if (moneyAmount > -1) {
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

			if (extraItemToShowIndex != -1) {
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), prevMouseXOffset, prevMouseYOffset + Game1.pixelZoom, components.Background.Width, Game1.tileSize * 3 / 2, Color.White, 1f, true);
				prevMouseYOffset += Game1.pixelZoom * 5;
				string[] array2 = Game1.objectInformation[extraItemToShowIndex].Split(new char[] {
						'/'
				});
				string text7 = array2[4];
				string text8 = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", new object[] {
						extraItemToShowAmount,
						text7
				});
				b.DrawString(font, text8, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.pixelZoom)) + new Vector2(2f, 2f), Game1.textShadowColor);
				b.DrawString(font, text8, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.pixelZoom)) + new Vector2(0f, 2f), Game1.textShadowColor);
				b.DrawString(font, text8, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.pixelZoom)) + new Vector2(2f, 0f), Game1.textShadowColor);
				b.DrawString(Game1.smallFont, text8, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4), (float) (prevMouseYOffset + Game1.pixelZoom)), Game1.textColor);
				b.Draw(Game1.objectSpriteSheet, new Vector2((float) (prevMouseXOffset + Game1.tileSize / 4 + (int) font.MeasureString(text8).X + Game1.tileSize / 3), (float) prevMouseYOffset), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, extraItemToShowIndex, 16, 16)), Color.White, 0f, Vector2.Zero, (float) Game1.pixelZoom, SpriteEffects.None, 1f);
			}
		}


		class Components {

			public List<HoverComponent> list = new List<HoverComponent>();

			public Components() {
				foreach (var component in this.GetType().GetProperties()) {
					list.Add(component.GetValue(this, null) as HoverComponent);
				}
			}

			public void Reset() {

				list.ForEach(x => {
					x.hidden = true;
					if (x is TextComponent)
						(x as TextComponent).text = "";	
				});
				Background = new Rectangle();
				titleBackground = new Rectangle();
				seperator = new Rectangle();
			}

			public void HideAll() {
				list.ForEach(x => { x.hidden = true; });
			}

			public void ExtendBackgroundWidth(params int[] sizes) {
				foreach (int size in sizes) {
					Background.Width = Math.Max(Background.Width, size);
				}
			}

			public Rectangle Background = new Rectangle();
			public Rectangle titleBackground = new Rectangle();
			public Rectangle seperator = new Rectangle();
			public TextComponent title = new TextComponent(Game1.dialogueFont, Color.Black);
			public TextComponent category = new TextComponent(Game1.smallFont, Color.Black);
			public TextComponent description = new TextComponent(Game1.smallFont, Color.Black);
			public TextComponent healing = new TextComponent(Game1.smallFont, Color.Black);
			public IconComponent healingIcon = new IconComponent(SourceRects.healingIcon);
			public TextComponent energy = new TextComponent(Game1.smallFont, Color.Black);
			public IconComponent energyIcon = new IconComponent(SourceRects.energyIcon);
			public IconComponent bundleIcon = new IconComponent(SourceRects.bundleIcon);
			public TextComponent bundleName = new TextComponent(Game1.smallFont, Color.White);
			public TextComponent price = new TextComponent(Game1.smallFont, Color.Black, 0.75f);
			public IconComponent currencyIcon = new IconComponent(Game1.debrisSpriteSheet, SourceRects.currencyIcon);
			public IconComponent fishIcon = new IconComponent(SourceRects.fishIcon);
			public IconComponent rainyIcon = new IconComponent(SourceRects.rainIcon);
			public IconComponent sunnyIcon = new IconComponent(SourceRects.sunnyIcon);
			public TextComponent fishTimes = new TextComponent(Game1.smallFont, Color.Black);
			public IconComponent springIcon = new IconComponent(SourceRects.springIcon);
			public IconComponent summerIcon = new IconComponent(SourceRects.summerIcon);
			public IconComponent fallIcon = new IconComponent(SourceRects.fallIcon);
			public IconComponent winterIcon = new IconComponent(SourceRects.winterIcon);

		}
		// End of Class
	}

	class HoverComponent {
		public bool hidden = true;
		public virtual int Height { get; }
		public virtual int Width { get; }
	}

	class TextComponent : HoverComponent {

		public TextComponent(SpriteFont font, Color color, float scale = 1f) {
			this.font = font;
			this.color = color;
			this.scale = scale;
		}

		public void Set(string text, SpriteFont font, Color color, float scale = 1f) {
			this.text = text;
			this.font = font;
			this.color = color;
			this.scale = scale;
		}

		public void draw(SpriteBatch b, Vector2 location) {
			b.DrawString(this.font, this.text, location, this.color);
		}

		private Vector2 size => font.MeasureString(text);
		public override int Height => (int)size.Y * Game1.pixelZoom;
		public override int Width => (int)size.X * Game1.pixelZoom;
		public int Length => text.Length;

		public SpriteFont font = null;
		public string text = null;
		public Color color = Color.Black;
		public float scale = 1f;
	}

	class IconComponent : HoverComponent {

		public IconComponent(Rectangle source, float scale = 1f) {
			this.source = source;
			this.scale = scale;
		}

		public IconComponent(Texture2D sheet, Rectangle source, float scale = 1f) {
			this.source = source;
			this.scale = scale;
			this.sheet = sheet;
		}

		public void drawWithShadow(SpriteBatch b, Vector2 location) {
			Utility.drawWithShadow(b, sheet, location, this.source, Color.White, 0f, Vector2.Zero, this.scale);

		}

		public Rectangle source = new Rectangle();
		public float scale = 1f;
		private Texture2D sheet = Game1.mouseCursors;

		public override int Height => source.Height;
		public override int Width => source.Width;

	}
}
