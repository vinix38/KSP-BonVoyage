using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using KSP.IO;
using UI;
using KSP.Localization;
using ToolbarWrapper;
using UnityEngine.SceneManagement;

namespace BonVoyage
{
    /// <summary>
    /// Mod's main class
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class BonVoyage : MonoBehaviour
    {
        #region Public properties

        public static BonVoyage Instance; // Mod's instance
        public const string Name = "BonVoyage"; // Name of the mod

        public MainWindowModel MainModel; // Main view's model
        public SettingsWindowModel SettingsModel; // Settings view's model
        public ControlWindowModel ControlModel; // Control view's model
        public bool ControlViewVisible { get { return controlViewVisible; } }

        public List<BVController> BVControllers; // Controllers list

        #endregion


        #region Private properties

        private ApplicationLauncherButton appLauncherButton; // Button in the game's toolbar
        private IButton toolbarButton; // Toolbar Continued button

        private bool gamePaused; // Is game paused?
        private bool showUI; // Is UI vissible? (F2 pressed)

        private bool mainViewVisible; // Is main view visible?
        private MainWindowView MainView { get; set; } // Mod's main view

        private bool setttingsViewVisible; // Is settings view visible?
        private SettingsWindowView SettingsView { get; set; } // Mod's settings view

        private bool controlViewVisible; // Is control view visible?
        private ControlWindowView ControlView { get; set; } // Mod's control view

        #endregion


        /// <summary>
        /// Plugin constructor
        /// </summary>
        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            CommonWindowProperties.ActiveSkin = UISkinManager.defaultSkin;
            CommonWindowProperties.UnitySkin = null;
            CommonWindowProperties.RefreshStyles();

            MainView = null;
            MainModel = null;
            mainViewVisible = false;
            SettingsView = null;
            SettingsModel = null;
            setttingsViewVisible = false;
            ControlView = null;
            ControlModel = null;
            controlViewVisible = false;

            toolbarButton = null;

            gamePaused = false;
            showUI = true;

            BVControllers = new List<BVController>();

            Configuration.Load();
        }


        /// <summary>
        /// Initial start
        /// </summary>
        public void Start()
        {
            DontDestroyOnLoad(this);
            GameEvents.onGUIApplicationLauncherReady.Add(AddLauncher);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveLauncher);
            GameEvents.onGameSceneSwitchRequested.Add(OnGameSceneSwitchRequested);
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
            GameEvents.onHideUI.Add(OnHideUI);
            GameEvents.onShowUI.Add(OnShowUI);
            GameEvents.onGamePause.Add(OnGamePause);
            GameEvents.onGameUnpause.Add(OnGameUnpause);


            // test
            //GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested); // GameScenes
            //GameEvents.onVesselGoOffRails.Add(onVesselGoOffRails); // Vessel
            //GameEvents.onVesselLoaded.Add(onVesselLoaded); // Vessel
            // test

            LoadControllers();
        }


        /// <summary>
        /// Clean up
        /// </summary>
        public void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncher);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(RemoveLauncher);
            GameEvents.onGameSceneSwitchRequested.Add(OnGameSceneSwitchRequested);
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
            GameEvents.onHideUI.Remove(OnHideUI);
            GameEvents.onShowUI.Remove(OnShowUI);
            GameEvents.onGamePause.Remove(OnGamePause);
            GameEvents.onGameUnpause.Remove(OnGameUnpause);

            // test
            //GameEvents.onGameSceneLoadRequested.Remove(onGameSceneLoadRequested); // GameScenes
            //GameEvents.onVesselGoOffRails.Remove(onVesselGoOffRails); // Vessel
            //GameEvents.onVesselLoaded.Remove(onVesselLoaded); // Vessel
            // test

            RemoveLauncher();

            Configuration.Save();
        }


        // test
        public void onGameSceneLoadRequested(GameScenes scenes)
        {
            Debug.LogWarning("BV: onGameSceneLoadRequested");
        }
        public void onVesselGoOffRails(Vessel vessel)
        {
            Debug.LogWarning("BV: onVesselGoOffRails");
        }
        public void onVesselLoaded(Vessel vessel)
        {
            Debug.LogWarning("BV: onVesselLoaded");
        }
        // test


        #region Game events

        /// <summary>
        /// Get Unity skin - only accessible in OnGUI
        /// </summary>
        void OnGUI()
        {
            if (CommonWindowProperties.UnitySkin == null)
            {
                CommonWindowProperties.UnitySkin = StyleConverter.Convert(GUI.skin);

                // Load Unity skin, if it was saved in the config
                if (Configuration.Skin == 1)
                {
                    CommonWindowProperties.ActiveSkin = CommonWindowProperties.UnitySkin;
                    CommonWindowProperties.RefreshStyles();
                }
            }
        }


        /// <summary>
        /// Game paused
        /// </summary>
        public void OnGamePause()
        {
            gamePaused = true;
        }


        /// <summary>
        /// Game unpaused
        /// </summary>
        public void OnGameUnpause()
        {
            gamePaused = false;
        }


        /// <summary>
        /// Show UI
        /// </summary>
        private void OnShowUI()
        {
            showUI = true;
        }


        /// <summary>
        /// Hide UI
        /// </summary>
        private void OnHideUI()
        {
            showUI = false;
        }


        /// <summary>
        /// Change UI state to hidden to prevent app button weirdness (PopupDialog is automatically hidden on scene change)
        /// </summary>
        /// <param name="ev"></param>
        public void OnGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> ev)
        {
            if (appLauncherButton != null)
                appLauncherButton.SetFalse(true);
            else
                mainViewVisible = false;
            if (controlViewVisible)
                ToggleControlWindow();
        }


        /// <summary>
        /// Vessel changed. Refresh vessel list in the main window if it's still visible and hide control window (scene was not switched)
        /// </summary>
        /// <param name="vessel"></param>
        public void OnVesselChange(Vessel vessel)
        {
            if (MainView != null)
                MainModel.RefreshVesselListLayout();
            if (controlViewVisible)
                ToggleControlWindow();
        }


        /// <summary>
        /// Reload list of controllers when new scene is loaded
        /// Deprecated?
        /// </summary>
        /// <param name="scenes"></param>
        public void OnLevelWasLoaded(GameScenes scene)
        {
            if ((scene == GameScenes.FLIGHT) || (scene == GameScenes.SPACECENTER) || (scene == GameScenes.TRACKSTATION))
            {
                LoadControllers();
            }
        }

        #endregion


        #region App launcher

        /// <summary>
        /// Launcher is ready to be initialized
        /// </summary>
        public void AddLauncher()
        {
            CreatePluginButton();
        }


        /// <summary>
        /// Create buttons for the plugin
        /// </summary>
        private void CreatePluginButton()
        {
            // Create button in the KSP toolbar if the check is selected or Toolbar Continued is unavailable
            if ((appLauncherButton == null) && (Configuration.KSPToolbar || !ToolbarManager.ToolbarAvailable))
            {
                appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
                    OnAppLaunchToggleOn,
                    OnAppLaunchToggleOff,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.SPACECENTER |
                    ApplicationLauncher.AppScenes.TRACKSTATION |
                    ApplicationLauncher.AppScenes.FLIGHT |
                    ApplicationLauncher.AppScenes.MAPVIEW,
                    GameDatabase.Instance.GetTexture(Tools.TextureFilePath("bon-voyage-icon"), false)
                );

                appLauncherButton.gameObject.SetTooltip(Localizer.Format("#LOC_BV_Title"), Localizer.Format("#LOC_BV_Tooltip"));
                appLauncherButton.onRightClick = OnRightClick;
            }

            // Create button in the Toolbar Continued
            if ((toolbarButton == null) && Configuration.ToolbarContinued && ToolbarManager.ToolbarAvailable)
            {
                toolbarButton = ToolbarManager.Instance.add(Name, "AppLauncherButton");
                toolbarButton.TexturePath = Tools.TextureFilePath("bon-voyage-icon-toolbar");
                toolbarButton.ToolTip = Localizer.Format("#LOC_BV_Title") + "\n" + Localizer.Format("#LOC_BV_Tooltip");
                toolbarButton.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER, GameScenes.TRACKSTATION, GameScenes.FLIGHT);
                toolbarButton.Visible = true;
                toolbarButton.OnClick += (ClickEvent e) => { OnTCClick(e); };
            }
        }


        /// <summary>
        /// Destroy app launcher button
        /// </summary>
        public void RemoveAppLauncherButton()
        {
            if (appLauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
                appLauncherButton = null;
            }
        }


        /// <summary>
        /// Destroy Toolbar Continued button
        /// </summary>
        public void RemoveToolbarContinuedButton()
        {
            if (toolbarButton != null)
            {
                toolbarButton.Destroy();
                toolbarButton = null;
            }
        }


        /// <summary>
        /// Destroy buttons for the plugin
        /// </summary>
        public void RemoveLauncher()
        {
            RemoveAppLauncherButton();

            RemoveToolbarContinuedButton();
        }


        /// <summary>
        /// Show GUI when button is clicked first time
        /// </summary>
        public void OnAppLaunchToggleOn()
        {
            mainViewVisible = true;
        }


        /// <summary>
        /// Hide GUI when button is clicked second time
        /// </summary>
        public void OnAppLaunchToggleOff()
        {
            mainViewVisible = false;
        }


        /// <summary>
        /// Right click on the application button
        /// </summary>
        public void OnRightClick()
        {
            ToggleControlWindow();
        }


        /// <summary>
        /// Click on the application button in Toolbar Continued
        /// </summary>
        public void OnTCClick(ClickEvent e)
        {
            if (e.MouseButton == 1)
                ToggleControlWindow();
            else
            {
                if (appLauncherButton != null) // If KSP app launcher button is created, then use his events
                {
                    if (mainViewVisible) // visible? -> hide
                        appLauncherButton.SetFalse(true);
                    else // show
                        appLauncherButton.SetTrue(true);
                }
                else
                    mainViewVisible = !mainViewVisible;
            }
        }

        #endregion


        #region Main View

        /// <summary>
        /// Show main window dialog
        /// </summary>
        private void ShowMainWindow()
        {
            if (MainView == null)
            {
                if (MainModel == null) // Create model for the Main View
                    MainModel = new MainWindowModel();
                
                MainView = new MainWindowView(MainModel, ToggleSettingsWindow, ToggleControlWindow, () => { appLauncherButton.SetFalse(true); });
                MainView.Show();
            }
        }


        /// <summary>
        /// Hide main window dialog
        /// </summary>
        private void HideMainWindow()
        {
            if (MainView != null)
            {
                if (setttingsViewVisible) // hide settings, when hiding main view
                    ToggleSettingsWindow();

                MainView.Dismiss();
                MainView = null;
            }
        }

        #endregion


        #region Settings view

        /// <summary>
        /// Toggle state of the settings window dialog
        /// </summary>
        private void ToggleSettingsWindow()
        {
            setttingsViewVisible = !setttingsViewVisible;

            if (setttingsViewVisible)
            {
                if (SettingsView == null)
                    ShowSettingsWindow();
            }
            else
            {
                if (SettingsView != null)
                    HideSettingsWindow();
            }
        }


        /// <summary>
        /// Show settings window dialog
        /// </summary>
        private void ShowSettingsWindow()
        {
            if (SettingsView == null)
            {
                if (SettingsModel == null) // Create model for the Settings View
                    SettingsModel = new SettingsWindowModel();
                
                SettingsView = new SettingsWindowView(SettingsModel, MainView.GetWindowPosition(), ToggleSettingsWindow);
                SettingsView.Show();
            }
        }

        /// <summary>
        /// Hide settings window dialog
        /// </summary>
        private void HideSettingsWindow()
        {
            if (SettingsView != null)
            {
                SettingsView.Dismiss();
                SettingsView = null;
            }
        }

        #endregion


        #region Control view

        /// <summary>
        /// Toggle state of the control window dialog
        /// </summary>
        public void ToggleControlWindow()
        {
            controlViewVisible = !controlViewVisible;

            if (controlViewVisible)
            {
                // Check if we are in flight and active vessel has BV controller and is not shutted down
                bool active = false;
                if (HighLogic.LoadedSceneIsFlight)
                {
                    Vessel vessel = FlightGlobals.ActiveVessel;
                    active = CheckActiveControllerForVessel(FlightGlobals.ActiveVessel);
                }

                if (active && (ControlView == null))
                    ShowControlWindow();
                else
                    controlViewVisible = false;
            }
            else
            {
                if (ControlView != null)
                    HideControlWindow();
            }
        }


        /// <summary>
        /// Show control window dialog
        /// </summary>
        private void ShowControlWindow()
        {
            if (ControlView == null)
            {
                if (ControlModel == null) // Create model for the Settings View
                    ControlModel = new ControlWindowModel();

                ControlView = new ControlWindowView(ControlModel, ToggleControlWindow);
                ControlView.Show();
            }
        }

        /// <summary>
        /// Hide control window dialog
        /// </summary>
        private void HideControlWindow()
        {
            if (ControlView != null)
            {
                ControlView.Dismiss();
                ControlView = null;
            }
        }

        #endregion


        /// <summary>
        /// Show/Hide windows during Update
        /// </summary>
        public void Update()
        {
            // Escape was pressed -> close opened windows (set launcher state to false, so next time a window will be opened)
            if (Input.GetKeyDown(KeyCode.Escape) && (MainView != null))
            {
                appLauncherButton.SetFalse(true);
            }

            // Main window
            if (mainViewVisible)
            {
                if (MainView == null)
                    ShowMainWindow();
            }
            else
            {
                if (MainView != null)
                    HideMainWindow();
            }
        }


        /// <summary>
        /// Reset windows when skin was changed
        /// </summary>
        public void ResetWindows()
        {
            bool settings = setttingsViewVisible; // Settings are closed by HideMainWindow automatically, so we need to store the value
            if (MainView != null)
            {
                HideMainWindow();
                ShowMainWindow();
            }
            if (settings)
                ToggleSettingsWindow();
            if (ControlView != null)
            {
                ToggleControlWindow(); // Close
                ToggleControlWindow(); // Open
            }
        }


        /// <summary>
        /// Set value of shutdown in a controller and refresh the main view after a change in the BonVoyageModule
        /// </summary>
        public void SetShutdownState(Guid vesselId, bool value)
        {
            for (int i = 0; i < BVControllers.Count; i++)
            {
                if (BVControllers[i].vessel.id == vesselId)
                {
                    BVControllers[i].Shutdown = value;
                    break;
                }
            }
            if (MainView != null)
                MainModel.RefreshVesselListLayout();
            if ((ControlView != null) && value)
                ToggleControlWindow();
        }


        /// <summary>
        /// Load BV controllers from the config
        /// </summary>
        public void LoadControllers()
        {
            Vessel vessel = null;
            ProtoPartSnapshot part = null;

            BVControllers.Clear();

            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                vessel = FlightGlobals.Vessels[i];
                ConfigNode vesselConfigNode = new ConfigNode();
                vessel.protoVessel.Save(vesselConfigNode);

                for (int k = 0; k < vessel.protoVessel.protoPartSnapshots.Count; k++)
                {
                    part = vessel.protoVessel.protoPartSnapshots[k];
                    ProtoPartModuleSnapshot module = part.FindModule("BonVoyageModule");
                    if (module != null)
                    {
                        ConfigNode BVModule = module.moduleValues;
                        string vesselType = BVModule.GetValue("vesselType");
                        if (vessel.isActiveVessel)
                            vesselType = vessel.FindPartModuleImplementing<BonVoyageModule>().vesselType;
                        BVController controller = null;
                        switch (vesselType)
                        {
                            case "0": // rover
                                controller = new RoverController(vessel, BVModule);
                                break;
                            case "1": // ship
                                controller = new ShipController(vessel, BVModule);
                                break;
                            default: // default to rover
                                controller = new RoverController(vessel, BVModule);
                                break;
                        }
                        BVControllers.Add(controller);
                    }
                }
            }
        }


        /// <summary>
        /// Check if vessel has controller and is active
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool CheckActiveControllerForVessel(Vessel v)
        {
            bool active = false;
            if (HighLogic.LoadedSceneIsFlight)
            {
                for (int i = 0; i < BVControllers.Count; i++)
                {
                    if (BVControllers[i].vessel.id == v.id)
                    {
                        if (!BVControllers[i].Shutdown)
                            active = true;
                        break;
                    }
                }
            }
            return active;
        }

    }

}
