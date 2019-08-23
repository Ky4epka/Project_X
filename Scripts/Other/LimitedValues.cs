using UnityEngine;
using System.Collections;

[System.Serializable]
public class LimitedValueInt
{
    public int Default_Value = 0;
    public int Default_MinValue = -1;
    public int Default_MaxValue = 1;

    public NotifyEvent_2P<int, int> OnChanged = new NotifyEvent_2P<int, int>();
    public NotifyEvent_NP OnMinBorder = new NotifyEvent_NP();
    public NotifyEvent_NP OnMaxBorder = new NotifyEvent_NP();

    [SerializeField]
    protected int fValue = 0;
    [SerializeField]
    protected int fMinValue = -1;
    [SerializeField]
    protected int fMaxValue = 1;


    public LimitedValueInt()
    {
        MinValue = Default_MinValue;
        MaxValue = Default_MaxValue;
        Value = Default_Value;
    }

    // Result: Принятое количество
    public int Add(int amount)
    {
        if (amount < 0)
            return 0;
        
        int value = fValue + amount;
        int result = amount;

        if (value > fMaxValue)
            value = fMaxValue;

        if (value == fValue)
            return 0;

        int old = fValue;
        fValue = value;
        OnChanged.Invoke(old, value);

        if (value == fMaxValue)
        {
            result -= (old + amount) - fMaxValue;
            OnMaxBorder.Invoke();
        }

        return result;
    }

    // Result: Принятое количество
    public int Sub(int amount)
    {
        if (amount < 0)
            return 0;

        int value = fValue - amount;
        int result = amount;

        if (value < fMinValue)
            value = fMinValue;

        if (value == fValue)
            return 0;

        int old = fValue;
        fValue = value;
        OnChanged.Invoke(old, value);

        if (value == fMinValue)
        {
            result += old;
            OnMinBorder.Invoke();
        }

        return result;
    }

    public int Value
    {
        get
        {
            return fValue;
        }

        set
        {
            if (fValue == value)
                return;

            int delta = value - fValue;

            if (delta == 0)
                return;
            else if (delta > 0)
                Add(delta);
            else
                Sub(-delta);
        }
    }

    public int MinValue
    {
        get
        {
            return fMinValue;
        }

        set
        {
            if ((fMinValue == value) ||
                (fMinValue >= fMaxValue))
                return;

            fMinValue = value;

            if (fValue <= fMinValue)
            {
                int old = fValue;
                fValue = fMinValue;
                OnChanged.Invoke(old, fValue);
                OnMinBorder.Invoke();
            }
        }
    }

    public int MaxValue
    {
        get
        {
            return fMaxValue;
        }

        set
        {
            if ((fMaxValue == value) ||
                (fMaxValue <= fMinValue))
                return;

            fMaxValue = value;

            if (fValue >= fMaxValue)
            {
                int old = fValue;
                fValue = fMaxValue;
                OnChanged.Invoke(old, fValue);
                OnMaxBorder.Invoke();
            }
        }
    }

}



[System.Serializable]
public class LimitedValueFloat
{
    public float Default_Value = 0f;
    public float Default_MinValue = -1;
    public float Default_MaxValue = 1f;

    public NotifyEvent_2P<float, float> OnChanged = new NotifyEvent_2P<float, float>();
    public NotifyEvent_NP OnMinBorder = new NotifyEvent_NP();
    public NotifyEvent_NP OnMaxBorder = new NotifyEvent_NP();

    [SerializeField]
    protected float fValue = 0f;
    [SerializeField]
    protected float fMinValue = -1f;
    [SerializeField]
    protected float fMaxValue = 1f;


    public LimitedValueFloat()
    {
        MinValue = Default_MinValue;
        MaxValue = Default_MaxValue;
        Value = Default_Value;
    }

    // Result: Принятое количество
    public float Add(float amount)
    {
        if (amount < 0)
            return 0;

        float value = fValue + amount;
        float result = amount;

        if (value > fMaxValue)
            value = fMaxValue;

        if (MathKit.NumbersEquals(value, fValue))
            return 0;

        float old = fValue;
        fValue = value;
        OnChanged.Invoke(old, value);

        if (MathKit.NumbersEquals(value, fMaxValue))
        {
            result -= (old + amount) - fMaxValue;
            OnMaxBorder.Invoke();
        }

        return result;
    }

    // Result: Принятое количество
    public float Sub(float amount)
    {
        if (amount < 0)
            return 0;

        float value = fValue - amount;
        float result = amount;

        if (value < fMinValue)
            value = fMinValue;

        if (MathKit.NumbersEquals(value, fValue))
            return 0;

        float old = fValue;
        fValue = value;
        OnChanged.Invoke(old, value);

        if (MathKit.NumbersEquals(value, fMinValue))
        {
            result += old;
            OnMinBorder.Invoke();
        }

        return result;
    }

    public float Value
    {
        get
        {
            return fValue;
        }

        set
        {
            if (MathKit.NumbersEquals(fValue, value))
                return;

            float delta = value - fValue;

            if (MathKit.NumbersEquals(delta, 0))
                return;
            else if (delta > 0)
                Add(delta);
            else
                Sub(-delta);
        }
    }

    public float MinValue
    {
        get
        {
            return fMinValue;
        }

        set
        {
            if ((MathKit.NumbersEquals(fMinValue, value)) ||
                (value > fMaxValue) || 
                (MathKit.NumbersEquals(value, fMaxValue)))
                return;

            fMinValue = value;

            if ((fValue < fMinValue) ||
                MathKit.NumbersEquals(fValue, fMinValue))
            {
                float old = fValue;
                fValue = fMinValue;
                OnChanged.Invoke(old, fValue);
                OnMinBorder.Invoke();
            }
        }
    }

    public float MaxValue
    {
        get
        {
            return fMaxValue;
        }

        set
        {
            if (MathKit.NumbersEquals(fMaxValue, value) ||
                (value < fMinValue) ||
                MathKit.NumbersEquals(value, fMinValue))
                return;

            fMaxValue = value;

            if ((fValue > fMaxValue) ||
                MathKit.NumbersEquals(fValue, fMaxValue))
            {
                float old = fValue;
                fValue = fMaxValue;
                OnChanged.Invoke(old, fValue);
                OnMaxBorder.Invoke();
            }
        }
    }

}
