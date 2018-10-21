using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BonVoyage
{
    /// <summary>
    /// Main mod's window - view part
    /// For emmbeding in a MultiOptionDialog
    /// </summary>
    public class MainWindowView : DialogGUIVerticalLayout
    {
        private PopupDialog dialog { get; set; }
        private MainWindowModel model;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowView(MainWindowModel m) : base(
            CommonWindowProperties.mainListMinWidth + 20, // min width
            CommonWindowProperties.mainListMinHeight, // min height
            CommonWindowProperties.mainWindowSpacing, // spacing
            CommonWindowProperties.mainElementPadding, // padding
            TextAnchor.UpperLeft // text anchor
        )
        {
            model = m;

            // Filter
            AddChild(new DialogGUIHorizontalLayout(
                new DialogGUIToggle(true, Localizer.Format("#LOC_BV_ActiveVessels"), model.ActiveVesselsChecked, 130f),
                new DialogGUIToggle(false, Localizer.Format("#LOC_BV_DisabledVessels"), model.DisabledVesselsChecked, 130f),
                new DialogGUIFlexibleSpace()
            ));
            
            // Column headers
            AddChild(new DialogGUIHorizontalLayout(
                new DialogGUISpace(0f),
                new DialogGUILabel(Localizer.Format("#LOC_BV_ColumnHeader_Name"), 150f) { guiStyle = CommonWindowProperties.Style_Label_Bold_Left },
                new DialogGUISpace(10f),
                new DialogGUILabel(Localizer.Format("#LOC_BV_ColumnHeader_Status"), 70f) { guiStyle = CommonWindowProperties.Style_Label_Bold_Center },
                new DialogGUISpace(10f),
                new DialogGUILabel(Localizer.Format("#LOC_BV_ColumnHeader_Body"), 60f) { guiStyle = CommonWindowProperties.Style_Label_Bold_Center },
                new DialogGUISpace(10f),
                new DialogGUILabel(Localizer.Format("#LOC_BV_ColumnHeader_Speed"), 60f) { guiStyle = CommonWindowProperties.Style_Label_Bold_Center },
                new DialogGUISpace(10f),
                new DialogGUILabel(Localizer.Format("#LOC_BV_ColumnHeader_Distance"), 90f) { guiStyle = CommonWindowProperties.Style_Label_Bold_Center }
            ));

            DialogGUIBase[] list = new DialogGUIBase[2 + 1];
            list[0] = new DialogGUIContentSizer(UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained, UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize, true);
            list[1] = new DialogGUIHorizontalLayout(
                new DialogGUILabel("long vessel name", 150f),
                new DialogGUISpace(10f),
                new DialogGUILabel("awaiting sunlight", 70f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center_White },
                new DialogGUISpace(10f),
                new DialogGUILabel("Kerbin", 60f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                new DialogGUISpace(10f),
                new DialogGUILabel("45.5 m/s", 60f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                new DialogGUISpace(10f),
                new DialogGUILabel("999 km", 90f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                new DialogGUISpace(10f),
                TooltipExtension.DeferTooltip(new DialogGUIButton("->", model.GotoVessel, 22f, 16f, false) { tooltipText = Localizer.Format("#LOC_BV_GoToVessel") })
            );
            list[2] = new DialogGUIHorizontalLayout(
                new DialogGUILabel("long vessel name", 150f),
                new DialogGUISpace(10f),
                new DialogGUILabel("awaiting sunlight", 70f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center_White },
                new DialogGUISpace(10f),
                new DialogGUILabel("Kerbin", 60f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                new DialogGUISpace(10f),
                new DialogGUILabel("45.5 m/s", 60f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                new DialogGUISpace(10f),
                new DialogGUILabel("999 km", 90f) { guiStyle = CommonWindowProperties.Style_Label_Normal_Center },
                new DialogGUISpace(10f),
                new DialogGUIButton("->", model.GotoVessel, 22f, 16f, false) { tooltipText = "tooltip" }
            );

            AddChild(new DialogGUIScrollList(new Vector2(CommonWindowProperties.mainListMinWidth, CommonWindowProperties.mainListMinHeight), false, true,
                new DialogGUIVerticalLayout(
                    CommonWindowProperties.mainListMinWidth,
                    CommonWindowProperties.mainListMinHeight,
                    CommonWindowProperties.mainWindowSpacing,
                    CommonWindowProperties.mainListPadding,
                    TextAnchor.UpperLeft,
                    list
                )
            ));
        }


        private static Rect geometry
        {
            get
            {
                Vector2 pos = MainWindowModel.MainWindowPosition;
                return new Rect(
                    pos.x / GameSettings.UI_SCALE,
                    pos.y / GameSettings.UI_SCALE,
                    CommonWindowProperties.mainListMinWidth + 20, CommonWindowProperties.mainListMinHeight);
            }
            set
            {
                MainWindowModel.MainWindowPosition = new Vector2(
                    value.x * GameSettings.UI_SCALE,
                    value.y * GameSettings.UI_SCALE
                );
            }
        }


        private Rect currentGeometry
        {
            get
            {
                Vector3 rt = dialog.GetComponent<RectTransform>().position;
                return new Rect(
                    rt.x / GameSettings.UI_SCALE / Screen.width + 0.5f,
                    rt.y / GameSettings.UI_SCALE / Screen.height + 0.5f,
                    CommonWindowProperties.mainListMinWidth + 20, CommonWindowProperties.mainListMinHeight);
            }
        }


        /// <summary>
        /// Show dialog window
        /// </summary>
        /// <returns></returns>
        public PopupDialog Show()
        {
            if (dialog == null)
            {
                dialog = PopupDialog.SpawnPopupDialog(
                    CommonWindowProperties.mainWindowAnchorMin, // min anchor
                    CommonWindowProperties.mainWindowAnchorMax, // max anchor
                    new MultiOptionDialog(
                        "BVMainWindow", // name
                        "", // message
                        Localizer.Format("#LOC_BV_Title"), // title
                        CommonWindowProperties.ActiveSkin, // skin
                        geometry, // position and size
                        this // dialog layout
                    ),
                    false, // persist across scenes
                    CommonWindowProperties.ActiveSkin, // skin
                    false // is modal
                );
            }

            return dialog;
        }


        /// <summary>
        /// Dismiss dialog window
        /// </summary>
        public void Dismiss()
        {
            if (dialog != null)
            {
                geometry = new Rect(currentGeometry.x, currentGeometry.y, CommonWindowProperties.mainListMinWidth + 20, CommonWindowProperties.mainListMinHeight);

                dialog.Dismiss();
                dialog = null;
            }
        }

    }

}
