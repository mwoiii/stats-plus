using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.AddressableAssets;
using LeTai.Asset.TranslucentImage;
using UnityEngine.UI;

namespace StatsMod
{
    // base code shamelessly taken from restartbutton mod
    public static class StatsScreen
    {
        public static RoR2.UI.HUD CurrentHud = null; // most recent hud object, for stats panel

        public static GameObject gameEndPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/GameEndReportPanel.prefab").WaitForCompletion(); // prefab of the results screen, for grabbing copies of objects

        public static GameObject panelPrefab => Assets.mainAssetBundle.LoadAsset<GameObject>("StatsPanel.prefab"); // prefab of the stats panel

        public static RoR2.UI.GameEndReportPanelController gameEndReportPanelController; // current report panel controller

        public static RoR2.UI.MPButton continueButton; // continue button on report screen ^

        public static GameObject graph; 

        public static void CreateStatsWindow()
        {
            if (CurrentHud != null)
            {
                // preventing panel from fading in blur when stats window is closed
                if (gameEndReportPanelController != null)
                {
                    gameEndReportPanelController.transform.Find("BlurPP").GetComponent<RoR2.PostProcessDuration>().maxDuration = 0;
                }

                // grabbed panel - has all the components that make things work after adding graphicsraycaster
                LocalUser localUser = CurrentHud.localUserViewer;
                var obj = Object.Instantiate(gameEndPrefab, CurrentHud.transform);
                obj.transform.SetParent(CurrentHud.transform);
                obj.GetComponent<RoR2.UI.MPEventSystemProvider>().eventSystem = localUser.eventSystem;
                Object.Destroy(obj.GetComponent<RoR2.UI.GameEndReportPanelController>());
                Object.Destroy(obj.GetComponent<RoR2.UI.InputSourceFilter>());
                Object.Destroy(obj.GetComponent<RoR2.StartEvent>());
                Object.Destroy(obj.GetComponent<RoR2.RefreshCanvasDrawOrder>());
                Object.Destroy(obj.transform.Find("BlurPP").gameObject);
                Object.Destroy(obj.transform.Find("Flash").gameObject);
                Object.Destroy(obj.transform.Find("UnlockStripTemplate").gameObject);
                Object.Destroy(obj.transform.Find("SafeArea (JUICED)").gameObject);
                obj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                obj.GetComponent<RectTransform>().anchorMax = Vector2.one;
                obj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                obj.AddComponent<GraphicRaycaster>();

                // background
                GameObject bg2 = new GameObject();
                bg2.name = "Background";
                bg2.transform.SetParent(obj.transform, false);
                bg2.AddComponent<TranslucentImage>().color = new Color(0f, 0f, 0f, 1f);
                bg2.GetComponent<TranslucentImage>().raycastTarget = true;
                bg2.GetComponent<TranslucentImage>().material = Resources.Load<GameObject>("Prefabs/UI/Tooltip").GetComponentInChildren<TranslucentImage>(true).material;
                bg2.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                bg2.GetComponent<RectTransform>().anchorMax = Vector2.one;
                bg2.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                bg2.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                // prefab
                GameObject panel = Object.Instantiate(panelPrefab, bg2.transform);
                panel.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                panel.GetComponent<RectTransform>().anchorMax = Vector2.one;
                panel.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                // main content panel
                Transform content = panel.transform.Find("SafeArea (JUICED)/BodyArea/StatsAndChatArea/StatsContainer/Stats Body/ScrollView/Viewport/Content");

                GameObject emptyContainer = content.Find("StatContainer").gameObject;

                RoR2.UI.HGButton button;

                RoR2.UI.LanguageTextMeshController labelText;

                Transform glyphTransform;

                GameObject statContainer;

                GameObject statsHeader;

                foreach (string stat in PlayerStatsDatabase.allStats)
                {
                    statContainer = Object.Instantiate(emptyContainer, content);

                    
                    statsHeader = Object.Instantiate(gameEndPrefab.transform.Find("SafeArea (JUICED)/BodyArea/StatsAndChatArea/StatsContainer/Stats And Player Nav/Stats Header").gameObject, statContainer.transform.Find("TitleContainer"));
                    labelText = statsHeader.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
                    if (labelText)
                    {
                        labelText.token = $"<size=50px>{stat}";
                    }
                    
                    int index = 0;
                    foreach (IndependentEntry i in RecordHandler.independentDatabase)
                    {
                        int currentIndex = index;
                        GameObject statButton = Object.Instantiate(continueButton.gameObject, statContainer.transform);
                        statButton.name = "StatButton";
                        button = statButton.GetComponent<RoR2.UI.HGButton>();
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => {
                            graph.GetComponent<GraphHandler>().PlotStat(stat, currentIndex);
                        });
                        labelText = statButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
                        if (labelText)
                        {
                            labelText.token = $"Plot for {i.playerName}";
                        }
                        glyphTransform = statButton.transform.Find("GenericGlyph");
                        if (glyphTransform)
                        {
                            glyphTransform.gameObject.SetActive(false);
                        }
                        index++;
                    }
                }
                Object.Destroy(emptyContainer);


                // graph
                if (graph != null)
                {
                    graph.transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    Log.Error("Graph window must be created first!");
                }

                // creation of the r script button
                GameObject scriptButton = Object.Instantiate(continueButton.gameObject, panel.transform.Find("SafeArea (JUICED)/BodyArea/RightArea/AcceptButtonArea").transform);
                scriptButton.name = "ScriptButton";
                button = scriptButton.GetComponent<RoR2.UI.HGButton>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    GUIUtility.systemCopyBuffer = RecordHandler.GetRScript();
                });
                labelText = scriptButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
                if (labelText)
                {
                    labelText.token = "Copy R code";
                }
                glyphTransform = scriptButton.transform.Find("GenericGlyph");
                if (glyphTransform)
                {
                    glyphTransform.gameObject.SetActive(false);
                }

                // creation of the close button
                GameObject closeButton = Object.Instantiate(continueButton.gameObject, panel.transform.Find("SafeArea (JUICED)/BodyArea/RightArea/AcceptButtonArea").transform);
                closeButton.name = "CloseButton";
                button = closeButton.GetComponent<RoR2.UI.HGButton>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    Object.Destroy(obj);
                    graph.transform.parent.gameObject.SetActive(false);
                });
                labelText = closeButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
                if (labelText)
                {
                    labelText.token = "Close";
                }
                glyphTransform = closeButton.transform.Find("GenericGlyph");
                if (glyphTransform)
                {
                    glyphTransform.gameObject.SetActive(false);
                }

            }
            else {
                Log.Warning("CurrentHud is null!");
            }
        }

        public static void CreateGraph()
        {
            // graph
            graph = new GameObject();
            graph.AddComponent<GraphHandler>();
            graph.AddComponent<GraphSettings>();
        }

        private static void GameEndReportPanelController_Awake(On.RoR2.UI.GameEndReportPanelController.orig_Awake orig, RoR2.UI.GameEndReportPanelController self)
        {
            orig(self);
            self.gameObject.AddComponent<StatsButtonController>();
        }

        private static void GameEndReportPanelController_SetDisplayData(On.RoR2.UI.GameEndReportPanelController.orig_SetDisplayData orig, RoR2.UI.GameEndReportPanelController self, RoR2.UI.GameEndReportPanelController.DisplayData newDisplayData)
        {
            orig(self, newDisplayData);

            if (self.TryGetComponent(out StatsButtonController graphScreenController))
            {
                graphScreenController.OnSetDisplayData(newDisplayData);
            }
        }

        private static RoR2.UI.GameEndReportPanelController GetHud(On.RoR2.GameOverController.orig_GenerateReportScreen orig, RoR2.GameOverController self, RoR2.UI.HUD hud)
        {
            CurrentHud = hud;
            var gerpc = orig(self, hud);
            return gerpc;
        }

        public static void Init()
        {
            On.RoR2.UI.GameEndReportPanelController.Awake += GameEndReportPanelController_Awake;
            On.RoR2.UI.GameEndReportPanelController.SetDisplayData += GameEndReportPanelController_SetDisplayData;
            On.RoR2.GameOverController.GenerateReportScreen += GetHud;
        }
    }
}

