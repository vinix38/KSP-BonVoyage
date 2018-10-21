using UnityEngine;
using KSP.UI.TooltipTypes;

namespace BonVoyage
{
    /// <summary>
    /// Extensions for a tooltip of DialogGUI* components
    /// </summary>
    static class TooltipExtension
    {
        private static readonly Tooltip_Text textTooltipPrefab = AssetBase.GetPrefab<Tooltip_Text>("Tooltip_Text");


        /// <summary>
        /// Create a tooltip object for a given GameObject
        /// </summary>
        /// <param name="gameObj"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        public static bool SetTooltip(this GameObject gameObj, string tooltip)
        {
            if (gameObj != null)
            {
                TooltipController_Text tt = (gameObj.GetComponent<TooltipController_Text>() ?? gameObj.AddComponent<TooltipController_Text>());
                if (tt != null)
                {
                    tt.textString = tooltip;
                    tt.prefab = textTooltipPrefab;
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Set up callbacks to activate a tooltip for a DialogGUI* object later.
        /// DialogGUI* objects don't have GameObjects until they're displayed, and you need that to add a tooltip, so we need an asynchronous strategy.
        /// </summary>
        /// <param name="gb"></param>
        /// <returns></returns>
        public static DialogGUIBase DeferTooltip(DialogGUIBase gb)
        {
            if (gb.tooltipText != "")
            {
                gb.OnUpdate = () => {
                    if ((gb.uiItem != null) && gb.uiItem.SetTooltip(gb.tooltipText))
                    {
                        gb.OnUpdate = () => { };
                    }
                };
            }
            return gb;
        }

    }

}
