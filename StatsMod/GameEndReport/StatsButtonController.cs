using RoR2;
using UnityEngine;

namespace StatsMod {
    // base code shamelessly taken from restartbutton mod
    public class StatsButtonController : MonoBehaviour {

        static bool ShouldShowOnReportScreen(RunReport runReport) {

            // Will show on all gamemodes now, as it doesn't actually break anything, 
            // just might not be as cool to look at due to time between records
            /*
            switch (Run.instance) {
                case EclipseRun:
                case WeeklyRun:
                    break;
                case InfiniteTowerRun:
                    return false;
            }
            */

            if (runReport is null)
                return false;

            if (!runReport.gameEnding)
                return false;

            return true;
        }

        Transform _statsButtonInstance;

        public bool ButtonIsVisible {
            get {
                return _statsButtonInstance && _statsButtonInstance.gameObject.activeSelf;
            }
            set {
                if (ButtonIsVisible == value)
                    return;

                if (!_statsButtonInstance && value) {
                    if (StatsScreen.continueButton) {
                        GameObject statsButton = Instantiate(StatsScreen.continueButton.gameObject, StatsScreen.continueButton.transform.parent); // creating a copy of the continue button
                        StatsScreen.continueButton.transform.SetAsLastSibling(); // moving the continue button back to the front
                        statsButton.name = "StatsButton";


                        RoR2.UI.HGButton button = statsButton.GetComponent<RoR2.UI.HGButton>(); // getting the actual button component..
                        button.onClick.RemoveAllListeners(); // ..and removing all listeners
                        button.onClick.AddListener(StatsScreen.CreateStatsWindow); // replacing functionality of the button

                        // changing text in the button
                        RoR2.UI.LanguageTextMeshController labelText = statsButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
                        if (labelText) {
                            labelText.token = "Stats Plus";
                        }

                        // this seems to stop [NO GAMEPAD BINDING] from appearing
                        Transform glyphTransform = statsButton.transform.Find("GenericGlyph");
                        if (glyphTransform) {
                            glyphTransform.gameObject.SetActive(false);
                        }

                        _statsButtonInstance = statsButton.transform;
                    }
                } else {
                    _statsButtonInstance.gameObject.SetActive(value);
                }
            }
        }

        public void OnSetDisplayData(RoR2.UI.GameEndReportPanelController.DisplayData newDisplayData) {
            ButtonIsVisible = ShouldShowOnReportScreen(newDisplayData.runReport);
        }

        void Awake() {
            StatsScreen.gameEndReportPanelController = GetComponent<RoR2.UI.GameEndReportPanelController>();
            StatsScreen.continueButton = StatsScreen.gameEndReportPanelController.continueButton;
            if (StatsScreen.graph == null) {
                StatsScreen.CreateGraph();
            }
        }
    }
}

