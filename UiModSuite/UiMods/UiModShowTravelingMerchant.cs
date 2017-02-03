using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace UiModSuite.UiMods {
    internal class UiModShowTravelingMerchant {
        List<int> daysMerchantVisits = new List<int>() { 5, 7, 12, 14, 19, 21, 26, 28 };

        public void toggleShowTravelingMerchant() {
            GraphicsEvents.OnPreRenderHudEvent -= drawTravelingMerchant;

            if( OptionsPage.getCheckboxValue( OptionsPage.Setting.SHOW_TRAVELING_MERCHANT ) == true ) {
                GraphicsEvents.OnPreRenderHudEvent += drawTravelingMerchant;
            }

        }

        private void drawTravelingMerchant( object sender, EventArgs e ) {
            if( daysMerchantVisits.Contains( Game1.dayOfMonth ) ) {
                var clickableTextureComponent = new ClickableTextureComponent( new Rectangle( ( int ) DemiacleUtility.getWidthInPlayArea() - 180, 260, 100, 74 ), Game1.content.Load<Texture2D>( "LooseSprites\\Cursors" ), new Rectangle( 192, 1411, 20, 20 ), 2 );
                clickableTextureComponent.draw( Game1.spriteBatch );

                if( clickableTextureComponent.containsPoint( Game1.getMouseX(), Game1.getMouseY() ) ) {
                    string tooltip = $"Traveling merchant is in town!";
                    IClickableMenu.drawHoverText( Game1.spriteBatch, tooltip, Game1.dialogueFont );
                }

            }
        }

    }
}