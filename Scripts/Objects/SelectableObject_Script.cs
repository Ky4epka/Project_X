using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[System.Serializable]
public enum SelectableObject_Script_VisualSelectionState
{
    Owner,
    Ally,
    Enemy
}

[RequireComponent(typeof(ObjectComponentsCollection_Script))]
[RequireComponent(typeof(ObjectAttaching_Script))]
public class SelectableObject_Script : MonoBehaviour {
    
        public UnityEvent OnPreSelection = new UnityEvent();
        public UnityEvent OnSelected = new UnityEvent();
        public SelectionIndicator_Script Indicator_Sample = null;

    [SerializeField]
    private ObjectComponentsCollection_Script fComps = null;
    [SerializeField]
    private SelectableObject_Script_VisualSelectionState fVisualState = SelectableObject_Script_VisualSelectionState.Owner;
    [SerializeField]
    private bool fPreSelection_CancelSelectAction = false;
    [SerializeField]
    private SelectionIndicator_Script fIndicator = null;
    [SerializeField]
    protected bool fSelectable = true;
    

    public void SetSelected(bool value, SelectableObject_Script_VisualSelectionState visual_state)
    {
        if (!Selectable)
            return;

        fPreSelection_CancelSelectAction = false;
        OnPreSelection.Invoke();

        if (fPreSelection_CancelSelectAction)
            return;

        fVisualState = visual_state;
        fIndicator.SetSelected(value, visual_state);
        OnSelected.Invoke();        
    }

    public bool GetSelected()
    {
        return fIndicator.GetSelected();
    }

    public SelectableObject_Script_VisualSelectionState GetVisualState()
    {
        return fVisualState;
    }

    public bool PreSelection_CancelSelectAction
    {
        get
        {
            return fPreSelection_CancelSelectAction;
        }

        set
        {
            fPreSelection_CancelSelectAction = value;
        }
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

            if (!fSelectable)
                SetSelected(false, SelectableObject_Script_VisualSelectionState.Owner);

            fComps.GameObject_Comp.Body_Collider.enabled = value;
            fIndicator.Selectable = value;
        }
    }

    IEnumerator DoPostInit()
    {
        yield return null;
        fIndicator = SelectionIndicator_Script.Instantiate(Indicator_Sample, GlobalCollector.Instance.SelectionIndicatorParent);
        fIndicator.gameObject.SetActive(true);
        fIndicator.Owner = fComps;
        
        if (fIndicator != null)
        {
            fComps.AttachingManager_Comp.AddAttach(fIndicator.transform, fIndicator.ToString(), 0, Vector3.zero);
        }
    }

    // Use this for initialization
    void Start () {
        fComps = this.GetComponent<ObjectComponentsCollection_Script>();

        if (Indicator_Sample == null)
            Indicator_Sample = GlobalCollector.Instance.Default_SelectionIndicator_Sample;

        fIndicator = SelectionIndicator_Script.Instantiate(Indicator_Sample, GlobalCollector.Instance.SelectionIndicatorParent);
        fIndicator.gameObject.SetActive(true);
        fIndicator.Owner = fComps;

        if (fIndicator != null)
        {
            fComps.AttachingManager_Comp.AddAttach(fIndicator.transform, fIndicator.ToString(), 0, Vector3.zero);
        }
    }
	
	// Update is called once per frame
	void Update () {
	}
}
