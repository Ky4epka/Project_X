using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ObjectWeaponProjectile_Script : HoldTargetBase_Script
{
    public GameObject_Script GameObject_Comp = null;
    public MovingObject_Script Move_Comp = null;
    public AnimationController_Script Renderer_Comp = null;

    public ObjectComponentsCollection_Script Owner = null;
    public ObjectWeapon_Script WeaponOwner = null;

    public float Damage = 1f;
    public float DelayBeforeDestroy = 1f;
    public float MaxPathValue = 0f;
    

    public UnityEvent OnLaunch = null;
    public UnityEvent OnFinish = null;

    public bool fLaunched = false;
    public bool fFinished = false;


    public void SetTransformData(Transform body)
    {
        GameObject_Comp.Body.position = body.position;
        GameObject_Comp.Body.rotation = body.rotation;
        GameObject_Comp.Body.parent = GlobalCollector.Instance.ProjectilesParent;
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        Move_Comp.SetTarget(target);
        DoLaunch();
        return true;
    }

    public virtual bool SetTarget(Vector3 target)
    {
        base.CancelTarget();
        
        Move_Comp.SetTarget(target);
        DoLaunch();
        return true;
    }

    public override bool CancelTarget()
    {
        if (base.CancelTarget())
        {
            fLaunched = false;
            return true;
        }

        return false;
    }

    public bool IsLaunched()
    {
        return fLaunched;
    }
    
    public bool Finished
    {
        get
        {
            return fFinished;
        }
    }
    
    private void DoLaunch()
    {
        Renderer_Comp.SetAnimationId(AnimationController_Script_AnimationId.Born, false);
        Move_Comp.MaxPathValue = MaxPathValue;
    }

    private void DoDestroy()
    {
        if (fFinished)
            return;

        fFinished = true;
        Renderer_Comp.SetAnimationId(AnimationController_Script_AnimationId.Destroy, false);
        OnFinish.Invoke();
        Move_Comp.CancelTarget();
        GameObject.Destroy(GameObject_Comp.Unit, DelayBeforeDestroy);
    }

    public void OnBodyOutOfMapBounds(MovingObject_Script sender)
    {
        DoDestroy();
    }

    public void OnBodyOutOfPath(MovingObject_Script sender)
    {
        DoDestroy();
    }

    public void OnBodyTargetAchieved(MovingObject_Script sender)
    {
        DoDestroy();
    }

    public void OnBodyCollision(MovingObject_Script sender, Collider2D[] col_list, int count)
    {
        for (int i=0; i<count; i++)
        {
            if (fFinished)
                return;

            Collider2D col = col_list[i];

            if (col.gameObject == Owner.GameObject_Comp.Unit)
                continue;

            ObjectComponentsCollection_Script comp = col.gameObject.GetComponent<ObjectComponentsCollection_Script>();

            if ((comp == null) || (comp.EnduranceObject_Comp == null) || (!comp.EnduranceObject_Comp.IsAlive))
                continue;

            comp.EnduranceObject_Comp.DoDamage(Owner.EnduranceObject_Comp, Damage);
            DoDestroy();
            break;
        }
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        Move_Comp.OnOutOfMapBounds.AddListener(OnBodyOutOfMapBounds);
        Move_Comp.OnOutOfPath.AddListener(OnBodyOutOfPath);
        Move_Comp.OnTargetAchieved.AddListener(OnBodyTargetAchieved);
        Move_Comp.OnObjectCollission.AddListener(OnBodyCollision);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
