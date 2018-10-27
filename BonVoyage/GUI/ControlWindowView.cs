using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace BonVoyage
{
    class ControlWindowView : DialogGUIVerticalLayout
    {
        private PopupDialog dialog { get; set; }
        private ControlWindowModel model;
        private UnityAction closeCallback { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public ControlWindowView(ControlWindowModel m, UnityAction close) : base(
            CommonWindowProperties.controlMinWidth, // min width
            CommonWindowProperties.controlMinHeight, // min height
            CommonWindowProperties.controlWindowSpacing, // spacing
            CommonWindowProperties.controlElementPadding, // padding
            TextAnchor.UpperLeft // text anchor
        )
        {
            model = m;
            closeCallback = close;
        }


        #region Window geometry
        private static Rect geometry
        {
            get
            {
                Vector2 pos = CommonWindowProperties.ControlWindowPosition;
                return new Rect(pos.x, pos.y, CommonWindowProperties.controlWindowWidth, CommonWindowProperties.controlWindowHeight);
            }
            set
            {
                CommonWindowProperties.ControlWindowPosition = new Vector2(value.x, value.y);
                Configuration.ControlWindowPosition = CommonWindowProperties.ControlWindowPosition;
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
                    CommonWindowProperties.controlWindowWidth, CommonWindowProperties.controlWindowHeight);
            }
        }
        #endregion


        /// <summary>
        /// Show dialog window
        /// </summary>
        /// <returns></returns>
        public PopupDialog Show()
        {
            if (dialog == null)
            {
                dialog = PopupDialog.SpawnPopupDialog(
                    CommonWindowProperties.controlWindowAnchorMin, // min anchor
                    CommonWindowProperties.controlWindowAnchorMax, // max anchor
                    new MultiOptionDialog(
                        "BVControlWindow", // name
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

                CommonWindowProperties.AddFloatingButton(
                    dialog.transform,
                    -CommonWindowProperties.controlElementPadding.right - CommonWindowProperties.controlWindowSpacing, -CommonWindowProperties.controlElementPadding.top,
                    CommonWindowProperties.ActiveSkin.button,
                    "X",
                    Localizer.Format("#LOC_BV_Close"),
                    closeCallback
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
                Rect g = currentGeometry;
                geometry = new Rect(g.x, g.y, CommonWindowProperties.controlWindowWidth, CommonWindowProperties.controlWindowHeight);

                dialog.Dismiss();
                dialog = null;
            }
        }

    }

}
