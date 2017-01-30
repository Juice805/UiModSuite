using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemiacleSvm.UiMods {
    public class OptionData {

        public OptionsElement optionsElement;
        public Action actionOnSettingChange;
        public Func<bool> evaluationDelegate;

        public OptionData(OptionsElement optionsElement, Action actionOnSettingChange ) {
            this.optionsElement = optionsElement;
            this.actionOnSettingChange = actionOnSettingChange;
        }

        public OptionData( OptionsElement optionsElement ) {
            this.optionsElement = optionsElement;
        }

        // currently only has 1 use... questionable
        internal static OptionData createTitle( string title ) {
            return new OptionData( new OptionsElement( title ) );
        }
        
        public void addDelegateCheckIfEnabled( Func<bool> evaluationDelegate ) {
            this.evaluationDelegate = evaluationDelegate;
        }

        public bool evaluateEnabled() {
            return evaluationDelegate.Invoke();
        }

    }
}
