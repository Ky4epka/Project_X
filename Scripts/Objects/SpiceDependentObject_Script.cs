using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UnitObject_Script))]
public class SpiceDependentObject_Script : MonoBehaviour
{

    public NotifyEvent_3P<SpiceDependentObject_Script, float, float> OnSpiceValueChanged = new NotifyEvent_3P<SpiceDependentObject_Script, float, float>();
    public NotifyEvent_3P<SpiceDependentObject_Script, float, float> OnSpiceLimitValueChanged = new NotifyEvent_3P<SpiceDependentObject_Script, float, float>();
    public NotifyEvent<SpiceDependentObject_Script> OnSpiceOverflow = new NotifyEvent<SpiceDependentObject_Script>();

    public float Default_SpiceLimitValue = 0f;

    protected float fSpiceLimitValue = 0f;
    protected UnitObject_Script fUnit = null;


    public float SpiceLimitValue
    {
        get
        {
            return fSpiceLimitValue;
        }

        set
        {
            if (MathKit.NumbersEquals(fSpiceLimitValue, value, Mathf.Epsilon))
                return;

            fSpiceLimitValue = value;
            fUnit.Owner.ParametersValues.SpiceLimitRepository.ModufyValue(fUnit, fSpiceLimitValue);
        }
    }

    public void Event_OnSpiceChanged(float old_value, float new_value)
    {
    }

    public void Event_OnSpiceLimitChanged(float new_value)
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
            old_owner.ParametersValues.OnSpiceChanged.RemoveListener(Event_OnSpiceChanged);
            old_owner.ParametersValues.OnSpiceLimitChanged.RemoveListener(Event_OnSpiceLimitChanged);
            old_owner.ParametersValues.SpiceLimitRepository.DeleteValue(fUnit);
        }

        if (new_owner != null)
        {
            new_owner.ParametersValues.OnSpiceChanged.AddListener(Event_OnSpiceChanged);
            new_owner.ParametersValues.OnSpiceLimitChanged.AddListener(Event_OnSpiceLimitChanged);
            new_owner.ParametersValues.SpiceLimitRepository.AddValue(fUnit, fSpiceLimitValue);
        }
    }

    public void Event_OnDie(UnitObject_Script sender)
    {
        UpdateOwner(null, sender.Owner);
    }

    // Use this for initialization
    void Start()
    {
        fUnit = this.GetComponent<UnitObject_Script>();
        fUnit.OnOwnerChanged.AddListener(Event_OnOwnerChanged);
        fUnit.OnDie.AddListener(Event_OnDie);
        UpdateOwner(fUnit.Owner, null);
        SpiceLimitValue = Default_SpiceLimitValue;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
