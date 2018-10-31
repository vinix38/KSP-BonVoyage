using KSP.Localization;
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
        #region Public properties

        public override double AverageSpeed { get { return ((angle <= 90) ? (averageSpeed * speedMultiplier) : (averageSpeedAtNight * speedMultiplier)); } }

        #endregion


        #region Private properties

        // Config values
        private double averageSpeed = 0;
        private double averageSpeedAtNight = 0;
        private bool solarPowered = false;
        private bool manned = false;
        private double vesselHeightFromTerrain = 0;
        // Config values

        private double speedMultiplier;
        private double angle; // Angle between the main body and the main sun
        private double electricPower_Solar; // Electric power from solar panels
        private double electricPower_Other; // Electric power from other power sources
        private double requiredPower; // Power required by wheels
        private double maxSpeedBase; // maximum speed without modifiers
        private int wheelsPercentualModifier; // Speed modifier based on wheels
        private int crewSpeedBonus; // Speed modifier based on the available crew
        WheelTestResult wheelTestResult = new WheelTestResult();

        // Reduction of speed based on difference between required and available power in percents
        private double SpeedReduction
        {
            get
            {
                double speedReduction = 0;
                if (requiredPower > (electricPower_Solar + electricPower_Other))
                    speedReduction = (requiredPower - (electricPower_Solar + electricPower_Other)) / requiredPower * 100;
                return speedReduction;
            }
        }

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

            speedMultiplier = 1.0;
            angle = 0;
            electricPower_Solar = 0;
            electricPower_Other = 0;
            requiredPower = 0;
            maxSpeedBase = 0;
            wheelsPercentualModifier = 0;
            crewSpeedBonus = 0;
        }


        /// <summary>
        /// Get controller type
        /// </summary>
        /// <returns></returns>
        public override int GetControllerType()
        {
            return 0;
        }


        #region Status window texts

        public override List<DisplayedSystemCheckResult> GetDisplayedSystemCheckResults()
        {
            base.GetDisplayedSystemCheckResults();

            DisplayedSystemCheckResult result = new DisplayedSystemCheckResult
            {
                Label = Localizer.Format("#LOC_BV_Control_AverageSpeed"),
                Text = averageSpeed.ToString("F") + " m/s",
                Tooltip = 
                    averageSpeed > 0
                    ?
                    Localizer.Format("#LOC_BV_Control_SpeedBase") + ": " + maxSpeedBase.ToString("F") + " m/s\n"
                        + Localizer.Format("#LOC_BV_Control_WheelsModifier") + ": " + wheelsPercentualModifier.ToString("F") + "%\n"
                        + (manned ? Localizer.Format("#LOC_BV_Control_DriverBonus") + ": " + crewSpeedBonus.ToString() : Localizer.Format("#LOC_BV_Control_UnmannedPenalty") + ": 80%\n")
                        + (SpeedReduction > 0 ? Localizer.Format("#LOC_BV_Control_PowerPenalty") + ": " + (SpeedReduction > 75 ? "100" : SpeedReduction.ToString("F")) + "%\n" : "")
                        + Localizer.Format("#LOC_BV_Control_SpeedAtNight") + ": " + averageSpeedAtNight.ToString("F") + " m/s"
                    :
                    Localizer.Format("#LOC_BV_Control_WheelsNotOnline")
            };
            displayedSystemCheckResults.Add(result);

            result = new DisplayedSystemCheckResult
            {
                Label = Localizer.Format("#LOC_BV_Control_GeneratedPower"),
                Text = (electricPower_Solar + electricPower_Other).ToString("F"),
                Tooltip = Localizer.Format("#LOC_BV_Control_SolarPower") + ": " + electricPower_Solar.ToString("F") + "\n" + Localizer.Format("#LOC_BV_Control_GeneratorPower") + ": " + electricPower_Other.ToString("F")
            };
            displayedSystemCheckResults.Add(result);

            result = new DisplayedSystemCheckResult
            {
                Label = Localizer.Format("#LOC_BV_Control_RequiredPower"),
                Text = requiredPower.ToString("F") 
                    + (SpeedReduction == 0 ? "" : 
                        (((SpeedReduction > 0) && (SpeedReduction <= 75)) 
                            ? " (" + Localizer.Format("#LOC_BV_Control_PowerReduced") + " " + SpeedReduction.ToString("F") + "%)" 
                            : " (" + Localizer.Format("#LOC_BV_Control_NotEnoughPower") + ")")),
                Tooltip = ""
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
        public override bool FindRoute(double lat, double lon)
        {
            return FindRoute(lat, lon, TileTypes.Land);
        }

        #endregion


        /// <summary>
        /// Check the systems
        /// </summary>
        public override void SystemCheck()
        {
            // Test stock wheels
            WheelTestResult testResultStockWheels = CheckStockWheels();
            // Test KSPWheels
            WheelTestResult testResultKSPkWheels = CheckKSPWheels();
            // Sum it
            wheelTestResult.powerRequired = testResultStockWheels.powerRequired + testResultKSPkWheels.powerRequired;
            wheelTestResult.maxSpeedSum = testResultStockWheels.maxSpeedSum + testResultKSPkWheels.maxSpeedSum;
            wheelTestResult.inTheAir = testResultStockWheels.inTheAir + testResultKSPkWheels.inTheAir;
            wheelTestResult.operable = testResultStockWheels.operable + testResultKSPkWheels.operable;
            wheelTestResult.damaged = testResultStockWheels.damaged + testResultKSPkWheels.damaged;
            wheelTestResult.online = testResultStockWheels.online + testResultKSPkWheels.online;
            wheelTestResult.maxWheelRadius = testResultStockWheels.maxWheelRadius + testResultKSPkWheels.maxWheelRadius;

            // Generally, moving at high speed requires less power than wheels' max consumption. Maximum required power of controller will 35% of wheels power requirement 
            requiredPower = wheelTestResult.powerRequired / 100 * 35;
        }


        #region Wheels

        /// <summary>
        /// Result of the test of wheels
        /// </summary>
        private struct WheelTestResult
        {
            public double powerRequired; // Total power required
            public double maxSpeedSum; // Sum of max speeds of all online wheels
            public int inTheAir; // Count of wheels in the air
            public int operable; // Count of operable wheels
            public int damaged; // Count of damaged wheels
            public int online; // Count of online wheels
            public float maxWheelRadius; // Maximum of radii of all aplicable wheels

            public WheelTestResult(double powerRequired, double maxSpeedSum, int inTheAir, int operable, int damaged, int online, float maxWheelRadius)
            {
                this.powerRequired = powerRequired;
                this.maxSpeedSum = maxSpeedSum;
                this.inTheAir = inTheAir;
                this.operable = operable;
                this.damaged = damaged;
                this.online = online;
                this.maxWheelRadius = maxWheelRadius;
            }
        }


        /// <summary>
        /// Test stock wheels implementing standard module ModuleWheelBase
        /// </summary>
        /// <returns></returns>
        private WheelTestResult CheckStockWheels()
        {
            double powerRequired = 0;
            double maxSpeedSum = 0;
            int inTheAir = 0;
            int operable = 0;
            int damaged = 0;
            int online = 0;
            float maxWheelRadius = 0;

            // Get wheel modules
            List<Part> wheels = new List<Part>();
            for (int i = 0; i < vessel.parts.Count; i++)
            {
                var part = vessel.parts[i];
                if (part.Modules.Contains("ModuleWheelBase"))
                    wheels.Add(part);
            }

            for (int i = 0; i < wheels.Count; i++)
            {
                ModuleWheelBase wheelBase = wheels[i].FindModuleImplementing<ModuleWheelBase>();

                // Skip legs
                if (wheelBase.wheelType == WheelType.LEG)
                    continue;

                // Save max wheel radius for height compensations
                if (wheelBase.radius < maxWheelRadius)
                    maxWheelRadius = wheelBase.radius;

                // Check damaged wheels
                ModuleWheels.ModuleWheelDamage wheelDamage = wheels[i].FindModuleImplementing<ModuleWheels.ModuleWheelDamage>();
                if (wheelDamage != null)
                {
                    if (wheelDamage.isDamaged)
                    {
                        damaged++;
                        continue;
                    }
                }

                // Whether or not wheel is touching the ground
                if (!wheelBase.isGrounded)
                {
                    inTheAir++;
                    continue;
                }
                else
                    operable++;

                // Check motorized wheels
                ModuleWheels.ModuleWheelMotor wheelMotor = wheels[i].FindModuleImplementing<ModuleWheels.ModuleWheelMotor>();
                if (wheelMotor != null)
                {
                    // Wheel is on
                    if (wheelMotor.motorEnabled)
                    {
                        powerRequired += wheelMotor.avgResRate;
                        online++;
                        double maxWheelSpeed = 0;
                        UnityEngine.Debug.LogWarning("maxWheelSpeed = " + wheelMotor.wheelSpeedMax);
                        // RoveMax M1 and RoveMax M1-F (Making History expansion) don't have max wheels speed defined, so we just set it to something sensible
                        if ((wheelMotor.part.name == "roverWheel1") || (wheelMotor.part.name == "roverWheelM1-F "))
                            maxWheelSpeed = 42;
                        else
                            maxWheelSpeed = wheelMotor.wheelSpeedMax;
                        maxSpeedSum += maxWheelSpeed;
                    }
                }
            }

            return new WheelTestResult(powerRequired, maxSpeedSum, inTheAir, operable, damaged, online, maxWheelRadius);
        }


        /// <summary>
        /// Test KSPWheels implementing module KSPWheelBase
        /// </summary>
        /// <returns></returns>
        private WheelTestResult CheckKSPWheels()
        {
            double powerRequired = 0;
            double maxSpeedSum = 0;
            int inTheAir = 0;
            int operable = 0;
            int damaged = 0;
            int online = 0;
            float maxWheelRadius = 0;

            // Get wheel modules
            List<Part> wheels = new List<Part>();
            for (int i = 0; i < vessel.parts.Count; i++)
            {
                var part = vessel.parts[i];
                if (part.Modules.Contains("KSPWheelBase"))
                    wheels.Add(part);
            }

            for (int i = 0; i < wheels.Count; i++)
            {
                List<PartModule> partModules = wheels[i].Modules.GetModules<PartModule>();
                PartModule wheelBase = partModules.Find(t => t.moduleName == "KSPWheelBase");

                // Save max wheel radius for height compensations
                float radius = (float)wheelBase.Fields.GetValue("wheelRadius");
                if (radius < maxWheelRadius)
                    maxWheelRadius = radius;

                // Check damaged wheels
                if (wheelBase.Fields.GetValue("persistentState").ToString() == "BROKEN")
                {
                    damaged++;
                    continue;
                }

                // Whether or not wheel is touching the ground
                PartModule wheelDamage = partModules.Find(t => t.moduleName == "KSPWheelDamage");
                float maxSafeSpeed = 0f; // We compare this value later with maxDrivenSpeed to eliminate unreal gear ratios
                if (wheelDamage != null)
                {
                    if ((float)wheelDamage.Fields.GetValue("loadStress") == 0)
                    {
                        inTheAir++;
                        continue;
                    }
                    else
                    {
                        operable++;
                        maxSafeSpeed = (float)wheelDamage.Fields.GetValue("maxSafeSpeed");
                    }
                }

                // Check motorized wheels
                PartModule wheelMotor = partModules.Find(t => t.moduleName == "KSPWheelMotor");
                if (wheelMotor != null)
                {
                    // Wheel is on
                    if (!(bool)wheelMotor.Fields.GetValue("motorLocked"))
                    {
                        online++;
                        powerRequired += (float)wheelMotor.Fields.GetValue("maxECDraw") * (float)wheelMotor.Fields.GetValue("motorOutput") / 100; // Motor output can be limited by a slider
                        if (maxSafeSpeed > 0)
                            maxSpeedSum += Math.Min((float)wheelMotor.Fields.GetValue("maxDrivenSpeed"), maxSafeSpeed);
                        else
                            maxSpeedSum += (float)wheelMotor.Fields.GetValue("maxDrivenSpeed");
                    }
                }

                // Check tracks
                PartModule wheelTracks = partModules.Find(t => t.moduleName == "KSPWheelTracks");
                if (wheelTracks != null)
                {
                    operable++; // We will count them as two wheels, so add another operable wheel
                    if (!(bool)wheelTracks.Fields.GetValue("motorLocked"))
                    {
                        online += 2;
                        powerRequired += (float)wheelTracks.Fields.GetValue("maxECDraw") * (float)wheelMotor.Fields.GetValue("motorOutput") / 100; // Motor output can be limited by a slider
                        if (maxSafeSpeed > 0)
                            maxSpeedSum += 2 * Math.Min((float)wheelMotor.Fields.GetValue("maxDrivenSpeed"), maxSafeSpeed);
                        else
                            maxSpeedSum += 2 * (float)wheelMotor.Fields.GetValue("maxDrivenSpeed");
                    }
                }
            }

            return new WheelTestResult(powerRequired, maxSpeedSum, inTheAir, operable, damaged, online, maxWheelRadius);
        }

        #endregion


        #region Power
        #endregion

    }

}
