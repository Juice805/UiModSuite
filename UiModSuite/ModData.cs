using System;
using System.Xml.Serialization;

namespace DemiacleSvm {
    public class ModData {

        // Uses the directory path as its namespace for loading and whatnot
        string nameSpace = ModEntry.helper.DirectoryPath;

        // Dictionary of int => bool is used because OptionsCheckbox uses int as a stored value.
        // The int used here is actually a hash value of the npc name string
        public SerializableDictionary<int, bool> locationOfTownsfolkOptions = new SerializableDictionary<int, bool>();

        // Dictionary of string => bool is used for easy creating and loading of checkboxes programatically
        // Look at UiModWithOptions for details
        //public SerializableDictionary<string, bool> uiOptions = new SerializableDictionary<string, bool>();

        public SerializableDictionary<string, bool> checkboxOptions = new SerializableDictionary<string, bool>();
        public SerializableDictionary<string, int> sliderOptions = new SerializableDictionary<string, int>();
        public SerializableDictionary<string, int> dropDownOptions = new SerializableDictionary<string, int>();

        // A list of strings that contain the action to do when an option button is pressed
        [XmlIgnore]
        public SerializableDictionary<string, Action> actionList = new SerializableDictionary<string, Action>();

        //public SerializableDictionary<string, > checkboxOptions = new SerializableDictionary<string, bool>();

    }
}
