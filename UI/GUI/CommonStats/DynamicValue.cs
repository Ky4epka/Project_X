using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DynamicValue : MonoBehaviour {

    public TextMeshProUGUI Direct_Text = null;
    public float Default_TargetValue = 0f;
    public float Default_ValueSpeed = 1f;


    [SerializeField]
    protected TextMeshProUGUI fText = null;
    [SerializeField]
    protected float fTargetValue = 0;
    [SerializeField]
    protected float fValue = 0;
    [SerializeField]
    protected float fValueSpeed = 1;


    public TextMeshProUGUI Text
    {
        get
        {
            return fText;
        }

        set
        {
            fText = value;
            UpdateUI();
        }
    }

    public float TargetValue
    {
        get
        {
            return fTargetValue;
        }

        set
        {
            fTargetValue = value;
        }
    }

    public float ValueSpeed
    {
        get
        {
            return fValueSpeed;
        }

        set
        {
            fValueSpeed = value;
        }
    }

    public float Value
    {
        get
        {
            return fValue;
        }

        set
        {
            fValue = value;
        }
    }

    protected virtual void UpdateUI()
    {
        if (fText == null)
            return;

        int value = (int)fValue;
        fText.text = string.Concat("$", value.ToString());
    }


	// Use this for initialization
	void Start () {
        TargetValue = Default_TargetValue;
        ValueSpeed = Default_ValueSpeed;
        Text = Direct_Text;
	}
	
	// Update is called once per frame
	void Update () {		
        if (!MathKit.NumbersEquals(fTargetValue, fValue))
        {
            float delta = fTargetValue - fValue;

            if (!MathKit.NumbersEquals(delta, 0))
            {
                float direction = 1;

                if (delta < 0)
                    direction = -1;

                float value = fValue + fValueSpeed * direction * Time.deltaTime;

                if (MathKit.NumbersEquals(value, fTargetValue, fValueSpeed * Time.deltaTime))
                {
                    fValue = fTargetValue;
                }
                else
                    fValue = value;

                UpdateUI();
            }
        }
	}
}
