using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiModSuite.UiMods {
    class ModOptionsSlider : OptionsSlider {
        public ModOptionsSlider( string label, int whichOption, int defaultSetting = 0 ) : base( label, whichOption, -1, -1 ) {
            value = defaultSetting;
        }
    }
}
