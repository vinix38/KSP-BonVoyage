using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    /// <summary>
    /// Enum of vessel states
    /// </summary>
    public enum VesselState
    {
        Idle = 0,
        ControllerDisabled = 1,
        Current = 2,
        Moving = 3,
        AwaitingSunlight = 4
    }


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
            set
            {
                shutdown = value;
                if (shutdown)
                    state = VesselState.ControllerDisabled;
                else
                    state = VesselState.Idle;
            }
        }

        #endregion


        #region Private properties

        protected ConfigNode BVModule; // Config node of BonVoyageModule

        // Config values
        private bool active = false;
        private bool shutdown = false;
        private double targetLatitude = 0;
        private double targetLongitude = 0;
        private double distanceToTarget = 0;
        private double distanceTravelled = 0;
        private double lastTimeUpdated = 0;
        // Config values
        
        private List<PathUtils.WayPoint> path = null; // Path to destination

        private VesselState state;

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
            lastTimeUpdated = double.Parse(BVModule.GetValue("lastTimeUpdated") != null ? BVModule.GetValue("lastTimeUpdated") : "0");
            if (BVModule.GetValue("pathEncoded") != null)
                path = PathUtils.DecodePath(BVModule.GetValue("pathEncoded"));

            state = VesselState.Idle;
            if (shutdown)
                state = VesselState.ControllerDisabled;
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
        /// Get vessel state
        /// </summary>
        /// <returns></returns>
        public VesselState GetVesselState()
        {
            if (vessel.isActiveVessel)
                return VesselState.Current;
            return state;
        }


        /// <summary>
        /// Get textual reprezentation of the vessel status
        /// </summary>
        /// <returns></returns>
        public string GetVesselStateText()
        {
            if (vessel.isActiveVessel)
                return Localizer.Format("#LOC_BV_Status_Current");
            switch (state)
            {
                case VesselState.Idle:
                    return Localizer.Format("#LOC_BV_Status_Idle");
                case VesselState.ControllerDisabled:
                    return Localizer.Format("#LOC_BV_Status_Disabled");
                case VesselState.AwaitingSunlight:
                    return Localizer.Format("#LOC_BV_Status_AwaitingSunlight");
                case VesselState.Moving:
                    return Localizer.Format("#LOC_BV_Status_Moving");
                default:
                    return Localizer.Format("#LOC_BV_Status_Idle");
            }
        }

        #endregion

    }

}
