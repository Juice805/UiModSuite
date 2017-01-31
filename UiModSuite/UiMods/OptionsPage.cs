using UiModSuite;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;

namespace UiModSuite.UiMods {
    class OptionsPage : OptionsWindow {

        public enum Setting : int {
            ALLOW_EXPERIENCE_BAR_TO_FADE_OUT = 1,
            SHOW_EXPERIENCE_BAR = 2,
            SHOW_EXP_GAIN = 3,
            SHOW_LEVEL_UP_ANIMATION = 4,

            SHOW_HEART_FILLS = 5,

            SHOW_EXTRA_ITEM_INFORMATION = 6,

            SHOW_LOCATION_Of_TOWNSPEOPLE = 7,

            SHOW_LUCK_ICON = 8,
        }

        internal OptionsPage( List<OptionsElement> options ) {
            this.options = options;
        }

        public static void syncSettingsToLoadedData( List<OptionsElement> listOfOptions ) {
            foreach( var option in listOfOptions ) {

                if( option is ModOptionsCheckbox ) {
                    var checkbox = ( ModOptionsCheckbox ) option;

                    if( ModEntry.modData.boolSettings.ContainsKey( option.whichOption ) == false ) {
                        ModEntry.modData.boolSettings.Add( option.whichOption, checkbox.isChecked );
                    } else {
                        checkbox.isChecked = ModEntry.modData.boolSettings[ option.whichOption ];
                    }
                }

                if( option is ModOptionsSlider ) {
                    var slider = ( ModOptionsSlider ) option;

                    if( ModEntry.modData.intSettings.ContainsKey( option.whichOption ) == false ) {
                        ModEntry.modData.intSettings.Add( option.whichOption, slider.value );
                    } else {
                        slider.value = ModEntry.modData.intSettings[ option.whichOption ];
                    }
                }

                if( option is ModOptionsDropDown ) {
                    var dropDown = ( ModOptionsDropDown ) option;

                    if( ModEntry.modData.intSettings.ContainsKey( option.whichOption ) == false ) {
                        ModEntry.modData.intSettings.Add( option.whichOption, dropDown.selectedOption );
                    } else {
                        dropDown.selectedOption = ModEntry.modData.intSettings[ option.whichOption ];
                    }
                }

                if( option is ModOptionsPlusMinus ) {
                    var plusMinus = ( ModOptionsPlusMinus ) option;

                    if( ModEntry.modData.intSettings.ContainsKey( option.whichOption ) == false ) {
                        ModEntry.modData.intSettings.Add( option.whichOption, plusMinus.selected );
                    } else {
                        plusMinus.selected = ModEntry.modData.intSettings[ option.whichOption ];
                    }
                }
            }
        }

        internal static int getSliderValue( Setting setting ) {
            return ModEntry.modData.intSettings[ (int) setting ];
        }

        internal static bool getCheckboxValue( Setting setting ) {
            return ModEntry.modData.boolSettings[ (int) setting ];
        }

        internal static string getSelectValue( Setting setting ) {
            return ModEntry.modData.stringSettings[ (int) setting ];
        }

    }
}
