using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BonVoyage
{
    /// <summary>
    /// Common settings for all windows
    /// </summary>
    static class CommonWindowProperties
    {
        public static UISkinDef UnitySkin { get; set; }
        public static UISkinDef ActiveSkin { get; set; } // Actual skin used

        public static RectOffset mainElementPadding = new RectOffset(5, 5, 10, 10);
        public static RectOffset mainListPadding = new RectOffset(4, 4, 3, 3);

        public static readonly Vector2 mainWindowAnchorMin = new Vector2(0.5f, 1f);
        public static readonly Vector2 mainWindowAnchorMax = new Vector2(0.5f, 1f);

        public const float mainWindowSpacing = 3f;
        public const float mainListMinWidth = 570f;
        public const float mainListMinHeight = 320f;

        #region Styles

        public static readonly UIStyleState StyleState_White = new UIStyleState() { textColor = Color.white };
        public static readonly UIStyleState StyleState_Green = new UIStyleState() { textColor = Color.green };
        public static readonly UIStyleState StyleState_Yellow = new UIStyleState() { textColor = Color.yellow };
        public static readonly UIStyleState StyleState_Red = new UIStyleState() { textColor = Color.red };
        public static readonly UIStyleState StyleState_Grey = new UIStyleState() { textColor = Color.grey };

        public static UIStyle Style_Label_Bold_Left;
        public static UIStyle Style_Label_Bold_Center;
        public static UIStyle Style_Label_Normal_Center;
        public static UIStyle Style_Label_Normal_Center_White;
        public static UIStyle Style_Label_Normal_Center_Green;
        public static UIStyle Style_Label_Normal_Center_Yellow;
        public static UIStyle Style_Label_Normal_Center_Red;
        public static UIStyle Style_Label_Normal_Center_Grey;

        #endregion


        /// <summary>
        /// Refresh styles after skin change
        /// </summary>
        public static void RefreshStyles()
        {
            // Style_Label_Bold_Left
            Style_Label_Bold_Left = new UIStyle(ActiveSkin.label);
            Style_Label_Bold_Left.fontStyle = FontStyle.Bold;

            // Style_Label_Bold_Center
            Style_Label_Bold_Center = new UIStyle(ActiveSkin.label);
            Style_Label_Bold_Center.fontStyle = FontStyle.Bold;
            Style_Label_Bold_Center.alignment = TextAnchor.UpperCenter;

            // Style_Label_Normal_Center
            Style_Label_Normal_Center = new UIStyle(ActiveSkin.label);
            Style_Label_Normal_Center.alignment = TextAnchor.UpperCenter;

            // Style_Label_Normal_Center_White
            Style_Label_Normal_Center_White = new UIStyle(ActiveSkin.label);
            Style_Label_Normal_Center_White.alignment = TextAnchor.UpperCenter;
            Style_Label_Normal_Center_White.active = StyleState_White;
            Style_Label_Normal_Center_White.normal = StyleState_White;
            Style_Label_Normal_Center_White.disabled = StyleState_White;
            Style_Label_Normal_Center_White.highlight = StyleState_White;

            // Style_Label_Normal_Center_Green
            Style_Label_Normal_Center_Green = new UIStyle(ActiveSkin.label);
            Style_Label_Normal_Center_Green.alignment = TextAnchor.UpperCenter;
            Style_Label_Normal_Center_Green.active = StyleState_Green;
            Style_Label_Normal_Center_Green.normal = StyleState_Green;
            Style_Label_Normal_Center_Green.disabled = StyleState_Green;
            Style_Label_Normal_Center_Green.highlight = StyleState_Green;

            // Style_Label_Normal_Center_Yellow
            Style_Label_Normal_Center_Yellow = new UIStyle(ActiveSkin.label);
            Style_Label_Normal_Center_Yellow.alignment = TextAnchor.UpperCenter;
            Style_Label_Normal_Center_Yellow.active = StyleState_Yellow;
            Style_Label_Normal_Center_Yellow.normal = StyleState_Yellow;
            Style_Label_Normal_Center_Yellow.disabled = StyleState_Yellow;
            Style_Label_Normal_Center_Yellow.highlight = StyleState_Yellow;

            // Style_Label_Normal_Center_Red
            Style_Label_Normal_Center_Red = new UIStyle(ActiveSkin.label);
            Style_Label_Normal_Center_Red.alignment = TextAnchor.UpperCenter;
            Style_Label_Normal_Center_Red.active = StyleState_Red;
            Style_Label_Normal_Center_Red.normal = StyleState_Red;
            Style_Label_Normal_Center_Red.disabled = StyleState_Red;
            Style_Label_Normal_Center_Red.highlight = StyleState_Red;

            // Style_Label_Normal_Center_Grey
            Style_Label_Normal_Center_Grey = new UIStyle(ActiveSkin.label);
            Style_Label_Normal_Center_Grey.alignment = TextAnchor.UpperCenter;
            Style_Label_Normal_Center_Grey.active = StyleState_Grey;
            Style_Label_Normal_Center_Grey.normal = StyleState_Grey;
            Style_Label_Normal_Center_Grey.disabled = StyleState_Grey;
            Style_Label_Normal_Center_Grey.highlight = StyleState_Grey;
        }

    }

}
