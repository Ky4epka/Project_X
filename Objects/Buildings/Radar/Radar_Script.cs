using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnergyDependentObject_Script))]
[RequireComponent(typeof(BuildingObject_Script))]
public class Radar_Script : MonoBehaviour {

    protected BuildingObject_Script fBuilding_Comp = null;
    protected Player_Script fBindedPlayer = null;

    public Player_Script BindedPlayer
    {
        get
        {
            return fBindedPlayer;
        }

        set
        {
            bool radar_state = false;

            if (fBindedPlayer != null)
            {
                fBindedPlayer.ParametersValues.OnEnergyParamsChanged.RemoveListener(Event_OnEnergyParametersChanged);

                UnitGroup_Script list = new UnitGroup_Script();
                ObjectTypeInfo_Script obj_info = this.GetComponent<ObjectTypeInfo_Script>();
                fBindedPlayer.Units.MatchUnitsByTypeName(obj_info.TypeInfo.TypeName, ref list);

                radar_state = list.Count == 0;
            }

            fBindedPlayer = value;

            if (fBindedPlayer != null)
            {
                fBindedPlayer.ParametersValues.OnEnergyParamsChanged.AddListener(Event_OnEnergyParametersChanged);
                radar_state = !fBindedPlayer.ParametersValues.EnergyOverflows;
            }

            UpdateRadarState(radar_state);
        }
    }

    public void Event_OnOwnerChanged(UnitObject_Script sender, Player_Script new_owner, Player_Script old_owner)
    {
        BindedPlayer = new_owner;
    }

    public void Event_OnEnergyParametersChanged(Player_Script sender)
    {
        UpdateRadarState(!sender.ParametersValues.EnergyOverflows);
    }

    public void Event_OnDie(UnitObject_Script sender)
    {
        BindedPlayer = null;
    }

    public void Event_OnUIInitialized(UI_Script sender)
    {
        fBuilding_Comp.Unit.OnDie.AddListener(Event_OnDie);
        fBuilding_Comp.Unit.OnOwnerChanged.AddListener(Event_OnOwnerChanged);
        BindedPlayer = fBuilding_Comp.Unit.Owner;
    }

    protected virtual void UpdateRadarState(bool active)
    {
        GlobalCollector.Instance.GameController.UI.GUI.Radar.Active = active;
    }


    IEnumerator DoInit()
    {
        yield return null;

        if (!GlobalCollector.Instance.GameController.UI.Initialized)
            GlobalCollector.Instance.GameController.UI.OnInitialize.AddListener(Event_OnUIInitialized);
        else
            Event_OnUIInitialized(GlobalCollector.Instance.GameController.UI);
    }

	// Use this for initialization
	void Start () {
        fBuilding_Comp = this.GetComponent<BuildingObject_Script>();
        StartCoroutine(DoInit());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
