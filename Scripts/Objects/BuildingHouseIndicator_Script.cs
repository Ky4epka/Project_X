using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BuildingHouseIndicator_Script : MonoBehaviour {

        public float RotateSpeed = 1f;

        private Transform fBody = null;
        private GameObject fUnit = null;
        private SpriteRenderer fSRenderer = null;
        private BuildingObject_Script fBuilding = null;


    public void Attach(BuildingObject_Script building)
    {
        if (fBuilding != null)
            fBuilding.Unit.OnOwnerChanged.RemoveListener(Event_OnOwnerChanged);

        fBuilding = building;

        if (fBuilding != null)
        {
            fBody.parent = building.Unit.CompCollection.GameObject_Comp.Body;
            building.Unit.OnOwnerChanged.AddListener(Event_OnOwnerChanged);
        }
        else
        {
            fBody.parent = GlobalCollector.Instance.HouseIndicatorsDefaultParent;
        }

        UpdateIndicator();
    }

    public Transform Body
    {
        get
        {
            return fBody;
        }
    }

    public GameObject Unit
    {
        get
        {
            return fUnit;
        }
    }

    public void Event_OnOwnerChanged(UnitObject_Script sender, Player_Script new_owner, Player_Script old_owner)
    {
        UpdateIndicator();
    }

    protected void UpdateIndicator()
    {
        if (fBuilding == null)
        {
            fSRenderer.sprite = null;
            Debug.LogWarning(name + ": Cannot update indicator! Reason: No parent building");
            return;
        }

        fSRenderer.sprite = GlobalCollector.Instance.Default_HouseIndicatorSprite;

        if (fBuilding.Unit.Owner.HouseInfo.UnitsMaterial == null)
            fSRenderer.material = GlobalCollector.Instance.Default_HouseUnitMaterial;
        else
            fSRenderer.material = fBuilding.Unit.Owner.HouseInfo.UnitsMaterial;
    }

	// Use this for initialization
	void Awake() {
        fBody = this.transform;
        fUnit = this.gameObject;
        fSRenderer = this.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        fBody.Rotate(0f, 0f, -RotateSpeed * Time.deltaTime);
	}
}
