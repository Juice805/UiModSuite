using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UiModSuite.Options;

namespace UiModSuite.UiMods {
    class UiModItemRolloverInformation {

        Item hoverItem;
        CommunityCenter communityCenter;
        Dictionary<string, List<int>> prunedRequiredBundles = new Dictionary<string, List<int>>();
        Dictionary<string, string> bundleData;
        ClickableTextureComponent bundleIcon =  new ClickableTextureComponent("", new Rectangle( 0, 0, Game1.tileSize, Game1.tileSize), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), (float) Game1.pixelZoom, false);

        /// <summary>
        /// This mod displays an improved tooltip
        /// </summary>
        public void toggleOption() {

            PlayerEvents.InventoryChanged -= populateRequiredBundled;
            GraphicsEvents.OnPostRenderEvent -= drawAdvancedToolip;
            GraphicsEvents.OnPreRenderEvent -= getHoverItem;

            if ( ModOptionsPage.getCheckboxValue( ModOptionsPage.Setting.SHOW_EXTRA_ITEM_INFORMATION ) ) {

                // Load bundle data
                communityCenter = ( CommunityCenter ) Game1.getLocationFromName( "CommunityCenter" );
                bundleData = Game1.content.Load<Dictionary<string, string>>( "Data\\Bundles" );

                // Parse data to easily work with bundle data
                populateRequiredBundled( null, null );

                PlayerEvents.InventoryChanged += populateRequiredBundled;
                GraphicsEvents.OnPostRenderEvent += drawAdvancedToolip;
                GraphicsEvents.OnPreRenderEvent += getHoverItem;
            }
        }

        /// <summary>
        /// Finds all the bundles still needing resources
        /// </summary>
        private void populateRequiredBundled( object sender, EventArgs e ) {

            prunedRequiredBundles.Clear();

            foreach( var item in bundleData ) {

                // This code sucks....
                string bundleArea = item.Key.Split( '/' )[ 0 ];
                int noteInt = 0;

                switch( bundleArea ) {
                    case "Pantry":
                        noteInt = 0;
                        break;
                    case "Crafts Room":
                        noteInt = 1;
                        break;
                    case "Fish Tank":
                        noteInt = 2;
                        break;
                    case "Boiler Room":
                        noteInt = 3;
                        break;
                    case "Vault":
                        noteInt = 4;
                        break;
                    case "Bulletin Board":
                        noteInt = 5;
                        break;
                    default:
                        continue;
                }

                // Ignore items if bundles cannot be turned in
                if( communityCenter.shouldNoteAppearInArea( noteInt ) == false ) {
                    continue;
                }

                // Required since the index in the bundles are all wonky
                int indexInSavedBundleData = Convert.ToInt32( item.Key.Split( '/' )[ 1 ] );

                string[] data = item.Value.Split( '/' );
                string bundleType = data[ 0 ];
                //string reward = data[ 1 ];
                string[] requiredItems = data[ 2 ].Split( ' ' );

                List<int> prunedRequiredItems = new List<int>();

                // Add only every 3rd entry ( required item tile index )
                for( int i = 0; i < requiredItems.Count(); i++ ) {
                    if( i % 3 == 0 ) {
                        if( Convert.ToInt32( requiredItems[ i ] ) == -1 ) {
                            continue;
                        }

                        //ModEntry.Log( $"count is {indexInSavedBundleData} and prunedItems is {prunedRequiredItems.Count()}" );
                        if( communityCenter.bundles[ indexInSavedBundleData ][ prunedRequiredItems.Count() ] == false ) {
                            prunedRequiredItems.Add( Convert.ToInt32( requiredItems[ i ] ) );
                        }
                    }
                }

                prunedRequiredBundles.Add( bundleType, prunedRequiredItems );
            }
        }

        /// <summary>
        /// Draw it!
        /// </summary>
        private void drawAdvancedToolip( object sender, EventArgs e ) {

            if( hoverItem == null ) {
                return;
            }

            string sellForAmount = "";
            string harvestPrice = "";

            int truePrice = getTruePrice( hoverItem );

            if( truePrice > 0 && hoverItem.Name != "Scythe" ) {
                sellForAmount = "\n  " + truePrice / 2;

                if( hoverItem.canStackWith( hoverItem ) && hoverItem.getStack() > 1 ) {
                    sellForAmount += $" ({ truePrice / 2 * hoverItem.getStack() })";
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

            string advancedTitle = hoverItem.Name + sellForAmount + harvestPrice;

            // Prepare tooltip
            float tooltipHeight;
            Vector2 iconPosition = calculateIconPosition( hoverItem.Name + sellForAmount + harvestPrice, out tooltipHeight );
            float iconPositionX = iconPosition.X;
            float iconPositionY = iconPosition.Y;

            // Draw transparency fix
            //Game1.spriteBatch.Draw( Game1.staminaRect, new Rectangle( Game1.getMouseX(), Game1.getMouseY(), 12, (int) tooltipHeight ), Color.White );

            // Draw tooltip
            IClickableMenu.drawToolTip( Game1.spriteBatch, hoverItem.getDescription(), advancedTitle, hoverItem, false, -1, 0, -1, -1, null, -1 );

            // Draw icons inside description text
            if( sellForAmount != "" ) {

                // Draw coin icon
                Game1.spriteBatch.Draw( Game1.debrisSpriteSheet, new Vector2( iconPositionX, iconPositionY ), new Rectangle?( Game1.getSourceRectForStandardTileSheet( Game1.debrisSpriteSheet, 8, 16, 16 ) ), Color.White, 0f, new Vector2( 8f, 8f ), ( float ) Game1.pixelZoom, SpriteEffects.None, 0.95f );
               
                // Draw harvest icon
                if( isDrawingHarvestPrice ) {
                    var spriteRectangle = new Rectangle( 60, 428, 10, 10 );
                    Game1.spriteBatch.Draw( Game1.mouseCursors, new Vector2( iconPositionX + Game1.dialogueFont.MeasureString( sellForAmount ).X - 10, iconPositionY - 20 ), spriteRectangle, Color.White, 0.0f, Vector2.Zero, ( float ) Game1.pixelZoom, SpriteEffects.None, 0.85f );
                }
            }

            // Draw bundle info
            foreach( var bundleType in prunedRequiredBundles ) {
                if( bundleType.Value.Contains( hoverItem.parentSheetIndex ) ) {
                    int xPos = ( int ) iconPositionX - 64;
                    int yPos = ( int ) iconPositionY - 110;

                    int bgPositionX = xPos + 52;
                    int bgPositionY = yPos - 2;
                    int totalWidth = 288;
                    int height = 36;
                    int amountOfSections = 36;
                    int sectionWidth = totalWidth / amountOfSections;
                    int amountOfSectionsWithoutAlpha = 6;

                    for( int i = 0; i < amountOfSections; i++ ) {
                        float alpha;
                        if( i < amountOfSectionsWithoutAlpha ) {
                            alpha = 0.92f;
                        } else {
                            alpha = 0.92f - ( i - amountOfSectionsWithoutAlpha ) * ( 1f / ( amountOfSections - amountOfSectionsWithoutAlpha ) ) ;
                        }
                        Game1.spriteBatch.Draw( Game1.staminaRect, new Rectangle( bgPositionX + (sectionWidth * i), bgPositionY, sectionWidth, height ), Color.Crimson * alpha );
                    }

                    Game1.spriteBatch.DrawString( Game1.dialogueFont, bundleType.Key, new Vector2( xPos + 72, yPos ), Color.White );

                    bundleIcon.bounds.X = xPos + 16;
                    bundleIcon.bounds.Y = yPos;
                    bundleIcon.scale = 3f;
                    bundleIcon.draw( Game1.spriteBatch );

                    break;
                }
            }

            restoreMenuState();
        }

        /// <summary>
        /// This restores the hover item so it will be usable for lookup anything or any other mod
        /// </summary>
        private void restoreMenuState() {
            if( Game1.activeClickableMenu is ItemGrabMenu ) {
                var itemGrabMenu = ( ItemGrabMenu ) Game1.activeClickableMenu;
                itemGrabMenu.hoveredItem = hoverItem;
            }
        }

        /// <summary>
        /// Gets the correct item price as ore prices use the price property
        /// </summary>
        /// <param name="hoverItem">The item</param>
        /// <returns>The correct sell price</returns>
        public static int getTruePrice( Item hoverItem ) {

            // Overwrite ores cause salePrice() is not accurate for some reason...???
            switch( hoverItem.parentSheetIndex ) {
                case 378:
                case 380:
                case 382:
                case 384:
                case 388:
                case 390:
                    return ( int ) ( ( double ) ( (hoverItem as StardewValley.Object).price * 2 ) * ( 1.0 + ( double ) ( hoverItem as StardewValley.Object ).quality * 0.25 ) );
                default:
                    return hoverItem.salePrice();
            }
        }

        /// <summary>
        /// Calculates coin and harvest icons
        /// </summary>
        /// <param name="descriptionToMeasureY">The description text</param>
        /// <param name="height">The height to calculate and return</param>
        /// <returns>The icon position</returns>
        private Vector2 calculateIconPosition( string descriptionToMeasureY, out float height ) {
            float iconPositionX = Game1.getMousePosition().X + 78;
            float iconPositionY = 0;
            height = 0;
            // Width of 11 border pixels
            height += Game1.pixelZoom * 11;

            // Mouse height
            height += 9 * Game1.pixelZoom;
            height += Game1.smallFont.MeasureString( hoverItem.getDescription() ).Y;
            height += Game1.smallFont.MeasureString( hoverItem.getCategoryName() ).Y;
            height += Game1.dialogueFont.MeasureString( descriptionToMeasureY ).Y;

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

            float fixIconTopX = 0;

            // If tooltip is outside Y view bounds
            if( Game1.getMouseY() + height > Game1.viewport.Height ) {
                int offsetY = 112;
                iconPositionY = Game1.viewport.Height - height + offsetY;
            } else {
                int yOffsetFromTopOfBox = 112;
                iconPositionY = Game1.getMousePosition().Y + yOffsetFromTopOfBox;
                fixIconTopX = 16;
            }

            iconPositionX -= fixIconTopX;

            int offsetMouseAndBorder = 64;
            int textWidth = Math.Max( ( int ) Game1.dialogueFont.MeasureString( hoverItem.Name ).X, ( int ) Game1.smallFont.MeasureString( hoverItem.getDescription() ).X );
            int tooltipWidthAndMouseX = ( int ) Game1.getMouseX() + textWidth + offsetMouseAndBorder;

            // If tooltip is outside X view bounds
            if( tooltipWidthAndMouseX > Game1.viewport.Width ) {
                iconPositionX = Game1.viewport.Width - textWidth + 14;
                if( Game1.getMouseY() + height > Game1.viewport.Height ) {
                    // Do nothing
                } else {
                    iconPositionY += 16;
                }
            }

            return new Vector2( iconPositionX, iconPositionY );
        }

        /// <summary>
        /// Gets the hover item and removes the vanilla tooltip displays **HACKY
        /// </summary>
        private void getHoverItem( object sender, EventArgs e ) {

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
                        typeof( InventoryPage ).GetField( "hoverText", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( inventoryPage, "" );
                    }
                }
            }

            // Remove hovers from chests and shipping bin
            if( Game1.activeClickableMenu is ItemGrabMenu ) {
                var itemGrabMenu  = ( ItemGrabMenu ) Game1.activeClickableMenu;
                hoverItem = itemGrabMenu.hoveredItem;
                itemGrabMenu.hoveredItem = null;
            }
        }

    }
}
