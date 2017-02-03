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
        private List<ModOptionsElement> options = new List<ModOptionsElement>();
        private OptionsPageButton optionPageButton;

        public OptionsPageHandler( ) {
            //ControlEvents.KeyPressed += onKeyPress;
            TimeEvents.DayOfMonthChanged += saveModData;
            MenuEvents.MenuChanged += addModOptionButtonToMenu;
            MenuEvents.MenuClosed += removeModOptionButtonFromMenu;

            var uiModAccurateHearts = new UiModAccurateHearts();
            var uiModLocationOfTownsfolk = new UiModLocationOfTownsfolk();
            var uiModShowTravelingMerchant = new UiModShowTravelingMerchant();
            var uiModItemrolloverInformation = new UiModItemRolloverInformation();
            var uiModExperience = new UiModExperience();
            var uiModluckOfDay = new UiModLuckOfDay();
            var uiModDisplayCropAndBarrelTime = new UiModDisplayCropAndBarrelTime();
            var uiModDisplayBirthdayIcon = new UiModDisplayBirthdayIcon();
            var uiModDisplayCalendarAndBillboardOnGameMenuButton = new UiModDisplayCalendarAndBillboardOnGameMenuButton();
            var uiModDisplayAnimalNeedsPet = new UiModDisplayAnimalNeedsPet();

            options.Add( new ModOptionsElement( "UiModeSuite v0.1: Demiacle" ) );
            options.Add( new ModOptionsCheckbox( "Show experience bar", ( int ) OptionsPage.Setting.SHOW_EXPERIENCE_BAR, () => { } ) );
            options.Add( new ModOptionsCheckbox( "Allow experience bar to fade out", ( int ) OptionsPage.Setting.ALLOW_EXPERIENCE_BAR_TO_FADE_OUT, () => { } ) );
            options.Add( new ModOptionsCheckbox( "Show experience gain", ( int ) OptionsPage.Setting.SHOW_EXP_GAIN , () => { } ) );
            options.Add( new ModOptionsCheckbox( "Show level up animation", ( int ) OptionsPage.Setting.SHOW_LEVEL_UP_ANIMATION, uiModExperience.togglLevelUpAnimation ) );
            options.Add( new ModOptionsCheckbox( "Show heart fills", ( int ) OptionsPage.Setting.SHOW_HEART_FILLS, uiModAccurateHearts.toggleVisibleHearts ) );
            options.Add( new ModOptionsCheckbox( "Show extra item information", ( int ) OptionsPage.Setting.SHOW_EXTRA_ITEM_INFORMATION, uiModItemrolloverInformation.toggleOption ) );
            options.Add( new ModOptionsCheckbox( "Show townspeople on map", ( int ) OptionsPage.Setting.SHOW_LOCATION_Of_TOWNSPEOPLE, uiModLocationOfTownsfolk.toggleShowNPCLocationOnMap ) );
            options.Add( new ModOptionsCheckbox( "Show traveling merchant icon", ( int ) OptionsPage.Setting.SHOW_TRAVELING_MERCHANT, uiModShowTravelingMerchant.toggleShowTravelingMerchant ) );
            options.Add( new ModOptionsCheckbox( "Show luck icon", ( int ) OptionsPage.Setting.SHOW_LUCK_ICON, uiModluckOfDay.toggleOption ) );
            options.Add( new ModOptionsCheckbox( "Show hover info on crops and barrels", ( int ) OptionsPage.Setting.SHOW_CROP_AND_BARREL_TOOLTIP_ON_HOVER, uiModDisplayCropAndBarrelTime.toggleOption ) );
            options.Add( new ModOptionsCheckbox( "Show birthday icon reminder", ( int ) OptionsPage.Setting.SHOW_BIRTHDAY_ICON, uiModDisplayBirthdayIcon.toggleOption ) );

            //OptionsPage.syncSettingsToLoadedData( options );
        }

        private void removeModOptionButtonFromMenu( object sender, EventArgsClickableMenuClosed e ) {
            if( optionPageButton == null ) {
                return;
            }

            GraphicsEvents.OnPostRenderEvent -= drawButton;
            ControlEvents.MouseChanged -= optionPageButton.onLeftClick;
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

        public void setActiveClickableMenuToOptionsPage() {
            var gameMenu = ( GameMenu ) Game1.activeClickableMenu;
            gameMenu.currentTab = 8;
        }

    }
}
