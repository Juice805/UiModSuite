using DemiacleSvm;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;

namespace DemiacleSvm.UiMods {
    class OptionsPage : IClickableMenu {

        private List<OptionData> options = new List<OptionData>();
        private readonly int OPTION_HEIGHT = ( int ) Game1.dialogueFont.MeasureString( "t" ).Y + 40;
        private List<UiModWithOptions> modsWithOptions;
        private bool canClick = true;
        private const int AMOUNT_OF_OPTIONS_TO_SHOW = 9;
        private int currentItemIndex = 0;
        private ClickableTextureComponent scrollBar;
        private ClickableTextureComponent upArrow;
        private ClickableTextureComponent downArrow;
        private Rectangle scrollBarRunner;
        private bool scrolling;

        internal OptionsPage( int x, int y, int width, int height, List<UiModWithOptions> modsWithOptions ) : base( x, y, width, height ) {

            upArrow = new ClickableTextureComponent( new Rectangle( this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom ), Game1.mouseCursors, new Rectangle( 421, 459, 11, 12 ), ( float ) Game1.pixelZoom );
            downArrow = new ClickableTextureComponent( new Rectangle( this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom ), Game1.mouseCursors, new Rectangle( 421, 472, 11, 12 ), ( float ) Game1.pixelZoom );
            scrollBar = new ClickableTextureComponent( new Rectangle( this.upArrow.bounds.X + Game1.pixelZoom * 3, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom ), Game1.mouseCursors, new Rectangle( 435, 463, 6, 10 ), ( float ) Game1.pixelZoom );
            scrollBarRunner = new Rectangle( this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, this.scrollBar.bounds.Width, height - Game1.tileSize * 2 - this.upArrow.bounds.Height - Game1.pixelZoom * 2 );

            int positionOfCheckboxesX = this.xPositionOnScreen + 40;

            this.modsWithOptions = modsWithOptions;

            OptionData title = OptionData.createTitle( "Demiacle Ui Mods - V0.1" );
            title.optionsElement.bounds.X = xPositionOnScreen + 30;

            options.Add( title );

            // Adds all options to a list
            foreach( UiModWithOptions mod in modsWithOptions ) {
                foreach( var option in mod.options ) {
                    options.Add( option );
                    option.optionsElement.bounds.X = xPositionOnScreen + 30;
                }
            }

        }

        private void setOptionsToDisplay() {

            int count = 0;
            int yOffsetFromWindow = 30;

            // Hide all options
            foreach( var optionData in options ) {
                optionData.optionsElement.bounds.Y = Game1.viewport.Height;
            }

            // Display only the currect ones
            for( int i = currentItemIndex; i < Math.Min( options.Count, currentItemIndex + AMOUNT_OF_OPTIONS_TO_SHOW + 1 ); i++ ) {
                options[ i ].optionsElement.bounds.Y = yPositionOnScreen + count * 60 + yOffsetFromWindow;
                count++;
            }

        }

        public override void draw( SpriteBatch b ) {
            setOptionsToDisplay();

            IClickableMenu.drawTextureBox( b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White );

            for( int i = currentItemIndex; i < Math.Min( options.Count, currentItemIndex + AMOUNT_OF_OPTIONS_TO_SHOW ); i++ ) {
                options[ i ].optionsElement.draw( b, 0, 0 );
            }

            // TODO Add hardhware option fix
            Game1.spriteBatch.Draw( Game1.mouseCursors, new Vector2( ( float ) Game1.getMouseX(), ( float ) Game1.getMouseY() ), new Microsoft.Xna.Framework.Rectangle?( Game1.getSourceRectForStandardTileSheet( Game1.mouseCursors, Game1.mouseCursor, 16, 16 ) ), Color.White * Game1.mouseCursorTransparency, 0.0f, Vector2.Zero, ( float ) Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f );

        }

        private void setScrollBarToCurrentIndex() {
            scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max( 1, this.options.Count - AMOUNT_OF_OPTIONS_TO_SHOW ) * this.currentItemIndex + this.upArrow.bounds.Bottom + Game1.pixelZoom;
        }

        public override void leftClickHeld( int x, int y ) {
            if( GameMenu.forcePreventClose ) {
                return;
            }
            base.leftClickHeld( x, y );
            if( this.scrolling ) {
                int y2 = this.scrollBar.bounds.Y;
                this.scrollBar.bounds.Y = Math.Min( this.yPositionOnScreen + this.height - Game1.tileSize - Game1.pixelZoom * 3 - this.scrollBar.bounds.Height, Math.Max( y, this.yPositionOnScreen + this.upArrow.bounds.Height + Game1.pixelZoom * 5 ) );
                float num = ( float ) ( y - this.scrollBarRunner.Y ) / ( float ) this.scrollBarRunner.Height;
                this.currentItemIndex = Math.Min( this.options.Count - 7, Math.Max( 0, ( int ) ( ( float ) this.options.Count * num ) ) );
                this.setScrollBarToCurrentIndex();
                if( y2 != this.scrollBar.bounds.Y ) {
                    Game1.playSound( "shiny4" );
                    return;
                }
            }
        }

        private void downArrowPressed() {
            this.downArrow.scale = this.downArrow.baseScale;
            this.currentItemIndex++;
            this.setScrollBarToCurrentIndex();
        }

        private void upArrowPressed() {
            this.upArrow.scale = this.upArrow.baseScale;
            this.currentItemIndex--;
            this.setScrollBarToCurrentIndex();
        }

        public override void receiveLeftClick( int x, int y, bool playSound = true ) {
            if( !canClick ) {
                return;
            } else {
                canClick = false;
            }

            // Handle scrollBar click
            if( this.scrollBar.containsPoint( x, y ) ) {
                this.scrolling = true;
            }

            // Handle arrow clicks
            if( this.downArrow.containsPoint( x, y ) && this.currentItemIndex < Math.Max( 0, options.Count - 7 ) ) {
                this.downArrowPressed();
                Game1.playSound( "shwip" );
            } else if( this.upArrow.containsPoint( x, y ) && this.currentItemIndex > 0 ) {
                this.upArrowPressed();
                Game1.playSound( "shwip" );
            }

            // Handle option clicks
            for( int i = currentItemIndex; i < Math.Min( options.Count, currentItemIndex + AMOUNT_OF_OPTIONS_TO_SHOW ); i++ ) {

                // Handle checkbox clicks
                if( options[ i ].optionsElement is OptionsCheckbox ) {
                    if ( options[ i ].optionsElement.bounds.Contains( x, y ) ) {
                        var checkBox = ( OptionsCheckbox ) options[ i ].optionsElement;
                        string optionLabel = options[ i ].optionsElement.label;
                        checkBox.receiveLeftClick( x, y );
                        ModEntry.modData.checkboxOptions[ checkBox.label ] = checkBox.isChecked;

                        if ( ModEntry.modData.actionList.ContainsKey( checkBox.label )) {
                            ModEntry.modData.actionList[ checkBox.label ].Invoke();
                        }
                    }
                }

                // Handle slider clicks

                // Handle DropDown clicks

            }

            // Check if option is greyed out
            foreach( var item in options ) {
                if( item.evaluationDelegate != null ) {
                    if( item.evaluationDelegate.Invoke() == false ) {
                        item.optionsElement.greyedOut = true;
                    } else {
                        item.optionsElement.greyedOut = false;
                    }
                }
            }

        }

        public override void releaseLeftClick( int x, int y ) {
            base.releaseLeftClick( x, y );
            scrolling = false;
            canClick = true;
        }

        public override void receiveScrollWheelAction( int direction ) {
            base.receiveScrollWheelAction( direction );
            if( direction > 0 && this.currentItemIndex > 0 ) {
                this.upArrowPressed();
                Game1.playSound( "shiny4" );
                return;
            }
            if( direction < 0 && this.currentItemIndex < Math.Max( 0, this.options.Count - AMOUNT_OF_OPTIONS_TO_SHOW ) ) {
                this.downArrowPressed();
                Game1.playSound( "shiny4" );
            }
        }

        public override void receiveRightClick( int x, int y, bool playSound = true ) {
        }
    }
}
