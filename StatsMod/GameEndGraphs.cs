using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace StatsMod
{
    // base code shamelessly taken from restartbutton mod
    public class StatsButtonController : MonoBehaviour
    {

        // idk if this should be touched
        static bool shouldShowOnReportScreen(RunReport runReport)
        {
            // Eclipse and Prismatics for some reason just immediately disconnect on run end, no matter what.
            // This is fine for singleplayer since we can just override the behavior, but there's nothing we can do for clients in multiplayer
            if (!NetworkServer.dontListen)
            {
                switch (Run.instance)
                {
                    case EclipseRun:
                    case WeeklyRun:
                        return false;
                }
            }

            if (runReport is null)
                return false;

            if (!runReport.gameEnding || runReport.gameEnding.isWin)
                return false;

            return true;
        }

        Transform _statsButtonInstance;

        public bool IsVisible
        {
            get
            {
                return _statsButtonInstance && _statsButtonInstance.gameObject.activeSelf;
            }
            set
            {
                if (IsVisible == value)
                    return;

                if (!_statsButtonInstance && value)
                {
                    RoR2.UI.MPButton continueButton = _gameEndPanelController.continueButton;
                    if (continueButton)
                    {
                        GameObject statsButton = Instantiate(continueButton.gameObject, continueButton.transform.parent); // creating a copy of the continue button
                        _gameEndPanelController.continueButton.transform.SetAsLastSibling(); // moving the continue button back to the front
                        statsButton.name = "StatsButton";

                        RoR2.UI.HGButton button = statsButton.GetComponent<RoR2.UI.HGButton>(); // getting the actual button component..
                        button.onClick.RemoveAllListeners(); // ..and removing all listeners
                        button.onClick.AddListener(onGraphsClicked); // replacing functionality of the button

                        // changing text in the button
                        RoR2.UI.LanguageTextMeshController labelText = statsButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
                        if (labelText)
                        {
                            labelText.token = "Graphs";
                        }

                        // this seems to stop [NO GAMEPAD BINDING] from appearing
                        Transform glyphTransform = statsButton.transform.Find("GenericGlyph");
                        if (glyphTransform)
                        {
                            glyphTransform.gameObject.SetActive(false);
                        }

                        _statsButtonInstance = statsButton.transform;
                    }
                }
                else
                {
                    _statsButtonInstance.gameObject.SetActive(value);
                }
            }
        }

        /*
        potentially useful for creating graph window
        - HGPopoutPanel
        - RuleCategoryController
        */


        RoR2.UI.GameEndReportPanelController _gameEndPanelController;

        void onGraphsClicked()
        {
            Log.Info("Die");
        }

        // probably not to touch
        void Awake()
        {
            _gameEndPanelController = GetComponent<RoR2.UI.GameEndReportPanelController>();
        }

        // and this too
        public void OnSetDisplayData(RoR2.UI.GameEndReportPanelController.DisplayData newDisplayData)
        {
            IsVisible = NetworkServer.active && shouldShowOnReportScreen(newDisplayData.runReport);
        }

        // putting the button in the end screen
        static void GameEndReportPanelController_Awake(On.RoR2.UI.GameEndReportPanelController.orig_Awake orig, RoR2.UI.GameEndReportPanelController self)
        {
            orig(self);
            self.gameObject.AddComponent<StatsButtonController>();
        }

        // idk but it's probably important to keep things consistent
        static void GameEndReportPanelController_SetDisplayData(On.RoR2.UI.GameEndReportPanelController.orig_SetDisplayData orig, RoR2.UI.GameEndReportPanelController self, RoR2.UI.GameEndReportPanelController.DisplayData newDisplayData)
        {
            orig(self, newDisplayData);

            if (self.TryGetComponent(out StatsButtonController respawnButtonController))
            {
                respawnButtonController.OnSetDisplayData(newDisplayData);
            }
        }

        public static void Init()
        {
            On.RoR2.UI.GameEndReportPanelController.Awake += StatsButtonController.GameEndReportPanelController_Awake;
            On.RoR2.UI.GameEndReportPanelController.SetDisplayData += StatsButtonController.GameEndReportPanelController_SetDisplayData;
        }
    }
}
