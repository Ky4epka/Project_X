using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ValuesCollection_DataStruct
{
    public float Value = 0;

    public ValuesCollection_DataStruct(float value)
    {
        Value = value;
    }
}

[System.Serializable]
public class ValuesCollection<KeyType>
{
    public NotifyEvent<float> OnChanged = new NotifyEvent<float>();


    [SerializeField]
    protected Dictionary<KeyType, ValuesCollection_DataStruct> fValues = new Dictionary<KeyType, ValuesCollection_DataStruct>();
    [SerializeField]
    protected float fRepository = 0f;
    [SerializeField]
    protected Vector2 fBounds = Vector2.zero;
    
    public Vector2 Bounds
    {
        get
        {
            return fBounds;
        }

        set
        {
            fBounds = value;
            
            if (fBounds.x > fBounds.y)
            {
                float temp = fBounds.x;
                fBounds.x = fBounds.y;
                fBounds.y = temp;
            }
        }
    }

    public float Repository
    {
        get
        {
            return fRepository;
        }
    }

    public ValuesCollection()
    {
    }
    
    public void AddValue(KeyType key, float value)
    {
        fValues.Add(key, new ValuesCollection_DataStruct(value));
        fRepository += value;
        EnsureRange();
        OnChanged.Invoke(fRepository);
    }

    public void DeleteValue(KeyType key)
    {
        ValuesCollection_DataStruct data;
        fValues.TryGetValue(key, out data);
        fRepository -= data.Value;
        fValues.Remove(key);
        EnsureRange();
        OnChanged.Invoke(fRepository);
    }

    public void ModufyValue(KeyType key, float new_value)
    {
        ValuesCollection_DataStruct data;
        fValues.TryGetValue(key, out data);

        if (data != null)
        {
            data.Value = new_value;
        }

        RecalcValues();
    }

    protected bool IsNoBounds()
    {
        return (fBounds.x == 0) && (fBounds.y == 0);
    }

    protected void EnsureRange()
    {
        if (IsNoBounds())
            return;

        if (fRepository < fBounds.x)
            fRepository = fBounds.x;
        else if (fRepository > fBounds.y)
            fRepository = fBounds.y;
    }
    
    protected float RecalcValues()
    {
        fRepository = 0;

        foreach (KeyValuePair<KeyType, ValuesCollection_DataStruct> pair in fValues)
        {
            fRepository += pair.Value.Value;
        }
        
        EnsureRange();
        OnChanged.Invoke(fRepository);
        return fRepository;
    }
}
