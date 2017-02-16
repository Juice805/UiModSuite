﻿using UiModSuite;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using UiModSuite.UiMods;

namespace UiModSuite.Options {
    class ModOptionsPage : OptionsWindow {

        public enum Setting : int {
            ALLOW_EXPERIENCE_BAR_TO_FADE_OUT = 1,
            SHOW_EXPERIENCE_BAR = 2,
            SHOW_EXP_GAIN = 3,
            SHOW_LEVEL_UP_ANIMATION = 4,

            SHOW_HEART_FILLS = 5,

            SHOW_EXTRA_ITEM_INFORMATION = 6,

            SHOW_LOCATION_Of_TOWNSPEOPLE = 7,

            SHOW_LUCK_ICON = 8,
            SHOW_TRAVELING_MERCHANT = 9,
            SHOW_LOCATION_OF_TOWNSPEOPLE_SHOW_QUEST_ICON = 10,
            SHOW_CROP_AND_BARREL_TOOLTIP_ON_HOVER = 11,
            SHOW_BIRTHDAY_ICON = 12,
            SHOW_ANIMALS_NEED_PETS = 13,
            SHOW_SPRINKLER_SCARECROW_RANGE = 14,
        }

        internal ModOptionsPage( List<ModOptionsElement> options ) {
            this.options = options;
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
