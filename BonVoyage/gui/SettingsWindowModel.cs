using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    /// <summary>
    /// Settings window - model part
    /// </summary>
    public class SettingsWindowModel
    {
        private BonVoyage module;
        private bool dewarpChecked = false;
        private bool kspSkin = true;
        private bool kspToolbar = true;
        private bool toolbarContinued = false;


        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsWindowModel(BonVoyage m)
        {
            module = m;
        }


        /// <summary>
        /// Automatic dewarp checkbox
        /// </summary>
        /// <param name="value"></param>
        public void DewarpChecked(bool value)
        {
            dewarpChecked = value;
        }


        /// <summary>
        /// Get the state of Autmatic dewarp toggle
        /// </summary>
        /// <returns></returns>
        public bool GetDewarpToggleState()
        {
            return dewarpChecked;
        }


        /// <summary>
        /// Change active skin
        /// </summary>
        private void ChangeSkin()
        {
            if (kspSkin)
                CommonWindowProperties.ActiveSkin = UISkinManager.defaultSkin;
            else
                CommonWindowProperties.ActiveSkin = CommonWindowProperties.UnitySkin;
            CommonWindowProperties.RefreshStyles();
            module.ResetWindows();
        }


        /// <summary>
        /// KSP skin checkbox
        /// </summary>
        /// <param name="value"></param>
        public void KSPSkinChecked(bool value)
        {
            kspSkin = value;
            ChangeSkin();
        }


        /// <summary>
        /// Unity skin checkbox
        /// </summary>
        /// <param name="value"></param>
        public void UnitySkinChecked(bool value)
        {
            kspSkin = !value;
            ChangeSkin();
        }


        /// <summary>
        /// Get the state of KSP skin toggle
        /// </summary>
        /// <returns></returns>
        public bool GetKSPSkinToggleState()
        {
            return kspSkin;
        }


        /// <summary>
        /// Get the state of Unity skin toggle
        /// </summary>
        /// <returns></returns>
        public bool GetUnitySkinToggleState()
        {
            return !kspSkin;
        }


        /// <summary>
        /// KSP toolbar checkbox
        /// </summary>
        /// <param name="value"></param>
        public void KSPToolbarChecked(bool value)
        {
            kspToolbar = value;
        }


        /// <summary>
        /// Toolbar Continued checkbox
        /// </summary>
        /// <param name="value"></param>
        public void TCChecked(bool value)
        {
            toolbarContinued = value;
        }


        /// <summary>
        /// Get the state of KSP toolbar toggle
        /// </summary>
        /// <returns></returns>
        public bool GetKSPToolbarToggleState()
        {
            return kspToolbar;
        }


        /// <summary>
        /// Get the state of Toolbar Continued toggle
        /// </summary>
        /// <returns></returns>
        public bool GetTCToggleState()
        {
            return toolbarContinued;
        }

    }

}
