using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum ObjectWeapon_Script_ActivationMethods
{
    TargetRadius,
    Endurance,
    TargetEndurance
}

[System.Serializable]
public enum ObjectWeapon_Script_ActivationMethodConditions
{
    InRange,
    OutRange,
    LessThan,
    MoreThan,
    Equals
}

public class ObjectWeapon_Script : HoldTargetBase_Script
{
    public AttackingObjectTower_Script Default_TowerOwner = null;
    public ObjectWeaponProjectile_Script Projectile_Sample = null;

    public Vector2Int UnitsRange = Vector2Int.zero;
    public int ProjectilesCount = 1;
    public float AttackingConeAngle = 1f;
    public ObjectWeapon_Script_ActivationMethods ActivationMethod = ObjectWeapon_Script_ActivationMethods.TargetRadius;
    public ObjectWeapon_Script_ActivationMethodConditions ActivationMethodCondition = ObjectWeapon_Script_ActivationMethodConditions.LessThan;
    public AnimationController_Script_AnimationId Tower_FireAnimId = AnimationController_Script_AnimationId.Attack_1;
    public List<float> ReloadSequence = new List<float>();

    public AttackingObjectTower_Script fTower = null;
    public bool fActive = false;
    public bool fFreeze = false;
    public int fSequenceIndex = -1;
    public float fSequenceTime = 0f;
    public bool fAttackConditionMatch = false;
    public int fFixedFramesCount = 0;

    private const int FFC_ATTACK_CONDITION_MATCH_RATE = 10;
    
    public void SetActivate(bool value)
    {
        if (fActive == value)
            return;

        fActive = value;
    }

    public bool GetActivate()
    {
        return fActive;
    }

    public void SetFreeze(bool value)
    {
        if (fFreeze == value)
            return;

        fFreeze = value;
    }

    public bool GetFreeze()
    {
        return fFreeze;
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        if (base.SetTarget(target))
        {
            return true;
        }

        return false;
    }

    public override bool CancelTarget()
    {
        if (base.CancelTarget())
        {
            return true;
        }

        return false;
    }

    public void SetTower(AttackingObjectTower_Script tower)
    {
        if (fTower == tower)
            return;

        fTower = tower;            
    }

    public AttackingObjectTower_Script GetTower()
    {
        return fTower;
    }

    public bool CheckAttackConditions()
    {
        if (Mathf.Abs(fTower.GetAngleDelta()) > AttackingConeAngle)
        {
            return false;
        }

        switch (ActivationMethod)
        {
            case ObjectWeapon_Script_ActivationMethods.TargetRadius:
                if (Target != null)
                {
                    float dist = Vector3.Distance(Target.GameObject_Comp.Body.position, fTower.Direct_BodyObject.GameObject_Comp.Body.position) / GlobalCollector.Cell_Size;
                    return CheckValueOnCondition(dist, UnitsRange, ActivationMethodCondition); 
                }

                break;

            case ObjectWeapon_Script_ActivationMethods.Endurance:
                return CheckValueOnCondition(fTower.Direct_BodyObject.EnduranceObject_Comp.Endurance, UnitsRange, ActivationMethodCondition);

            case ObjectWeapon_Script_ActivationMethods.TargetEndurance:
                if (Target != null)
                {
                    return CheckValueOnCondition(fTower.Target.EnduranceObject_Comp.Endurance, UnitsRange, ActivationMethodCondition);
                }

                break;
        }

        return false;
    }


    private bool CheckValueOnCondition(float value, Vector2 range, ObjectWeapon_Script_ActivationMethodConditions condition)
    {
        switch (condition)
        {
            case ObjectWeapon_Script_ActivationMethodConditions.InRange:
                return (value >= range.x) && (value <= range.y);

            case ObjectWeapon_Script_ActivationMethodConditions.OutRange:
                return (value < range.x) || (value > range.y);

            case ObjectWeapon_Script_ActivationMethodConditions.LessThan:
                return value <= range.x;

            case ObjectWeapon_Script_ActivationMethodConditions.MoreThan:
                return value >= range.y;

            case ObjectWeapon_Script_ActivationMethodConditions.Equals:
                return Mathf.Abs(value - range.x) <= Vector2.kEpsilon;
        }

        return false;
    }

    private void PushProjectile()
    {
        for (int i = 0; i < ProjectilesCount; i++)
        {
            ObjectWeaponProjectile_Script projectile = ObjectWeaponProjectile_Script.Instantiate(Projectile_Sample);
            projectile.gameObject.SetActive(true);
            projectile.Owner = fTower.Direct_BodyObject;
            projectile.WeaponOwner = this;
            projectile.SetTransformData(fTower.Tower_ProjectilePivots[i % fTower.Tower_ProjectilePivots.Count]);
            projectile.SetTarget(Target.GameObject_Comp.Body.position);

            if (fTower.Direct_BodyObject.AnimController_Comp!= null)
            {
                fTower.Direct_BodyObject.AnimController_Comp.SetAnimationId(Tower_FireAnimId, false);
            }
        }

    }

    void UpdateProjectileReloads()
    {
        if (fAttackConditionMatch || 
            (fSequenceIndex != -1))
        {
            fSequenceTime += Time.fixedDeltaTime;

            if (fSequenceIndex == -1)
            {
                fSequenceIndex = 0;
                fSequenceTime = ReloadSequence[0];
            }

            if (fSequenceTime >= ReloadSequence[fSequenceIndex])
            {
                fSequenceIndex++;
                fSequenceTime = 0;

                if (fSequenceIndex >= ReloadSequence.Count)
                {
                    if (!fAttackConditionMatch)
                        fSequenceIndex = -1;
                    else
                    {
                        fSequenceIndex = 0;
                    }
                }
                
                if ((Target != null) &&
                    (Target.EnduranceObject_Comp.IsAlive))
                {
                    PushProjectile();
                }
            }
        }

    }

    private void FixedUpdate()
    {
        if ((fTower == null) ||
            fFreeze)
            return;

        fFixedFramesCount++;

        if (fFixedFramesCount % FFC_ATTACK_CONDITION_MATCH_RATE == 0)
        {
            fAttackConditionMatch = CheckAttackConditions();
        }
        
        UpdateProjectileReloads();
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        SetTower(Default_TowerOwner);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
