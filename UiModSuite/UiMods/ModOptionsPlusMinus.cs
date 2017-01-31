using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiModSuite.UiMods {
    class ModOptionsPlusMinus : OptionsPlusMinus {
        public ModOptionsPlusMinus( string label, int whichOption, List<string> options ) : base( label, whichOption, options, -1, -1 ) {
            
        }
    }
}
