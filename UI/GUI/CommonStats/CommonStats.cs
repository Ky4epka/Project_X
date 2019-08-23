using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonStats : MonoBehaviour {

    public DynamicValue Direct_SpiceValue = null;
    public FillingBar Direct_SpiceLimit = null;
    public FillingBar Direct_EnergyLimit = null;

    protected Player_Script fBindedPlayer = null;

    public float TargetSpiceValue
    {
        get
        {
            return Direct_SpiceValue.TargetValue;
        }

        set
        {
            Direct_SpiceValue.TargetValue = value;
        }
    }

    public float SpiceLimit_CurrentValue
    {
        get
        {
            return Direct_SpiceLimit.Progress;
        }

        set
        {
            Direct_SpiceLimit.Progress = (int)value;
        }
    }

    public float SpiceLimit_Max
    {
        get
        {
            return Direct_SpiceLimit.Max;
        }

        set
        {
            Direct_SpiceLimit.Max = (int)value;
        }
    }

    public float EnergyLimit_CurrentValue
    {
        get
        {
            return Direct_EnergyLimit.Progress;
        }
        set
        {
            Direct_EnergyLimit.Progress = (int)value;
        }
    }

    public float EnergyLimit_Max
    {
        get
        {
            return Direct_EnergyLimit.Max;
        }

        set
        {
            Direct_EnergyLimit.Max = (int)value;
        }
    }

    public Player_Script BindedPlayer
    {
        get
        {
            return fBindedPlayer;
        }

        set
        {
            if (fBindedPlayer != null)
            {
                fBindedPlayer.ParametersValues.OnSpiceChanged.RemoveListener(Event_OnSpiceChanged);
                fBindedPlayer.ParametersValues.OnSpiceLimitChanged.RemoveListener(Event_OnSpiceLimitChanged);
                fBindedPlayer.ParametersValues.OnEnergyChanged.RemoveListener(Event_OnEnergyChanged);
                fBindedPlayer.ParametersValues.OnEnergyLimitChanged.RemoveListener(Event_OnEnergyLimitChanged);
            }

            fBindedPlayer = value;

            if (fBindedPlayer != null)
            {
                fBindedPlayer.ParametersValues.OnSpiceChanged.AddListener(Event_OnSpiceChanged);
                fBindedPlayer.ParametersValues.OnSpiceLimitChanged.AddListener(Event_OnSpiceLimitChanged);
                fBindedPlayer.ParametersValues.OnEnergyChanged.AddListener(Event_OnEnergyChanged);
                fBindedPlayer.ParametersValues.OnEnergyLimitChanged.AddListener(Event_OnEnergyLimitChanged);
            }

            UpdatePlayerStats();
        }
    }

    public void Event_OnBindedPlayerDestroy(Player_Script sender)
    {
        BindedPlayer = null;
    }

    public void Event_OnSpiceChanged(float old_value, float new_value)
    {
        UpdatePlayerStats();
    }

    public void Event_OnSpiceLimitChanged(float value)
    {
        UpdatePlayerStats();
    }

    public void Event_OnEnergyChanged(float value)
    {
        UpdatePlayerStats();
    }
    
    public void Event_OnEnergyLimitChanged(float value)
    {
        UpdatePlayerStats();
    }

    protected void UpdatePlayerStats()
    {
        if (fBindedPlayer == null)
        {
            return;
        }

        TargetSpiceValue = fBindedPlayer.ParametersValues.CurrentSpice.Value;
        SpiceLimit_Max = fBindedPlayer.ParametersValues.SpiceLimitRepository.Repository;
        SpiceLimit_CurrentValue = fBindedPlayer.ParametersValues.CurrentSpice.Value;
        EnergyLimit_Max = fBindedPlayer.ParametersValues.EnergyLimitRepository.Repository;
        EnergyLimit_CurrentValue = fBindedPlayer.ParametersValues.EnergyRepository.Repository;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
