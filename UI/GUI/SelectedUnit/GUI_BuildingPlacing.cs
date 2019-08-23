using UnityEngine;
using System.Collections;

public class GUI_BuildingPlacing : MonoBehaviour
{
    public GUI_BuildingPlacingArea Direct_BuildingPlacingArea = null;

    [SerializeField]
    protected GUI_BuildingPlacingArea fBuildingPlacingArea = null;
    [SerializeField]
    protected BuildingObject_Script fProductionBuilding = null;

    public GUI_BuildingPlacingArea BuildingPlacingArea
    {
        get
        {
            return fBuildingPlacingArea;
        }

        set
        {
            if (fBuildingPlacingArea != null)
            {
                fBuildingPlacingArea.OnPlace.RemoveListener(Event_OnBuildingPlacingArea_Place);
                fBuildingPlacingArea.OnCancel.RemoveListener(Event_OnBuildingPlacingArea_Cancel);
            }

            fBuildingPlacingArea = value;

            if (fBuildingPlacingArea != null)
            {
                fBuildingPlacingArea.OnPlace.AddListener(Event_OnBuildingPlacingArea_Place);
                fBuildingPlacingArea.OnCancel.AddListener(Event_OnBuildingPlacingArea_Cancel);
            }
        }
    }

    public BuildingObject_Script ProductionBuilding
    {
        get
        {
            return fProductionBuilding;
        }

        set
        {
            if (fProductionBuilding != null)
            {
                fProductionBuilding.OnProducedBuildingsListChanged.RemoveListener(Event_OnProducingBuildingListChanged);
                fProductionBuilding.OnCurrentProducedBuldingPlaced.RemoveListener(Event_OnCurrentProducedBuildingPlaced);
                fProductionBuilding.OnCurrentProducedBuildingBeginPlacing.RemoveListener(Event_OnCurrentProducedBuildingBeginPlacing);
                fProductionBuilding.OnCurrentProducedBuildingCancelPlacing.RemoveListener(Event_OnCurrentProducedBuildingCancelPlacing);
                fProductionBuilding.CancelPlacingCurrentProducedBuilding();
                fBuildingPlacingArea.HidePlacingArea();
            }

            fProductionBuilding = value;

            if (fProductionBuilding != null)
            {
                fProductionBuilding.OnProducedBuildingsListChanged.AddListener(Event_OnProducingBuildingListChanged);
                fProductionBuilding.OnCurrentProducedBuldingPlaced.AddListener(Event_OnCurrentProducedBuildingPlaced);
                fProductionBuilding.OnCurrentProducedBuildingBeginPlacing.AddListener(Event_OnCurrentProducedBuildingBeginPlacing);
                fProductionBuilding.OnCurrentProducedBuildingCancelPlacing.AddListener(Event_OnCurrentProducedBuildingCancelPlacing);
            }

            UpdateUI();
        }
    }

    IEnumerator DoUpdateUI()
    {
        yield return null;

        UpdateUI();
    }

    public void Event_OnProducingBuildingListChanged(BuildingObject_Script sender)
    {
        StartCoroutine(DoUpdateUI());
    }

    public void Event_OnCurrentProducedBuildingPlaced(BuildingObject_Script sender)
    {
    }

    public void Event_OnCurrentProducedBuildingBeginPlacing(BuildingObject_Script sender)
    {
    }

    public void Event_OnCurrentProducedBuildingCancelPlacing(BuildingObject_Script sender)
    {

    }

    public void Event_OnBuildingPlacingArea_Place(GUI_BuildingPlacingArea sender)
    {
        if (fBuildingPlacingArea == null)
            return;

        fProductionBuilding.PlaceCurrentProducedBuilding(fBuildingPlacingArea.PlacingPivot);
    }
    
    public void Event_OnBuildingPlacingArea_Cancel(GUI_BuildingPlacingArea sender)
    {
    }

    protected void UpdateUI()
    {
        if (fProductionBuilding != null)
        {
            BuildingObject_Script building = fProductionBuilding.TakeFirstProducedBuildingForPlace();

            if (building != null)
                fBuildingPlacingArea.ShowPlacingArea(building.Size);
            else
                fBuildingPlacingArea.HidePlacingArea();
        }
        else
        {
            fBuildingPlacingArea.HidePlacingArea();
        }
    }

    IEnumerator DoInit()
    {
        yield return null;
        BuildingPlacingArea = Direct_BuildingPlacingArea;
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(DoInit());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
