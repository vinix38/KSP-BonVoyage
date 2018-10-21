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

        #region Window position
        public static Vector2 MainWindowPosition = new Vector2(0.5f, 0.7f);
        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowModel(BonVoyage m)
        {
            module = m;

            //CommonWindowProperties.ActiveSkin = CommonWindowProperties.UnitySkin;
            //CommonWindowProperties.RefreshStyles();
        }


        /// <summary>
        /// Active vessels checkbox
        /// </summary>
        /// <param name="value"></param>
        public void ActiveVesselsChecked(bool value)
        {
            ScreenMessages.PostScreenMessage("ActiveVesselsChecked = " + value.ToString());
        }


        /// <summary>
        /// Disabled vessels checkbox
        /// </summary>
        /// <param name="value"></param>
        public void DisabledVesselsChecked(bool value)
        {
            ScreenMessages.PostScreenMessage("DisabledVesselsChecked = " + value.ToString());
        }


        public void GotoVessel()
        {
            ScreenMessages.PostScreenMessage("Go to vessel");
        }

    }

}
