using DemiacleSvm.UiMods;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;

namespace DemiacleSvm {
    public class ModEntry : Mod {

        public static ModData modData;
        public static ModEntry modEntry;
        public static IModHelper helper;
        // TODO Replace with helper method mod already has a saver and loader so change to use that
        public static readonly string modDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\Mods\\Demiacle_UiMod\\";
        public const string saveFilePostfix = "_modData.xml";
        public static Boolean isTesting = false;
            
        public override void Entry(IModHelper helper) {
            ModEntry.helper = helper;
            ModEntry.modEntry = this;
            modData = new ModData();

            // Loads the correct settings on character load
            PlayerEvents.LoadedGame += loadModData;
           
            // Skip Intro
            MenuEvents.MenuChanged += SkipIntro.onMenuChange;

        }

        internal static void Log( string log ) {
            if( isTesting ) {
                System.Console.WriteLine( log );
                return;
            }

            modEntry.Monitor.Log( log );
        }

        /// <summary>
        /// Loads mod specific data
        /// </summary>
        internal void loadModData( object sender, EventArgsLoadedGameChanged e ) {

            string playerName = Game1.player.name;

            // File: \Mods\Demiacle_SVM\playerName_modData.xml
            // load file 
            if( File.Exists( modDirectory + playerName + saveFilePostfix ) ) {
                this.Monitor.Log( $"Mod data already exists for player {playerName}.... loading" );
                var loadedData = new ModData();
                Serializer.ReadFromXmlFile( out loadedData, playerName );

                // Only load options valid for this build
                foreach( var data in loadedData.checkboxOptions ) {
                    if( modData.checkboxOptions.ContainsKey( data.Key ) ) {
                        modData.checkboxOptions[ data.Key ] = loadedData.checkboxOptions[ data.Key ];
                    }
                }

                // Always load character location data
                // Beware this may need a check later
                modData.locationOfTownsfolkOptions = loadedData.locationOfTownsfolkOptions;
                    
                // If need to add more options create object here and merge with loaded data

            // create file and ModData
            } else {
                this.Monitor.Log( $"Mod data does not exist for player {playerName}... creating file" );
                updateModData();
            }

            initializeMods();

        }

        private void initializeMods() {

            var uiModAccurateHearts = new UiModAccurateHearts();
            var uiModLocationOfTownsfolk = new UiModLocationOfTownsfolk();
            var uiModItemrolloverInformation = new UiModItemRolloverInformation();
            var uiModExperience = new UiModExperience();
            var uiModluckOfDay = new UiModLuckOfDay();

            var uiMods = new List<UiModWithOptions>();
            uiMods.Add( uiModAccurateHearts );
            uiMods.Add( uiModLocationOfTownsfolk );
            uiMods.Add( uiModItemrolloverInformation );
            uiMods.Add( uiModExperience );
            uiMods.Add( uiModluckOfDay );

            var optionPageHandler = new OptionsPageHandler( uiMods );
        }

        /// <summary>
        /// Overwrites the current modData to file
        /// </summary>
        internal static void updateModData() {
            Serializer.WriteToXmlFile( modData, Game1.player.name );
        }

    }
}
