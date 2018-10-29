using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine.EventSystems;

namespace BonVoyage
{
    /// <summary>
    /// Control window - model part
    /// </summary>
    public class ControlWindowModel
    {
        private double latitude = 0f;
        public string Latitude
        {
            get { return latitude.ToString(); }
            set
            {
                if ((value.Length == 0) || (value == "."))
                    latitude = 0f;
                else
                    latitude = Convert.ToDouble(value);
            }
        }

        private double longitude = 0f;
        public string Longitude
        {
            get { return longitude.ToString(); }
            set
            {
                if ((value.Length == 0) || (value == "."))
                    longitude = 0f;
                else
                    longitude = Convert.ToDouble(value);
            }
        }

        private bool controllerActive; // Is controller active and doing it's behind the scenes magic?


        /// <summary>
        /// Constructor
        /// </summary>
        public ControlWindowModel()
        {
            // Load from configuration
            CommonWindowProperties.ControlWindowPosition = Configuration.ControlWindowPosition;

            controllerActive = false;
        }


        /// <summary>
        /// If controller is active, some buttons will be disabled
        /// </summary>
        /// <returns></returns>
        public bool EnableButtons()
        {
            return !controllerActive;
        }


        /// <summary>
        /// Add control lock/unlock listeners to a text field
        /// </summary>
        /// <param name="text"></param>
        private void TMPFieldOnSelect(string text)
        {
            InputLockManager.SetControlLock(ControlTypes.KEYBOARDINPUT | ControlTypes.UI, "BonVoyageInputFieldLock");
        }
        private void TMPFieldOnDeselect(string text)
        {
            InputLockManager.RemoveControlLock("BonVoyageInputFieldLock");
        }
        public void AddLockControlToTextField(DialogGUITextInput field)
        {
            field.OnUpdate = () => {
                if (field.uiItem != null)
                {
                    field.OnUpdate = () => { };
                    TMP_InputField TMPField = field.uiItem.GetComponent<TMP_InputField>();
                    TMPField.onSelect.AddListener(TMPFieldOnSelect);
                    TMPField.onDeselect.AddListener(TMPFieldOnDeselect);
                }
            };
        }


        /// <summary>
        /// Return text of the control button
        /// </summary>
        /// <returns></returns>
        public string GetGoButtonText()
        {
            if (!controllerActive)
                return Localizer.Format("#LOC_BV_Control_Go");
            else
                return Localizer.Format("#LOC_BV_Control_Deactivate");
        }


        /// <summary>
        /// Go button was clicked
        /// </summary>
        public void GoButtonClicked()
        {
            controllerActive = !controllerActive;
            if (controllerActive)
                ScreenMessages.PostScreenMessage("GO!");
            else
                ScreenMessages.PostScreenMessage("Disable");
        }


        /// <summary>
        /// System check button was clicked
        /// </summary>
        public void SystemCheckButtonClicked()
        {
            ScreenMessages.PostScreenMessage("System check");
        }


        /// <summary>
        ///  Return saved latitude
        /// </summary>
        /// <returns></returns>
        public string GetLatitude()
        {
            return Latitude;
        }


        /// <summary>
        /// Return saved longitude
        /// </summary>
        /// <returns></returns>
        public string GetLongitude()
        {
            return Longitude;
        }


        /// <summary>
        /// Set button was clicked
        /// </summary>
        public void SetButtonClicked()
        {
            ScreenMessages.PostScreenMessage("Latitude = " + latitude.ToString() + " ; Longitude = " + longitude.ToString());
        }

    }

}
