using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiModSuite.Options {
    class IconHandler {

        // Handles icon display x offset
        public static int amountOfVisibleIcons = 0;

        internal static int getIconXPosition() {
            int iconX = (int) DemiacleUtility.getWidthInPlayArea() - 134 - ( 46 * amountOfVisibleIcons );
            amountOfVisibleIcons++;
            return iconX;
        }

        internal static void reset( object sender, EventArgs e ) {
            amountOfVisibleIcons = 0;
        }
    }
}
