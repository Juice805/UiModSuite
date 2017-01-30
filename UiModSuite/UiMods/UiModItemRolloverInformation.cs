using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DemiacleSvm.UiMods {

    /// <summary>
    /// Handler that overrides Toolbar and InventoryMenu
    /// </summary>
    class UiModItemRolloverInformation :UiModWithOptions {

        public const string SHOW_EXTRA_ITEM_INFORMATION = "Show extra tooltip information";

        Item hoverItem;
        private bool isDrawingShopInformation = false;

        public UiModItemRolloverInformation() {
            addCheckboxOption( SHOW_EXTRA_ITEM_INFORMATION, true, onSettingChange );
        }

        private void onSettingChange() {
            var setting = ( bool ) ModEntry.modData.checkboxOptions[ SHOW_EXTRA_ITEM_INFORMATION ];

            GraphicsEvents.OnPreRenderEvent -= removeDefaultHoverItems;
            GraphicsEvents.OnPostRenderEvent -= drawAdvancedToolip;

            if ( setting ) {
                GraphicsEvents.OnPostRenderEvent += drawAdvancedToolip;
                GraphicsEvents.OnPreRenderEvent += removeDefaultHoverItems;
            }
        }

        private void drawAdvancedToolip( object sender, EventArgs e ) {

            if( hoverItem == null ) {
                return;
            }

            string sellForAmount = "";
            string harvestPrice = "";

            if( hoverItem.salePrice() > 0 && hoverItem.Name != "Scythe" ) {
                sellForAmount = "\n  " + hoverItem.salePrice() / 2;

                if( hoverItem.canStackWith( hoverItem ) && hoverItem.getStack() > 1 ) {
                    sellForAmount += $" ({ hoverItem.salePrice() / 2 * hoverItem.getStack() })";
                }
            }

            bool isDrawingHarvestPrice = false;

            // Adds the price of the fully grown crop to the display text only if it is a seed
            if( hoverItem is StardewValley.Object && ( ( StardewValley.Object ) hoverItem ).type == "Seeds" && sellForAmount != "" ) {

                if( hoverItem.Name != "Mixed Seeds" || hoverItem.Name != "Winter Seeds" ) {
                    var crop = new Crop( hoverItem.parentSheetIndex, 0, 0 );
                    var debris = new Debris( crop.indexOfHarvest, Game1.player.position, Game1.player.position );
                    var item = new StardewValley.Object( debris.chunkType, 1 );
                    harvestPrice += $"    { item.price }";
                    isDrawingHarvestPrice = true;
                }

            }

            // Draws harvest info for seeds in shop
            if( Game1.activeClickableMenu is ShopMenu ) {
                if( isDrawingShopInformation && ( ( StardewValley.Object ) hoverItem ).type == "Seeds" && harvestPrice != "" ) {
                    int positionX = Game1.getMouseX() + 50;
                    int positionY = Game1.getMouseY() - 30;

                    // Box and text
                    Game1.drawDialogueBox( positionX, positionY - 100, 220, 176, false, true );
                    //Game1.drawWithBorder( harvestPrice, Color.Gray, Color.Black, new Vector2( positionX, positionY ) );
                    Game1.spriteBatch.DrawString( Game1.dialogueFont, harvestPrice, new Vector2( positionX + 26, positionY + 4 ), Color.Black );

                    // Harvest icon
                    var spriteRectangle = new Rectangle( 60, 428, 10, 10 );
                    Game1.spriteBatch.Draw( Game1.mouseCursors, new Vector2( positionX + 38, positionY ), spriteRectangle, Color.White, 0.0f, Vector2.Zero, ( float ) Game1.pixelZoom, SpriteEffects.None, 0.85f );

                    isDrawingShopInformation = false;
                }

                return;
            }
            

            IClickableMenu.drawToolTip( Game1.spriteBatch, hoverItem.getDescription(), hoverItem.Name + sellForAmount + harvestPrice, hoverItem, false, -1, 0, -1, -1, null, -1 );
            string test = hoverItem.getDescription();

            // Draw coin
            if( sellForAmount != "" ) {
                float height = 0;

                // Width of 11 border pixels
                height += Game1.pixelZoom * 11;

                // Mouse height
                height += 9 * Game1.pixelZoom;

                height += Game1.smallFont.MeasureString( hoverItem.getDescription() ).Y;

                height += Game1.smallFont.MeasureString( hoverItem.getCategoryName() ).Y;

                height += Game1.dialogueFont.MeasureString( hoverItem.Name + sellForAmount + harvestPrice ).Y;

                // Size of attachmentSlots
                height += ( Game1.tileSize + 4 ) * hoverItem.attachmentSlots();

                // If item is edible
                if( hoverItem is StardewValley.Object && ( hoverItem as StardewValley.Object ).edibility != -300 ) {

                    // Size of health and stamina display
                    if( ( hoverItem as StardewValley.Object ).edibility < 0 ) {
                        height += 39;
                    } else {
                        height += 78;
                    }

                    string[] info = Game1.objectInformation[ hoverItem.parentSheetIndex ].Split( '/' );
                    if( info.Length > 5 && info[ 5 ].Equals( "drink" ) ) {

                    }

                    // Size of buff display
                    if( Game1.objectInformation[ ( hoverItem as StardewValley.Object ).parentSheetIndex ].Split( '/' ).Length >= 7 ) {
                        string[] buffIconsToDisplay = Game1.objectInformation[ ( hoverItem as StardewValley.Object ).parentSheetIndex ].Split( '/' )[ 6 ].Split( ' ' );
                        for( int i = 0; i < buffIconsToDisplay.Count(); i++ ) {
                            if( buffIconsToDisplay[ i ] != "0" ) {
                                height += 34;
                            }
                        }
                        height += 4;
                    }
                }

                float iconPositionY = 0;
                float fixIconTopX = 0;
                float iconPositionX = Game1.getMousePosition().X + 78;

                // If tooltip is outside Y view bounds
                if( Game1.getMouseY() + height > Game1.viewport.Height ) {
                    int offsetY = 112;
                    iconPositionY = Game1.viewport.Height - height + offsetY;
                } else {
                    int yOffsetFromTopOfBox = 112;
                    iconPositionY = Game1.getMousePosition().Y + yOffsetFromTopOfBox;
                    fixIconTopX = 16;
                }

                int offsetMouseAndBorder = 64;
                int textWidth = Math.Max( ( int ) Game1.dialogueFont.MeasureString( hoverItem.Name ).X, ( int ) Game1.smallFont.MeasureString( hoverItem.getDescription() ).X );
                int tooltipWidthAndMouseX = (int) Game1.getMouseX() + textWidth + offsetMouseAndBorder;

                // If tooltip is outside X view bounds
                if( tooltipWidthAndMouseX > Game1.viewport.Width ) {
                    iconPositionX = Game1.viewport.Width - textWidth + 14;
                    if( Game1.getMouseY() + height > Game1.viewport.Height ) {
                        // Do nothing
                    } else {
                        iconPositionY += 16;
                    }
                }


                Game1.spriteBatch.Draw( Game1.debrisSpriteSheet, new Vector2( iconPositionX - fixIconTopX, iconPositionY ), new Rectangle?( Game1.getSourceRectForStandardTileSheet( Game1.debrisSpriteSheet, 8, 16, 16 ) ), Color.White, 0f, new Vector2( 8f, 8f ), ( float ) Game1.pixelZoom, SpriteEffects.None, 0.95f );
               
                // Draw harvest icon
                if( isDrawingHarvestPrice ) {
                    var spriteRectangle = new Rectangle( 60, 428, 10, 10 );
                    Game1.spriteBatch.Draw( Game1.mouseCursors, new Vector2( iconPositionX + Game1.dialogueFont.MeasureString( sellForAmount ).X - 10 - fixIconTopX, iconPositionY - 20 ), spriteRectangle, Color.White, 0.0f, Vector2.Zero, ( float ) Game1.pixelZoom, SpriteEffects.None, 0.85f );
                }

            }

        }

        internal void removeDefaultHoverItems( object sender, EventArgs e ) {

            // Remove hovers from toolbar
            for( int j = 0; j < Game1.onScreenMenus.Count; j++ ) {
                if( Game1.onScreenMenus[ j ] is Toolbar ) {
                    var menu = Game1.onScreenMenus[ j ] as Toolbar;

                    hoverItem = ( Item ) typeof( Toolbar ).GetField( "hoverItem", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( menu );
                    typeof( Toolbar ).GetField( "hoverItem", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( menu, null );
                }
            }

            // Remove hovers from inventory
            if( Game1.activeClickableMenu is GameMenu ) {

                // Get pages from GameMenu            
                var pages = ( List<IClickableMenu> ) typeof( GameMenu ).GetField( "pages", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( Game1.activeClickableMenu );

                // Overwrite Inventory Menu
                for( int i = 0; i < pages.Count; i++ ) {
                    if( pages[ i ] is InventoryPage ) {
                        var inventoryPage = ( InventoryPage ) pages[ i ];
                        hoverItem = ( Item ) typeof( InventoryPage ).GetField( "hoveredItem", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( inventoryPage );
                        typeof( InventoryPage ).GetField( "hoveredItem", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( inventoryPage, null );
                    }
                }
            }

            // Remove hovers from chests and shipping bin
            if( Game1.activeClickableMenu is ItemGrabMenu ) {
                var itemGrabMenu  = ( ItemGrabMenu ) Game1.activeClickableMenu;
                hoverItem = ( Item ) typeof( ItemGrabMenu ).GetField( "hoveredItem", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).GetValue( itemGrabMenu );
                typeof( ItemGrabMenu ).GetField( "hoveredItem", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).SetValue( itemGrabMenu, null );
            }

            if( Game1.activeClickableMenu is ShopMenu ) {
                var shopMenu = ( ShopMenu ) Game1.activeClickableMenu;
                hoverItem = ( Item ) typeof( ShopMenu ).GetField( "hoveredItem", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).GetValue( shopMenu );
                isDrawingShopInformation = true;
            }
        }

    }
}
