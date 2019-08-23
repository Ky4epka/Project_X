using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobalCollector : MonoBehaviour
{
        public const int INVALID_INDEX = -1;
        public const float DieEnduranceValue = 1f;

        public const float Cell_Size = 32f;
        public const float Half_Cell_Size = Cell_Size / 2f;
        public const float Spice_MinValue = 1;

        public const int MAX_OVERLAPS_FOR_COLLIDERS = 20;
        public const int UPDATE_TICKS_FOR_PRODUCTION_ITEM_PROCESS = 5;

        public const bool IsDebug = true;
        public const bool ShowWarnings = true;

        public const float Object_DelayBeforeRemove = 1f;

        public CameraController MainCamera = null;
        public InputManager InputMan = null;
        public GameController GameController = null;

        public List<Player_Script_HouseInfo> HousesInfo = new List<Player_Script_HouseInfo>();

        public Player_Script LocalPlayer = null;
        public Map_Script Current_Map = null;

        public Transform SelectionIndicatorParent = null;
        public Transform EnduranceIndicatorParent = null;
        public Transform ExplosionsParent = null;
        public Transform ProjectilesParent = null;
        public Transform HouseIndicatorsDefaultParent = null;
        public Transform TypedObjectsParent = null;
        public EnduranceIndicator_Script Default_EnduranceIndicator_Sample = null;
        public SelectionIndicator_Script Default_SelectionIndicator_Sample = null;

        public Vector2Int HouseIndicatorAttachOffset = Vector2Int.zero;
        public BuildingHouseIndicator_Script Default_HouseIndicator = null;
        public GameObject Default_OkPlaceIndicatorSample = null;
        public List<Sprite> Default_HouseIndicatorSprites = null;
        public Sprite Default_HouseIndicatorSprite = null;
        public Material Default_HouseUnitMaterial = null;

        public TypeObjectManager_Script TypeObjectManager = null;

        public const string CarrierTypeName = "Carrier";
        public const int Carrier_UnloadRadius = 10;
        public const int LoadingPlatform_FreeCellToUnloadRadius = 20;
        public const int LoadingPlatform_CarrierLoadRadius = 15;

        public Sprite FogOfWar_FogCellSprite = null;

        public static bool Spice_Inexhaustible = false;
        public static int Spice_CellMaxValue = 1000;
        public List<Sprite> Spice_StateSpriteList = new List<Sprite>();

        public static float Guard_CheckPeriod = 1f;
        public static float Vision_UpdatePeriod = 0.5f;

        public static byte FogOfWar_UnexploredCellOpaque = 255;
        public static byte FogOfWar_FogCellOpaque = 128;
        public static float FogOfWar_FogIOSpeed = 255;
        public static float FogOfWar_VisionTime = 1f;
        public static Color FogOfWar_UnexploredCellColor = Color.black;

        public static Color AtreidesColor = Color.blue;
        public static Color OrdosColor = Color.green;
        public static Color HarkonnenColor = Color.red;
        public static Color FreemanColor = Color.white;
        public static Color ImperatorColor = Color.cyan;

        public static float GUI_Radar_UpdateRate = 1f;

        public static Vector2Int MaxBuildingSize = new Vector2Int(10, 10);
        
    [SerializeField]
        private static GlobalCollector fInstance = null;

    public static GlobalCollector GetInstance()
    {
        return Instance;
    }

    public static GlobalCollector Instance
    {
        get
        {
            return fInstance;
        }
    }
    
    public static bool CheckDistanceBetweenOnCarrierLoadRadius(Vector3 pos1, Vector3 pos2)
    {
        return Vector3.Distance(pos1, pos2) >= LoadingPlatform_CarrierLoadRadius * Cell_Size;
    }

    public Player_Script_HouseInfo HouseInfoById(Player_Script_House id)
    {
        foreach (Player_Script_HouseInfo info in HousesInfo)
            if (info.House == id)
                return info;

        return null;
    }

    // Use this for initialization
    void Awake()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    private void OnValidate()
    {
        if (fInstance == null)
            fInstance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
