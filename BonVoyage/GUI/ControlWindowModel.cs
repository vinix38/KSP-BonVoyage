using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BonVoyage
{
    /// <summary>
    /// Control window - model part
    /// </summary>
    public class ControlWindowModel
    {
        // Displayed stats list
        private DialogGUIVerticalLayout statsListLayout = null;


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
        private BVController currentController;


        /// <summary>
        /// Constructor
        /// </summary>
        public ControlWindowModel()
        {
            // Load from configuration
            CommonWindowProperties.ControlWindowPosition = Configuration.ControlWindowPosition;

            controllerActive = false;
            currentController = null;
        }


        /// <summary>
        /// Set current controller
        /// </summary>
        /// <param name="c"></param>
        public void SetController(BVController controller)
        {
            currentController = controller;
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

            if (currentController != null)
            {
                RefreshStatsListLayout();
            }
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


        /// <summary>
        /// Pick on map button was clicked
        /// </summary>
        public void PickOnMapkButtonClicked()
        {
            ScreenMessages.PostScreenMessage("Pick on map");
        }


        /// <summary>
        /// Current target button was clicked
        /// </summary>
        public void CurrentTargetButtonClicked()
        {
            ScreenMessages.PostScreenMessage("Current target");
        }


        /// <summary>
        /// Current waypoint button was clicked
        /// </summary>
        public void CurrentWaypointButtonClicked()
        {
            ScreenMessages.PostScreenMessage("Current waypoint");
        }


        /// <summary>
        /// Create table row for displayed result
        /// </summary>
        /// <param name="result"></param>
        /// <returns>DialogGUIHorizontalLayout row</returns>
        private DialogGUIHorizontalLayout CreateListLayoutRow(DisplayedSystemCheckResult result)
        {
            DialogGUIHorizontalLayout row = null;

            if (result.Tooltip.Length > 0)
            {
                row = new DialogGUIHorizontalLayout(
                    new DialogGUILabel(result.Label + ":", 100f),
                    new DialogGUILabel(result.Text),
                    new DialogGUISpace(1f),
                    // Add a button with transparent background and label style just to display a tooltip when hovering over it
                    // Transparent sprite is needed to hide button borders
                    TooltipExtension.DeferTooltip(new DialogGUIButton(CommonWindowProperties.transparent, "(?)", () => { }, 17f, 18f, false) { tooltipText = result.Tooltip, guiStyle = CommonWindowProperties.Style_Button_Label })
                );
            }
            else
            {
                row = new DialogGUIHorizontalLayout(
                    new DialogGUILabel(result.Label + ":", 100f),
                    new DialogGUILabel(result.Text)
                );
            }

            return row;
        }


        /// <summary>
        /// Get layout of the list of stats
        /// </summary>
        /// <returns></returns>
        public DialogGUIVerticalLayout GetStatsListLayout()
        {
            if (currentController != null)
            {
                List<DisplayedSystemCheckResult> resultsList = currentController.GetDisplayedSystemCheckResults();

                DialogGUIBase[] list = new DialogGUIBase[1 + resultsList.Count];
                int index = 0;
                for (int i = 0; i < resultsList.Count; i++)
                {
                    list[index] = CreateListLayoutRow(resultsList[i]);
                    index++;
                }
                list[index] = new DialogGUISpace(3f);
                statsListLayout = new DialogGUIVerticalLayout(list);
            }
            else
            {
                statsListLayout = new DialogGUIVerticalLayout(new DialogGUISpace(3f));
            }
            return statsListLayout;
        }


        /// <summary>
        /// Clear layout of the list of stats
        /// </summary>
        public void ClearStatsListLayout()
        {
            statsListLayout = null;
        }


        /// <summary>
        /// Refresh list of stats without
        /// </summary>
        public void RefreshStatsListLayout()
        {
            Stack<Transform> stack = new Stack<Transform>();  // some data on hierarchy of GUI components
            stack.Push(statsListLayout.uiItem.gameObject.transform); // need the reference point of the parent GUI component for position and size

            List<DialogGUIBase> rows = statsListLayout.children;

            // Clear list
            while (rows.Count > 0)
            {
                DialogGUIBase child = rows.ElementAt(0); // Get child
                rows.RemoveAt(0); // Drop row
                child.uiItem.gameObject.DestroyGameObjectImmediate(); // Free memory up
            }

            // Add rows
            if (currentController != null)
            {
                List<DisplayedSystemCheckResult> resultsList = currentController.GetDisplayedSystemCheckResults();

                for (int i = 0; i < resultsList.Count; i++)
                {
                    rows.Add(CreateListLayoutRow(resultsList[i]));
                    rows.Last().Create(ref stack, CommonWindowProperties.ActiveSkin); // required to force the GUI creatio﻿n
                }
            }
            rows.Add(new DialogGUISpace(3f));
            rows.Last().Create(ref stack, CommonWindowProperties.ActiveSkin); // required to force the GUI creatio﻿n
        }

    }

}
