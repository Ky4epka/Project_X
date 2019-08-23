using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager_Script : MonoBehaviour
{
    public List<Player_Script> Default_Players = new List<Player_Script>();

    protected PlayerGroup_Script fPlayers = new PlayerGroup_Script();

    public void InputManagerEvent_OnUnitPointerDown(UnitObject_Script unit, PointerEventData data)
    {
        Player_Script local = GlobalCollector.Instance.LocalPlayer;


        if (data.button == PointerEventData.InputButton.Left)
        {
            if (local.HasSelected() && 
                local.IsSelectedSelfUnits() &&
                !local.IsSelectedBuildings())
            {
                if (!local.IsAlly(unit.Owner))
                {
                    local.Selected.Order_Attack(unit);
                }
                else
                {
                    local.Selected.Order_Follow(unit);
                }
            }
            else
            {
                local.SelectUnit(unit);
            }
        }
        else if (data.button == PointerEventData.InputButton.Right)
        {
            local.ClearSelection();
        }
    }

    public void InputManagerEvent_OnMapCellPointerDown(MapCell_Script map_cell, PointerEventData data)
    {
        Player_Script local = GlobalCollector.Instance.LocalPlayer;

        if (data.button == PointerEventData.InputButton.Left)
        {
            if (local.HasSelected() && 
                (local.IsSelectedSelfUnits()))
            {
                local.Selected.Order_Attack(map_cell.Body.position);
            }
        }
        else if (data.button == PointerEventData.InputButton.Right)
        {
            local.ClearSelection();
        }
    }

    // Use this for initialization
    void Start()
    {
        GlobalCollector.Instance.InputMan.OnUnitPointerDown.AddListener(InputManagerEvent_OnUnitPointerDown);
        GlobalCollector.Instance.InputMan.OnMapCellPointerDown.AddListener(InputManagerEvent_OnMapCellPointerDown);

        fPlayers.AddRange(Default_Players);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
