using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;

namespace UiModSuite.Options {
    internal class UiModDisplayScarecrowAndSprinklerRange {

        List<Point> effectiveArea = new List<Point>();

        public UiModDisplayScarecrowAndSprinklerRange() {

            
        }

        private void checkDrawTileOutlines( object sender, EventArgs e ) {

            effectiveArea.Clear();
            if( Game1.player.CurrentItem == null || Game1.activeClickableMenu != null || Game1.eventUp != false ) {
                return;
            }
            var player = Game1.player;


            string itemName = Game1.player.CurrentItem.Name;
            

            if( itemName.Contains( "arecrow" ) ) {

                int centerX = ( Game1.getMouseX() + Game1.viewport.X ) / Game1.tileSize;
                int centerY = ( Game1.getMouseY() + Game1.viewport.Y ) / Game1.tileSize;

                int width = 17;
                int height = 17;

                for( int w = 0; w < width; w++ ) {
                    for( int h = 0; h < height; h++ ) {

                        // Don't count distances of 12 from center
                        if( Math.Abs( w - 8 ) + Math.Abs( h - 8 ) > 12 ) {
                            continue;
                        }

                        effectiveArea.Add( new Point( tileUnderMouseX() + w - 8, tileUnderMouseY() + h - 8 ) );
                    }
                }

            } else if( itemName.Contains( "Iridium Sprinkler" ) ) {

                int width = 5;
                int height = 5;

                for( int i = 0;  i < width; i++ ) {
                    for( int j = 0; j < height; j++ ) {
                        effectiveArea.Add( new Point( tileUnderMouseX() + i - 2, tileUnderMouseY() + j - 2 ) );
                    }
                }

            } else if( itemName.Contains( "Quality Sprinkler" ) ) {

                int width = 3;
                int height = 3;

                for( int i = 0; i < width; i++ ) {
                    for( int j = 0; j < height; j++ ) {
                        effectiveArea.Add( new Point( tileUnderMouseX() + i - 1, tileUnderMouseY() + j - 1 ) );
                    }
                }

            } else if( itemName.Contains( "Sprinkler" ) ) {

                effectiveArea.Add( new Point( tileUnderMouseX(), tileUnderMouseY() - 1 ) );
                effectiveArea.Add( new Point( tileUnderMouseX() - 1, tileUnderMouseY() ) );
                effectiveArea.Add( new Point( tileUnderMouseX(), tileUnderMouseY() + 1 ) );
                effectiveArea.Add( new Point( tileUnderMouseX() + 1, tileUnderMouseY() ) );

            }

        }

        private int tileUnderMouseX() {
            return ( Game1.getMouseX() + Game1.viewport.X ) / Game1.tileSize;
        }


        private int tileUnderMouseY() {
            return ( Game1.getMouseY() + Game1.viewport.Y ) / Game1.tileSize;
        }


        private void drawTileOutlines( object sender, EventArgs e ) {
            if( effectiveArea.Count < 1 ) {
                return;
            }

            foreach( var item in effectiveArea ) {
                Game1.spriteBatch.Draw( Game1.mouseCursors, Game1.GlobalToLocal( new Vector2( ( float ) ( item.X * Game1.tileSize ), ( float ) ( item.Y * Game1.tileSize ) ) ), new Rectangle?( new Rectangle( 194, 388, 16, 16 ) ), Color.White * 0.7f, 0.0f, Vector2.Zero, ( float ) Game1.pixelZoom, SpriteEffects.None, 0.01f );
            }



        }

        internal void toggleOption() {
            GraphicsEvents.OnPostRenderEvent -= drawTileOutlines;
            GameEvents.FourthUpdateTick -= checkDrawTileOutlines;
            if( ModOptionsPage.getCheckboxValue( ModOptionsPage.Setting.SHOW_SPRINKLER_SCARECROW_RANGE ) ) {
                GraphicsEvents.OnPostRenderEvent += drawTileOutlines;
                GameEvents.FourthUpdateTick += checkDrawTileOutlines;
            }
        }
    }
}