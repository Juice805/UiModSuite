using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiModSuite.UiMods {
    class UiModDisplayCropAndBarrelTime {

        Dictionary<int, string> indexOfCropNames = new Dictionary<int, string>();

        internal void toggleOption() {

            GraphicsEvents.OnPostRenderEvent -= drawHoverTooltip;

            if( OptionsPage.getCheckboxValue( OptionsPage.Setting.SHOW_CROP_AND_BARREL_TOOLTIP_ON_HOVER ) ) {

                GraphicsEvents.OnPostRenderEvent += drawHoverTooltip;
            }

        }

        private void drawHoverTooltip( object sender, EventArgs e ) {
            
            if( Game1.currentLocation.objects.ContainsKey( Game1.currentCursorTile ) ) {
                StardewValley.Object groundObject = Game1.currentLocation.objects[ Game1.currentCursorTile ];

                // handle object information from objects.cask and maybe others? needs testing
                if( groundObject.minutesUntilReady > 0 ) {
                    //groundObject.minutesUntilReady;

                    // Parse string
                    int hours = groundObject.minutesUntilReady / 60;
                    int minutes = groundObject.minutesUntilReady % 60;

                    string tooltip;
                    if( hours > 0 ) {
                        tooltip = $"{hours} hours, {minutes} minutes";
                    } else {
                        tooltip = $"{minutes} minutes";
                    }

                    IClickableMenu.drawHoverText( Game1.spriteBatch, tooltip, Game1.smallFont );
                    return;
                }

            }

            if( Game1.currentLocation.terrainFeatures.ContainsKey( Game1.currentCursorTile ) ) {

                TerrainFeature terrainFeature = Game1.currentLocation.terrainFeatures[ Game1.currentCursorTile ];

                if( terrainFeature is HoeDirt && ( terrainFeature as HoeDirt).crop != null ) {
                    var hoeDirt = (HoeDirt) terrainFeature;

                    if( hoeDirt.crop.dead ) {
                        return;
                    }

                    int daysUntilHarvest = 0;

                    for( int i = 0; i < hoeDirt.crop.phaseDays.Count - 1; i++ ) {

                        // Subtract amount of days spent in this phase
                        if( hoeDirt.crop.currentPhase == i ) {
                            daysUntilHarvest -= hoeDirt.crop.dayOfCurrentPhase;
                        }

                        // Count amount of days in each phase that hasn't been reached yet
                        if( i >= hoeDirt.crop.currentPhase ) {
                            daysUntilHarvest += hoeDirt.crop.phaseDays[ i ]; 
                        }

                    }

                    // If fully grown and will grow more harvest
                    if( hoeDirt.crop.fullyGrown && hoeDirt.crop.dayOfCurrentPhase !=0 ) {
                        daysUntilHarvest = hoeDirt.crop.dayOfCurrentPhase;
                    }
                
                    string tooltip;

                    if( daysUntilHarvest == 0 ) {
                        tooltip = "Ready to harvest!";
                    } else {
                        string cropName;

                        // Cache crop name 
                        if( indexOfCropNames.ContainsKey( hoeDirt.crop.indexOfHarvest ) ) {
                            cropName = indexOfCropNames[ hoeDirt.crop.indexOfHarvest ];
                        }else {
                            var debris = new Debris( hoeDirt.crop.indexOfHarvest, Vector2.Zero, Vector2.Zero );
                            var item = new StardewValley.Object( debris.chunkType, 1 );
                            cropName = item.name;
                            indexOfCropNames.Add( hoeDirt.crop.indexOfHarvest, cropName );
                        }

                        tooltip = $"{cropName}: {daysUntilHarvest} days";
                    }

                    IClickableMenu.drawHoverText( Game1.spriteBatch, tooltip, Game1.smallFont );
                }

            }

        }

    }
}
