using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrderManager_Script_OrderType
{
    None = 0,
    Follow,
    Attack
}

public enum OrderManager_Script_OrderTargetType
{
    None = 0,
    Point,
    Object
}

public struct OrderManager_Script_OrderData
{
    public OrderManager_Script_OrderType OrderType;

    private OrderManager_Script_OrderTargetType fTargetType;
    private Vector3 fPoint;
    private UnitObject_Script fUnit;

    public Vector3 Point
    {
        get
        {
            return fPoint;
        }

        set
        {
            if (fTargetType == OrderManager_Script_OrderTargetType.Point)
                fPoint = value;
        }
    }

    public UnitObject_Script Unit
    {
        get
        {
            return fUnit;
        }

        set
        {
            if (fTargetType == OrderManager_Script_OrderTargetType.Object)
                fUnit = value;
        }
    }

    public OrderManager_Script_OrderTargetType TargetType
    {
        get
        {
            return fTargetType;
        }
    }

    public void ResetOrderType()
    {
        OrderType = OrderManager_Script_OrderType.None;
    }

    public void SetTargetType()
    {
        fTargetType = OrderManager_Script_OrderTargetType.None;
        fPoint = Vector3.zero;
        fUnit = null;
    }

    public void SetTargetType(Vector3 point)
    {
        fTargetType = OrderManager_Script_OrderTargetType.Point;
        fPoint = point;
        fUnit = null;
    }

    public void SetTargetType(UnitObject_Script _object)
    {
        fTargetType = OrderManager_Script_OrderTargetType.Object;
        fPoint = Vector3.zero;
        fUnit = _object;
    }
    
    public OrderManager_Script_OrderData(OrderManager_Script_OrderType order_type, Vector3 point)
    {
        OrderType = order_type;
        fTargetType = OrderManager_Script_OrderTargetType.Point;
        fPoint = point;
        fUnit = null;
    }

    public OrderManager_Script_OrderData(OrderManager_Script_OrderType order_type, UnitObject_Script unit)
    {
        OrderType = order_type;
        fTargetType = OrderManager_Script_OrderTargetType.Object;
        fPoint = Vector3.zero;
        fUnit = unit;
    }
}

[RequireComponent(typeof(GameObject_Script))]
[RequireComponent(typeof(UnitObject_Script))]
public class OrderManager_Script : MonoBehaviour {

    public int Default_GuardRadius = 1;
    public int Default_PursuitRadius = 1;

    public delegate bool OrderPerform(OrderManager_Script sender, ref OrderManager_Script_OrderData order);

    public OrderPerform OnOrderPerform = null;
    public NotifyEvent_2P<OrderManager_Script, OrderManager_Script_OrderData> OnOrder = new NotifyEvent_2P<OrderManager_Script, OrderManager_Script_OrderData>();
    public NotifyEvent_2P<OrderManager_Script, OrderManager_Script_OrderData> OnOrderCancel = new NotifyEvent_2P<OrderManager_Script, OrderManager_Script_OrderData>();
    public OrderPerform OnTakeAimOrderPerform = null;
    public NotifyEvent_3P<OrderManager_Script, OrderManager_Script, OrderManager_Script_OrderData> OnTakeAim = new NotifyEvent_3P<OrderManager_Script, OrderManager_Script, OrderManager_Script_OrderData>();
    public NotifyEvent_3P<OrderManager_Script, OrderManager_Script, OrderManager_Script_OrderData> OnTakeAimCancel = new NotifyEvent_3P<OrderManager_Script, OrderManager_Script, OrderManager_Script_OrderData>();
    public NotifyEvent_2P<OrderManager_Script, OrderManager_Script_OrderData> OnOrderComplete = new NotifyEvent_2P<OrderManager_Script, OrderManager_Script_OrderData>();

    [SerializeField]
    protected UnitObject_Script fUnitObject_Comp = null;
    [SerializeField]
    protected UnitGroup_Script fMatchGroup = new UnitGroup_Script();
    [SerializeField]
    protected float fAttackTime = 0f;
    [SerializeField]
    protected int fGuardRadius = 0;
    [SerializeField]
    protected bool fPursuit = false;
    [SerializeField]
    protected int fPursuitRadius = 0;
    [SerializeField]
    protected Vector3 fGuardLastPos = Vector3.zero;
    [SerializeField]
    protected bool fDisallowOrdering = false;
    [SerializeField]
    protected bool fDisallowPurposeful = false;
    [SerializeField]
    protected List<OrderManager_Script> fPurposefulList = new List<OrderManager_Script>();
    [SerializeField]
    protected OrderManager_Script_OrderData fOrderData = new OrderManager_Script_OrderData();
    [SerializeField]
    protected OrderManager_Script_OrderData fOrderDataBuffer = new OrderManager_Script_OrderData();
    protected bool fHandle = false;

    public void Order_Follow(Vector3 target)
    {
        fOrderDataBuffer.OrderType = OrderManager_Script_OrderType.Follow;
        fOrderDataBuffer.SetTargetType(target);
        Order(fOrderDataBuffer);
    }

    public void Order_Follow(UnitObject_Script target)
    {
        fOrderDataBuffer.OrderType = OrderManager_Script_OrderType.Follow;
        fOrderDataBuffer.SetTargetType(target);
        Order(fOrderDataBuffer);
    }

    public void Order_Attack(Vector3 target)
    {
        fOrderDataBuffer.OrderType = OrderManager_Script_OrderType.Attack;
        fOrderDataBuffer.SetTargetType(target);
        Order(fOrderDataBuffer);
    }

    public void Order_Attack(UnitObject_Script target)
    {
        fOrderDataBuffer.OrderType = OrderManager_Script_OrderType.Attack;
        fOrderDataBuffer.SetTargetType(target);
        Order(fOrderDataBuffer);
    }

    public void Order(OrderManager_Script_OrderData order)
    {
        Order_Cancel();

        if (order.OrderType == OrderManager_Script_OrderType.None)
            return;

        fOrderDataBuffer = order;
        
        if ((OnOrderPerform != null) && 
            (!OnOrderPerform(this, ref fOrderDataBuffer)))
        {
            return;
        }

        switch (order.TargetType)
        {
            case OrderManager_Script_OrderTargetType.Point:
                if (CanMove && 
                    fUnitObject_Comp.CompCollection.MovingObject_Comp.SetTarget(order.Point) &&
                    fUnitObject_Comp.CompCollection.Tower_Comp.TowerType == AttackingObjectTower_Script_TowerTypes.FreeRotate)
                {
                    fUnitObject_Comp.CompCollection.Tower_Comp.RotateToBodyDirection();
                }
                break;
            case OrderManager_Script_OrderTargetType.Object:
                fOrderDataBuffer = order;
                fOrderDataBuffer.Unit = this.Unit;
                
                if (!IsCorrectTarget(order.Unit) ||
                    ((order.Unit.CompCollection.OrderManager_Comp.OnTakeAimOrderPerform != null) &&
                    (!order.Unit.CompCollection.OrderManager_Comp.OnTakeAimOrderPerform(order.Unit.CompCollection.OrderManager_Comp, ref fOrderDataBuffer))))
                    break;

                switch (order.OrderType)
                {
                    case OrderManager_Script_OrderType.Follow:
                        if (CanMove && 
                            !order.Unit.DisallowPurposeful &&
                            fUnitObject_Comp.CompCollection.MovingObject_Comp.SetTarget(order.Unit.CompCollection))
                        {
                            DoTargetUnit(order.Unit.CompCollection.OrderManager_Comp);
                        }
                        break;
                    case OrderManager_Script_OrderType.Attack:
                        if (order.Unit.DisallowPurposeful)
                            break;

                        if ((CanAttack &&
                            fUnitObject_Comp.CompCollection.AttackingObject_Comp.SetTarget(order.Unit.CompCollection)) || 
                            (CanMove &&
                            fUnitObject_Comp.CompCollection.MovingObject_Comp.SetTarget(order.Unit.CompCollection)))
                        {
                            DoTargetUnit(order.Unit.CompCollection.OrderManager_Comp);
                        }
                        break;
                }
                break;
        }

        fOrderData = order;
        OnOrder.Invoke(this, fOrderData);
    }

    public void Order_Cancel()
    {
        if (fOrderData.Unit != null)
        {
            fOrderData.Unit.CompCollection.OrderManager_Comp.fPurposefulList.Remove(this);
            fOrderData.Unit.CompCollection.OrderManager_Comp.OnTakeAimCancel.Invoke(fOrderData.Unit.CompCollection.OrderManager_Comp, this, fOrderData);
            fOrderData.Unit.OnDie.RemoveListener(Event_OnTargetUnitDie);
        }

        OnOrderCancel.Invoke(this, fOrderData);
        fOrderData.OrderType = OrderManager_Script_OrderType.None;
        fOrderData.SetTargetType();
        fPursuit = false;

        if (fUnitObject_Comp.CompCollection.MovingObject_Comp != null)
            fUnitObject_Comp.CompCollection.MovingObject_Comp.CancelTarget();

        if (fUnitObject_Comp.CompCollection.AttackingObject_Comp != null)
            fUnitObject_Comp.CompCollection.AttackingObject_Comp.CancelTarget();
    }
    
    public bool IsCorrectTarget(UnitObject_Script target)
    {
        return (target != null) && (!target.IsDie());
    }

    public int GuardRadius
    {
        get
        {
            return fGuardRadius;
        }

        set
        {
            fGuardRadius = value;
        }
    }

    public int PursuitRadius
    {
        get
        {
            return fPursuitRadius;
        }

        set
        {
            fPursuitRadius = value;
        }
    }

    public bool DisallowOrdering
    {
        get
        {
            return fDisallowOrdering;
        }

        set
        {
            if (value == fDisallowOrdering)
                return;

            fDisallowOrdering = value;
            
            if (fDisallowOrdering)
                Order_Cancel();
        }
    }

    public bool DisallowPurposeful
    {
        get
        {
            return fDisallowPurposeful;
        }

        set
        {
            if (value == fDisallowPurposeful)
                return;

            fDisallowPurposeful = value;

            if (fDisallowPurposeful)
                CancelPurposefulList();
        }
    }

    public bool DisableMoveTracking
    {
        get
        {
            return (fUnitObject_Comp.CompCollection.MovingObject_Comp != null) && (fUnitObject_Comp.CompCollection.MovingObject_Comp.DisableTracking);
        }

        set
        {
            if (fUnitObject_Comp.CompCollection.MovingObject_Comp == null)
                return;

            fUnitObject_Comp.CompCollection.MovingObject_Comp.DisableTracking = value;
        }
    }

    public bool CanMove
    {
        get
        {
            return (!fDisallowOrdering) && (fUnitObject_Comp.CompCollection.MovingObject_Comp != null) && (!fUnitObject_Comp.CompCollection.MovingObject_Comp.DisallowTarget);
        }
    }

    public bool CanAttack
    {
        get
        {
            return (!fDisallowOrdering) && (fUnitObject_Comp.CompCollection.AttackingObject_Comp != null) && (!fUnitObject_Comp.CompCollection.AttackingObject_Comp.DisallowTarget);
        }
    }

    public UnitObject_Script Unit
    {
        get
        {
            return fUnitObject_Comp;
        }
    }

    public void Event_OnTargetUnitDie(UnitObject_Script sender)
    {
        Order_Cancel();
    }

    protected void DoTargetUnit(OrderManager_Script target)
    {
        target.fPurposefulList.Add(this);
        target.OnTakeAim.Invoke(target, this, fOrderData);
        target.Unit.OnDie.AddListener(Event_OnTargetUnitDie);
    }
    
    public void CancelPurposefulList()
    {
        foreach (OrderManager_Script item in fPurposefulList)
        {
            item.Order_Cancel();
        }

        fPurposefulList.Clear();
    }

    public void Event_OnTargetAchieved(MovingObject_Script sender)
    {
        OnOrderComplete.Invoke(this, fOrderData);
    }
    
    protected void Order_PursuitAttack(UnitObject_Script target)
    {
        fGuardLastPos = fUnitObject_Comp.CompCollection.GameObject_Comp.Body.position;
        Order_Attack(target);
        fPursuit = (fUnitObject_Comp.CompCollection.MovingObject_Comp != null) && (!fUnitObject_Comp.CompCollection.MovingObject_Comp.DisallowTarget);
    }

    UnitObject_Script TargetNeeded()
    {
        fMatchGroup.UnitsInRadius(fUnitObject_Comp.CompCollection.GameObject_Comp.Body.position, fGuardRadius);

        return fMatchGroup.FirstEnemy(fUnitObject_Comp.CompCollection.Unit_Comp.Owner);
    }
    
    protected IEnumerator DoPostInit()
    {
        yield return null;

        if (fUnitObject_Comp.CompCollection.MovingObject_Comp != null)
        {
            fUnitObject_Comp.CompCollection.MovingObject_Comp.OnTargetAchieved.AddListener(Event_OnTargetAchieved);
        }
    }

    // Use this for initialization
    protected void Start () {
        fUnitObject_Comp = this.GetComponent<UnitObject_Script>();
        GuardRadius = Default_GuardRadius;
        PursuitRadius = Default_PursuitRadius;
        StartCoroutine(DoPostInit());
	}
	
	// Update is called once per frame
	void Update () {
        if (!CanAttack)
            return;

        fAttackTime += Time.deltaTime;

        if (fAttackTime >= GlobalCollector.Guard_CheckPeriod)
        {
            if (fPursuit)
            {
                if (Vector3.Distance(fGuardLastPos, fUnitObject_Comp.CompCollection.GameObject_Comp.Body.position) >= fPursuitRadius * GlobalCollector.Cell_Size)
                {
                    fPursuit = false;
                    Order_Attack(fGuardLastPos);
                }
            }
            else if (!fUnitObject_Comp.CompCollection.AttackingObject_Comp.HasTarget() && 
                     ((fUnitObject_Comp.CompCollection.MovingObject_Comp == null) || (!fUnitObject_Comp.CompCollection.MovingObject_Comp.HasTarget())))
            {
                fAttackTime = 0;
                UnitObject_Script unit = TargetNeeded();

                if (unit != null)
                {
                    Order_PursuitAttack(unit);
                }
            }
            else
            {
                if (((fUnitObject_Comp.CompCollection.MovingObject_Comp == null) ||
                     fUnitObject_Comp.CompCollection.MovingObject_Comp.DisallowTarget) && 
                    (Vector3.Distance(fUnitObject_Comp.CompCollection.GameObject_Comp.Body.position, fUnitObject_Comp.CompCollection.AttackingObject_Comp.Target.GameObject_Comp.Body.position) >=
                     fUnitObject_Comp.CompCollection.AttackingObject_Comp.AttackRadius_Max * GlobalCollector.Cell_Size))
                {
                    Order_Cancel();
                }
            }
        }
	}
}
