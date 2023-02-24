using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


//mostly for debug purposes
public class Map : MonoBehaviour
{
    public enum ObjectTypes
    {
        Zone,
        Centre,
        Mound,
        Wall,
        Gate,
        Subject,
        Structures,
        Greed,
        TreeCover,
        SafeZone
        //IndividualTree,
    }

    public static Map Instance { get; private set; }
    float relativeScale; // world pos * relativescale / zoom + offSet = map pos
    float zoom;
    const float minZoom = 1f, maxZoom = 8f, zoomSpeed = .8f, moveSpeed = 2f;
    Vector2 offset;

    [SerializeField] RectTransform zones, island, subjects, structures, greed, mounds, gates, walls;
    [SerializeField] RectTransform player;
    [SerializeField] RectTransform fullMapTransform, minimapTransform, mapObjects, mapOptions;
    [SerializeField] Canvas fullMapCanvas, miniMapCanvas;
    PlayerInput input;

    bool[] toggles;
    bool isMinimapEnabled = false;
    [SerializeField] GameObject circle, wallObj, zoneObj, treeCover;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        input = GetComponent<PlayerInput>();
        zoomAction = input.actions.FindAction("Zoom");
        moveAction = input.actions.FindAction("Move");
        input.DeactivateInput();
        toggles = new bool[System.Enum.GetNames(typeof(ObjectTypes)).Length];
        Setup();
    }

    void Setup()
    {
        var zonesRef = LevelController.zones;

        for (int i = 0; i < zonesRef.Length - 1; i++)
        {
            var ui = Instantiate(zoneObj, zones).transform;
            ui.GetChild(0).GetComponent<RectTransform>().sizeDelta = 2 * zonesRef[i].bottomRight.r * Vector2.one;
            ui.GetChild(1).GetComponent<RectTransform>().sizeDelta = 2 * zonesRef[i].topLeft.r * Vector2.one;
            ui.GetChild(1).GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, zonesRef[i].bottomRight.ThetaDegrees);
            ui.GetChild(1).GetComponent<Image>().fillAmount = zonesRef[i].Width / (Mathf.PI * 2);
            ui.GetChild(1).GetComponent<Image>().color = new Color(Random.Range(0, 0.3f), Random.Range(0.7f, 1f), Random.Range(0, 0.3f));
        }

        float oceanBorderWidth = 20;
        float height = fullMapTransform.rect.height - 2 * oceanBorderWidth;
        island.sizeDelta = 2 * LevelController.Instance.islandRadius * Vector2.one;
        relativeScale = height / (2 * LevelController.Instance.islandRadius);
        mapObjects.localScale = relativeScale * zoom * Vector3.one;
    }

    InputAction zoomAction;
    InputAction moveAction;
    private void LateUpdate()
    {
        if (miniMapCanvas.enabled)
        {
            player.localPosition = LevelController.player.position.V2BirdsEyeDisplacement();
            mapObjects.anchoredPosition = relativeScale * zoom * -player.localPosition;
            minimapTransform.localRotation = Quaternion.Euler(0, 0, LevelController.player.rotation.eulerAngles.y);
            player.localRotation = Quaternion.Euler(0, 0, -LevelController.player.rotation.eulerAngles.y);
        }
        else
        {
            if (!Mathf.Approximately(zoomAction.ReadValue<float>(), 0f))
            {
                zoom += zoomAction.ReadValue<float>() * zoomSpeed * Time.deltaTime;
                zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
                mapObjects.localScale = relativeScale * zoom * Vector3.one;
            }

            if (moveAction.ReadValue<Vector2>() != Vector2.zero)
            {
                offset += moveSpeed * Time.deltaTime / zoom / relativeScale * moveAction.ReadValue<Vector2>();
                //offset.x = Mathf.Clamp();
                mapObjects.localPosition = offset;
                player.localPosition = offset + 1 / relativeScale / zoom * LevelController.player.position.V2BirdsEyeDisplacement();
            }
        }
    }

    public GameObject CreateMapObject(ObjectTypes mapObject, int subTypeEnum)
    {
        GameObject obj = null;

        switch (mapObject)
        {
            case ObjectTypes.Mound:
                obj = Instantiate(circle, mounds);
                obj.GetComponent<Image>().color = GetColor(mapObject, 0);
                (obj.transform as RectTransform).sizeDelta = new Vector2(1, IslandGenerator.wallWidth) * 2;
                break;
            case ObjectTypes.Wall:
                obj = Instantiate(wallObj, walls);
                obj.GetComponent<Image>().color = GetColor(mapObject, subTypeEnum);
                (obj.transform as RectTransform).sizeDelta = new Vector2(1, IslandGenerator.wallWidth) * 2;
                break;
            case ObjectTypes.Gate:
                obj = Instantiate(wallObj, gates);
                obj.GetComponent<Image>().color = GetColor(mapObject, subTypeEnum);
                (obj.transform as RectTransform).sizeDelta = new Vector2(1, IslandGenerator.wallWidth) * 2;
                break;
            case ObjectTypes.Subject:
                obj = Instantiate(circle, subjects);
                obj.GetComponent<Image>().color = GetColor(mapObject, subTypeEnum);
                (obj.transform as RectTransform).sizeDelta = new Vector2(1, 1) * 5;
                break;
            case ObjectTypes.Structures:
                // each structure should have an associated image eventually
                obj = Instantiate(wallObj, subjects);
                obj.GetComponent<Image>().color = Color.black;
                (obj.transform as RectTransform).sizeDelta = new Vector2(1, 1) * 10;
                break;
            case ObjectTypes.Greed:
                obj = Instantiate(circle, greed);
                obj.GetComponent<Image>().color = GetColor(mapObject, subTypeEnum);
                (obj.transform as RectTransform).sizeDelta = new Vector2(1, 1) * 5;
                break;
        }

        return obj;
    }

    public Color GetColor(ObjectTypes mapObject, int subTypeEnum)
    {
        Color color = Color.white;

        switch (mapObject)
        {
            case ObjectTypes.Zone:
                color = new(Random.Range(0, 0.3f), Random.Range(0.7f, 1f), Random.Range(0, 0.3f));
                break;
            case ObjectTypes.Centre:
                color = Color.HSVToRGB(0.084f, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f));
                break;
            case ObjectTypes.Mound:
                color = new(.4f, .3f, 0);
                break;
            case ObjectTypes.Wall:
                switch ((Gate.WallLevels)subTypeEnum)
                {
                    case Gate.WallLevels.level1:
                        color = new(0.72f, 0.6f, 0.25f);
                        break;
                    case Gate.WallLevels.level2:
                        break;
                    case Gate.WallLevels.level3:
                        color = new(0.6f, 0.8f, 0.8f);
                        break;
                    case Gate.WallLevels.level4:
                        color = new(0.4f, 0.6f, 0.6f);
                        break;
                    default:
                        break;
                }
                break;
            case ObjectTypes.Gate:
                switch ((Gate.WallLevels)subTypeEnum)
                {
                    case Gate.WallLevels.level1:
                        color = new(0.72f, 0.6f, 0.25f);
                        break;
                    case Gate.WallLevels.level2:
                        break;
                    case Gate.WallLevels.level3:
                        color = new(0.6f, 0.8f, 0.8f);
                        break;
                    case Gate.WallLevels.level4:
                        color = new(0.4f, 0.6f, 0.6f);
                        break;
                    default:
                        break;
                }
                break;
            case ObjectTypes.Subject:
                switch ((SubjectBase.Roles)subTypeEnum)
                {
                    case SubjectBase.Roles.Villager:
                        color = new(0.8f, 0.8f, 0.8f);
                        break;
                    case SubjectBase.Roles.Builder:
                        color = Color.blue;
                        break;
                    case SubjectBase.Roles.Archer:
                        color = Color.green;
                        break;
                    case SubjectBase.Roles.Farmer:
                        color = new(1f, 0.8f, 0.1f);
                        break;
                    case SubjectBase.Roles.Knight:
                        color = new(0.2f, 0.85f, 1f);
                        break;
                    default:
                        break;
                }
                break;
            case ObjectTypes.Structures:
                // perhaps just try images?
                break;
            case ObjectTypes.Greed:
                switch ((GreedTracker.GreedType)subTypeEnum)
                {
                    case GreedTracker.GreedType.Greedling:
                        color = new(0.8f, 0, 0.8f);
                        break;
                    case GreedTracker.GreedType.MaskedGreedling:
                        color = new(0.8f, 0, 0.8f);
                        break;
                    case GreedTracker.GreedType.Floater:
                        color = new(0.8f, 0, 0.6f);
                        break;
                    case GreedTracker.GreedType.Breeder:
                        color = new(0.6f, 0, 0.6f);
                        break;
                    default:
                        break;
                }
                break;
            case ObjectTypes.TreeCover:
                color = new(0, 0.4f, 0);
                break;
            case ObjectTypes.SafeZone:
                color = Color.HSVToRGB(0.14f, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f));
                break;
            default: 
                break;
        }

        return color;
    }

    public void ChangeVisibility(ObjectTypes type, bool visible)
    {
        toggles[(int)type] = visible;
        Zone[] zonesRef; 
        switch (type)
        {
            case ObjectTypes.Zone:
                zones.GetComponent<Canvas>().enabled = visible;
                break;
            case ObjectTypes.Centre:
                for (int i = 0; i < LevelController.numCentralZones; i++)
                {
                    Color color = GetColor(
                                            visible ? type :
                                            toggles[(int)ObjectTypes.SafeZone] ? ObjectTypes.SafeZone :
                                            ObjectTypes.Zone, 0);
                    zones.GetChild(i).GetChild(1).GetComponent<Image>().color = color;
                }
                break;
            case ObjectTypes.Mound:
                mounds.GetComponent<Canvas>().enabled = visible;
                break;
            case ObjectTypes.Wall:
                walls.GetComponent<Canvas>().enabled = visible;
                gates.GetComponent<Canvas>().enabled = visible;
                break;
            case ObjectTypes.Gate:
                foreach (RectTransform gate in gates)
                {
                    gate.sizeDelta = new Vector2(1, IslandGenerator.wallWidth) * (visible ? 3 : 2);
                }
                break;
            case ObjectTypes.Subject:
                subjects.GetComponent<Canvas>().enabled = visible;
                break;
            case ObjectTypes.Structures:
                structures.GetComponent<Canvas>().enabled = visible;
                break;
            case ObjectTypes.Greed:
                greed.GetComponent<Canvas>().enabled = visible;
                break;
            case ObjectTypes.TreeCover:
                treeCover.SetActive(visible);
                break;
            case ObjectTypes.SafeZone:
                zonesRef = LevelController.zones;

                for (int i = 0; i < zonesRef.Length - 1; i++)
                {
                    if (visible)
                    {
                        if (!(toggles[(int)ObjectTypes.Centre] && zonesRef[i].isCentralZone))
                        {
                            zones.GetChild(i).GetChild(1).GetComponent<Image>().color = GetColor(type, 0);
                        }
                    }
                    else if(!(toggles[(int)ObjectTypes.Centre] && zonesRef[i].isCentralZone))
                    {
                        zones.GetChild(i).GetChild(1).GetComponent<Image>().color = GetColor(ObjectTypes.Zone, 0);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void UpdateSafety(int zoneIndex)
    {
        ObjectTypes type;

        if (toggles[(int)ObjectTypes.Centre] && zoneIndex < LevelController.numCentralZones)
        {
            type = ObjectTypes.Centre;
        }
        else if (toggles[(int)ObjectTypes.SafeZone] && LevelController.zones[zoneIndex].IsSafe)
        {
            type = ObjectTypes.SafeZone;
        }
        else
            type = ObjectTypes.Zone;

        zones.GetChild(zoneIndex).GetChild(0).GetComponent<Image>().color = GetColor(type, 0); ;
    }

    public void ToggleMinimap()
    {
        isMinimapEnabled = !isMinimapEnabled;
        enabled = isMinimapEnabled;
        if (isMinimapEnabled)
        {
            mapOptions.Find("Zone")     .GetComponent<MapToggle>().Toggle(false);
            //mapOptions.Find("Subject")  .GetComponent<MapToggle>().Toggle(true);
            //mapOptions.Find("Mound")    .GetComponent<MapToggle>().Toggle(true);
            //mapOptions.Find("Tree")     .GetComponent<MapToggle>().Toggle(false);
            //mapOptions.Find("Greed")    .GetComponent<MapToggle>().Toggle(false);
            //mapOptions.Find("Structure").GetComponent<MapToggle>().Toggle(false);
            //mapOptions.Find("Wall")     .GetComponent<MapToggle>().Toggle(false);
            mapOptions.Find("Gate")     .GetComponent<MapToggle>().Toggle(false);
            //mapOptions.Find("Centre")   .GetComponent<MapToggle>().Toggle(false);
            //mapOptions.Find("Safe")     .GetComponent<MapToggle>().Toggle(false);
        }
        zoom = 3f;
        mapObjects.localScale = relativeScale * zoom * Vector3.one;
        miniMapCanvas.enabled = isMinimapEnabled;
    }

    public void EnableMap()
    {
        RuntimeConsole.Instance.HideConsole();
        LevelController.player.GetComponent<PlayerInput>().DeactivateInput();

        mapObjects.SetParent(fullMapTransform);
        zoom = 8f;
        mapObjects.localScale = relativeScale * zoom * Vector3.one;

        player.localPosition = LevelController.player.position.V2BirdsEyeDisplacement();
        mapObjects.anchoredPosition = relativeScale * zoom * -player.localPosition;

        fullMapCanvas.enabled = true;
        input.ActivateInput();
        enabled = true;

        miniMapCanvas.enabled = false;
    }

    public void DisableMap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LevelController.player.GetComponent<PlayerInput>().ActivateInput();
            input.DeactivateInput();

            mapObjects.SetParent(minimapTransform);
            fullMapCanvas.enabled = false;
            zoom = 3f;
            mapObjects.localScale = relativeScale * zoom * Vector3.one;

            if (isMinimapEnabled)
                miniMapCanvas.enabled = true;
            else
                enabled = false;
        }
    }
}
