using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BonVoyage
{
    /// <summary>
    /// Configuration of the mod
    /// </summary>
    public static class Configuration
    {
        private static PluginConfiguration configuration; // Configuration data


        #region Public properties

        /// <summary>
        /// UI skin
        /// </summary>
        public static byte Skin
        {
            get { return configuration.GetValue<byte>("skin", 0); }
            set
            {
                configuration.SetValue("skin", value);
                configuration.save();
            }
        }


        /// <summary>
        /// Automatic dewarp
        /// </summary>
        public static bool AutomaticDewarp
        {
            get { return configuration.GetValue<bool>("dewarp", false); }
            set
            {
                configuration.SetValue("dewarp", value);
                configuration.save();
            }
        }


        /// <summary>
        /// Use KSP toolbar
        /// </summary>
        public static bool KSPToolbar
        {
            get { return configuration.GetValue<bool>("kspToolbar", true); }
            set
            {
                configuration.SetValue("kspToolbar", value);
                configuration.save();
            }
        }


        /// <summary>
        /// Use Toolbar Continued
        /// </summary>
        public static bool ToolbarContinued
        {
            get { return configuration.GetValue<bool>("toolbarContinued", false); }
            set
            {
                configuration.SetValue("toolbarContinued", value);
                configuration.save();
            }
        }


        /// <summary>
        /// Main window position
        /// </summary>
        public static Vector2 MainWindowPosition
        {
            get { return configuration.GetValue<Vector2>("mainWindow", new Vector2(0.5f, 0.5f)); }
            set
            {
                configuration.SetValue("mainWindow", value);
                configuration.save();
            }
        }


        /// <summary>
        /// Control window position
        /// </summary>
        public static Vector2 ControlWindowPosition
        {
            get { return configuration.GetValue<Vector2>("controlWindow", new Vector2(0.5f, 0.5f)); }
            set
            {
                configuration.SetValue("controlWindow", value);
                configuration.save();
            }
        }


        /// <summary>
        /// Active vessels
        /// </summary>
        public static bool ActiveControllers
        {
            get { return configuration.GetValue<bool>("activeControllers", true); }
            set
            {
                configuration.SetValue("activeControllers", value);
                configuration.save();
            }
        }


        /// <summary>
        /// Disabled vessels
        /// </summary>
        public static bool DisabledControllers
        {
            get { return configuration.GetValue<bool>("disabledControllers", false); }
            set
            {
                configuration.SetValue("disabledControllers", value);
                configuration.save();
            }
        }

        #endregion


        /// <summary>
        /// Load configuration from file
        /// </summary>
        public static void Load()
        {
            configuration = PluginConfiguration.CreateForType<BonVoyage>();
            configuration.load();
        }


        /// <summary>
        /// Save configuration to file
        /// </summary>
        public static void Save()
        {
            configuration.save();
        }

    }

}
