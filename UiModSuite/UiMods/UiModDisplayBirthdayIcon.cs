﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using UiModSuite.Options;


namespace UiModSuite.UiMods {
    internal class UiModDisplayBirthdayIcon {

        internal void toggleOption() {
            GraphicsEvents.OnPreRenderHudEvent -= drawBirthdayIcon;

            if( ModOptionsPage.getCheckboxValue( ModOptionsPage.Setting.SHOW_BIRTHDAY_ICON) ) {
                GraphicsEvents.OnPreRenderHudEvent += drawBirthdayIcon;
            }

        }

        private void drawBirthdayIcon( object sender, EventArgs e ) {

            if( Game1.eventUp ) {
                return;
            }

            // Draw birthday icon
            foreach( GameLocation location in Game1.locations ) {
                foreach( NPC npc in location.characters ) {
                    if( npc.isBirthday( Game1.currentSeason, Game1.dayOfMonth ) ) {
                        // draw headshot of npc whos birthday it is
                        Rectangle rect = UiModLocationOfTownsfolk.getHeadShot( npc );

                        int iconPositionX = IconHandler.getIconXPosition();
                        int iconPositionY = 256;

                        float scale = 2.9f;

                        Game1.spriteBatch.Draw( Game1.mouseCursors, new Vector2( iconPositionX, iconPositionY ), new Rectangle( 913 / 4, 1638 / 4, 16, 16 ), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1 );
                        var npcMugShot = new ClickableTextureComponent( npc.name, new Rectangle( iconPositionX - 7, iconPositionY - 2, (int) ( 16 * scale ), (int) ( 16 * scale ) ), null, npc.name, npc.sprite.Texture, rect, 2f, false );
                        npcMugShot.draw( Game1.spriteBatch );

                        if( npcMugShot.containsPoint( Game1.getMouseX(), Game1.getMouseY() ) ) {
                            string tooltip = $"{npc.name}'s Birthday";
                            IClickableMenu.drawHoverText( Game1.spriteBatch, tooltip, Game1.dialogueFont );
                        }
                    }
                }
            }

        }

    }
}