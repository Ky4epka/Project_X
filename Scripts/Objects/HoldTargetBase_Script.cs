using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectComponentsCollection_Script))]
public class HoldTargetBase_Script : MonoBehaviour {

        public NotifyEvent_2P<HoldTargetBase_Script, ObjectComponentsCollection_Script> OnTargetChange = new NotifyEvent_2P<HoldTargetBase_Script, ObjectComponentsCollection_Script>();
        public NotifyEvent<HoldTargetBase_Script> OnTargetCancel = new NotifyEvent<HoldTargetBase_Script>();

    [SerializeField]
    protected GameObject fUnit = null;
    [SerializeField]
    protected ObjectComponentsCollection_Script fTarget_Comp = null;
    [SerializeField]
    protected HoldTargetBase_Script fBaseTarget = null;
    [SerializeField]
    protected bool fDisallowTarget = false;


    public virtual bool SetTarget(ObjectComponentsCollection_Script target)
    {
        if ((target == fTarget_Comp) ||
            fDisallowTarget)
            return false;

        CancelTarget();

        if ((target == null) ||
            (target == fUnit)
           )
            return false;

        fTarget_Comp = target;
        fTarget_Comp.EnduranceObject_Comp.OnDie.AddListener(OnTargetDie);
        OnTargetChange.Invoke(this, target);
        return true;
    }

    public virtual ObjectComponentsCollection_Script GetTarget()
    {
        return fTarget_Comp;
    }

    public ObjectComponentsCollection_Script Target
    {
        get
        {
            return GetTarget();
        }

        set
        {
            SetTarget(value);
        }
    }

    public virtual bool CancelTarget()
    {
        if (fTarget_Comp != null)
        {
            fTarget_Comp.EnduranceObject_Comp.OnDie.RemoveListener(OnTargetDie);
            fTarget_Comp = null;
            OnTargetCancel.Invoke(this);
        }

        return true;
    }

    public virtual bool HasTarget()
    {
        return (fTarget_Comp != null);
    }

    public bool DisallowTarget
    {
        get
        {
            return fDisallowTarget;
        }

        set
        {
            if (fDisallowTarget == value)
                return;

            fDisallowTarget = value;

            if (fDisallowTarget)
                CancelTarget();
        }
    }

    public void OnTargetDie(EnduranceObject_Script sender)
    {
        CancelTarget();
    }
    
	// Use this for initialization
	protected virtual void Start () {
        fUnit = this.gameObject;
	}

    private void OnDestroy()
    {
        CancelTarget();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
