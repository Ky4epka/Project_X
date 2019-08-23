using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TEST_CarrierAbility : MonoBehaviour {

    public int radius = 0;

    public bool MatchCell(MapCell_Script cell, object arg)
    {
        bool result = Vector3.Distance(cell.WordlPosition, (arg as MapCell_Script).WordlPosition) >= (radius) * GlobalCollector.Cell_Size;

        return result;
    }

    public void Event_OnCellDown(MapCell_Script sender, PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Middle)
        {
            List<string> list = new List<string>();
            list.Add("Devastator");
            list.Add("Tank");
            list.Add("SiegeTank");
            GlobalCollector.Instance.LocalPlayer.CarrierManager.Request_OrderToPoint(sender.WordlPosition, list);
        }
        else if (data.button == PointerEventData.InputButton.Right)
        {
            GlobalCollector.Instance.Current_Map.DistributeSpice(sender.WordlPosition, 10000);
        }
    }

    public void Event_OnUnitDown(UnitObject_Script sender, PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Middle)
        {
            BuildingObject_Script building = sender.GetComponent<BuildingObject_Script>();

            if ((building != null) &&
                (building.LoadingPlatform != null))
            {
                List<string> list = new List<string>();
                list.Add("Devastator");
                list.Add("Tank");
                list.Add("SiegeTank");
                GlobalCollector.Instance.LocalPlayer.CarrierManager.Request_OrderToPlatform(building.LoadingPlatform, list);
            }
        }
    }

	// Use this for initialization
	void Start () {
        //GlobalCollector.Instance.InputMan.OnMapCellPointerDown.AddListener(Event_OnCellDown);
        //GlobalCollector.Instance.InputMan.OnUnitPointerDown.AddListener(Event_OnUnitDown);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
