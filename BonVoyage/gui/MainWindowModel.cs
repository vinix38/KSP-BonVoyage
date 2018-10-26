using KSP.Localization;
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
        private bool activeControllersChecked = true;
        private bool disabledControllersChecked = false;

        // Displayed vessel list
        private DialogGUIVerticalLayout vesselListLayout = null;


        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowModel(BonVoyage m)
        {
            module = m;

            // Load from configuration
            CommonWindowProperties.MainWindowPosition = Configuration.MainWindowPosition;
            activeControllersChecked = Configuration.ActiveControllers;
            disabledControllersChecked = Configuration.DisabledControllers;
        }


        /// <summary>
        /// Active controllers checkbox
        /// </summary>
        /// <param name="value"></param>
        public void ActiveControllersChecked(bool value)
        {
            activeControllersChecked = value;
            Configuration.ActiveControllers = value;
        }


        /// <summary>
        /// Get the state of Active controllers toggle
        /// </summary>
        /// <returns></returns>
        public bool GetActiveControllersToggleState()
        {
            return activeControllersChecked;
        }


        /// <summary>
        /// Disabled controllers checkbox
        /// </summary>
        /// <param name="value"></param>
        public void DisabledControllersChecked(bool value)
        {
            disabledControllersChecked = value;
            Configuration.DisabledControllers = value;
        }


        /// <summary>
        /// Get the state of Disabled controllers toggle
        /// </summary>
        /// <returns></returns>
        public bool GetDisabledControllersToggleState()
        {
            return disabledControllersChecked;
        }


        /// <summary>
        /// Switch to vessel
        /// </summary>
        /// <param name="vesselId"></param>
        public void SwitchToVessel(Guid vesselId)
        {
            for (int i = 0; i < module.BVControllers.Count; i++)
            {
                if (module.BVControllers[i].vessel.id == vesselId)
                {
                    Vessel v = module.BVControllers[i].vessel;
                    if (v.loaded)
                        FlightGlobals.SetActiveVessel(v);
                    else
                    {
                        GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
                        FlightDriver.StartAndFocusVessel("persistent", FlightGlobals.Vessels.IndexOf(v));
                    }
                }
            }
        }


        /// <summary>
        /// Get layout of the list of vessels
        /// </summary>
        /// <returns></returns>
        public DialogGUIVerticalLayout GetVesselListLayout()
        {
            // Count disabled controllers
            int disabledControllersCount = 0;
            int controllersCount = module.BVControllers.Count;
            for (int i = 0; i < controllersCount; i++)
            {
                if (module.BVControllers[i].Shutdown)
                    disabledControllersCount++;
            }

            int listLength = 1;
            if (activeControllersChecked)
                listLength += controllersCount - disabledControllersCount;
            if (disabledControllersChecked)
                listLength += disabledControllersCount;

            DialogGUIBase[] list = new DialogGUIBase[listLength];
            list[0] = new DialogGUIContentSizer(UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained, UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize, true);

            if (listLength > 1) // anything is checked
            {
                int counter = 1;
                for (int i = 0; i < controllersCount; i++)
                {
                    DialogGUIHorizontalLayout row = null;
                    BVController controller = module.BVControllers[i];

                    if ((activeControllersChecked && !controller.Shutdown) || (disabledControllersChecked && controller.Shutdown))
                    {
                        UIStyle statusStyle;
                        switch (controller.GetVesselStatus())
                        {
                            case 0:
                                statusStyle = CommonWindowProperties.Style_Label_Normal_Center_Grey;
                                break;
                            default:
                                statusStyle = CommonWindowProperties.Style_Label_Normal_Center_Grey;
                                break;
                        }

                        row =  new DialogGUIHorizontalLayout(
                            new DialogGUILabel(controller.vessel.GetDisplayName(), 150f),
                            new DialogGUISpace(10f),
                            new DialogGUILabel(controller.GetVesselStatusText(), 70f) { guiStyle = statusStyle },
                            new DialogGUISpace(10f),
                            new DialogGUILabel(controller.vessel.mainBody.bodyDisplayName.Replace("^N", ""), 60f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                            new DialogGUISpace(10f),
                            new DialogGUILabel("-", 60f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                            new DialogGUISpace(10f),
                            new DialogGUILabel("-", 90f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                            new DialogGUISpace(10f),
                            (
                                !controller.vessel.isActiveVessel
                                ?
                                TooltipExtension.DeferTooltip(new DialogGUIButton("->", delegate { SwitchToVessel(controller.vessel.id); }, 22f, 16f, false) { tooltipText = Localizer.Format("#LOC_BV_SwitchToVessel") })
                                :
                                new DialogGUISpace(10f)
                            )

                        );
                        row.SetOptionText(controller.vessel.id.ToString()); // ID of the row (vessel ID)
                    }

                    if (row != null)
                    {
                        list[counter] = row;
                        counter++;
                        if (counter >= listLength) // break if we are at the end of list
                            break;
                    }
                }
            }

            vesselListLayout = new DialogGUIVerticalLayout(
                CommonWindowProperties.mainListMinWidth,
                CommonWindowProperties.mainListMinHeight,
                CommonWindowProperties.mainWindowSpacing,
                CommonWindowProperties.mainListPadding,
                TextAnchor.UpperLeft,
                list
            );

            return vesselListLayout;
        }


        /// <summary>
        /// Clear layout of the list of vessels
        /// </summary>
        public void ClearVesselListLayout()
        {
            vesselListLayout = null;
        }

    }

}
