using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace UiModSuite.UiMods {
    class OptionsPageHandler {
        private List<OptionsElement> options = new List<OptionsElement>();
        private OptionsPageButton optionPageButton;

        public OptionsPageHandler( ) {
            ControlEvents.KeyPressed += onKeyPress;
            TimeEvents.DayOfMonthChanged += saveModData;
            MenuEvents.MenuChanged += addModOptionButtonToMenu;
            MenuEvents.MenuClosed += removeModOptionButtonFromMenu;

            options.Add( new OptionsElement( "UiModeSuite v0.1: Demiacle" ) );
            options.Add( new ModOptionsCheckbox( "Show experience bar", ( int ) OptionsPage.Setting.SHOW_EXPERIENCE_BAR ) );
            options.Add( new ModOptionsCheckbox( "Allow experience bar to fade out", ( int ) OptionsPage.Setting.ALLOW_EXPERIENCE_BAR_TO_FADE_OUT ) );
            options.Add( new ModOptionsCheckbox( "Show experience gain", ( int ) OptionsPage.Setting.SHOW_EXP_GAIN ) );
            options.Add( new ModOptionsCheckbox( "Show level up animation", ( int ) OptionsPage.Setting.SHOW_LEVEL_UP_ANIMATION ) );
            options.Add( new ModOptionsCheckbox( "Show heart fills", ( int ) OptionsPage.Setting.SHOW_HEART_FILLS ) );
            options.Add( new ModOptionsCheckbox( "Show extra item information", ( int ) OptionsPage.Setting.SHOW_EXTRA_ITEM_INFORMATION ) );
            options.Add( new ModOptionsCheckbox( "Show townspeople on map", ( int ) OptionsPage.Setting.SHOW_LOCATION_Of_TOWNSPEOPLE ) );
            options.Add( new ModOptionsCheckbox( "Show luck icon", ( int ) OptionsPage.Setting.SHOW_LUCK_ICON ) );

            OptionsPage.syncSettingsToLoadedData( options );
        }



        private void removeModOptionButtonFromMenu( object sender, EventArgsClickableMenuClosed e ) {
            GraphicsEvents.OnPostRenderEvent -= drawButton;
            optionPageButton = null;
        }

        private void addModOptionButtonToMenu( object sender, EventArgsClickableMenuChanged e ) {
            if ( !( Game1.activeClickableMenu is GameMenu ) ) {
                return;
            }

            // Remove before adding just for good measure
            GraphicsEvents.OnPostRenderEvent -= drawButton;
            GraphicsEvents.OnPostRenderEvent += drawButton;

            optionPageButton = new OptionsPageButton( this );

            var optionMenu = new OptionsPage( options );
            List<IClickableMenu> pages =  ModEntry.helper.Reflection.GetPrivateField<List<IClickableMenu>>( Game1.activeClickableMenu, "pages" ).GetValue();
            pages.Add( optionMenu );

        }

        private void drawButton( object sender, EventArgs e ) {
            if( !( Game1.activeClickableMenu is GameMenu ) || optionPageButton == null ) {
                return;
            }

            // Don't draw when map is displayed
            var gameMenu = ( GameMenu ) Game1.activeClickableMenu;
            if( gameMenu.currentTab == GameMenu.mapTab ) {
                return;
            }

            optionPageButton.draw( Game1.spriteBatch );

            string hoverText = ModEntry.helper.Reflection.GetPrivateField<string>( Game1.activeClickableMenu, "hoverText" ).GetValue();

            // Redraw hover text so that it overlaps icon
            if( hoverText == "Exit Game" ) {
                IClickableMenu.drawHoverText( Game1.spriteBatch, "Exit Game", Game1.smallFont );
            }

        }

        private void saveModData( object sender, EventArgsIntChanged e ) {
            ModEntry.updateModData();
        }

        private void onKeyPress( object sender, EventArgsKeyPressed e ) {

            if( e.KeyPressed == Keys.Escape && Game1.activeClickableMenu is OptionsPage ) {
                Game1.activeClickableMenu = null;
            }

        }

        public void setActiveClickableMenuToOptionsPage() {
            var gameMenu = ( GameMenu ) Game1.activeClickableMenu;
            gameMenu.currentTab = 8;
        }

    }
}
