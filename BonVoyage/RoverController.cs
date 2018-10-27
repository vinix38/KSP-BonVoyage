using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    /// <summary>
    /// Rover controller. Child of BVController
    /// </summary>
    public class RoverController : BVController
    {
        #region Private properties

        // Config values
        private double averageSpeed = 0;
        private double averageSpeedAtNight = 0;
        private bool solarPowered = false;
        private bool manned = false;
        private double vesselHeightFromTerrain = 0;
        // Config values

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v"></param>
        /// <param name="module"></param>
        public RoverController(Vessel v, ConfigNode module) : base (v, module)
        {
            // Load values from config
            averageSpeed = double.Parse(BVModule.GetValue("averageSpeed") != null ? BVModule.GetValue("averageSpeed") : "0");
            averageSpeedAtNight = double.Parse(BVModule.GetValue("averageSpeedAtNight") != null ? BVModule.GetValue("averageSpeedAtNight") : "0");
            solarPowered = bool.Parse(BVModule.GetValue("solarPowered") != null ? BVModule.GetValue("solarPowered") : "false");
            manned = bool.Parse(BVModule.GetValue("manned") != null ? BVModule.GetValue("manned") : "false");
            vesselHeightFromTerrain = double.Parse(BVModule.GetValue("vesselHeightFromTerrain") != null ? BVModule.GetValue("vesselHeightFromTerrain") : "0");
        }


        /// <summary>
        /// Get controller type
        /// </summary>
        /// <returns></returns>
        public override int GetControllerType()
        {
            return 0;
        }

    }

}
