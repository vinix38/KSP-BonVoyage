using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    /// <summary>
    /// Basic controller
    /// </summary>
    public class BVController
    {
        #region Public properties

        public Vessel vessel; // Vessel containing BonVoyageModule

        public bool Shutdown
        {
            get { return shutdown; }
        }

        #endregion


        #region Private properties

        private ConfigNode BVModule; // Config node of BonVoyageModule

        // Config values
        private bool active = false;
        private bool shutdown = false;
        private double targetLatitude = 0;
        private double targetLongitude = 0;
        private double distanceToTarget = 0;
        private double distanceTravelled = 0;
        private double averageSpeed = 0;
        private double averageSpeedAtNight = 0;
        private double lastTimeUpdated = 0;
        private bool solarPowered = false;
        private bool manned = false;
        private double vesselHeightFromTerrain = 0;
        // Config values
        
        private List<PathUtils.WayPoint> path = null; // Path to destination

        private byte status;

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v"></param>
        /// <param name="module"></param>
        public BVController(Vessel v, ConfigNode module)
        {
            vessel = v;
            BVModule = module;

            // Load values from config
            active = bool.Parse(BVModule.GetValue("active") != null ? BVModule.GetValue("active") : "false");
            shutdown = bool.Parse(BVModule.GetValue("shutdown") != null ? BVModule.GetValue("shutdown") : "false");
            targetLatitude = double.Parse(BVModule.GetValue("targetLatitude") != null ? BVModule.GetValue("targetLatitude") : "0");
            targetLongitude = double.Parse(BVModule.GetValue("targetLongitude") != null ? BVModule.GetValue("targetLongitude") : "0");
            distanceToTarget = double.Parse(BVModule.GetValue("distanceToTarget") != null ? BVModule.GetValue("distanceToTarget") : "0");
            distanceTravelled = double.Parse(BVModule.GetValue("distanceTravelled") != null ? BVModule.GetValue("distanceTravelled") : "0");
            averageSpeed = double.Parse(BVModule.GetValue("averageSpeed") != null ? BVModule.GetValue("averageSpeed") : "0");
            averageSpeedAtNight = double.Parse(BVModule.GetValue("averageSpeedAtNight") != null ? BVModule.GetValue("averageSpeedAtNight") : "0");
            lastTimeUpdated = double.Parse(BVModule.GetValue("lastTimeUpdated") != null ? BVModule.GetValue("lastTimeUpdated") : "0");
            solarPowered = bool.Parse(BVModule.GetValue("solarPowered") != null ? BVModule.GetValue("solarPowered") : "false");
            manned = bool.Parse(BVModule.GetValue("manned") != null ? BVModule.GetValue("manned") : "false");
            vesselHeightFromTerrain = double.Parse(BVModule.GetValue("vesselHeightFromTerrain") != null ? BVModule.GetValue("vesselHeightFromTerrain") : "0");
            if (BVModule.GetValue("pathEncoded") != null)
                path = PathUtils.DecodePath(BVModule.GetValue("pathEncoded"));

            status = 0;
        }


        /// <summary>
        /// Get controller type
        /// </summary>
        /// <returns></returns>
        public virtual int GetControllerType()
        {
            return -1;
        }


        #region Main window texts

        /// <summary>
        /// Get vessel status
        /// </summary>
        /// <returns></returns>
        public byte GetVesselStatus()
        {
            return status;
        }


        /// <summary>
        /// Get textual reprezentation of the vessel status
        /// </summary>
        /// <returns></returns>
        public string GetVesselStatusText()
        {
            switch (status)
            {
                case 0:
                    return Localizer.Format("#LOC_BV_Status_Idle");
                default:
                    return Localizer.Format("#LOC_BV_Status_Idle");
            }
        }

        #endregion

    }

}
