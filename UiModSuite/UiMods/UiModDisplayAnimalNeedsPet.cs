using System;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Timers;

namespace UiModSuite.UiMods {
    internal class UiModDisplayAnimalNeedsPet {

        private Timer timer;
        private float scale;
        private float movementYPerDraw;
        private float alpha;

        public UiModDisplayAnimalNeedsPet() {
            LocationEvents.CurrentLocationChanged += onLocationChange;
            timer = new Timer();
            timer.Elapsed += triggerDraw;
        }

        private void onLocationChange( object sender, EventArgsCurrentLocationChanged e ) {
            if( e.NewLocation is AnimalHouse || e.NewLocation is Farm ) {
                timer.Interval = 5000;
                timer.Start();
            } else {
                timer.Stop();
                GraphicsEvents.OnPostRenderEvent -= drawHoverTooltip;
            }
        }

        private void triggerDraw( object sender, ElapsedEventArgs e ) {
            GraphicsEvents.OnPostRenderEvent += drawHoverTooltip;
            scale = 4f;
            movementYPerDraw = -3;
            alpha = 1;
        }

        private void drawHoverTooltip( object sender, EventArgs e ) {

            if( Game1.eventUp || Game1.activeClickableMenu != null ) {
                return;
            }

            StardewValley.SerializableDictionary<long, FarmAnimal> animals;

            // Get animals from the current location
            if( Game1.currentLocation is AnimalHouse ) {
                var animalHouse = ( AnimalHouse ) Game1.currentLocation;
                animals = animalHouse.animals;
            } else if( Game1.currentLocation is Farm ) {
                var farm = ( Farm ) Game1.currentLocation;
                animals = farm.animals;
            } else {
                return;
            }

            foreach( var animal in animals.Values ) {

                if( animal.wasPet == false ) {

                    float positionX = animal.position.X;
                    float positionY = animal.position.Y;

                    if( Game1.viewport.Width > Game1.currentLocation.map.DisplayWidth ) {
                        positionX += ( ( Game1.viewport.Width - Game1.currentLocation.map.DisplayWidth ) / 2 ) + 18;
                    } else {
                        positionX -= Game1.viewport.X - 16;
                    }

                    if( Game1.viewport.Height > Game1.currentLocation.map.DisplayHeight ) {
                        positionY += ( ( Game1.viewport.Height - Game1.currentLocation.map.DisplayHeight ) / 2 ) - 50;
                    } else {
                        positionY -= Game1.viewport.Y + 54;
                    }

                    Game1.spriteBatch.Draw( Game1.mouseCursors, new Vector2( positionX, positionY + movementYPerDraw ), new Rectangle( 32, 0, 16, 16 ), Color.White * alpha, 0, Vector2.Zero, 4f, SpriteEffects.None, 1 );
                }

            }

            scale += 0.01f;
            movementYPerDraw += 0.3f;
            alpha -= 0.014f;

            if( alpha < 0.1f ) {
                GraphicsEvents.OnPostRenderEvent -= drawHoverTooltip;
            }

        }
    }
}