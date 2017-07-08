﻿using System;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Microsoft.Xna.Framework.Input;
using UiModSuite.Options;
using System.Reflection;
using System.Collections.Generic;
using StardewConfigFramework;

namespace UiModSuite.UiMods {
    internal class DisplayCalendarAndBillboardOnGameMenuButton {

        ClickableTextureComponent showBillboardButton = new ClickableTextureComponent( new Rectangle( 0, 0, 99, 60 ), Game1.content.Load<Texture2D>( "Maps\\summer_town" ), new Rectangle( 122, 291, 35, 20 ), 3 );
        string hoverText;

		private ModOptionToggle option;
		private ModOptionToggle extraInfoOption;

		public DisplayCalendarAndBillboardOnGameMenuButton()
		{
			this.extraInfoOption = ModEntry.Options.GetOptionWithIdentifier("displayExtraItemInfo") as ModOptionToggle;
			if (this.extraInfoOption == null)
			{
				this.extraInfoOption = new ModOptionToggle("displayExtraItemInfo", "Show extra item information");
				ModEntry.Options.AddModOption(this.extraInfoOption);
			}

			this.option = ModEntry.Options.GetOptionWithIdentifier("displayCal&Billboard") as ModOptionToggle;
			if (this.option == null)
			{
				this.option = new ModOptionToggle("displayCal&Billboard", "Display calendar/billboard button");
				ModEntry.Options.AddModOption(this.option);
			}

			this.option.ValueChanged += toggleOption;
			toggleOption(this.option.identifier, this.option.IsOn);
		}

		/// <summary>
		/// This mod displays a button in GameMenu to show billboard and calendar anywhere
		/// </summary>
		internal void toggleOption(string identifier, bool IsOn)
		{

			GraphicsEvents.OnPostRenderGuiEvent -= renderButtons;
            GraphicsEvents.OnPreRenderGuiEvent -= removeDefaultTooltips;
            ControlEvents.MouseChanged -= onBilboardIconClick;

			if( IsOn ) {
                GraphicsEvents.OnPostRenderGuiEvent += renderButtons;
                GraphicsEvents.OnPreRenderGuiEvent += removeDefaultTooltips;
                ControlEvents.MouseChanged += onBilboardIconClick;
            }
        }

        /// <summary>
        /// Removes default tooltips to allow drawing standard tooltips over the billboard icon only if better tooltips are not showing
        /// </summary>
        private void removeDefaultTooltips( object sender, EventArgs e ) {

            if( Game1.activeClickableMenu is GameMenu == false ) {
                return;
            }

			if( extraInfoOption.IsOn == false ) {
                var pages = ( List<IClickableMenu> ) typeof( GameMenu ).GetField( "pages", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( Game1.activeClickableMenu );
                var inventoryPage = ( InventoryPage ) pages[ GameMenu.inventoryTab ];
                hoverText = ( string ) typeof( InventoryPage ).GetField( "hoverText", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( inventoryPage );
                typeof( InventoryPage ).GetField( "hoverText", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( inventoryPage, "" );
            }
        }

        /// <summary>
        /// Handles the click for the bilboard icon on the GameMenu
        /// </summary>
        private void onBilboardIconClick( object sender, EventArgsMouseStateChanged e ) {

            if( ( Game1.activeClickableMenu is GameMenu ) == false ) {
                return;
            }

            if( ( Game1.activeClickableMenu as GameMenu ).currentTab != GameMenu.inventoryTab ) {
                return;
            }

            if( e.NewState.LeftButton == ButtonState.Pressed && showBillboardButton.containsPoint( Game1.getMouseX(), Game1.getMouseY() ) ) {
                if( Game1.getMouseX() < showBillboardButton.bounds.X + ( showBillboardButton.bounds.Width / 2 ) ) {
                    Game1.activeClickableMenu = new Billboard();
                } else {
                    Game1.activeClickableMenu = new Billboard( true);
                }
            }
        }

        /// <summary>
        /// Draws the billboard button below the inventory screen
        /// </summary>
        private void renderButtons( object sender, EventArgs e ) {

            if( ( Game1.activeClickableMenu is GameMenu) == false ) {
                return;
            }

            if( ( Game1.activeClickableMenu as GameMenu ).currentTab != GameMenu.inventoryTab ) {
                return;
            }

            // Set button position
            showBillboardButton.bounds.X = Game1.activeClickableMenu.xPositionOnScreen + Game1.activeClickableMenu.width - 160;
            showBillboardButton.bounds.Y = Game1.activeClickableMenu.yPositionOnScreen + Game1.activeClickableMenu.height - 300;

            showBillboardButton.draw( Game1.spriteBatch );

            if( showBillboardButton.containsPoint( Game1.getMouseX(), Game1.getMouseY() ) ) {

                string tooltip;
                if( Game1.getMouseX() < showBillboardButton.bounds.X + ( showBillboardButton.bounds.Width / 2 )  ) {
                    tooltip = "Calendar";
                } else {
                    tooltip = "Billboard";
                }

                IClickableMenu.drawHoverText( Game1.spriteBatch, tooltip, Game1.dialogueFont );
            }

            // Redraw tooltips if advanced tooltips are not showing
			if( extraInfoOption.IsOn == false ) {
                var pages = ( List<IClickableMenu> ) typeof( GameMenu ).GetField( "pages", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( Game1.activeClickableMenu );
                var inventoryPage = ( InventoryPage ) pages[ GameMenu.inventoryTab ];
                var hoverTitle = ( string ) typeof( InventoryPage ).GetField( "hoverTitle", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( inventoryPage );
                var hoveredItem = ( Item ) typeof( InventoryPage ).GetField( "hoveredItem", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( inventoryPage );
                var heldItem = ( Item ) typeof( InventoryPage ).GetField( "heldItem", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( inventoryPage );
                IClickableMenu.drawToolTip( Game1.spriteBatch, hoverText, hoverTitle, hoveredItem, heldItem != null, -1, 0, -1, -1, ( CraftingRecipe ) null, -1 );
            }
        }

    }
}