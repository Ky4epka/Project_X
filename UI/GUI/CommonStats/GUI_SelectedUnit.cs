using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_SelectedUnit : MonoBehaviour {

    public GameObject Direct_BackgroundUnit = null;
    public IconType Direct_IconType = null;
    public FillingBar Direct_FillingBar = null;
    public GUI_ProductionItemList Direct_ProductionList = null;
    public GUI_QueryList Direct_QueryList = null;
    public GUI_BuildingPlacing Direct_BuildingPlacing = null;

    protected Player_Script fBindedPlayer = null;
    protected UnitObject_Script fSelectedUnit = null;
    protected GameObject fIconTypeUnit = null;
    protected GameObject fFillingBarUnit = null;
    protected GUI_ProductionItemList fProductionList = null;
    protected GUI_QueryList fQueryList = null;
    protected GUI_BuildingPlacing fBuildingPlacing = null;
    protected IconType fIconType = null;
    protected FillingBar fFillingBar = null;


    public Player_Script BindedPlayer
    {
        get
        {
            return fBindedPlayer;
        }

        set
        {
            SelectedUnit = null;

            if (fBindedPlayer != null)
            {
                fBindedPlayer.OnSelectionChanged.RemoveListener(Event_OnSelectionChanged);
            }

            fBindedPlayer = value;

            if (fBindedPlayer != null)
            {
                fBindedPlayer.OnSelectionChanged.AddListener(Event_OnSelectionChanged);
            }
        }
    }

    public UnitObject_Script SelectedUnit
    {
        get
        {
            return fSelectedUnit;
        }

        set
        {
            if (fProductionList != null)
                fProductionList.Clear();

            if (fQueryList != null)
                fQueryList.ProductionObject = null;

            if (fSelectedUnit != null)
            {
                fSelectedUnit.OnDie.RemoveListener(Event_OnUnitDie);
                fSelectedUnit.OnEnduranceChanged.RemoveListener(Event_OnEnduranceChanged);

                if (fSelectedUnit.IsBuilding() &&
                    fSelectedUnit.Building_Comp.Production_Comp != null)
                    fSelectedUnit.Building_Comp.Production_Comp.OnProductionListChanged.RemoveListener(Event_ProductionListChanged);

                if (fBuildingPlacing != null)
                    fBuildingPlacing.ProductionBuilding = null;
            }

            fSelectedUnit = value;

            if (fSelectedUnit != null)
            {
                fSelectedUnit.OnDie.AddListener(Event_OnUnitDie);
                fSelectedUnit.OnEnduranceChanged.AddListener(Event_OnEnduranceChanged);

                if (fSelectedUnit.IsBuilding() &&
                    fSelectedUnit.Building_Comp.Production_Comp != null)
                {
                    fSelectedUnit.Building_Comp.Production_Comp.OnProductionListChanged.AddListener(Event_ProductionListChanged);
                    fProductionList.ItemsFromBuildingProductionObject(fSelectedUnit.Building_Comp.Production_Comp);
                }

                if ((fSelectedUnit != null) &&
                    (fSelectedUnit.IsBuilding() &&
                    fSelectedUnit.CanProduce))
                {
                    if (fQueryList != null)
                        fQueryList.ProductionObject = fSelectedUnit.Building_Comp.Production_Comp;

                    if (fBuildingPlacing != null)
                        fBuildingPlacing.ProductionBuilding = fSelectedUnit.Building_Comp;
                }
            }

            UpdateUI();
        }
    }
    
    public IconType IconType
    {
        get
        {
            return fIconType;
        }

        set
        {
            fIconType = value;

            if (fIconType != null)
                fIconTypeUnit = fIconType.gameObject;
            else
                fIconTypeUnit = null;
        }
    }

    public FillingBar FillingBar
    {
        get
        {
            return fFillingBar;
        }

        set
        {
            fFillingBar = value;

            if (fFillingBar != null)
                fFillingBarUnit = fFillingBar.gameObject;
            else
                fFillingBarUnit = null;
        }
    }
    
    public GUI_ProductionItemList ProductionList
    {
        get
        {
            return fProductionList;
        }

        set
        {
            if (fProductionList != null)
            {
                fProductionList.OnItemClick.RemoveListener(Event_OnProductionItemClicked);
            }

            fProductionList = value;

            if (fProductionList != null)
            {
                fProductionList.OnItemClick.AddListener(Event_OnProductionItemClicked);
            }
        }
    }

    public GUI_QueryList QueryList
    {
        get
        {
            return fQueryList;
        }

        set
        {
            if (fQueryList != null)
            {
                fQueryList.ProductionObject = null;
                fQueryList.OnItemClicked.RemoveListener(Event_OnQueryItemClicked);
            }

            fQueryList = value;

            if (fQueryList != null)
            {
                if ((fSelectedUnit != null) &&
                    (fSelectedUnit.IsBuilding() &&
                    fSelectedUnit.CanProduce))
                {
                    fQueryList.ProductionObject = fSelectedUnit.Building_Comp.Production_Comp;
                }

                fQueryList.OnItemClicked.AddListener(Event_OnQueryItemClicked);
            }
        }
    }

    public GUI_BuildingPlacing BuildingPlacing
    {
        get
        {
            return fBuildingPlacing;
        }

        set
        {
            fBuildingPlacing = value;
        }
    }

    public void Event_OnUnitDie(UnitObject_Script unit)
    {
        SelectedUnit = null;
    }

    public void Event_OnSelectionChanged(Player_Script sender)
    {
        if (sender.Selected.Count > 0)
            SelectedUnit = sender.Selected[0];
        else
            SelectedUnit = null;
    }

    public void Event_OnEnduranceChanged(UnitObject_Script sender, float old_value, float new_value)
    {
        UpdateUI();
    }

    public void Event_ProductionListChanged(ProductionObject_Script sender)
    {
        UpdateUI();
    }

    public void Event_OnProductionItemClicked(GUI_ProductionItemList sender, GUI_ProductionItem item)
    {
        if ((fSelectedUnit != null) &&
            fSelectedUnit.IsBuilding() &&
            fSelectedUnit.Building_Comp.Production_Comp != null)
        {
            if (fSelectedUnit.Owner.ParametersValues.CurrentSpice.Value >= item.TypeInfo.TypeData.Base_Cost)
            {
                //fSelectedUnit.Owner.ParametersValues.CurrentSpice.Sub(item.TypeInfo.TypeData.Base_Cost);
            }

            fSelectedUnit.Building_Comp.Production_Comp.ProductItem(item.TypeInfo.TypeName);
        }
    }

    public void Event_OnQueryItemClicked(GUI_QueryList sender, GUI_QueryItem item)
    {
        if (item.QueryItem != null)
            item.QueryItem.Cancel();

        fQueryList.ProductionObject.ProductionQuery.DeleteItem(item.QueryItem);
    }

    protected virtual void UpdateUI()
    {
        bool sel_unit_null = fSelectedUnit == null;

        if (fIconType != null)
        {
            if (sel_unit_null)
            {
                fIconType.ObjectType = null;
            }
            else
                fIconType.ObjectType = fSelectedUnit.ObjectTypeInfo.ObjectType;
        }

        if (fFillingBar != null)
        {

            if (sel_unit_null)
            {
                fFillingBar.Min = 0;
                fFillingBar.Max = 1;
                fFillingBar.Progress = 0;
            }
            else
            {
                fFillingBar.Min = 0;
                fFillingBar.Max = (int)fSelectedUnit.MaxEndurance;
                fFillingBar.Progress = (int)fSelectedUnit.Endurance;
            }
        }

        if (fProductionList != null)
        {
            fProductionList.Clear();

            if (sel_unit_null ||
                !fSelectedUnit.IsBuilding() ||
                !fSelectedUnit.CanProduce)
            {
                fProductionList.Visible = false;
            }
            else 
            {
                fProductionList.Visible = true;
                fProductionList.ItemsFromBuildingProductionObject(fSelectedUnit.Building_Comp.Production_Comp);
            }
        }

        if (fQueryList != null)
        {
            if (sel_unit_null ||
                !fSelectedUnit.IsBuilding() ||
                !fSelectedUnit.CanProduce)
            {
                fQueryList.Visible = false;
            }
            else
            {
                fQueryList.Visible = true;
            }
        }

        if (fBuildingPlacing != null)
        {
            if (sel_unit_null ||
                !fSelectedUnit.IsBuilding() ||
                !fSelectedUnit.CanProduce)
            {
            }
        }

        Direct_BackgroundUnit.SetActive(!sel_unit_null);
        fFillingBarUnit.SetActive(!sel_unit_null);
        fIconTypeUnit.SetActive(!sel_unit_null);
    }

    // Use this for initialization
    void Start () {
        IconType = Direct_IconType;
        FillingBar = Direct_FillingBar;
        ProductionList = Direct_ProductionList;
        QueryList = Direct_QueryList;
        BuildingPlacing = Direct_BuildingPlacing;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
