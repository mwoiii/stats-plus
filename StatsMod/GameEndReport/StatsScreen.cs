using LeTai.Asset.TranslucentImage;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace StatsMod {
    public static class StatsScreen {
        public static RoR2.UI.HUD CurrentHud = null; // most recent hud object, for stats panel

        public static GameObject gameEndPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/GameEndReportPanel.prefab").WaitForCompletion(); // prefab of the results screen, for grabbing copies of objects

        public static GameObject panelPrefab => Assets.mainAssetBundle.LoadAsset<GameObject>("StatsPanel.prefab"); // prefab of the stats panel

        public static RoR2.UI.GameEndReportPanelController gameEndReportPanelController; // current report panel controller

        public static RoR2.UI.MPButton continueButton; // continue button on report screen ^

        public static GameObject graph;

        public static void CreateStatsWindow() {
            if (CurrentHud != null) {
                // preventing panel from fading in blur when stats window is closed
                if (gameEndReportPanelController != null) {
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

                foreach (string stat in PlayerStatsDatabase.allStats) {
                    // make new container
                    GameObject statContainer = Object.Instantiate(emptyContainer, content);

                    // configure the header
                    GameObject statsHeader = Object.Instantiate(gameEndPrefab.transform.Find("SafeArea (JUICED)/BodyArea/StatsAndChatArea/StatsContainer/Stats And Player Nav/Stats Header").gameObject, statContainer.transform.Find("TitleContainer"));
                    RoR2.UI.LanguageTextMeshController labelText = statsHeader.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
                    if (labelText) {
                        labelText.token = $"{StatTokens.titlePrefix}{stat}";
                    }
                    TooltipProvider toolTip = statsHeader.AddComponent<TooltipProvider>();
                    toolTip.titleToken = $"STATSMOD_STAT_TITLE_{stat}";
                    toolTip.bodyToken = $"STATSMOD_STAT_BODY_{stat}";
                    toolTip.titleColor = Color.gray;
                    toolTip.bodyColor = Color.white;

                    // individual player buttons
                    CreatePlayerPlotButtons(statContainer, labelText, stat);

                }

                Object.Destroy(emptyContainer);


                // graph
                if (graph != null) {
                    graph.transform.parent.gameObject.SetActive(true);
                } else {
                    Log.Error("Graph window must be created first!");
                }

                // plot title (fragile code based UI element please be careful)
                var plotTitle = new GameObject("plotTitle");
                plotTitle.transform.SetParent(graph.transform);

                var rt = plotTitle.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var txt = plotTitle.AddComponent<HGTextMeshProUGUI>();
                plotTitle.AddComponent<RoR2.UI.LanguageTextMeshController>();
                txt.text = "meow";
                txt.color = Color.white;
                txt.fontSize = 8;
                txt.alignment = TextAlignmentOptions.Center;
                txt.transform.localPosition = new Vector3(0, 450, 0); 
                txt.transform.localScale = new Vector3(10, 10, 10);
                txt.raycastTarget = false;

                // creation of the r script button
                CreateRScriptButton(panel);

                // creation of the close button
                CreateCloseButton(panel, obj);

                PlotStartingStat();

            } else {
                Log.Warning("CurrentHud is null!");
            }
        }

        private static void PlotStartingStat() {
            // the first plot doesn't have lines for some reason
            // instead of understanding and fixing this, what if plot on start......
            graph.GetComponent<GraphHandler>().PlotStat("maxHealth", -1);
        }

        public static void CreatePlayerPlotButtons(GameObject statContainer, RoR2.UI.LanguageTextMeshController labelText, string stat) {
            // get reference before The Reckoning
            Transform tempButtonHolder = statContainer.transform.Find("ButtonHolder");

            int index = 0;
            foreach (IndependentEntry entry in RecordHandler.independentDatabase) {
                CreateStatPlotButton(statContainer, labelText, stat, index, entry.playerName);
                index++;
            }

            // -1: special number for all players
            CreateStatPlotButton(statContainer, labelText, stat, -1, "all players");

            // The Reckoning
            Object.Destroy(tempButtonHolder.gameObject);
        }

        private static void CreateRScriptButton(GameObject panel) {
            GameObject scriptButton = Object.Instantiate(continueButton.gameObject, panel.transform.Find("SafeArea (JUICED)/BodyArea/RightArea/AcceptButtonArea").transform);
            scriptButton.name = "ScriptButton";
            RoR2.UI.HGButton button = scriptButton.GetComponent<RoR2.UI.HGButton>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                GUIUtility.systemCopyBuffer = RecordHandler.GetRScript();
            });
            RoR2.UI.LanguageTextMeshController labelText = scriptButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
            if (labelText) {
                labelText.token = "Copy R code";
            }
            Transform glyphTransform = scriptButton.transform.Find("GenericGlyph");
            if (glyphTransform) {
                glyphTransform.gameObject.SetActive(false);
            }
        }

        private static void CreateCloseButton(GameObject panel, GameObject hud) {
            GameObject closeButton = Object.Instantiate(continueButton.gameObject, panel.transform.Find("SafeArea (JUICED)/BodyArea/RightArea/AcceptButtonArea").transform);
            closeButton.name = "CloseButton";
            RoR2.UI.HGButton button = closeButton.GetComponent<RoR2.UI.HGButton>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                Object.Destroy(hud);
                graph.transform.parent.gameObject.SetActive(false);
            });
            RoR2.UI.LanguageTextMeshController labelText = closeButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
            if (labelText) {
                labelText.token = "Close";
            }
            Transform glyphTransform = closeButton.transform.Find("GenericGlyph");
            if (glyphTransform) {
                glyphTransform.gameObject.SetActive(false);
            }
        }

        private static void CreateStatPlotButton(GameObject statContainer, RoR2.UI.LanguageTextMeshController labelText, string stat, int statIndex, string plotForName) {
            // button holder
            GameObject buttonHolder = Object.Instantiate(statContainer.transform.Find("ButtonHolder").gameObject, statContainer.transform);

            // standard plot button
            GameObject statButton = Object.Instantiate(continueButton.gameObject, buttonHolder.transform);
            statButton.name = "StatButton";
            LayoutElement layoutElement = statButton.GetComponent<LayoutElement>();
            layoutElement.minWidth = -1f;
            layoutElement.preferredWidth = 230f;
            RoR2.UI.HGButton button = statButton.GetComponent<RoR2.UI.HGButton>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                graph.GetComponent<GraphHandler>().PlotStat(stat, statIndex);
            });
            labelText = statButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
            if (labelText) {

                string playerCol;
                if (statIndex != -1) { playerCol = ColorUtility.ToHtmlStringRGB(GraphSettings.Rainbow((float)statIndex / (float)RecordHandler.independentDatabase.Count)); } else { playerCol = "FFFFFF"; }

                labelText.token = $"Plot for <color=#{playerCol}>{plotForName}</color>";
            }
            Transform glyphTransform = statButton.transform.Find("GenericGlyph");
            if (glyphTransform) {
                glyphTransform.gameObject.SetActive(false);
            }

            // log button
            GameObject logStatButton = Object.Instantiate(statButton.gameObject, buttonHolder.transform);
            logStatButton.GetComponent<LayoutElement>().preferredWidth = 20f;
            labelText = logStatButton.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
            if (labelText) {
                labelText.token = "<color=#999999>log10</color>";
            }

            RoR2.UI.HGButton logbutton = logStatButton.GetComponent<RoR2.UI.HGButton>();
            logbutton.onClick.RemoveAllListeners();
            logbutton.onClick.AddListener(() => {
                graph.GetComponent<GraphHandler>().PlotStat(stat, statIndex, true);
            });
        }

        public static void CreateGraph() {
            // graph
            graph = new GameObject();
            graph.AddComponent<GraphHandler>();
            graph.AddComponent<GraphSettings>();
        }

        private static void GameEndReportPanelController_Awake(On.RoR2.UI.GameEndReportPanelController.orig_Awake orig, RoR2.UI.GameEndReportPanelController self) {
            orig(self);
            self.gameObject.AddComponent<StatsButtonController>();
        }

        private static void GameEndReportPanelController_SetDisplayData(On.RoR2.UI.GameEndReportPanelController.orig_SetDisplayData orig, RoR2.UI.GameEndReportPanelController self, RoR2.UI.GameEndReportPanelController.DisplayData newDisplayData) {
            orig(self, newDisplayData);

            if (self.TryGetComponent(out StatsButtonController graphScreenController)) {
                graphScreenController.OnSetDisplayData(newDisplayData);
            }
        }

        private static RoR2.UI.GameEndReportPanelController GetHud(On.RoR2.GameOverController.orig_GenerateReportScreen orig, RoR2.GameOverController self, RoR2.UI.HUD hud) {
            CurrentHud = hud;
            var gerpc = orig(self, hud);
            return gerpc;
        }

        public static void Init() {
            On.RoR2.UI.GameEndReportPanelController.Awake += GameEndReportPanelController_Awake;
            On.RoR2.UI.GameEndReportPanelController.SetDisplayData += GameEndReportPanelController_SetDisplayData;
            On.RoR2.GameOverController.GenerateReportScreen += GetHud;
        }
    }
}

