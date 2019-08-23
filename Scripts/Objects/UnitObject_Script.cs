using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;



[RequireComponent(typeof(EnduranceObject_Script))]
[RequireComponent(typeof(AnimationController_Script))]
[RequireComponent(typeof(OrderManager_Script))]
public class UnitObject_Script : MonoBehaviour, IPointerDownHandler {

    public Player_Script Default_Owner = null;
    public List<SpriteRenderer> Default_HouseColorRenderers = new List<SpriteRenderer>();

    // Sender, current_player, old_player
    public NotifyEvent_3P<UnitObject_Script, Player_Script, Player_Script> OnOwnerChanged = new NotifyEvent_3P<UnitObject_Script, Player_Script, Player_Script>();
    public NotifyEvent<UnitObject_Script> OnDie = new NotifyEvent<UnitObject_Script>();
    public NotifyEvent_3P<UnitObject_Script, float, float> OnEnduranceChanged = new NotifyEvent_3P<UnitObject_Script, float, float>();
    public NotifyEvent_3P<UnitObject_Script, UnitObject_Script, float> OnTakeDamage = new NotifyEvent_3P<UnitObject_Script, UnitObject_Script, float>();
    public NotifyEvent_3P<UnitObject_Script, UnitObject_Script, float> OnCauseDamage = new NotifyEvent_3P<UnitObject_Script, UnitObject_Script, float>();
    public NotifyEvent_2P<UnitObject_Script, bool> OnCarrierChanged = new NotifyEvent_2P<UnitObject_Script, bool>();
    public NotifyEvent_2P<UnitObject_Script, bool> OnLoadingPlatformChanged = new NotifyEvent_2P<UnitObject_Script, bool>();

    protected Player_Script fOwner = null;
    protected ObjectComponentsCollection_Script fComps = null;
    protected bool fInit = true;
    protected bool fOnCarrier = false;
    protected bool fOnPlatform = false;
    protected bool fVisible = true;

    public Player_Script Owner
    {
        get
        {
            return fOwner;
        }

        set
        {
            if (fOwner != null)
                fOwner.RemoveUnit(this);

            Player_Script old_owner = fOwner;

            if (value == null)
                value = GlobalCollector.Instance.LocalPlayer;

            fOwner = value;

            if (fOwner != null)
            {
                fOwner.AddUnit(this);
                SetDisplayedPlayerColor(fOwner);
            }

            OnOwnerChanged.Invoke(this, value, old_owner);
        }
    }

    public float Endurance
    {
        get
        {
            return fComps.EnduranceObject_Comp.Endurance;
        }

        set
        {
            fComps.EnduranceObject_Comp.Endurance = value;
        }
    }

    public float MaxEndurance
    {
        get
        {
            return fComps.EnduranceObject_Comp.MaxEndurance;
        }

        set
        {
            fComps.EnduranceObject_Comp.MaxEndurance = value;
        }
    }

    public Vector3 Position
    {
        get
        {
            if (fComps.MovingObject_Comp == null)
                return fComps.GameObject_Comp.Position;

            return fComps.MovingObject_Comp.Position;
        }

        set
        {

            if (fComps.MovingObject_Comp != null)
                fComps.MovingObject_Comp.Position = value;
            else
                fComps.GameObject_Comp.Body.position = value;
        }
    }

    public Vector3 Direction
    {
        get
        {
            if (fComps.RotatingObject_Comp == null)
                return fComps.GameObject_Comp.Body.right;
            else
                return fComps.RotatingObject_Comp.Direction;
        }

        set
        {
            if (fComps.RotatingObject_Comp != null)
                fComps.RotatingObject_Comp.Direction = value;
            else
                fComps.GameObject_Comp.Body.right = value;
        }
    }

    public Vector2Int MapPosition
    {
        get
        {
            return fComps.MovingObject_Comp.MapPosition;
        }

        set
        {
            fComps.MovingObject_Comp.MapPosition = value;
        }
    }

    public bool Invulnerable
    {
        get
        {
            return fComps.EnduranceObject_Comp.Invulnerable;
        }

        set
        {
            fComps.EnduranceObject_Comp.Invulnerable = value;
        }
    }

    public bool Selectable
    {
        get
        {
            return (fComps.SelectableObject_Comp != null) && (fComps.SelectableObject_Comp.Selectable);
        }

        set
        {
            if (fComps.SelectableObject_Comp == null)
                return;

            if (!value)
                fOwner.UnselectUnit(this);

            fComps.SelectableObject_Comp.Selectable = value;
        }
    }

    public bool UseVision
    {
        get
        {
            if (fComps.VisionManager_Comp == null)
                return false;

            return fComps.VisionManager_Comp.Use;
        }

        set
        {
            if (fComps.VisionManager_Comp != null)
                fComps.VisionManager_Comp.Use = value;
        }
    }

    public bool OnCarrier
    {
        get
        {
            return fOnCarrier;
        }

        set
        {
            if (fOnCarrier == value)
                return;

            fOnCarrier = value;
            DisallowOrdering = value;
            DisallowPurposeful = value;
            DisableMoveTracking = value;
            Selectable = !value;
            ShowEnduranceIndicator = !value;
            UseVision = !value;

            OnCarrierChanged.Invoke(this, fOnCarrier);
        }
    }

    public bool OnPlatform
    {
        get
        {
            return fOnPlatform;
        }

        set
        {
            if (fOnPlatform == value)
                return;

            fOnPlatform = value;
            DisallowOrdering = value;
            DisallowPurposeful = value;
            DisableMoveTracking = value;
            Selectable = !value;
            ShowEnduranceIndicator = !value;
            UseVision = !value;
            OnLoadingPlatformChanged.Invoke(this, fOnPlatform);
        }
    }

    public void ChangeOnCarrierToPlatform()
    {
        if (fOnPlatform || !fOnCarrier)
        {
            return;
        }

        fOnPlatform = true;
        fOnCarrier = false;
    }

    public void ChangeOnPlatformToCarrier()
    {
        if (fOnCarrier || !fOnPlatform)
        {
            return;
        }

        fOnCarrier = true;
        fOnPlatform = false;
    }

    public bool DisallowOrdering
    {
        get
        {
            return fComps.OrderManager_Comp.DisallowOrdering;
        }
        set
        {
            fComps.OrderManager_Comp.DisallowOrdering = value;
        }
    }

    public bool DisallowPurposeful
    {
        get
        {
            return fComps.OrderManager_Comp.DisallowPurposeful;
        }

        set
        {
            fComps.OrderManager_Comp.DisallowPurposeful = value;
        }
    }

    public bool DisableMoveTracking
    {
        get
        {
            return fComps.OrderManager_Comp.DisableMoveTracking;
        }

        set
        {
            fComps.OrderManager_Comp.DisableMoveTracking = value;
        }
    }


    public bool CanMove
    {
        get
        {
            return fComps.OrderManager_Comp.CanMove;
        }
    }

    public bool CanAttack
    {
        get
        {
            return fComps.OrderManager_Comp.CanAttack;
        }
    }

    public bool CanProduce
    {
        get
        {
            return (fComps.Production_Comp != null) && 
                   (fComps.Production_Comp.ItemCount > 0);
        }
    }

    public bool Visible
    {
        get
        {
            return fVisible;
        }

        set
        {
            if (fVisible == value)
                return;

            fVisible = value;

            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer renderer in renderers)
            {
                renderer.enabled = value;
            }
        }
    }

    public bool ShowEnduranceIndicator
    {
        get
        {
            if (fComps.EnduranceIndicator_Comp != null)
                return fComps.EnduranceIndicator_Comp.Visible;
            else
                return false;
        }

        set
        {
            if (fComps.EnduranceIndicator_Comp != null)
                fComps.EnduranceIndicator_Comp.Visible = value;
        }
    }

    public BuildingObject_Script Building_Comp
    {
        get
        {
            return fComps.BuildingObject_Comp;
        }
    }

    public void DoDamage(UnitObject_Script source, float value)
    {
        fComps.EnduranceObject_Comp.DoDamage(source.fComps.EnduranceObject_Comp, value);
    }

    public void SetSelected(Player_Script player, bool value)
    {
        if (player == GlobalCollector.Instance.LocalPlayer)
        {
            SelectableObject_Script_VisualSelectionState state = SelectableObject_Script_VisualSelectionState.Owner;

            if (player == Owner)
            {
                state = SelectableObject_Script_VisualSelectionState.Owner;
            }
            else if (player.IsAlly(Owner))
            {
                state = SelectableObject_Script_VisualSelectionState.Ally;
            }
            else
            {
                state = SelectableObject_Script_VisualSelectionState.Enemy;
            }

            fComps.SelectableObject_Comp.SetSelected(value, state);
        }
    }

    public bool GetSelecteds()
    {
        return fComps.SelectableObject_Comp.GetSelected();
    }

    public bool IsAlive()
    {
        return fComps.EnduranceObject_Comp.IsAlive;
    }

    public bool IsDie()
    {
        return !IsAlive();
    }

    public bool IsGroundUnit()
    {
        return (fComps.MovingObject_Comp != null) && (fComps.MovingObject_Comp.MoveType == MovingObject_Script_MoveType.Ground);
    }

    public void Order(OrderManager_Script_OrderData data)
    {
        fComps.OrderManager_Comp.Order(data);
    }

    public void Order_Follow(Vector3 target)
    {
        fComps.OrderManager_Comp.Order_Follow(target);
    }

    public void Order_Follow(UnitObject_Script target)
    {
        fComps.OrderManager_Comp.Order_Follow(target);
    }

    public void Order_Attack(Vector3 target)
    {
        fComps.OrderManager_Comp.Order_Attack(target);
    }

    public void Order_Attack(UnitObject_Script target)
    {
        fComps.OrderManager_Comp.Order_Attack(target);
    }

    public void Order_Cancel()
    {
        fComps.OrderManager_Comp.Order_Cancel();
    }

    public bool IsBuilding()
    {
        return fComps.BuildingObject_Comp != null;
    }

    public void Kill()
    {
        fComps.EnduranceObject_Comp.Kill();
    }

    public void SetDisplayedPlayerColor(Player_Script player)
    {
        Material mat = GlobalCollector.Instance.Default_HouseUnitMaterial;

        if ((player != null) &&
            (player.HouseInfo.UnitsMaterial != null))
            mat = player.HouseInfo.UnitsMaterial;
        
        foreach (SpriteRenderer renderer in Default_HouseColorRenderers)
        {
            renderer.material = mat;
        }
    }

    public ObjectComponentsCollection_Script CompCollection
    {
        get
        {
            return fComps;
        }
    }
    
    public void Event_OnDie(EnduranceObject_Script sender)
    {
        Selectable = false;

        if (fComps.AnimController_Comp != null)
            fComps.AnimController_Comp.SetAnimationId(AnimationController_Script_AnimationId.Destroy, false);
    }

    public void Event_OnEnduranceChanged(EnduranceObject_Script sender, float old_value, float new_value)
    {
        OnEnduranceChanged.Invoke(this, old_value, new_value);
    }

    public void Event_OnTakeDamage(EnduranceObject_Script source, EnduranceObject_Script reciever, float value)
    {
        OnTakeDamage.Invoke(source.GetComponent<UnitObject_Script>(), reciever.GetComponent<UnitObject_Script>(), value);
    }

    public void Event_OnCauseDamage(EnduranceObject_Script reciever, EnduranceObject_Script source, float value)
    {
        OnCauseDamage.Invoke(reciever.GetComponent<UnitObject_Script>(), source.GetComponent<UnitObject_Script>(), value);
    }

    public void Event_OnAnimationStop(AnimationController_Script sender, AnimationController_Script_AnimationId anim_id)
    {
        switch (anim_id)
        {
            case AnimationController_Script_AnimationId.Destroy:
                DoDestroy();
                break;
        }
    }

    public void Event_OnAnimationCancel(AnimationController_Script sender, AnimationController_Script_AnimationId anim_id)
    {
        switch (anim_id)
        {
            case AnimationController_Script_AnimationId.Destroy:
                DoDestroy();
                break;
        }
    }

    public void OnPointerDown(PointerEventData pdata)
    {
        GlobalCollector.Instance.InputMan.SelectionsEvent_OnPointerDown(pdata);
    }

    public ObjectTypeInfo_Script ObjectTypeInfo
    {
        get
        {
            return this.GetComponent<ObjectTypeInfo_Script>();
        }
    }
        
    protected void DoDestroy()
    {
        TypeObjectManager_Script.DestroyObjectInstance(this.GetComponent<ObjectTypeInfo_Script>(), GlobalCollector.Object_DelayBeforeRemove);
    }

    // Use this for initialization
    void Start () {
        fComps = this.GetComponent<ObjectComponentsCollection_Script>();

        Owner = Default_Owner;
	}

    private void OnDestroy()
    {
        fComps.EnduranceObject_Comp.OnDie.RemoveListener(Event_OnDie);

        if (fComps.AnimController_Comp != null)
        {
            fComps.AnimController_Comp.OnEndAnimation.RemoveListener(Event_OnAnimationStop);
            fComps.AnimController_Comp.OnCancelAnimation.RemoveListener(Event_OnAnimationCancel);
        }
    }

    // Update is called once per frame
    void Update () {
		if (fInit)
        {
            fInit = false;
            fComps.EnduranceObject_Comp.OnDie.AddListener(Event_OnDie);
            fComps.EnduranceObject_Comp.OnEnduranceChanged.AddListener(Event_OnEnduranceChanged);

            if (fComps.AnimController_Comp != null)
            {
                fComps.AnimController_Comp.OnEndAnimation.AddListener(Event_OnAnimationStop);
                fComps.AnimController_Comp.OnCancelAnimation.AddListener(Event_OnAnimationCancel);
            }
        }
	}
}
