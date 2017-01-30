using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace DemiacleSvm.UiMods {
    abstract class UiModWithOptions {
        public List<OptionData> options = new List<OptionData>();

        /// <summary>
        /// Adds a checkbox option to be saved and loaded upon character load
        /// </summary>
        /// <param name="label">The string to display for the option</param>
        /// <param name="defaultSettingValue">The default value of the option</param>
        /// <param name="actionOnSettingChange">A generic action that will be called every time the option changes</param>
        public OptionData addCheckboxOption( string label, bool defaultSettingValue, Action actionOnSettingChange = null ) {

            var checkBox = new OptionsCheckbox( label, label.GetHashCode() );
            var optionData = new OptionData( checkBox, actionOnSettingChange );

            checkBox.isChecked = loadOptionData<bool>( label, defaultSettingValue, ModEntry.modData.checkboxOptions );

            if( actionOnSettingChange != null ) {
                actionOnSettingChange.Invoke();
                ModEntry.modData.actionList.Add( label, actionOnSettingChange );
            }

            options.Add( optionData );

            return optionData;
        }

        /// <summary>
        /// Adds a slider option to be saved and loaded upon character load
        /// </summary>
        /// <param name="label">The string to display for the option</param>
        /// <param name="defaultSettingValue">The default value of the option</param>
        /// <param name="actionOnSettingChange">A generic action that will be called every time the option changes</param>
        public OptionData addSliderOption( string label, int defaultSettingValue, Action actionOnSettingChange = null ) {

            var sliderOption = new OptionsSlider( label, label.GetHashCode() );
            var optionData = new OptionData( sliderOption, actionOnSettingChange );

            sliderOption.value = loadOptionData<int>( label, defaultSettingValue, ModEntry.modData.sliderOptions );

            if( actionOnSettingChange != null ) {
                actionOnSettingChange.Invoke();
                ModEntry.modData.actionList.Add( label, actionOnSettingChange );
            }

            options.Add( optionData );

            return optionData;
        }

        /// <summary>
        /// Adds an drop list option to be saved an dloaded upon character load
        /// </summary>
        /// <param name="label">The string to display for the option</param>
        /// <param name="options">The default value is the first option chosen</param>
        /// <param name="actionOnSettingChange">A generic action that will be called every time the option changes</param>
        public OptionData addDropDownOption( string label, string[] listOfOptions, Action actionOnSettingChange = null ) {

            var optionDropDown = new OptionsDropDown( label, label.GetHashCode() );
            var optionData = new OptionData( optionDropDown, actionOnSettingChange );

            optionDropDown.selectedOption = loadOptionData<int>( label, listOfOptions[0].GetHashCode(), ModEntry.modData.dropDownOptions );

            if( actionOnSettingChange != null ) {
                actionOnSettingChange.Invoke();
                ModEntry.modData.actionList.Add( label, actionOnSettingChange );
            }

            options.Add( optionData );

            return optionData;
        }

        // TODO add Checkboxes to group so only one option is selected out of group

        // TODO add checkboxes that go greyed out for condition

        /// <summary>
        /// Returns the loaded option data or the default value 
        /// </summary>
        /// <typeparam name="T">The type of data to load and return</typeparam>
        /// <param name="key">The value used to find the data</param>
        /// <param name="defaultDataValue">The value returned if the data does not load</param>
        /// <param name="saveDataLocation">The dictionary to load from</param>
        /// <returns></returns>
        private T loadOptionData<T>( string key, T defaultDataValue, SerializableDictionary<string,T> saveDataLocation ) {

            // Load value if saved data exists
            if( saveDataLocation.ContainsKey( key ) ) {
                return saveDataLocation[ key ];

            // Create option and return default value if it doesn't exist
            } else {
                saveDataLocation.Add( key, defaultDataValue );
                return defaultDataValue;
            }
        }

        public void addTitle( string label ) {
            var optionData = new OptionData( new OptionsElement( label ) );
            options.Add( optionData );
        }

        //optionsinputlistener

    }
}
