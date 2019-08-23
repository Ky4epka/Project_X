using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingObject_Script : HoldTargetBase_Script {

        private const int FFC_WEAPON_ACTIVATION_REFRESH_RATE = 10;
        private const int FFC_WEAPON_PROCESSING_RATE = 3;


        public bool Default_Active = false;
        public bool Default_FreezeWeapons = false;
        public List<ObjectWeapon_Script> Default_Weapons = new List<ObjectWeapon_Script>();

    [SerializeField]
    private ObjectComponentsCollection_Script fUnit_Comp = null;
    [SerializeField]
    private bool fActive = false;
    [SerializeField]
    private bool fFreezeWeapons = false;
    [SerializeField]
    private List<ObjectWeapon_Script> fWeapons = new List<ObjectWeapon_Script>();
    [SerializeField]
    private bool fInit = true;
    

    public void SetActivate(bool value)
    {
        if (fActive == value)
            return;

        fActive = true;

        foreach (ObjectWeapon_Script weapon in fWeapons)
        {
            weapon.SetActivate(value);
        }
    }

    public bool GetActivate()
    {
        return fActive;
    }

    public void SetFreezeWeapons(bool value)
    {
        if ((fFreezeWeapons == value))
            return;

        fFreezeWeapons = value;

        foreach (ObjectWeapon_Script weapon in fWeapons)
        {
            weapon.SetFreeze(value);
        }
    }

    public bool GetFreezeWeapons()
    {
        return fFreezeWeapons;
    }

    public override bool SetTarget(ObjectComponentsCollection_Script target)
    {
        if (base.SetTarget(target))
        {
            float max_y_dist = System.Single.MinValue;

            foreach (ObjectWeapon_Script weapon in fWeapons)
            {
                weapon.SetTarget(target);

                if (weapon.UnitsRange.y > max_y_dist)
                {
                    max_y_dist = weapon.UnitsRange.y;
                }
            }

            if (fUnit_Comp.MovingObject_Comp != null)
            {
                fUnit_Comp.MovingObject_Comp.SetTarget(target, (int)max_y_dist - 1);
            }

            fUnit_Comp.Tower_Comp.SetTarget(target);

            return true;
        }

        return false;
    }

    public override bool CancelTarget()
    {
        if (base.CancelTarget())
        {
            fUnit_Comp.Tower_Comp.CancelTarget();

            if (fUnit_Comp.MovingObject_Comp != null)
            {
                fUnit_Comp.MovingObject_Comp.CancelTarget();
            }
            
            foreach (ObjectWeapon_Script weapon in fWeapons)
            {
                weapon.CancelTarget();
            }
            return true;
        }

        return false;
    }

    public void AddWeapon(ObjectWeapon_Script weapon)
    {
        if (fWeapons.Contains(weapon))
            return;

        fWeapons.Add(weapon);
        weapon.SetTarget(Target);
        weapon.SetActivate(fActive);
        weapon.SetFreeze(fFreezeWeapons);
    }

    public void AddWeaponRange(List<ObjectWeapon_Script> weapons)
    {
        foreach (ObjectWeapon_Script weapon in weapons)
        {
            AddWeapon(weapon);
        }
    }
    
    public void RemoveWeapon(ObjectWeapon_Script weapon)
    {
        weapon.CancelTarget();
        weapon.SetActivate(false);
        weapon.SetFreeze(false);
        fWeapons.Remove(weapon);
    }

    public void RemoveWeaponRange(List<ObjectWeapon_Script> weapons)
    {
        foreach (ObjectWeapon_Script weapon in weapons)
        {
            RemoveWeapon(weapon);
        }
    }
    
    public ObjectWeapon_Script GetWeaponAt(int index)
    {
        return fWeapons[index];
    }

    public float AttackRadius_Max
    {
        get
        {
            float result = System.Single.MinValue;

            foreach (ObjectWeapon_Script weapon in fWeapons)
            {
                if (weapon.UnitsRange.y > result)
                {
                    result = weapon.UnitsRange.y;
                }
            }

            return result;
        }
    }

    public float AttackRadius_Min
    {
        get
        {
            float result = System.Single.MaxValue;

            foreach (ObjectWeapon_Script weapon in fWeapons)
            {
                if (weapon.UnitsRange.x < result)
                {
                    result = weapon.UnitsRange.x;
                }
            }

            return result;
        }
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
        fUnit_Comp = this.GetComponent<ObjectComponentsCollection_Script>();
        SetActivate(Default_Active);
        SetFreezeWeapons(Default_FreezeWeapons);
        AddWeaponRange(Default_Weapons);
	}
	
	// Update is called once per frame
	void Update () {
		if (fInit)
        {
            fInit = false;
        }
	}
}
