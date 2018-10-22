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

            CommonWindowProperties.ActiveSkin = CommonWindowProperties.UnitySkin;
            CommonWindowProperties.RefreshStyles();
        }


        /// <summary>
        /// Active vessels checkbox
        /// </summary>
        /// <param name="value"></param>
        public void ActiveVesselsChecked(bool value)
        {
            activeVesselsChecked = value;
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
