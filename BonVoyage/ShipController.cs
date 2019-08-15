using KSP.Localization;
using KSP.UI.Screens;
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

        internal override double AverageSpeed { get { return ((angle <= 90) ? (averageSpeed * speedMultiplier) : (averageSpeedAtNight * speedMultiplier)); } }

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
        EngineTestResult engineTestResult = new EngineTestResult(); // Result of a test of engines
        private int throttle = 100; // Allowed values: 100, 75, 50, 25

        // Basic values
        private double thrust0 = 350; // 350kN
        private double speed0 = 50; // 50m/s
        private double mass0 = 25; // 25t
        private double density0 = 1.11; // 1.11 - half of density of water plus half of density of air on Kerbin

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

        internal override List<DisplayedSystemCheckResult[]> GetDisplayedSystemCheckResults()
        {
            base.GetDisplayedSystemCheckResults();

            DisplayedSystemCheckResult[] result = new DisplayedSystemCheckResult[] {
                new DisplayedSystemCheckResult
                {
                    Toggle = false,
                    Label = Localizer.Format("#LOC_BV_Control_AverageSpeed"),
                    Text = averageSpeed.ToString("F") + " m/s",
                    Tooltip = Localizer.Format("#LOC_BV_Control_SpeedBase") + ": " + maxSpeedBase.ToString("F") + " m/s\n"
                        + (manned ? Localizer.Format("#LOC_BV_Control_DriverBonus") + ": " + crewSpeedBonus.ToString() + "%\n" : Localizer.Format("#LOC_BV_Control_UnmannedPenalty") + ": 80%\n")
                        + Localizer.Format("#LOC_BV_Control_SpeedAtNight") + ": " + averageSpeedAtNight.ToString("F") + " m/s\n"
                        + Localizer.Format("#LOC_BV_Control_UsedThrust") + ": " + engineTestResult.maxThrustSum.ToString("F") + " kN"
                }
            };
            displayedSystemCheckResults.Add(result);

            result = new DisplayedSystemCheckResult[] {
                new DisplayedSystemCheckResult
                {
                    Toggle = false,
                    Label = Localizer.Format("#LOC_BV_Control_Throttle"),
                    Text = "",
                    Tooltip = Localizer.Format("#LOC_BV_Control_Throttle_Tooltip")
                }
            };
            displayedSystemCheckResults.Add(result);

            result = new DisplayedSystemCheckResult[] {
                new DisplayedSystemCheckResult {
                    Toggle = true,
                    Text = "100%",
                    Tooltip = "",
                    GetToggleValue = GetThrottle100,
                    ToggleSelectedCallback = UseThrottle100
                },
                new DisplayedSystemCheckResult {
                    Toggle = true,
                    Text = "75%",
                    Tooltip = "",
                    GetToggleValue = GetThrottle75,
                    ToggleSelectedCallback = UseThrottle75
                },
                new DisplayedSystemCheckResult {
                    Toggle = true,
                    Text = "50%",
                    Tooltip = "",
                    GetToggleValue = GetThrottle50,
                    ToggleSelectedCallback = UseThrottle50
                }
            };
            displayedSystemCheckResults.Add(result);

            result = new DisplayedSystemCheckResult[] {
                new DisplayedSystemCheckResult {
                    Toggle = true,
                    Text = "25%",
                    Tooltip = "",
                    GetToggleValue = GetThrottle25,
                    ToggleSelectedCallback = UseThrottle25
                }
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

            // Test stock engines
            EngineTestResult testResultStockEngines = CheckStockEngines();
            // Sum it (for future tests of non stock engines or different types of engines - jets, rocket etc.)
            engineTestResult.maxThrustSum = testResultStockEngines.maxThrustSum;

            // Throttle
            engineTestResult.maxThrustSum = engineTestResult.maxThrustSum * (Convert.ToDouble(throttle) / 100);
            for (int p = 0; p < propellants.Count; p++)
                propellants[p].FuelFlow = propellants[p].FuelFlow * (Convert.ToDouble(throttle) / 100);

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

            // Compute max speed - based on the drag equation
            double Z = (density0 / (0.5 * vessel.mainBody.atmDensityASL + 0.5 * vessel.mainBody.oceanDensity)) * (mass0 / vessel.GetTotalMass()) * (engineTestResult.maxThrustSum / thrust0);
            maxSpeedBase = Math.Sqrt(Z) * speed0;
            if (maxSpeedBase > speed0) // We are over max allowed speed, then we need to decrease max available thrust
            {
                maxSpeedBase = speed0;
                engineTestResult.maxThrustSum = engineTestResult.maxThrustSum / Z;
                for (int p = 0; p < propellants.Count; p++)
                    propellants[p].FuelFlow = propellants[p].FuelFlow / Z;
            }

            averageSpeed = 0.7 * maxSpeedBase * (1 + Convert.ToDouble(crewSpeedBonus) / 100);

            // Unmanned ships drive with 80% speed penalty
            if (!manned)
                averageSpeed = averageSpeed * 0.2;

            //// Base average speed at night is the same as average speed - for now
            averageSpeedAtNight = averageSpeed;
        }


        #region Engines

        /// <summary>
        /// Result of the test of wheels
        /// </summary>
        private struct EngineTestResult
        {
            internal double maxThrustSum; // Sum of max thrusts of all enabled engines

            internal EngineTestResult(double maxThrustSum)
            {
                this.maxThrustSum = maxThrustSum;
            }
        }


        /// <summary>
        /// Test stock engines implementing standard modules ModuleEnginesFX, ModuleEngines
        /// </summary>
        /// <returns></returns>
        private EngineTestResult CheckStockEngines()
        {
            double maxThrustSum = 0;
            propellants.Clear();
            
            // Get jet engines' modules
            List<Part> jets = new List<Part>();
            for (int i = 0; i < vessel.parts.Count; i++)
            {
                var part = vessel.parts[i];
                if (part.Modules.Contains("ModuleEnginesFX"))
                    jets.Add(part);
            }

            for (int i = 0; i < jets.Count; i++)
            {
                List<ModuleEnginesFX> enginesFx = jets[i].FindModulesImplementing<ModuleEnginesFX>();
                if (enginesFx != null)
                {
                    for (int k = 0; k < enginesFx.Count; k++)
                    {
                        if (!enginesFx[k].engineShutdown && enginesFx[k].isOperational)
                        {
                            // Max thrust
                            maxThrustSum += enginesFx[k].maxThrust * enginesFx[k].thrustPercentage / 100;

                            // Propellants used in ISP computation - what is not used is usually air
                            for (int p = 0; p < enginesFx[k].propellants.Count; p++)
                            {
                                if (!enginesFx[k].propellants[p].ignoreForIsp)
                                {
                                    var ir = propellants.Find(x => x.Name == enginesFx[k].propellants[p].name);
                                    if (ir == null)
                                    {
                                        ir = new Fuel();
                                        ir.Name = enginesFx[k].propellants[p].name;
                                        propellants.Add(ir);
                                    }
                                    ir.FuelFlow += enginesFx[k].getMaxFuelFlow(enginesFx[k].propellants[p]) * enginesFx[k].thrustPercentage / 100;
                                }
                            }
                        }
                    }
                }
            }


            // Get rocket engines' modules
            List<Part> rockets = new List<Part>();
            for (int i = 0; i < vessel.parts.Count; i++)
            {
                var part = vessel.parts[i];
                if (part.Modules.Contains("ModuleEngines"))
                    rockets.Add(part);
            }

            for (int i = 0; i < rockets.Count; i++)
            {

            }


            return new EngineTestResult(maxThrustSum);
        }

        #endregion


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
            
            // At least one engine must be on
            if (engineTestResult.maxThrustSum == 0)
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_BV_Warning_EnginesNotOnline"), 5f).color = Color.yellow;
                return false;
            }

            // Get fuel amount
            IResourceBroker broker = new ResourceBroker();
            for (int i = 0; i < propellants.Count; i++)
            {
                propellants[i].MaximumAmountAvailable = broker.AmountAvailable(vessel.rootPart, propellants[i].Name, 1, ResourceFlowMode.ALL_VESSEL);

                if (propellants[i].MaximumAmountAvailable == 0)
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_BV_Warning_NotEnoughFuel"), 5f).color = Color.yellow;
                    return false;
                }
            }

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


        /// <summary>
        /// Update vessel
        /// </summary>
        /// <param name="currentTime"></param>
        internal override void Update(double currentTime)
        {
            if (vessel == null)
                return;
            if (vessel.isActiveVessel)
            {
                lastTimeUpdated = 0;
                if (active)
                    ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_BV_AutopilotActive"), 10f).color = Color.red;
                return;
            }

            if (!active || vessel.loaded)
                return;

            // If we don't know the last time of update, then set it and wait for the next update cycle
            if (lastTimeUpdated == 0)
            {
                State = VesselState.Idle;
                lastTimeUpdated = currentTime;
                BVModule.SetValue("lastTimeUpdated", currentTime.ToString());
                return;
            }

            Vector3d shipPos = vessel.mainBody.position - vessel.GetWorldPos3D();
            Vector3d toMainStar = vessel.mainBody.position - FlightGlobals.Bodies[mainStarIndex].position;
            angle = Vector3d.Angle(shipPos, toMainStar); // Angle between rover and the main star

            // Speed penalties at twighlight and at night
            if ((angle > 90) && manned) // night
                speedMultiplier = 0.25;
            else if ((angle > 85) && manned) // twilight
                speedMultiplier = 0.5;
            else if ((angle > 80) && manned) // twilight
                speedMultiplier = 0.75;
            else // day
                speedMultiplier = 1.0;

            double deltaT = currentTime - lastTimeUpdated; // Time delta from the last update
            double deltaTOver = 0; // deltaT which is calculated from a value over the maximum propellant amout available

            for (int i = 0; i < propellants.Count; i++)
            {
                propellants[i].CurrentAmountUsed += propellants[i].FuelFlow * deltaT * speedMultiplier * speedMultiplier; // If speed is reduced, then thrust and subsequently fuel flow are reduced by square (from drag equation)
                if (propellants[i].CurrentAmountUsed > propellants[i].MaximumAmountAvailable)
                    deltaTOver = Math.Max(deltaTOver, (propellants[i].CurrentAmountUsed - propellants[i].MaximumAmountAvailable) / (propellants[i].FuelFlow * speedMultiplier * speedMultiplier));
            }
            if (deltaTOver > 0)
            {
                deltaT -= deltaTOver;
                // Reduce the amount of used propellants
                for (int i = 0; i < propellants.Count; i++)
                    propellants[i].CurrentAmountUsed -= propellants[i].FuelFlow * deltaTOver * speedMultiplier * speedMultiplier;
            }

            double deltaS = AverageSpeed * deltaT; // Distance delta from the last update
            distanceTravelled += deltaS;

            if (distanceTravelled >= distanceToTarget) // We reached the target
            {
                if (!MoveSafely(targetLatitude, targetLongitude))
                    distanceTravelled -= deltaS;
                else
                {
                    distanceTravelled = distanceToTarget;

                    active = false;
                    arrived = true;
                    BVModule.SetValue("active", "False");
                    BVModule.SetValue("arrived", "True");
                    BVModule.SetValue("distanceTravelled", distanceToTarget.ToString());
                    BVModule.SetValue("pathEncoded", "");

                    // Dewarp
                    if (Configuration.AutomaticDewarp)
                    {
                        if (TimeWarp.CurrentRate > 3) // Instant drop to 50x warp
                            TimeWarp.SetRate(3, true);
                        if (TimeWarp.CurrentRate > 0) // Gradual drop out of warp
                            TimeWarp.SetRate(0, false);
                        ScreenMessages.PostScreenMessage(vessel.vesselName + " " + Localizer.Format("#LOC_BV_VesselArrived") + " " + vessel.mainBody.bodyDisplayName.Replace("^N", ""), 5f);
                    }

                    NotifyArrival();
                }
                State = VesselState.Idle;
            }
            else
            {
                try // There is sometimes null ref exception during scene change
                {
                    int step = Convert.ToInt32(Math.Floor(distanceTravelled / PathFinder.StepSize)); // In which step of the path we are
                    double remainder = distanceTravelled % PathFinder.StepSize; // Current remaining distance from the current step
                    double bearing = 0;

                    if (step < path.Count - 1)
                        bearing = GeoUtils.InitialBearing( // Bearing to the next step from previous step
                            path[step].latitude,
                            path[step].longitude,
                            path[step + 1].latitude,
                            path[step + 1].longitude
                        );
                    else
                        bearing = GeoUtils.InitialBearing( // Bearing to the target from previous step
                            path[step].latitude,
                            path[step].longitude,
                            targetLatitude,
                            targetLongitude
                        );

                    // Compute new coordinates, we are moving from the current step, distance is "remainder"
                    double[] newCoordinates = GeoUtils.GetLatitudeLongitude(
                        path[step].latitude,
                        path[step].longitude,
                        bearing,
                        remainder,
                        vessel.mainBody.Radius
                    );

                    // Move
                    if (!MoveSafely(newCoordinates[0], newCoordinates[1]))
                    {
                        distanceTravelled -= deltaS;
                        State = VesselState.Idle;
                    }
                    else
                        State = VesselState.Moving;
                }
                catch { }
            }

            Save(currentTime);

            // Stop the ship, we don't have enough of propellant
            if (deltaTOver > 0)
            {
                active = false;
                arrived = true;
                BVModule.SetValue("active", "False");
                BVModule.SetValue("arrived", "True");
                BVModule.SetValue("pathEncoded", "");

                // Dewarp
                if (Configuration.AutomaticDewarp)
                {
                    if (TimeWarp.CurrentRate > 3) // Instant drop to 50x warp
                        TimeWarp.SetRate(3, true);
                    if (TimeWarp.CurrentRate > 0) // Gradual drop out of warp
                        TimeWarp.SetRate(0, false);
                    ScreenMessages.PostScreenMessage(vessel.vesselName + " " + Localizer.Format("#LOC_BV_Warning_Stopped") + ". " + Localizer.Format("#LOC_BV_Warning_NotEnoughFuel"), 5f).color = Color.red;
                }

                NotifyNotEnoughFuel();
                State = VesselState.Idle;
            }
        }


        /// <summary>
        /// Save move of a ship. We need to prevent hitting an active vessel.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns>true if rover was moved, false otherwise</returns>
        private bool MoveSafely(double latitude, double longitude)
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                Vector3d newPos = vessel.mainBody.GetWorldSurfacePosition(latitude, longitude, 0);
                Vector3d actPos = FlightGlobals.ActiveVessel.GetWorldPos3D();
                double distance = Vector3d.Distance(newPos, actPos);
                if (distance <= 2400)
                    return false;
            }

            vessel.latitude = latitude;
            vessel.longitude = longitude;
            vessel.altitude = vesselHeightFromTerrain;

            return true;
        }


        /// <summary>
        /// Notify, that rover has arrived
        /// </summary>
        private void NotifyArrival()
        {
            MessageSystem.Message message = new MessageSystem.Message(
                Localizer.Format("#LOC_BV_Title_ShipArrived"), // title
                "<color=#74B4E2>" + vessel.vesselName + "</color> " + Localizer.Format("#LOC_BV_VesselArrived") + " " + vessel.mainBody.bodyDisplayName.Replace("^N", "") + ".\n<color=#AED6EE>"
                + Localizer.Format("#LOC_BV_Control_Lat") + ": " + targetLatitude.ToString("F2") + "</color>\n<color=#AED6EE>" + Localizer.Format("#LOC_BV_Control_Lon") + ": " + targetLongitude.ToString("F2") + "</color>", // message
                MessageSystemButton.MessageButtonColor.GREEN,
                MessageSystemButton.ButtonIcons.COMPLETE
            );
            MessageSystem.Instance.AddMessage(message);
        }


        /// <summary>
        /// Notify, that ship has not enough fuel
        /// </summary>
        private void NotifyNotEnoughFuel()
        {
            MessageSystem.Message message = new MessageSystem.Message(
                Localizer.Format("#LOC_BV_Title_ShipStopped"), // title
                "<color=#74B4E2>" + vessel.vesselName + "</color> " + Localizer.Format("#LOC_BV_Warning_Stopped") + ". " + Localizer.Format("#LOC_BV_Warning_NotEnoughFuel") + ".\n<color=#AED6EE>", // message
                MessageSystemButton.MessageButtonColor.RED,
                MessageSystemButton.ButtonIcons.ALERT
            );
            MessageSystem.Instance.AddMessage(message);
        }


        /// <summary>
        /// Return status of throttle
        /// </summary>
        /// <returns></returns>
        internal bool GetThrottle100()
        {
            return throttle == 100;
        }
        internal bool GetThrottle75()
        {
            return throttle == 75;
        }
        internal bool GetThrottle50()
        {
            return throttle == 50;
        }
        internal bool GetThrottle25()
        {
            return throttle == 25;
        }


        /// <summary>
        /// Set throttle level
        /// </summary>
        /// <param name="value"></param>
        internal void UseThrottle100(bool value)
        {
            throttle = 100;
        }
        internal void UseThrottle75(bool value)
        {
            throttle = 75;
        }
        internal void UseThrottle50(bool value)
        {
            throttle = 50;
        }
        internal void UseThrottle25(bool value)
        {
            throttle = 25;
        }

    }
}
