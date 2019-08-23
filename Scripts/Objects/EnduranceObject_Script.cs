using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EnduranceObject_Script_OnEndurance: UnityEvent<EnduranceObject_Script, float, float>
{

}

[System.Serializable]
public class EnduranceObject_Script_OnDamage : UnityEvent<EnduranceObject_Script, EnduranceObject_Script, float>
{

}

[System.Serializable]
public class EnduranceObject_Script_OnNotify : UnityEvent<EnduranceObject_Script>
{

}


public class EnduranceObject_Script : MonoBehaviour {

    // Source, current_value, new_value
    public EnduranceObject_Script_OnEndurance OnEnduranceChanged = new EnduranceObject_Script_OnEndurance();

    // Source, Reciever, value
    public EnduranceObject_Script_OnDamage OnTakeDamage = new EnduranceObject_Script_OnDamage();
    // Reciever, Source, value
    public EnduranceObject_Script_OnDamage OnCauseDamage = new EnduranceObject_Script_OnDamage();

    // Source
    public EnduranceObject_Script_OnNotify OnDie = new EnduranceObject_Script_OnNotify();

    public float Default_MaxEndurance = 1f;
    public float Default_Endurance = 1f;

    public static float DieEnduranceValue = 1f;

    [SerializeField]
    private float fMaxEndurance = 1f;
    [SerializeField]
    private float fEndurance = 1f;
    [SerializeField]
    private bool fIsAlive = true;

    private bool fInit = true;
    [SerializeField]
    private bool fInvulnerable = false;
    

    public void SetMaxEndurance(float value)
    {
        if (value < GlobalCollector.DieEnduranceValue)
        {
            Debug.LogWarning("Endurance value(" + value + ") must be a positive");
            return;
        }

        fMaxEndurance = value;

        if (fEndurance > fMaxEndurance)
            SetEndurance(value);
    }

    public float GetMaxEndurance()
    {
        return fMaxEndurance;
    }

    public float MaxEndurance
    {
        get
        {
            return fMaxEndurance;
        }

        set
        {
            SetEndurance(value);
        }
    }

    public void SetEndurance(float value)
    {
        if (value >= fMaxEndurance) value = fMaxEndurance;
        else if (value <= DieEnduranceValue) DoDie();

        float tmp = fEndurance;
        fEndurance = value;

        OnEnduranceChanged.Invoke(this, tmp, value);
    }

    public float GetEndurance()
    {
        return fEndurance;
    }

    public float Endurance
    {
        get
        {
            return fEndurance;
        }

        set
        {
            SetEndurance(fEndurance);
        }
    }

    public bool Invulnerable
    {
        get
        {
            return fInvulnerable;
        }

        set
        {
            fInvulnerable = value;
        }
    }

    public void AddEndurance(float value)
    {
        SetEndurance(fEndurance + value);
    }

    public float GetEnduranceToMaxRatio()
    {
        return fEndurance / fMaxEndurance;
    }

    public void DoDamage(EnduranceObject_Script source, float value)
    {
        if (value < 0)
        {
            Debug.LogWarning("Damage value(" + value + ") must be a positive");
            return;
        }

        if (fInvulnerable)
            return;

        float endurance = fEndurance - value;

        if (endurance < GlobalCollector.DieEnduranceValue)
        {
            endurance = GlobalCollector.DieEnduranceValue;
            value += endurance;
        }

        OnCauseDamage.Invoke(source, this, value);
        AddEndurance(-value);
        OnTakeDamage.Invoke(this, source, value);
    }

    public void Kill()
    {
        SetEndurance(GlobalCollector.DieEnduranceValue);
    }

    public bool IsAlive
    {
        get
        {
            return fIsAlive;
        }
    }

    private void DoDie()
    {
        fIsAlive = false;
        fEndurance = GlobalCollector.DieEnduranceValue;
        OnDie.Invoke(this);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (fInit)
        {
            fInit = false;
            SetMaxEndurance(Default_MaxEndurance);
            SetEndurance(Default_Endurance);
        }
	}
}
