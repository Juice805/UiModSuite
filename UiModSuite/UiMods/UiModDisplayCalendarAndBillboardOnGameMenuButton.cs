﻿using System;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Microsoft.Xna.Framework.Input;

namespace UiModSuite.UiMods {
    internal class UiModDisplayCalendarAndBillboardOnGameMenuButton {

        ClickableTextureComponent showBillboardButton = new ClickableTextureComponent( new Rectangle( 0, 0, 99, 60 ), Game1.content.Load<Texture2D>( "Maps\\summer_town" ), new Rectangle( 122, 291, 35, 20 ), 3 );
        //ClickableTextureComponent showCalendarButton = new ClickableTextureComponent( new Rectangle( 0, 0, 0, 0 ), Game1.mouseCursors, new Rectangle( 0, 0, 0, 0 ), 1 );

        public UiModDisplayCalendarAndBillboardOnGameMenuButton() {
            GraphicsEvents.OnPostRenderGuiEvent += renderButtons;
            ControlEvents.MouseChanged += onMouseClick;
        }

        private void onMouseClick( object sender, EventArgsMouseStateChanged e ) {
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

        private void renderButtons( object sender, EventArgs e ) {

            if( ( Game1.activeClickableMenu is GameMenu) == false ) {
                return;
            }

            if( ( Game1.activeClickableMenu as GameMenu ).currentTab != GameMenu.inventoryTab ) {
                return;
            }

            // Set button position
            showBillboardButton.bounds.X = Game1.activeClickableMenu.xPositionOnScreen + Game1.activeClickableMenu.width - 160;
            showBillboardButton.bounds.Y = Game1.activeClickableMenu.yPositionOnScreen + Game1.activeClickableMenu.height - 100;

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

        }
    }
}