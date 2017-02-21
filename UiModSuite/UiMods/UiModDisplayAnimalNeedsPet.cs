using System;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Timers;
using UiModSuite.Options;

namespace UiModSuite.UiMods {
    internal class UiModDisplayAnimalNeedsPet {

        private Timer timer;
        private float scale;
        private float movementYPerDraw;
        private float alpha;
        private StardewValley.Object wool = new StardewValley.Object( 440, 1 );

        public UiModDisplayAnimalNeedsPet() {
            timer = new Timer();
            timer.Elapsed += triggerDraw;
        }

        private void onLocationChange( object sender, EventArgsCurrentLocationChanged e ) {

            // Only start timer in places where animals exist
            if( e.NewLocation is AnimalHouse || e.NewLocation is Farm ) {
                timer.Interval = 5000;
                timer.Start();
            } else {
                timer.Stop();
                GraphicsEvents.OnPreRenderHudEvent -= drawHoverTooltip;
            }

        }

        private void triggerDraw( object sender, ElapsedEventArgs e ) {
            GraphicsEvents.OnPreRenderHudEvent -= drawHoverTooltip;
            GraphicsEvents.OnPreRenderHudEvent += drawHoverTooltip;
            scale = 4f;
            movementYPerDraw = -3;
            alpha = 1;
        }

        private void drawHoverTooltip( object sender, EventArgs e ) {

            if( Game1.eventUp || Game1.activeClickableMenu != null ) {
                return;
            }

            StardewValley.SerializableDictionary<long, FarmAnimal> animals = getAnimalsInCurrentLocation();

            if( animals == null ) {
                return;
            }

            foreach( var animal in animals.Values ) {

                if( animal.isEmoting ) {
                    continue;
                }

                // Draw if needs pet
                if( animal.wasPet == false ) {
                    Vector2 handPosition = getPositionAboveAnimal( animal );
                    if( animal.type.Contains( "Cow" ) || animal.type.Contains( "Sheep" ) | animal.type.Contains( "Goat" ) || animal.type.Contains( "Pig" ) ) {
                        handPosition.X += 50;
                        handPosition.Y += 50;
                    }
                    Game1.spriteBatch.Draw( Game1.mouseCursors, new Vector2( handPosition.X, handPosition.Y + movementYPerDraw ), new Rectangle( 32, 0, 16, 16 ), Color.White * alpha, 0, Vector2.Zero, 4f, SpriteEffects.None, 1 );
                }

            }

            scale += 0.01f;
            movementYPerDraw += 0.3f;
            alpha -= 0.014f;

            if( alpha < 0.1f ) {
                GraphicsEvents.OnPreRenderHudEvent -= drawHoverTooltip;
            }

        }

        private Vector2 getPositionAboveAnimal( FarmAnimal animal ) {
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

            return new Vector2( positionX, positionY );
        }

        private StardewValley.SerializableDictionary<long, FarmAnimal> getAnimalsInCurrentLocation() {

            // Get animals from the current location
            if( Game1.currentLocation is AnimalHouse ) {
                var animalHouse = ( AnimalHouse ) Game1.currentLocation;
                return animalHouse.animals;
            } else if( Game1.currentLocation is Farm ) {
                var farm = ( Farm ) Game1.currentLocation;
                return farm.animals;
            } else {
                return null;
            }

        }

        internal void toggleOption() {
            timer.Stop();
            LocationEvents.CurrentLocationChanged -= onLocationChange;
            GraphicsEvents.OnPreRenderHudEvent -= drawAnimalHasProduct;

            if( ModOptionsPage.getCheckboxValue( ModOptionsPage.Setting.SHOW_ANIMALS_NEED_PETS ) ) {
                timer.Start();
                LocationEvents.CurrentLocationChanged += onLocationChange;
                GraphicsEvents.OnPreRenderHudEvent += drawAnimalHasProduct;
            }
        }

        private void drawAnimalHasProduct( object sender, EventArgs e ) {
            if( Game1.eventUp || Game1.activeClickableMenu != null ) {
                return;
            }

            StardewValley.SerializableDictionary<long, FarmAnimal> animals = getAnimalsInCurrentLocation();

            if( animals == null ) {
                return;
            }

            foreach( var animal in animals.Values ) {

                // Draw nothing if emoting or if truffles
                if( animal.isEmoting || animal.currentProduce == 430 ) {
                    continue;
                }

                // Check if animal has a yield
                if( animal.currentProduce > 0 && animal.age >= ( int ) animal.ageWhenMature ) {
                    Vector2 speechBubblePosition = getPositionAboveAnimal( animal );

                    // Make that bubble into a sin!! ohhh offset by its hashname so nothing lines up... so twist!
                    speechBubblePosition.Y += (float) Math.Sin ( Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 300 + animal.name.GetHashCode() ) * 5;

                    // Draw speech bubble
                    Game1.spriteBatch.Draw( Game1.emoteSpriteSheet, new Vector2( speechBubblePosition.X + 14, speechBubblePosition.Y ), new Rectangle( 3 * ( Game1.tileSize / 4 ) % Game1.emoteSpriteSheet.Width, 3 * ( Game1.tileSize / 4 ) / Game1.emoteSpriteSheet.Width * ( Game1.tileSize / 4 ), Game1.tileSize / 4, Game1.tileSize / 4 ), Color.White * 0.9f, 0, Vector2.Zero, 4f, SpriteEffects.None, 1 );

                    // Draw item
                    Rectangle? sourceRectangle1 = new Microsoft.Xna.Framework.Rectangle?( Game1.currentLocation.getSourceRectForObject( animal.currentProduce ) );
                    Game1.spriteBatch.Draw( Game1.objectSpriteSheet, new Vector2( speechBubblePosition.X + 28, speechBubblePosition.Y + 8 ), sourceRectangle1, Color.White * 0.9f, 0, Vector2.Zero, 2.2f, SpriteEffects.None, ( float ) 1 );

                }

            }

        }

    }
}