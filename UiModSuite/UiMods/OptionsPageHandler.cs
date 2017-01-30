using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace DemiacleSvm.UiMods {
    class OptionsPageHandler {

        public const int WIDTH = 800;
        public const int HEIGHT = 600;

        private OptionsPage optionMenu;
        private readonly List<UiModWithOptions> optionMods = new List<UiModWithOptions>();

        public OptionsPageHandler( List<UiModWithOptions> optionMods ) {
            this.optionMods = optionMods;
            ControlEvents.KeyPressed += onKeyPress;
            TimeEvents.OnNewDay += saveModData;
        }

        private void saveModData( object sender, EventArgsNewDay e ) {
            ModEntry.updateModData();
        }

        private void onKeyPress( object sender, EventArgsKeyPressed e ) {
            if( !Game1.player.canMove ) {
                return;
            }

            if( $"{e.KeyPressed}" == "N" ) {
                if( Game1.activeClickableMenu != null && Game1.activeClickableMenu == optionMenu ) {
                    Game1.activeClickableMenu = null;
                } else {
                    optionMenu = new OptionsPage( Game1.viewport.Width / 2 - WIDTH / 2, Game1.viewport.Height / 2 - HEIGHT / 2, WIDTH, HEIGHT, optionMods );
                    Game1.activeClickableMenu = optionMenu;
                }
            }

        }

    }
}
