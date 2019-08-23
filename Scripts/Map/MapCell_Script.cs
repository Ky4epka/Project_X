using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct MapCell_Script_Editor_CellState
{
    public GameObject StateObject;
    public string StringKey;
}

[System.Serializable] 
public struct MapCell_SerializableParams
{
    public int PathCost;
    public Sprite Sprite;
    public float GroundSpeedScale;
    public float AirSpeedScale;
    public bool CanHasSpice;
    public float SpiceValue;
    public Color CellColor;
    public bool CanBuildingsPlaces;
    public bool CanGroundUnitsPlaces;
}

public class MapCell_Script : MonoBehaviour {

        public int Default_PathCost = 1;
        public Sprite Default_Sprite = null;
        public float Default_GroundSpeedScale = 1f;
        public float Default_AirSpeedScale = 1f;
        public bool Default_CanHasSpice = false;
        public bool Default_ShowSpice = false;
        public float Default_SpiceValue = 0;
        public bool Default_FOW_Use = false;
        public bool Default_FOW_UnexploredUse = false;
        public Color Default_CellColor = Color.black;
        public bool Default_CanBuildingsPlaces = false;
        public bool Default_CanGroundUnitsPlaces = true;

        public GameObject Unit = null;
        public Transform Body = null;
        public BoxCollider2D Collider = null;
        public SpriteRenderer SRenderer = null;
        public SpriteRenderer FogOfWarRenderer = null;
        public GameObject SpiceRendererUnit = null;
        public SpriteRenderer SpiceSpriteRenderer = null;
    
        public List<MapCell_Script_Editor_CellState> EditorStates = new List<MapCell_Script_Editor_CellState>();


        public TextMeshPro tmesh = null;


        private Sprite fSprite = null;
        private Map_Script fOwner = null;
        private bool fSelected = false;
        private bool fHovered = false;
        private bool fBordered = false;
        private int fXMapCoord = -1;
        private int fYMapCoord = -1;
        private int fPathCost = 0;
    // { Fog Of War (FOW) }
        private bool fFOW_Use = false;
        private bool fFOW_UnexploredUse = false;
        private bool fFOW_Unexplored = false;
        private float fFOW_FogOpaque = 0;
        private bool fFOW_ShowCell = false;
        private float fFOW_ShowTime = 0f;
        private int fFOW_OpaqueDir = 0;
        private float fFOW_MaxOpaque = 0f;
        private bool fFOW_ShowTimeBegin = false;
        private List<GameObject> fContainingObjects = new List<GameObject>();
    [SerializeField]
        private bool fCanHasSpice = false;
        private bool fShowSpice = false;
        private float fSpiceValue = 0;
    [SerializeField]
        private Color fCellColor = Color.black;
    private bool fCanBuildingsPlace = false;
    private bool fCanGroundUnitsPlaces = false;
    private float fGroundSpeedScale = 1f;
    private float fAirSpeedScale = 1f;

        private const int EDITOR_STATE_SELECTED = 0;
        private const int EDITOR_STATE_HOVERED = 1;
        private const int EDITOR_STATE_BORDERED = 2;


    /*
     * 
    public int PathCost;
    public string SpriteName;
    public float GroundSpeedScale;
    public float AirSpeedScale;
    public bool CanHasSpice;
    public float SpiceValue;
    public Color CellColor;
    public bool CanBuildingsPlaces;
    public bool CanGroundUnitsPlaces;
     */

    public void ApplySerializableParams(MapCell_SerializableParams _params)
    {
        SetPathCost(_params.PathCost);
        Sprite = _params.Sprite;
        GroundSpeedScale = _params.GroundSpeedScale;
        AirSpeedScale = _params.AirSpeedScale;
        CanHasSpice = _params.CanHasSpice;
        SpiceValue = _params.SpiceValue;
        CellColor = _params.CellColor;
        CanBuildingsPlace = _params.CanBuildingsPlaces;
        CanGroundUnitsPlaces = _params.CanGroundUnitsPlaces;
    }

    public Sprite Sprite
    {
        get
        {
            return SRenderer.sprite;
        }

        set
        {
            SRenderer.sprite = value;
        }
    }
    
    public float GroundSpeedScale
    {
        get
        {
            return fGroundSpeedScale;
        }

        set
        {
            fGroundSpeedScale = value;
        }
    }

    public float AirSpeedScale
    {
        get
        {
            return fAirSpeedScale;
        }

        set
        {
            fAirSpeedScale = value;
        }
    }

    public void SetOwner(Map_Script map)
    {
        fOwner = map;

        if (fOwner == null)
            return;

        if (fOwner.UseUnexploredArea)
            ResetUnexplored();
        else if (fOwner.UseFogOfWar)
            InstantlyShowFog();
        else
            FOW_UpdateOpaque(0);
        
    }

    public Map_Script GetOwner()
    {
        return fOwner;
    }

    public void SetSprite(Sprite s)
    {
        fSprite = s;
        SRenderer.sprite = s;
    }

    public Sprite GetSprite()
    {
        return fSprite;
    }

    public void SetSelected(bool value)
    {
        fSelected = value;

        if (IsEditorMode())
            SetEditorState(EDITOR_STATE_SELECTED, value);
    }

    public bool GetSelected()
    {
        return fSelected;
    }

    public void SetHovered(bool value)
    {
        fHovered = value;

        if (IsEditorMode())
            SetEditorState(EDITOR_STATE_HOVERED, value);
    }

    public bool GetHovered()
    {
        return fHovered;
    }

    public void SetBordered(bool value)
    {
        fBordered = value;

        if (IsEditorMode())
            SetEditorState(EDITOR_STATE_BORDERED, value);
    }

    public bool GetBordered()
    {
        return fBordered;
    }

    public void SetEditorState(int index, bool state)
    {
        if ((index < 0) || (index >= EditorStates.Count))
        {
            Debug.LogError("Unable set editor state! Reason: Invalid editor state index (" + index + ").");
            return;
        }

        EditorStates[index].StateObject.SetActive(state);
    }

    public void SetEditorState(string key, bool state)
    {
        int index = GetEditorStateIndex(key);

        if (index == -1)
        {
            Debug.LogError("Unable set editor state! Reason: Invalid editor state key (" + key + ").");
            return;
        }

        SetEditorState(index, state);
    }

    public bool GetEditorState(int index)
    {
        if ((index < 0) || (index >= EditorStates.Count))
        {
            Debug.LogError("Unable get editor state! Reason: Invalid editor state (" + index + ")!");
            return false;
        }

        return EditorStates[index].StateObject.activeSelf;
    }

    public bool GetEditorState(string key)
    {
        int index = GetEditorStateIndex(key);

        if (index == -1)
        {
            Debug.LogError("Unable get editor state! Reason: Invalid editor state key (" + key + ")");
            return false;
        }

        return GetEditorState(index);
    }

    public int GetEditorStateIndex(string key)
    {
        for (int i=0; i<EditorStates.Count; i++)
            if (EditorStates[i].StringKey.ToUpper() == key.ToUpper())
            {
                return i;
            }

        return -1;
    }

    public void ClearEditorStates()
    {
        for (int i=0; i<EditorStates.Count; i++)
        {
            EditorStates[i].StateObject.SetActive(false);
        }
    }

    public void SetMapCoord(int x, int y)
    {
        fXMapCoord = x;
        fYMapCoord = y;
        Body.position = fOwner.MapCoordToWorld(x, y);
    }

    public void SetMapCoord(Vector2Int coord)
    {
        SetMapCoord(coord.x, coord.y);
    }

    public Vector2Int GetMapCoord()
    {
        return new Vector2Int(fXMapCoord, fYMapCoord);
    }

    public Vector2Int MapCoord
    {
        get
        {
            return GetMapCoord();
        }

        set
        {
            SetMapCoord(value);
        }
    }

    public Vector3 WordlPosition
    {
        get
        {
            return Body.position;
        }
    }

    public Rect WorldBounds
    {
        get
        {
            Rect rect = new Rect();
            rect.center = Body.position;
            rect.size = Vector2.one * GlobalCollector.Cell_Size;
            return rect;
        }
    }
    
    public bool IsPassable()
    {
        return fCanGroundUnitsPlaces;
    }
        
    public void SetPathCost(int value)
    {
        fPathCost = value;
    }

    public int GetPathCost()
    {
        return fPathCost;
    }

    public bool IsEditorMode()
    {
        return EditorStates.Count > 0;
    }

    protected void TryShowSpiceSprite()
    {
        SpiceRendererUnit.SetActive(fShowSpice && (fSpiceValue > 0));

        float koef = (float)fSpiceValue / GlobalCollector.Spice_CellMaxValue;
        int index = (int)(koef * GlobalCollector.Instance.Spice_StateSpriteList.Count);

        if (index >= GlobalCollector.Instance.Spice_StateSpriteList.Count)
            index = GlobalCollector.Instance.Spice_StateSpriteList.Count - 1;

        if (fShowSpice)
            SpiceSpriteRenderer.sprite = GlobalCollector.Instance.Spice_StateSpriteList[index];
    }


    public float SpiceValue
    {
        get
        {
            return fSpiceValue;
        }

        set
        {
            if ((fSpiceValue == value) ||
                !fCanHasSpice)
                return;

            float delta = value - fSpiceValue;

            if (GlobalCollector.Spice_Inexhaustible &&
                (delta < 0))
                return;

            if (value > GlobalCollector.Spice_CellMaxValue)
                value = GlobalCollector.Spice_CellMaxValue;
            else if (value < 0)
                value = 0;

            fSpiceValue = value;

            TryShowSpiceSprite();
        }
    }

    public float TakeSpice(float required_amount)
    {
        if (required_amount < 0)
            return 0;

        float spice = SpiceValue - required_amount;
        float result = required_amount;

        if (spice < 0)
        {
            result = spice + required_amount;
            spice = 0;
        }

        if (!GlobalCollector.Spice_Inexhaustible)
            SpiceValue = spice;

        return result;
    }

    public float ReturnSpice(float amount)
    {
        if (amount < 0)
            return amount;

        float result = amount;
        float spice = SpiceValue + amount;

        if (spice > GlobalCollector.Spice_CellMaxValue)
        {
            result = amount - (spice - GlobalCollector.Spice_CellMaxValue);
            spice = GlobalCollector.Spice_CellMaxValue;
        }

        SpiceValue = spice;
        return result;
    }
    
    public void ResetUnexplored()
    {
        fFOW_Unexplored = true;
        fFOW_MaxOpaque = GlobalCollector.FogOfWar_UnexploredCellOpaque;
        FOW_UpdateOpaque(fFOW_MaxOpaque);
    }

    public void InstantlyShowFog()
    {
        fFOW_MaxOpaque = GlobalCollector.FogOfWar_FogCellOpaque;
        FOW_UpdateOpaque(fFOW_MaxOpaque);
    }

    public void FOW_ShowCell()
    {
        fFOW_ShowCell = true;
        fFOW_ShowTime = 0f;
        fFOW_OpaqueDir = -1;
        fFOW_ShowTimeBegin = false;
    }

    public void FOW_HideCell()
    {
        fFOW_ShowCell = false;
        fFOW_OpaqueDir = 1;
        fFOW_ShowTimeBegin = false;
    }

    public bool FOW_Use
    {
        get
        {
            return fFOW_Use;
        }

        set
        {
            fFOW_Use = value;

            if (fFOW_Use)
            {
                fFOW_MaxOpaque = GlobalCollector.FogOfWar_FogCellOpaque;
                FOW_HideCell();
            }
            else
            {
                fFOW_MaxOpaque = 0;
                FOW_ShowCell();
            }
        }
    }

    public bool FOW_Unexplored_Use
    {
        get
        {
            return fFOW_UnexploredUse;
        }

        set
        {
            fFOW_UnexploredUse = value;

            if (fFOW_UnexploredUse)
            {
                if (fFOW_Unexplored)
                    fFOW_MaxOpaque = GlobalCollector.FogOfWar_UnexploredCellOpaque;

                FOW_HideCell();
            }
            else
            {
                fFOW_MaxOpaque = 0;
                FOW_ShowCell();
            }
        }
    }

    public bool ShowSpice
    {
        get
        {
            return fShowSpice;
        }

        set
        {
            if (fShowSpice == value)
                return;

            fShowSpice = value;

            if (fShowSpice)
                TryShowSpiceSprite();
            else
                SpiceRendererUnit.SetActive(false);
        }
    }

    public bool CanHasSpice
    {
        get
        {
            return fCanHasSpice;
        }

        set
        {
            if (fCanHasSpice == value)
                return;

            if (!fCanHasSpice &&
                (SpiceValue > 0))
                SpiceValue = 0;

            fCanHasSpice = value;
        }
    }
    
    public List<GameObject> ContainingObjects
    {
        get
        {
            return fContainingObjects;
        }
    }

    public Color CellColor
    {
        get
        {
            return fCellColor;
        }

        set
        {
            fCellColor = value;
        }
    }

    public Color DynamicCellColor()
    {
        if (fFOW_Unexplored &&
            fFOW_UnexploredUse)
        {
            return GlobalCollector.FogOfWar_UnexploredCellColor;
        }
        else if (fContainingObjects.Count > 0)
        {
            UnitObject_Script unit = fContainingObjects[fContainingObjects.Count - 1].GetComponent<UnitObject_Script>();

            if (unit != null)
                return unit.Owner.HouseInfo.UnitsCellColor;
        }

        return fCellColor;
    }

    public bool CanBuildingsPlace
    {
        get
        {
            return fCanBuildingsPlace;
        }

        set
        {
            fCanBuildingsPlace = value;
        }
    }

    public bool IsAvailableToBuildings
    {
        get
        {
            return CanBuildingsPlace && 
                (fContainingObjects.Count == 0) &&
                IsPassable();
        }
    }
    
    public void AddContainingObject(GameObject _object)
    {
        if (fContainingObjects.Contains(_object))
            return;

        fContainingObjects.Add(_object);
    }

    public void RemoveContainingObject(GameObject _object)
    {
        fContainingObjects.Remove(_object);
    }

    private void UpdateText()
    {
        string text = "Containing objects: \n";

        for (int i=0; i<fContainingObjects.Count; i++)
        {
            text += fContainingObjects[i].name + "\n";
        }

        tmesh.text = text;
    }

    public bool HasGroundObjects()
    {
        UnitObject_Script unit;

        foreach (GameObject _object in fContainingObjects)
        {
            unit = _object.GetComponent<UnitObject_Script>();

            if ((unit != null) &&
                (unit.IsGroundUnit()))
                return true;
        }

        return false;
    }

    public bool CanGroundUnitsPlaces
    {
        get
        {
            return fCanGroundUnitsPlaces;
        }

        set
        {
            fCanGroundUnitsPlaces = value;
        }
    }

    public bool HasObject(GameObject _object)
    {
        return fContainingObjects.Contains(_object);
    }

    public bool ContainsPoint(Vector3 point)
    {
        return WorldBounds.Contains((Vector2)point);
    }

    public void Event_PointerDown(UnityEngine.EventSystems.BaseEventData data)
    {
        FOW_ShowCell();
    }

    // Editor fields do not copy
    public void Assign(MapCell_Script source)
    {
        SetOwner(source.GetOwner());
        SetSelected(source.GetSelected());
        SetHovered(source.GetHovered());
        SetBordered(source.GetBordered());
        SetMapCoord(source.GetMapCoord());
        SetPathCost(source.GetPathCost());
        SetSprite(source.GetSprite());
    }
    
    protected void FOW_UpdateOpaque(float opaque)
    {
        fFOW_FogOpaque = opaque;
        FogOfWarRenderer.color = new Color32(255, 255, 255, (byte)opaque);
    }

    // Use this for initialization
    void Start () {
        SetPathCost(Default_PathCost);
        SetSprite(Default_Sprite);
        CanHasSpice = Default_CanHasSpice;
        SpiceValue = Default_SpiceValue;
        ShowSpice = Default_ShowSpice;
        FOW_Use = Default_FOW_Use;
        FOW_Unexplored_Use = Default_FOW_UnexploredUse;
        CellColor = Default_CellColor;
        CanBuildingsPlace = Default_CanBuildingsPlaces;
        CanGroundUnitsPlaces = Default_CanGroundUnitsPlaces;
        GroundSpeedScale = Default_GroundSpeedScale;
        AirSpeedScale = Default_AirSpeedScale;

        FogOfWarRenderer.sprite = GlobalCollector.Instance.FogOfWar_FogCellSprite;
	}
    
    // Update is called once per frame
    public void Cell_Update () {
        if (fOwner == null)
            return;

        if (fFOW_OpaqueDir != 0)
        {
            float fow_opaque_delta = GlobalCollector.FogOfWar_FogIOSpeed * Time.deltaTime;
            float opaque = fFOW_FogOpaque;

            opaque += fFOW_OpaqueDir * fow_opaque_delta;

            if (opaque <= 0)
            {
                opaque = 0;
                fFOW_OpaqueDir = 0;

                if (fFOW_Unexplored)
                {
                    fFOW_Unexplored = false;

                    if (fFOW_Use)
                        fFOW_MaxOpaque = GlobalCollector.FogOfWar_FogCellOpaque;
                    else
                        fFOW_MaxOpaque = 0;
                }

                fFOW_ShowTimeBegin = true;
            }
            else if (opaque >= fFOW_MaxOpaque)
            {
                opaque = fFOW_MaxOpaque;
                fFOW_OpaqueDir = 0;
            }

            FOW_UpdateOpaque(opaque);
        }

        if (fFOW_ShowCell && fFOW_ShowTimeBegin)
        {
            fFOW_ShowTime += Time.deltaTime;

            if (fFOW_ShowTime >= GlobalCollector.FogOfWar_VisionTime)
            {
                FOW_HideCell();
            }
        }
	}
}
