using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapCellEditor_Script : MonoBehaviour {

        public MapCell_Script Cell = null;

        public List<Toggle> Toggles = null;

        public MovingObject_Script MovingObject = null;
        public ObjectComponentsCollection_Script Object = null;

        private const int TOGGLE_SET_START = 0;
        private const int TOGGLE_SET_END = 1;
        private const int TOGGLE_SET_INPASSABLE = 2;
        private const int TOGGLE_SELECTED = 3;
        private const int TOGGLE_MOVE_TO = 4;
        private const int TOGGLE_ADD_OBJECT = 5;
        private const int TOGGLE_MOVE_FOR = 6;
        private const int TOGGLE_ATTACK = 7;


    public void OnPointerEnter(BaseEventData data)
    {
        Cell.SetHovered(true);
    }

    public void OnPointerExit(BaseEventData data)
    {
        Cell.SetHovered(false);
    }

    public void OnPointerDown(BaseEventData data)
    {
        int sindex = GetSelectedToggleIndex();

        switch (sindex)
        {
            case -1:
                //Debug.Log("no toggles selected");
                break;
            case TOGGLE_SELECTED:
                Cell.SetEditorState("selected", !Cell.GetEditorState("selected"));
                break;
            case TOGGLE_SET_START:
                Cell.SetEditorState("start_cell", !Cell.GetEditorState("start_cell"));
                break;
            case TOGGLE_SET_END:
                Cell.SetEditorState("end_cell", !Cell.GetEditorState("end_cell"));
                break;
            case TOGGLE_SET_INPASSABLE:
                Cell.SetEditorState("impassable_cell", !Cell.GetEditorState("impassable_cell"));
                break;
            case TOGGLE_MOVE_TO:
                Debug.Log("Move to: " + Cell.Body.position);
                SelectableObject_Script selobj = null;// SelManager.GetFirstSelected();

                if (selobj == null)
                    break;

                ObjectComponentsCollection_Script mobj = selobj.GetComponent<ObjectComponentsCollection_Script>();

                if (mobj != null)
                {
                    Cell.SetEditorState("end_cell", true);
                    mobj.MovingObject_Comp.SetTarget(Cell.Body.position);
                }

                break;

            case TOGGLE_ADD_OBJECT:
                ObjectComponentsCollection_Script obj = ObjectComponentsCollection_Script.Instantiate(Object);
                obj.GameObject_Comp.Body.position = Cell.Body.position;
                obj.GameObject_Comp.Body.rotation = new Quaternion();
                break;

            case TOGGLE_MOVE_FOR:
                break;

            case TOGGLE_ATTACK:
                break;

            default:
               // SelManager.ClearSelection();
                break;
        }
    }

    public void OnObjectPreSelection(SelectableObject_Script obj)
    {
        /*
        if (SelManager.GetFirstSelected() != null)
        {
            SelManager.PreSelection_CancelSelect = true;

            int sindex = GetSelectedToggleIndex();
            switch (sindex)
            {
                case TOGGLE_MOVE_FOR:
                    SelectableObject_Script selobj = obj;
                    SelectableObject_Script fselobj = SelManager.GetFirstSelected();
                    ObjectComponentsCollection_Script tmobj = selobj.GetComponent<ObjectComponentsCollection_Script>();
                    ObjectComponentsCollection_Script mobj = fselobj.GetComponent<ObjectComponentsCollection_Script>();

                    if (mobj != null)
                    {
                        mobj.MovingObject_Comp.SetTarget(tmobj, 0);
                    }

                    break;
                case TOGGLE_ATTACK:
                    selobj = obj;
                    fselobj = SelManager.GetFirstSelected();
                    ObjectComponentsCollection_Script aobj = fselobj.GetComponent<ObjectComponentsCollection_Script>();
                    ObjectComponentsCollection_Script tobj = selobj.GetComponent<ObjectComponentsCollection_Script>();

                    if (aobj != null)
                    {
                        aobj.AttackingObject_Comp.SetTarget(tobj);
                    }
                    break;
            }
        } */
    }

    public void OnObjectSelected(SelectableObject_Script selected)
    {

    }

    int GetSelectedToggleIndex()
    {
        for (int i = 0; i < Toggles.Count; i++)
            if (Toggles[i].isOn)
                return i;

        return -1;
    }

	// Use this for initialization
	void Start () {
       // SelManager.OnSelected.AddListener(OnObjectSelected);
      //  SelManager.OnPreSelection.AddListener(OnObjectPreSelection);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
