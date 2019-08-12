using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BonVoyage
{
    /// <summary>
    /// Ship controller. Child of BVController
    /// </summary>
    internal class ShipController : BVController
    {
        #region internal properties

        internal override double AverageSpeed { get { return ((angle <= 90) || (batteries.UseBatteries && (batteries.CurrentEC > 0)) ? (averageSpeed * speedMultiplier) : (averageSpeedAtNight * speedMultiplier)); } }

        #endregion


        #region Private properties

        // Config values
        private double averageSpeed = 0;
        private double averageSpeedAtNight = 0;
        private bool manned = false;
        private double vesselHeightFromTerrain = 0;
        // Config values

        private double speedMultiplier;
        private double angle; // Angle between the main body and the main sun
        private double maxSpeedBase; // maximum speed without modifiers
        private int crewSpeedBonus; // Speed modifier based on the available crew

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v"></param>
        /// <param name="module"></param>
        internal ShipController(Vessel v, ConfigNode module) : base(v, module)
        {
            // Load values from config if it isn't the first run of the mod (we are reseting vessel on the first run)
            if (!Configuration.FirstRun)
            {
                averageSpeed = double.Parse(BVModule.GetValue("averageSpeed") != null ? BVModule.GetValue("averageSpeed") : "0");
                averageSpeedAtNight = double.Parse(BVModule.GetValue("averageSpeedAtNight") != null ? BVModule.GetValue("averageSpeedAtNight") : "0");
                manned = bool.Parse(BVModule.GetValue("manned") != null ? BVModule.GetValue("manned") : "false");
                vesselHeightFromTerrain = double.Parse(BVModule.GetValue("vesselHeightFromTerrain") != null ? BVModule.GetValue("vesselHeightFromTerrain") : "0");
            }

            speedMultiplier = 1.0;
            angle = 0;
            maxSpeedBase = 0;
            crewSpeedBonus = 0;
        }


        /// <summary>
        /// Get controller type
        /// </summary>
        /// <returns></returns>
        internal override int GetControllerType()
        {
            return 1;
        }


        #region Status window texts

        internal override List<DisplayedSystemCheckResult> GetDisplayedSystemCheckResults()
        {
            base.GetDisplayedSystemCheckResults();

            DisplayedSystemCheckResult result = new DisplayedSystemCheckResult
            {
                Toggle = false,
                Label = Localizer.Format("#LOC_BV_Control_AverageSpeed"),
                Text = averageSpeed.ToString("F") + " m/s",
                Tooltip =
                    Localizer.Format("#LOC_BV_Control_SpeedBase") + ": " + maxSpeedBase.ToString("F") + " m/s\n"
                        + (manned ? Localizer.Format("#LOC_BV_Control_DriverBonus") + ": " + crewSpeedBonus.ToString() + "%\n" : Localizer.Format("#LOC_BV_Control_UnmannedPenalty") + ": 80%\n")
                        + Localizer.Format("#LOC_BV_Control_SpeedAtNight") + ": " + averageSpeedAtNight.ToString("F") + " m/s"
                        + "Height from surface: " + vessel.GetHeightFromSurface().ToString()
            };
            displayedSystemCheckResults.Add(result);

            return displayedSystemCheckResults;
        }

        #endregion


        #region Pathfinder

        /// <summary>
        /// Find a route to the target
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        internal override bool FindRoute(double lat, double lon)
        {
            return FindRoute(lat, lon, TileTypes.Ocean);
        }

        #endregion


        /// <summary>
        /// Check the systems
        /// </summary>
        internal override void SystemCheck()
        {
            base.SystemCheck();

            // Manned
            manned = (vessel.GetCrewCount() > 0);

            // Pilots and Scouts (USI) increase base average speed
            crewSpeedBonus = 0;
            if (manned)
            {
                int maxPilotLevel = -1;
                int maxScoutLevel = -1;
                int maxDriverLevel = -1;

                List<ProtoCrewMember> crewList = vessel.GetVesselCrew();
                for (int i = 0; i < crewList.Count; i++)
                {
                    switch (crewList[i].trait)
                    {
                        case "Pilot":
                            if (maxPilotLevel < crewList[i].experienceLevel)
                                maxPilotLevel = crewList[i].experienceLevel;
                            break;
                        case "Scout":
                            if (maxScoutLevel < crewList[i].experienceLevel)
                                maxScoutLevel = crewList[i].experienceLevel;
                            break;
                        default:
                            if (crewList[i].HasEffect("AutopilotSkill"))
                                if (maxDriverLevel < crewList[i].experienceLevel)
                                    maxDriverLevel = crewList[i].experienceLevel;
                            break;
                    }
                }
                if (maxPilotLevel > 0)
                    crewSpeedBonus = 6 * maxPilotLevel; // up to 30% for a Pilot
                else if (maxDriverLevel > 0)
                    crewSpeedBonus = 4 * maxDriverLevel; // up to 20% for any driver (has AutopilotSkill skill)
                else if (maxScoutLevel > 0)
                    crewSpeedBonus = 2 * maxScoutLevel; // up to 10% for a Scout (Scouts disregard safety)
            }

            //// Test value = 50 m/s
            maxSpeedBase = 50;

            averageSpeed = 0.7 * maxSpeedBase * (1 + crewSpeedBonus / 100);

            // Unmanned ships drive with 80% speed penalty
            if (!manned)
                averageSpeed = averageSpeed * 0.2;

            //// Base average speed at night is the same as average speed - for now
            averageSpeedAtNight = averageSpeed;
        }


        /// <summary>
        /// Activate autopilot
        /// </summary>
        internal override bool Activate()
        {
            if (vessel.situation != Vessel.Situations.SPLASHED)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_BV_Warning_Splashed"), 5f).color = Color.yellow;
                return false;
            }

            SystemCheck();

            BonVoyageModule module = vessel.FindPartModuleImplementing<BonVoyageModule>();
            if (module != null)
            {
                vesselHeightFromTerrain = vessel.GetHeightFromSurface();

                module.averageSpeed = averageSpeed;
                module.averageSpeedAtNight = averageSpeedAtNight;
                module.manned = manned;
                module.vesselHeightFromTerrain = vesselHeightFromTerrain;
            }

            return base.Activate();
        }


        /// <summary>
        /// Deactivate autopilot
        /// </summary>
        internal override bool Deactivate()
        {
            SystemCheck();
            return base.Deactivate();
        }

    }
}
