using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using KSP.IO;
using UI;

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

        public MainWindowModel mainModel;

        #endregion


        #region Private properties

        private ApplicationLauncherButton appLauncherButton; // Button in the game's toolbar
        private bool gamePaused; // Is game paused?
        private bool showUI; // Is UI vissible? (F2 pressed)
        private bool mainViewVisible; // Is main view visible?

        private MainWindowView MainView { get; set; } // Mod's main view

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
            mainModel = null;

            gamePaused = false;
            showUI = true;
            mainViewVisible = false;

            CommonWindowProperties.RefreshStyles();
        }


        /// <summary>
        /// Initial start
        /// </summary>
        public void Start()
        {
            DontDestroyOnLoad(this);
            GameEvents.onGUIApplicationLauncherReady.Add(AddLauncher);
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
            GameEvents.onHideUI.Remove(OnHideUI);
            GameEvents.onShowUI.Remove(OnShowUI);
            GameEvents.onGamePause.Remove(OnGamePause);
            GameEvents.onGameUnpause.Remove(OnGameUnpause);

            RemoveLauncher();
        }


        /// <summary>
        /// Get Unity skin - only accessible in OnGUI
        /// </summary>
        void OnGUI()
        {
            if (CommonWindowProperties.UnitySkin == null)
            {
                CommonWindowProperties.UnitySkin = StyleConverter.Convert(GUI.skin);
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
                    GameDatabase.Instance.GetTexture("BonVoyage/Textures/bon-voyage-icon", false)
                );
            }
        }


        /// <summary>
        /// Destroy button for the plugin
        /// </summary>
        private void RemoveLauncher()
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
            mainViewVisible = true;
        }


        /// <summary>
        /// Hide GUI when button is clicked second time
        /// </summary>
        public void OnAppLaunchToggleOff()
        {
            mainViewVisible = false;
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
                if (mainModel == null) // Create model for the Main View
                {
                    mainModel = new MainWindowModel(this);
                }
                MainView = new MainWindowView(mainModel);
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
                MainView.Dismiss();
                MainView = null;
            }
        }

        #endregion


        /// <summary>
        /// Show/Hide windows during Update
        /// </summary>
        public void Update()
        {
            // Escape was pressed -> close opened windows (set launcher state to false, so next time a window will be opened)
            if (Input.GetKeyDown(KeyCode.Escape) && mainViewVisible)
            {
                appLauncherButton.SetFalse(true);
            }

            // Main window
            if (mainViewVisible)
            {
                if (MainView == null)
                {
                    ShowMainWindow();
                }
            }
            else
            {
                if (MainView != null)
                {
                    HideMainWindow();
                }
            }

            //if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.F11))
            //{
            //    PopupDialog.SpawnPopupDialog(
            //        new Vector2(0.5f, 0.5f),
            //        new Vector2(0.5f, 0.5f),
            //        new MultiOptionDialog(
            //            "",
            //            "",
            //            "Bon Voyage",
            //            HighLogic.UISkin,
            //            new Rect(0.5f, 0.5f, 150f, 60f),
            //            new DialogGUIFlexibleSpace(),
            //            new DialogGUIVerticalLayout(
            //                new DialogGUIFlexibleSpace(),
            //                new DialogGUILabel("test"),
            //                new DialogGUIFlexibleSpace(),
            //                new DialogGUIButton("Close", () => { }, 140.0f, 30.0f, true)
            //            )
            //        ),
            //        false,
            //        HighLogic.UISkin,
            //        false
            //    );
            //}
        }

    }

}
