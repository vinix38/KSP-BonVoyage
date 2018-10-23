using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BonVoyage
{
    /// <summary>
    /// Main mod's window - model part
    /// </summary>
    public class MainWindowModel
    {
        private BonVoyage module;
        private bool activeVesselsChecked = true;
        private bool disabledVesselsChecked = false;


        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowModel(BonVoyage m)
        {
            module = m;

            // Load from configuration
            CommonWindowProperties.MainWindowPosition = Configuration.MainWindowPosition;
            activeVesselsChecked = Configuration.ActiveVessels;
            disabledVesselsChecked = Configuration.DisabledVessels;
        }


        /// <summary>
        /// Active vessels checkbox
        /// </summary>
        /// <param name="value"></param>
        public void ActiveVesselsChecked(bool value)
        {
            activeVesselsChecked = value;
            Configuration.ActiveVessels = value;
        }


        /// <summary>
        /// Get the state of Active vessels toggle
        /// </summary>
        /// <returns></returns>
        public bool GetActiveVesselsToggleState()
        {
            return activeVesselsChecked;
        }


        /// <summary>
        /// Disabled vessels checkbox
        /// </summary>
        /// <param name="value"></param>
        public void DisabledVesselsChecked(bool value)
        {
            disabledVesselsChecked = value;
            Configuration.DisabledVessels = value;
        }


        /// <summary>
        /// Get the state of Disabled vessels toggle
        /// </summary>
        /// <returns></returns>
        public bool GetDisabledVesselsToggleState()
        {
            return disabledVesselsChecked;
        }


        public void GotoVessel()
        {
            ScreenMessages.PostScreenMessage("Go to vessel");
        }

    }

}
