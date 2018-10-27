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
        private bool dewarpChecked = false;
        private bool kspSkin = true;
        private bool kspToolbarChecked = true;
        private bool toolbarContinuedChecked = false;


        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsWindowModel()
        {
            // Load from configuration
            if (Configuration.Skin == 1)
                kspSkin = false;
            else
                kspSkin = true;
            dewarpChecked = Configuration.AutomaticDewarp;
            kspToolbarChecked = Configuration.KSPToolbar;
            toolbarContinuedChecked = Configuration.ToolbarContinued;
        }


        /// <summary>
        /// Automatic dewarp checkbox
        /// </summary>
        /// <param name="value"></param>
        public void DewarpChecked(bool value)
        {
            dewarpChecked = value;
            Configuration.AutomaticDewarp = value;
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
            {
                CommonWindowProperties.ActiveSkin = UISkinManager.defaultSkin;
                Configuration.Skin = 0;
            }
            else
            {
                CommonWindowProperties.ActiveSkin = CommonWindowProperties.UnitySkin;
                Configuration.Skin = 1;
            }
            CommonWindowProperties.RefreshStyles();
            BonVoyage.Instance.ResetWindows();
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
            kspToolbarChecked = value;
            Configuration.KSPToolbar = value;

            if (kspToolbarChecked)
                BonVoyage.Instance.AddLauncher();
            else
                BonVoyage.Instance.RemoveAppLauncherButton();
        }


        /// <summary>
        /// Toolbar Continued checkbox
        /// </summary>
        /// <param name="value"></param>
        public void TCChecked(bool value)
        {
            toolbarContinuedChecked = value;
            Configuration.ToolbarContinued = value;

            if (toolbarContinuedChecked)
                BonVoyage.Instance.AddLauncher();
            else
                BonVoyage.Instance.RemoveToolbarContinuedButton();
        }


        /// <summary>
        /// Get the state of KSP toolbar toggle
        /// </summary>
        /// <returns></returns>
        public bool GetKSPToolbarToggleState()
        {
            return kspToolbarChecked;
        }


        /// <summary>
        /// Get the state of Toolbar Continued toggle
        /// </summary>
        /// <returns></returns>
        public bool GetTCToggleState()
        {
            return toolbarContinuedChecked;
        }

    }

}
