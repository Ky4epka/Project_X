using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class SelectionIndicator_Script : MonoBehaviour {

        public NotifyEvent_2P<SelectionIndicator_Script, PointerEventData> OnPointerDown = new NotifyEvent_2P<SelectionIndicator_Script, PointerEventData>();

        public Sprite [] Default_VisualSelectionSprites = new Sprite[System.Enum.GetNames(typeof(SelectableObject_Script_VisualSelectionState)).Length];

    [SerializeField]
    protected Sprite[] fVisualSelectionSprites = new Sprite[System.Enum.GetNames(typeof(SelectableObject_Script_VisualSelectionState)).Length];

    [SerializeField]
    protected ObjectComponentsCollection_Script fOwner = null;
    [SerializeField]
    protected SpriteRenderer fRenderer = null;
    [SerializeField]
    protected BoxCollider2D fCollider = null;
    [SerializeField]
    protected bool fSelected = false;
    [SerializeField]
    protected SelectableObject_Script_VisualSelectionState fVisualState = SelectableObject_Script_VisualSelectionState.Owner;
    [SerializeField]
    protected bool fSelectable = true;
    

    public ObjectComponentsCollection_Script Owner
    {
        get
        {
            return fOwner;
        }

        set
        {
            fOwner = value;
        }
    }

    public void SetSelected(bool value, SelectableObject_Script_VisualSelectionState state)
    {
        if (!fSelectable)
            return;

        if (fSelected == value)
            return;

        fSelected = value;
        fVisualState = state;


        if (fSelected)
            fRenderer.sprite = fVisualSelectionSprites[(int)state];
        else
            fRenderer.sprite = null;
    }

    public bool GetSelected()
    {
        return fSelected;
    }

    public SelectableObject_Script_VisualSelectionState GetVisualSelectionState()
    {
        return fVisualState;
    }

    public bool Selectable
    {
        get
        {
            return fSelectable;
        }

        set
        {
            if (fSelectable == value)
                return;

            fSelectable = value;
            fRenderer.enabled = value;
            fCollider.enabled = value;
        }
    }

    public void SetVisualSelectionSprites(Sprite[] collection)
    {
        fVisualSelectionSprites = collection.Clone() as Sprite[];
    }

    public void Event_OnPointerDown(BaseEventData data)
    {
        OnPointerDown.Invoke(this, (PointerEventData)data);
    }

	// Use this for initialization
	void Start () {
        fRenderer = this.GetComponent<SpriteRenderer>();
        fCollider = this.GetComponent<BoxCollider2D>();

        SetVisualSelectionSprites(Default_VisualSelectionSprites);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
