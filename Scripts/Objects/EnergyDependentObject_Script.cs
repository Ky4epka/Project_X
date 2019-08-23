using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitObject_Script))]
public class EnergyDependentObject_Script : MonoBehaviour {

    public NotifyEvent_3P<EnergyDependentObject_Script, float, float> OnEnergyValueChanged = new NotifyEvent_3P<EnergyDependentObject_Script, float, float>();
    public NotifyEvent_3P<EnergyDependentObject_Script, float, float> OnEnergyLimitValueChanged = new NotifyEvent_3P<EnergyDependentObject_Script, float, float>();
    public NotifyEvent<EnergyDependentObject_Script> OnEnergyOverflow = new NotifyEvent<EnergyDependentObject_Script>();

    public float Default_EnergyValue = 0f;
    public float Default_EnergyLimitValue = 0f;
    public bool Default_Enabled = true;

    protected float fEnergyValue = 0f;
    protected float fEnergyLimitValue = 0f;
    protected UnitObject_Script fUnit = null;
    protected bool fEneryOverflows = false;
    protected bool fEnabled = false;
    protected bool fReady = false;


    public float EnergyValue
    {
        get
        {
            return fEnergyValue;
        }

        set
        {
            if (!fReady)
            {
                fEnergyValue = value;
                return;
            }

            if (!fEnabled)
                return;

            fEnergyValue = value;
            fUnit.Owner.ParametersValues.EnergyRepository.ModufyValue(fUnit, fEnergyValue);
        }
    }

    public float EnergyLimitValue
    {
        get
        {
            return fEnergyLimitValue;
        }

        set
        {
            if (!fReady)
            {
                fEnergyLimitValue = value;
                return;
            }

            if (!fEnabled)
                return;

            fEnergyLimitValue = value;
            fUnit.Owner.ParametersValues.EnergyLimitRepository.ModufyValue(fUnit, fEnergyLimitValue);
        }
    }

    public bool Enabled
    {
        get
        {
            return fEnabled;
        }

        set
        {
            fEnabled = value;

            if (!fReady)
                return;

            if (fEnabled)
            {
                fUnit.Owner.ParametersValues.EnergyRepository.ModufyValue(fUnit, fEnergyValue);
                fUnit.Owner.ParametersValues.EnergyRepository.ModufyValue(fUnit, fEnergyLimitValue);
            }
            else
            {
                fUnit.Owner.ParametersValues.EnergyRepository.DeleteValue(fUnit);
                fUnit.Owner.ParametersValues.EnergyRepository.DeleteValue(fUnit);
            }
        }
    }

    public void Event_OnEnergyChanged(float new_value)
    {
    }

    public void Event_OnEnergyLimitChanged(float new_value)
    {
    }

    public void Event_OnOwnerChanged(UnitObject_Script sender, Player_Script new_owner, Player_Script old_owner)
    {
        UpdateOwner(new_owner, old_owner);
    }

    protected void UpdateOwner(Player_Script new_owner, Player_Script old_owner)
    {
        if (old_owner != null)
        {
            old_owner.ParametersValues.OnEnergyChanged.RemoveListener(Event_OnEnergyChanged);
            old_owner.ParametersValues.OnEnergyLimitChanged.RemoveListener(Event_OnEnergyLimitChanged);
            old_owner.ParametersValues.EnergyRepository.DeleteValue(fUnit);
            old_owner.ParametersValues.EnergyLimitRepository.DeleteValue(fUnit);
        }

        if (new_owner != null)
        {
            new_owner.ParametersValues.OnEnergyChanged.AddListener(Event_OnEnergyChanged);
            new_owner.ParametersValues.OnEnergyLimitChanged.AddListener(Event_OnEnergyLimitChanged);
            new_owner.ParametersValues.EnergyRepository.AddValue(fUnit, fEnergyValue);
            new_owner.ParametersValues.EnergyLimitRepository.AddValue(fUnit, fEnergyLimitValue);
        }
    }

    public void Event_OnDie(UnitObject_Script sender)
    {
        Enabled = false;
    }

    public void Event_OnCarrier(UnitObject_Script sender, bool value)
    {
        Enabled = !value;
    }
    
    public void Event_OnLoadingPlatform(UnitObject_Script sender, bool value)
    {
        Enabled = !value;
    }

    IEnumerator DoInit()
    {
        yield return null;

        fReady = true;
        UpdateOwner(fUnit.Owner, null);
        fUnit.OnDie.AddListener(Event_OnDie);
        fUnit.OnCarrierChanged.AddListener(Event_OnCarrier);
        fUnit.OnLoadingPlatformChanged.AddListener(Event_OnLoadingPlatform);
        Enabled = Default_Enabled;
        EnergyValue = Default_EnergyValue;
        EnergyLimitValue = Default_EnergyLimitValue;
        Enabled = Enabled;
        EnergyValue = EnergyValue;
        EnergyLimitValue = EnergyLimitValue;
    }

	// Use this for initialization
	void Start () {
        fUnit = this.GetComponent<UnitObject_Script>();
        fUnit.OnOwnerChanged.AddListener(Event_OnOwnerChanged);
        Enabled = Default_Enabled;
        EnergyValue = Default_EnergyValue;
        EnergyLimitValue = Default_EnergyLimitValue;

        StartCoroutine(DoInit());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
