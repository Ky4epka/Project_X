using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public NotifyEvent_2P<UnitObject_Script, PointerEventData> OnUnitPointerDown = new NotifyEvent_2P<UnitObject_Script, PointerEventData>();
    public NotifyEvent_2P<MapCell_Script, PointerEventData> OnMapCellPointerDown = new NotifyEvent_2P<MapCell_Script, PointerEventData>();
    public NotifyEvent<KeyCode> OnKeyDown = new NotifyEvent<KeyCode>();
    public NotifyEvent<KeyCode> OnKeyUp = new NotifyEvent<KeyCode>();
    public NotifyEvent<KeyCode> OnKeyPress = new NotifyEvent<KeyCode>();
    public List<KeyCode> ReactKeys = new List<KeyCode>();


    public void SelectionsEvent_OnPointerDown(BaseEventData data)
    {
        PointerEventData pdata = data as PointerEventData;
        GameObject press_obj = pdata.pointerPressRaycast.gameObject;
                
        if (press_obj == null)
            return;
        
        UnitObject_Script unit = press_obj.GetComponent<UnitObject_Script>();
        SelectionIndicator_Script sel_ind;

        if (unit == null)
        {
            sel_ind = press_obj.GetComponent<SelectionIndicator_Script>();

            if (sel_ind != null)
            {
                unit = sel_ind.Owner.Unit_Comp;
            }
        }

        if ((unit != null) && 
            unit.Selectable)
        {
            OnUnitPointerDown.Invoke(unit, pdata);
        }
    }

    public void MapCellsEvent_OnPointerDown(BaseEventData data)
    {
        PointerEventData pdata = data as PointerEventData;
        GameObject press_obj = pdata.pointerPressRaycast.gameObject;

        if (press_obj == null)
            return;

        MapCell_Script cell = press_obj.GetComponent<MapCell_Script>();

        if (cell != null)
        {
            OnMapCellPointerDown.Invoke(cell, pdata);
        }
    }

    

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyCode key in ReactKeys)
        {
            if (Input.GetKeyDown(key))
            {
                OnKeyDown.Invoke(key);
            }

            if (Input.GetKeyUp(key))
            {
                OnKeyUp.Invoke(key);
            }

            if (Input.GetKey(key))
            {
                OnKeyPress.Invoke(key);
            }
        }
    }
}
