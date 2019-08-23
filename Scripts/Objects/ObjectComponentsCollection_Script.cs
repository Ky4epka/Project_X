using UnityEngine;
using System.Collections;

public class ObjectComponentsCollection_Script : MonoBehaviour
{
    public AttackingObjectTower_Script Direct_Tower_Comp = null;

    [SerializeField]
    private GameObject_Script fGameObject_Comp = null;
    [SerializeField]
    private EnduranceObject_Script fEnduranceObject_Comp = null;
    [SerializeField]
    private RotatingObject_Script fRotatingObject_Comp = null;
    [SerializeField]
    private MovingObject_Script fMovingObject_Comp = null;
    [SerializeField]
    private AttackingObject_Script fAttackingObject_Comp = null;
    [SerializeField]
    private OrderManager_Script fOrderManager_Comp = null;
    [SerializeField]
    private VisionManager_Script fVisionManager_Comp = null;
    [SerializeField]
    private ObjectDebugInfo_Script fDebugInfo = null;
    [SerializeField]
    private EnduranceIndicator_Script fEnduranceIndicator_Comp = null;
    [SerializeField]
    private ObjectAttaching_Script fAttachingManager_Comp = null;
    [SerializeField]
    private SelectableObject_Script fSelectableObject_Comp = null;
    [SerializeField]
    private BuildingObject_Script fBuildingObject_Comp = null;
    [SerializeField]
    private UnitObject_Script fUnit_Comp = null;
    [SerializeField]
    private AnimationController_Script fAnimController_Comp = null;
    [SerializeField]
    private CarrierLoadingPlatform_Script fCarrierLoadingPlatform_Comp = null;
    [SerializeField]
    private AttackingObjectTower_Script fAttackingTower_Comp = null;
    [SerializeField]
    private ProductionObject_Script fProduction_Comp = null;
    
    public GameObject_Script GameObject_Comp
    {
        get
        {
            return fGameObject_Comp;
        }
    }

    public EnduranceObject_Script EnduranceObject_Comp
    {
        get
        {
            return fEnduranceObject_Comp;
        }
    }

    public RotatingObject_Script RotatingObject_Comp
    {
        get
        {
            return fRotatingObject_Comp;
        }
    }

    public MovingObject_Script MovingObject_Comp
    {
        get
        {
            return fMovingObject_Comp;
        }
    }

    public AttackingObject_Script AttackingObject_Comp
    {
        get
        {
            return fAttackingObject_Comp;
        }
    }

    public OrderManager_Script OrderManager_Comp
    {
        get
        {
            return fOrderManager_Comp;
        }
    }

    public AttackingObjectTower_Script Tower_Comp
    {
        get
        {
            return fAttackingTower_Comp;
        }
    }

    public VisionManager_Script VisionManager_Comp
    {
        get
        {
            return fVisionManager_Comp;
        }
    }

    public ObjectDebugInfo_Script DebugInfo
    {
        get
        {
            return fDebugInfo;
        }
    }
    
    public EnduranceIndicator_Script EnduranceIndicator_Comp
    {
        get
        {
            return fEnduranceIndicator_Comp;
        }
    }


    public ObjectAttaching_Script AttachingManager_Comp
    {
        get
        {
            return fAttachingManager_Comp;
        }
    }

    public SelectableObject_Script SelectableObject_Comp
    {
        get
        {
            return fSelectableObject_Comp;
        }
    }

    public BuildingObject_Script BuildingObject_Comp
    {
        get
        {
            return fBuildingObject_Comp;
        }
    }

    public UnitObject_Script Unit_Comp
    {
        get
        {
            return fUnit_Comp;
        }
    }

    public AnimationController_Script AnimController_Comp
    {
        get
        {
            return fAnimController_Comp;
        }
    }

    public CarrierLoadingPlatform_Script CarrierLoadingPlatform_Comp
    {
        get
        {
            return fCarrierLoadingPlatform_Comp;
        }
    }

    public ProductionObject_Script Production_Comp
    {
        get
        {
            return fProduction_Comp;
        }
    }

    public void SetDebugInfo(GameObject obj)
    {
        fDebugInfo = obj.GetComponent<ObjectDebugInfo_Script>();
    }

    public void SetEnduranceIndicator(GameObject obj)
    {
        fEnduranceIndicator_Comp = obj.GetComponent<EnduranceIndicator_Script>();
    }




    public void Initialize()
    {
        fGameObject_Comp = this.GetComponent<GameObject_Script>();
        fEnduranceObject_Comp = this.GetComponent<EnduranceObject_Script>();
        fRotatingObject_Comp = this.GetComponent<RotatingObject_Script>();
        fMovingObject_Comp = this.GetComponent<MovingObject_Script>();
        fAttackingObject_Comp = this.GetComponent<AttackingObject_Script>();
        fOrderManager_Comp = this.GetComponent<OrderManager_Script>();
        fVisionManager_Comp = this.GetComponent<VisionManager_Script>();
        fAttachingManager_Comp = this.GetComponent<ObjectAttaching_Script>();
        fSelectableObject_Comp = this.GetComponent<SelectableObject_Script>();
        fBuildingObject_Comp = this.GetComponent<BuildingObject_Script>();
        fUnit_Comp = this.GetComponent<UnitObject_Script>();
        fAnimController_Comp = this.GetComponent<AnimationController_Script>();
        fCarrierLoadingPlatform_Comp = this.GetComponent<CarrierLoadingPlatform_Script>();
        fProduction_Comp = this.GetComponent<ProductionObject_Script>();

        if (Direct_Tower_Comp == null)
            fAttackingTower_Comp = this.GetComponent<AttackingObjectTower_Script>();
        else
            fAttackingTower_Comp = Direct_Tower_Comp;
    }

    private void Start()
    {
//        Initialize();
    }

    private void OnValidate()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
