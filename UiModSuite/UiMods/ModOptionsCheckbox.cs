using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace UiModSuite.UiMods {
    class ModOptionsCheckbox : OptionsCheckbox {

        public ModOptionsCheckbox( string label, int whichOption, bool defaultValue = true ) : base( label, whichOption, -1, -1 ) {
            isChecked = defaultValue;
        }

        public override void receiveLeftClick( int x, int y ) {
            base.receiveLeftClick( x, y );
            ModEntry.modData.boolSettings[ whichOption ] = isChecked;
        }
    }
}
