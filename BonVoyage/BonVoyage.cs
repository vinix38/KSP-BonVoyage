using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using KSP.IO;
using UI;
using KSP.Localization;

namespace BonVoyage
{
    /// <summary>
    /// Main mod's class
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class BonVoyage : MonoBehaviour
    {
        #region Public properties

        public static BonVoyage Instance; // Mod's instance

        public MainWindowModel MainModel; // Main view's model
        public SettingsWindowModel SettingsModel; // Main view's model

        public const string Name = "BonVoyage"; // Name of the mod

        #endregion


        #region Private properties

        private ApplicationLauncherButton appLauncherButton; // Button in the game's toolbar
        private bool gamePaused; // Is game paused?
        private bool showUI; // Is UI vissible? (F2 pressed)

        private MainWindowView MainView { get; set; } // Mod's main view
        private bool showMainView;
        private bool hideMainView;

        private bool setttingsViewVisible; // Is main view visible?
        private SettingsWindowView SettingsView { get; set; } // Mod's main view

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

            MainView = null;
            MainModel = null;
            SettingsView = null;
            SettingsModel = null;

            gamePaused = false;
            showUI = true;
            showMainView = false;
            hideMainView = false;
            setttingsViewVisible = false;

            Configuration.Load();
            CommonWindowProperties.RefreshStyles();
        }


        /// <summary>
        /// Initial start
        /// </summary>
        public void Start()
        {
            DontDestroyOnLoad(this);
            GameEvents.onGUIApplicationLauncherReady.Add(AddLauncher);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveLauncher);
            GameEvents.onHideUI.Add(OnHideUI);
            GameEvents.onShowUI.Add(OnShowUI);
            GameEvents.onGamePause.Add(OnGamePause);
            GameEvents.onGameUnpause.Add(OnGameUnpause);
        }


        /// <summary>
        /// Cleanup
        /// </summary>
        public void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncher);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(RemoveLauncher);
            GameEvents.onHideUI.Remove(OnHideUI);
            GameEvents.onShowUI.Remove(OnShowUI);
            GameEvents.onGamePause.Remove(OnGamePause);
            GameEvents.onGameUnpause.Remove(OnGameUnpause);

            RemoveLauncher();

            Configuration.Save();
        }


        /// <summary>
        /// Get Unity skin - only accessible in OnGUI
        /// </summary>
        void OnGUI()
        {
            if (CommonWindowProperties.UnitySkin == null)
            {
                CommonWindowProperties.UnitySkin = StyleConverter.Convert(GUI.skin);

                // Load Unity skin, if it was saved in config
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


        #region App launcher

        /// <summary>
        /// Launcher is ready to be initialized
        /// </summary>
        public void AddLauncher()
        {
            CreatePluginButton();
        }


        /// <summary>
        /// Create button for the plugin
        /// </summary>
        private void CreatePluginButton()
        {
            if (appLauncherButton == null)
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
        }


        /// <summary>
        /// Destroy button for the plugin
        /// </summary>
        public void RemoveLauncher()
        {
            if (appLauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
                appLauncherButton = null;
            }
        }


        /// <summary>
        /// Show GUI when button is clicked first time
        /// </summary>
        public void OnAppLaunchToggleOn()
        {
            showMainView = true;
        }


        /// <summary>
        /// Hide GUI when button is clicked second time
        /// </summary>
        public void OnAppLaunchToggleOff()
        {
            hideMainView = true;
        }


        /// <summary>
        /// Roght click on the application button
        /// </summary>
        public void OnRightClick()
        {
            ScreenMessages.PostScreenMessage("right click");
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
                    MainModel = new MainWindowModel(this);
                
                MainView = new MainWindowView(MainModel, ToggleSettingsWindow, () => { appLauncherButton.SetFalse(true); });
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
                {
                    ShowSettingsWindow();
                }
            }
            else
            {
                if (SettingsView != null)
                {
                    HideSettingsWindow();
                }
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
                    SettingsModel = new SettingsWindowModel(this);
                
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
            if (showMainView && (MainView == null))
            {
                showMainView = false;
                ShowMainWindow();
            }
            if (hideMainView && (MainView != null))
            {
                hideMainView = false;
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
        }

    }

}
