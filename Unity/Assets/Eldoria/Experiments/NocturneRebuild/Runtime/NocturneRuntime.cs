using System;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Eldoria.Experiments.Nocturne
{
    /// <summary>
    /// Opt-in, mechanically authentic gameplay composition for a separate art experiment.
    /// The complete canonical domain/gateway executes every command: visual parts are
    /// never permitted to invent a successful purchase, completed march or reward.
    /// </summary>
    public sealed class NocturneRuntime : MonoBehaviour
    {
        public Camera PlayCamera;
        public GameObject CityRoot, WorldRoot;
        public Transform BastionAnchor;
        public Transform[] BuildingAnchors = new Transform[4];
        public GameObject[] BastionModels = new GameObject[5];
        // Sawmill, Barracks, Granary, Stoneworks
        public GameObject[] BuildingModels = new GameObject[4];
        public GameObject UnbuiltPlot;
        public Text RealmTitle, Resources, Chapter, SelectionTitle, SelectionBody, Feedback;
        public Button[] BuildingTabs = new Button[5];
        public Button[] TargetButtons = new Button[8];
        public Button PrimaryButton, SecondaryButton;
        public Text PrimaryText, SecondaryText;
        public GameObject ContextPanel, WorldChoicesPanel;
        public Text CityNavText, WorldNavText;
        public Transform ContextPanelTransform;
        public bool NocturneDedicatedSave = true;

        private ICommandGateway gateway;
        private int shownBastion = -1;
        private readonly int[] shownBuildings = { -1, -1, -1, -1 };
        private GameObject activeBastion;
        private readonly GameObject[] activeBuildings = new GameObject[4];
        private string selected = "bastion";
        private bool world;
        private float refreshAt;
        private string info = "Toca un edificio. Cada mejora utiliza las reglas reales.";
        private Vector3 cityCamera, worldCamera;
        private Quaternion cityRotation, worldRotation;
        private float citySize, worldSize;
        private bool cameraInitialized;

        private static readonly string[] BuildingIds =
            { "sawmill", "barracks", "granary", "stoneworks" };
        private static readonly string[] BuildingLabels =
            { "ASERRADERO", "CUARTEL", "GRANERO", "CANTERA" };
        private static readonly string[] WorldTargetLabels =
            { "Bosque", "Mina", "Explorador corrupto", "Engendro", "Fisura", "Jabalí", "Lobos", "Acechador" };
        private static readonly string[] WorldTargetIds =
            { "forest-valoria", "quarry-valoria", "corrupt-scout", "engendro-valoria",
              "fissure-valoria", "hunt-boar", "hunt-wolf", "ash-stalker-valoria" };

        private void Awake()
        {
            if (PlayCamera == null || CityRoot == null || WorldRoot == null || BastionAnchor == null)
            {
                Debug.LogError("NOCTURNE: authoring scene is incomplete; refusing mock gameplay.");
                enabled = false;
                return;
            }
            // The experiment always gets its own save key/path, never the
            // production Eldoria save. Gameplay schema and authority stay real.
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            string saveFile = System.IO.Path.Combine(Application.persistentDataPath,
                "eldoria-nocturne-experiment-i-v.json");
            try
            {
                gateway = new LocalGateway(new SystemClock(), new FileStateStore(saveFile));
            }
            catch (Exception error)
            {
                Debug.LogError("NOCTURNE: isolated save unreadable, left untouched: " + error);
                enabled = false;
                return;
            }

            cityCamera = PlayCamera.transform.position;
            cityRotation = PlayCamera.transform.rotation;
            citySize = PlayCamera.orthographicSize;
            worldCamera = new Vector3(0, 20, -25);
            worldRotation = Quaternion.LookRotation(new Vector3(0, 0, 0)-worldCamera, Vector3.up);
            worldSize = 19f;
            cameraInitialized = true;

            for (int i = 0; i < BuildingTabs.Length; i++)
            {
                int index = i;
                if (BuildingTabs[index] != null)
                    BuildingTabs[index].onClick.AddListener(
                        () => SelectBuilding(index == 0 ? "bastion" : BuildingIds[index - 1]));
            }
            for (int i = 0; i < TargetButtons.Length && i < WorldTargetIds.Length; i++)
            {
                int index = i;
                if (TargetButtons[index] != null)
                    TargetButtons[index].onClick.AddListener(
                        () => ExecuteWorldTarget(WorldTargetIds[index]));
                if (TargetButtons[index] != null)
                    TargetButtons[index].gameObject.GetComponentInChildren<Text>().text = WorldTargetLabels[index];
            }
            SetView(false);
            Redraw(true);
        }

        private void Update()
        {
            if (gateway == null) return;
            if (Time.unscaledTime >= refreshAt)
            {
                refreshAt = Time.unscaledTime + .35f;
                bool changed = gateway.Advance();
                Redraw(changed);
            }
            TapScene();
        }

        public void ShowCity() { SetView(false); }
        public void ShowWorld() { SetView(true); }
        public void ShowHeroes()
        {
            var s = gateway.Snapshot();
            info = "HÉROES | Aldric: " + MarchCapacityRules.HeroLevel(s, "aldric") +
                   " · Lyra: " + (s.ChapterThree.LyraRecruited
                           ? "nivel " + MarchCapacityRules.HeroLevel(s, "lyra") : "bloqueada") +
                   " · Marchas activas: " + MarchCapacityRules.ActiveWorldCount(s) + "/3";
            SelectBuilding("heroes");
        }
        public void ShowMailbox()
        {
            var s = gateway.Snapshot();
            int count = s.Mailbox == null || s.Mailbox.Messages == null ? 0 : s.Mailbox.Messages.Count;
            info = "BUZÓN | " + count +
                   " cartas del registro de expediciones. Las recompensas se gestionan únicamente mediante el sistema original.";
            SelectBuilding("mail");
        }

        private void SetView(bool showWorld)
        {
            world = showWorld;
            CityRoot.SetActive(!world);
            WorldRoot.SetActive(world);
            if (cameraInitialized)
            {
                PlayCamera.transform.position = world ? worldCamera : cityCamera;
                PlayCamera.transform.rotation = world ? worldRotation : cityRotation;
                PlayCamera.orthographicSize = world ? worldSize : citySize;
            }
            if (CityNavText) CityNavText.color = world ? new Color(.65f,.62f,.55f) : new Color(.93f,.77f,.46f);
            if (WorldNavText) WorldNavText.color = world ? new Color(.93f,.77f,.46f) : new Color(.65f,.62f,.55f);
            if (WorldChoicesPanel) WorldChoicesPanel.SetActive(world);
            if (ContextPanel) ContextPanel.SetActive(!world);
            info = world
                ? "Frontera de Valoria: selecciona un destino y despliega una marcha."
                : "Valoria: selecciona el Bastión o sus edificios.";
            Redraw(true);
        }

        public void SelectBuilding(string id)
        {
            selected = id;
            if (world) SetView(false);
            Redraw(true);
        }

        private void TapScene()
        {
            if (world || PlayCamera == null) return;
            bool pressed = false;
            Vector2 pos = Vector2.zero;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                pos = Touchscreen.current.primaryTouch.position.ReadValue();
                pressed = true;
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                pos = Mouse.current.position.ReadValue();
                pressed = true;
            }
            if (!pressed) return;
            if (EventSystem.current != null)
            {
                var eventData = new PointerEventData(EventSystem.current) { position = pos };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, hits);
                if (hits.Count > 0) return;
            }
            var ray = PlayCamera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out var hit, 400f))
            {
                var target = hit.collider.GetComponentInParent<NocturneStructureTarget>();
                if (target != null && !string.IsNullOrEmpty(target.BuildingId))
                    SelectBuilding(target.BuildingId);
            }
        }

        private void ExecuteCommand(string kind, string targetId)
        {
            if (gateway == null) return;
            var s = gateway.Snapshot();
            try
            {
                var result = gateway.Execute(
                    new GameCommand(Guid.NewGuid().ToString("N"), s.PlayerId, kind, targetId, s.Revision));
                info = result.Ok ? "ORDEN ACEPTADA · " + result.Message : "NO DISPONIBLE · " + result.Message;
                Redraw(true);
            }
            catch (Exception error)
            {
                info = "ORDEN RECHAZADA · " + error.Message;
                Debug.LogError("NOCTURNE COMMAND " + kind + " / " + targetId + ": " + error);
                Redraw(false);
            }
        }

        private void ExecuteWorldTarget(string id)
        {
            if (id == "forest-valoria" || id == "quarry-valoria")
            {
                ExecuteCommand("Gather", id);
                return;
            }
            if (id == "fissure-valoria")
            {
                var c = gateway.Snapshot().ChapterThree;
                if (!c.FissureExplored)
                    ExecuteCommand("InvestigateFissure", id);
                else if (!c.LyraContacted)
                    ExecuteCommand("ContactLyra", id);
                else ExecuteCommand("Fight", id);
                return;
            }
            ExecuteCommand("Fight", id);
        }

        public void PrepareMarch() { ExecuteCommand("ConfigureMarch", "march-main"); }
        public void RecruitArchers() { ExecuteCommand("Recruit", "archer:t1"); }

        private void Redraw(bool refreshModels)
        {
            if (gateway == null) return;
            var state = gateway.Snapshot();
            if (refreshModels) RefreshModels(state);
            if (RealmTitle)
                RealmTitle.text = "VALORIA  ·  BASTIÓN " + Roman(Mathf.Clamp(state.BastionLevel,1,5));
            if (Resources)
                Resources.text = "MADERA " + state.Resources.Wood + "    PIEDRA " +
                                 state.Resources.Stone + "    COMIDA " + state.Resources.Food;
            if (Chapter)
                Chapter.text = "CAMPAÑA I–V  •  PODER " + state.MissionPower +
                               "  •  MARCHAS " + MarchCapacityRules.ActiveWorldCount(state) + "/3";
            if (Feedback) Feedback.text = info;
            if (ContextPanel == null || world) return;
            string header = "BASTIÓN";
            string description = "";
            string primary = "";
            string secondary = "";
            Action primaryAction = null, secondaryAction = null;
            if (selected == "bastion")
            {
                header = "BASTIÓN " + Roman(Mathf.Clamp(state.BastionLevel,1,5));
                if (state.BastionLevel < 5)
                {
                    var next = BastionProgressionCatalog.ForLevel(state.BastionLevel+1);
                    string missing = BastionProgressionCatalog.MissingSummary(state,next);
                    description = string.IsNullOrEmpty(missing)
                        ? "Preparado para la siguiente etapa." : "OBJETIVOS PENDIENTES · " + missing;
                    primary = "MEJORAR BASTIÓN";
                    primaryAction = () => ExecuteCommand("AdvanceBastion", "bastion");
                }
                else description = "BASTIÓN V. La frontera, las minas y las expediciones continúan activas.";
                if (state.BastionUpgradeCompletesUtcTicks > 0)
                {
                    description = "MEJORA EN CURSO · " + TimeRemaining(state.BastionUpgradeCompletesUtcTicks);
                    primary = "";
                }
                secondary = "PREPARAR MARCHA";
                secondaryAction = PrepareMarch;
            }
            else
            {
                int index = Array.IndexOf(BuildingIds, selected);
                if (index >= 0)
                {
                    int level = BuildingLevel(state,index);
                    header = BuildingLabels[index] + " · " + (level > 0 ? "NIVEL " + level : "SIN CONSTRUIR");
                    description = index==0 ? "Madera para el asentamiento." :
                                  index==1 ? "Entrenamiento y capacidad de marcha." :
                                  index==2 ? "Abastecimiento para la ciudad." :
                                             "Extracción de piedra y minerales.";
                    if (level == 0)
                    {
                        primary = "CONSTRUIR";
                        primaryAction = () => ExecuteCommand("Build", BuildingIds[index]);
                    }
                    else if (level < Mathf.Min(5,state.BastionLevel))
                    {
                        primary = "MEJORAR EDIFICIO";
                        primaryAction = () => ExecuteCommand("UpgradeBuilding", BuildingIds[index]);
                    }
                    if (index == 1 && level > 0)
                    {
                        secondary = "ENTRENAR ARQUEROS";
                        secondaryAction = RecruitArchers;
                    }
                    if (state.BuildingCompletesUtcTicks > 0)
                        description += " · OBRA EN CURSO " + TimeRemaining(state.BuildingCompletesUtcTicks);
                }
                else if (selected == "heroes")
                {
                    header = "SALA DE HÉROES";
                    description = info;
                    primary = "PREPARAR MARCHA";
                    primaryAction = PrepareMarch;
                }
                else if (selected == "mail")
                {
                    header = "BUZÓN";
                    description = info;
                }
            }
            if (SelectionTitle) SelectionTitle.text = header;
            if (SelectionBody) SelectionBody.text = description;
            ConfigureButton(PrimaryButton,PrimaryText,primary,primaryAction);
            ConfigureButton(SecondaryButton,SecondaryText,secondary,secondaryAction);
        }

        private static void ConfigureButton(Button button, Text label, string caption, Action action)
        {
            if (button == null) return;
            button.gameObject.SetActive(action != null && !string.IsNullOrEmpty(caption));
            if (label != null) label.text = caption;
            button.onClick.RemoveAllListeners();
            if (action != null) button.onClick.AddListener(() => action());
        }

        private void RefreshModels(PlayerState s)
        {
            if (s.BastionLevel != shownBastion)
            {
                if (activeBastion) Destroy(activeBastion);
                int index = Mathf.Clamp(s.BastionLevel-1,0,4);
                if (BastionModels != null && index < BastionModels.Length && BastionModels[index] != null)
                {
                    activeBastion = Instantiate(BastionModels[index], BastionAnchor);
                    activeBastion.transform.localPosition = Vector3.zero;
                    activeBastion.transform.localRotation = Quaternion.identity;
                    AttachHitbox(activeBastion,"bastion",new Vector3(0,3.5f,0),new Vector3(11,8,11));
                }
                shownBastion = s.BastionLevel;
            }
            for (int i=0;i<shownBuildings.Length;i++)
            {
                int level = BuildingLevel(s,i);
                if (level == shownBuildings[i]) continue;
                if (activeBuildings[i]) Destroy(activeBuildings[i]);
                if (BuildingAnchors == null || i >= BuildingAnchors.Length || !BuildingAnchors[i]) continue;
                GameObject candidate = level > 0 ? BuildingModels[i] : UnbuiltPlot;
                if (candidate)
                {
                    activeBuildings[i] = Instantiate(candidate, BuildingAnchors[i]);
                    activeBuildings[i].transform.localPosition = Vector3.zero;
                    activeBuildings[i].transform.localRotation = Quaternion.identity;
                    AttachHitbox(activeBuildings[i],BuildingIds[i],
                        new Vector3(0,1.65f,0),new Vector3(5f,4f,4f));
                }
                shownBuildings[i] = level;
            }
        }

        private static void AttachHitbox(GameObject model,string buildingId,Vector3 center,Vector3 size)
        {
            if (!model) return;
            var marker=model.GetComponent<NocturneStructureTarget>();
            if (!marker) marker=model.AddComponent<NocturneStructureTarget>();
            marker.BuildingId=buildingId;
            var box=model.GetComponent<BoxCollider>();
            if (!box) box=model.AddComponent<BoxCollider>();
            box.center=center;
            box.size=size;
        }

        private static int BuildingLevel(PlayerState s,int index)
        {
            switch (index)
            {
                case 0: return s.SawmillLevel;
                case 1: return s.BarracksLevel;
                case 2: return s.ChapterThree == null ? 0 : s.ChapterThree.GranaryLevel;
                case 3: return s.ChapterThree == null ? 0 : s.ChapterThree.StoneworksLevel;
                default: return 0;
            }
        }

        private static string TimeRemaining(long targetTicks)
        {
            long ticks = Math.Max(0,targetTicks-DateTime.UtcNow.Ticks);
            TimeSpan t=TimeSpan.FromTicks(ticks);
            return t.ToString(@"mm\:ss");
        }

        private static string Roman(int level)
        {
            switch(level) {case 1:return "I";case 2:return "II";case 3:return "III";case 4:return "IV";default:return "V";}
        }
    }

    public sealed class NocturneStructureTarget : MonoBehaviour
    {
        public string BuildingId;
    }
}
