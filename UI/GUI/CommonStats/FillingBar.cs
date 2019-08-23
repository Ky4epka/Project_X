using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FillingBar_IndicateState
{
    Period,
    Lifetime
}

public class FillingBar : MonoBehaviour {

    public Image Direct_BackgroundImage = null;
    public Image Direct_ValueImage = null;

    public Color Default_Color_Normal = Color.white;
    public Color Default_Color_Max = Color.red;
    public int Default_Min = 0;
    public int Default_Max = 100;
    public int Default_Progress = 0;
    public float Default_MaxIndicate_Period = 1f;
    public float Default_MaxIndicate_Lifetime = 1f;
    public bool Default_UseMaxIndicate = true;

    [SerializeField]
    protected Color fColor_Normal = Color.white;
    [SerializeField]
    protected Color fColor_Max = Color.white;
    [SerializeField]
    protected Image fBackgroundImage = null;
    [SerializeField]
    protected Image fValueImage = null;
    [SerializeField]
    protected RectTransform fValueImageRT = null;
    [SerializeField]
    protected int fProgress = 0;
    [SerializeField]
    protected int fMin = 0;
    [SerializeField]
    protected int fMax = 0;
    [SerializeField]
    protected float fMaxIndicate_Time = 0f;
    [SerializeField]
    protected float fMaxIndicate_Period = 1f;
    [SerializeField]
    protected float fMaxIndicate_Lifetime = 1f;
    [SerializeField]
    protected FillingBar_IndicateState fIndicateState = FillingBar_IndicateState.Period;
    [SerializeField]
    protected bool fUseMaxIndicate = false;
    [SerializeField]
    protected GameObject fUnit = null;

    public Color Color_Normal
    {
        get
        {
            return fColor_Normal;
        }
        set
        {
            fColor_Normal = value;
            UpdateUI();
        }
    }

    public Color Color_Max
    {
        get
        {
            return fColor_Max;
        }
        set
        {
            fColor_Max = value;
            UpdateUI();
        }
    }

    public int Progress
    {
        get
        {
            return fProgress;
        }

        set
        {
            value = MathKit.EnsureRange(value, fMin, fMax);
            fProgress = value;
            UpdateUI();
        }
    }

    public int Min
    {
        get
        {
            return fMin;
        }

        set
        {
            if (value > fMax)
            {
                value = fMax;
            }

            fMin = value;
            Progress = Progress;
            UpdateUI();
        }
    }

    public int Max
    {
        get
        {
            return fMax;
        }
        set
        {
            if (value < fMin)
                value = fMin;

            fMax = value;
            Progress = Progress;
            UpdateUI();
        }
    }

    public Image BackgroundImage
    {
        get
        {
            return fBackgroundImage;
        }

        set
        {
            fBackgroundImage = value;
        }
    }

    public Image ValueImage
    {
        get
        {
            return fValueImage;
        }

        set
        {
            fValueImage = value;
            fValueImageRT = fValueImage.rectTransform;
        }
    }
    
    public float MaxIndicate_Period
    {
        get
        {
            return fMaxIndicate_Period;
        }

        set
        {
            fMaxIndicate_Period = value;
        }
    }

    public float MaxIndicate_Lifetime
    {
        get
        {
            return fMaxIndicate_Lifetime;
        }

        set
        {
            fMaxIndicate_Lifetime = value;
        }
    }

    public bool UseMaxIndicate
    {
        get
        {
            return fUseMaxIndicate;
        }

        set
        {
            fUseMaxIndicate = value;
            UpdateUI();
        }
    }

    public bool Visible
    {
        get
        {
            if (fUnit == null)
                fUnit = this.gameObject;

            return fUnit.activeSelf;
        }

        set
        {
            fUnit.SetActive(value);
        }
    }

    protected virtual void UpdateUI()
    {
        if ((fValueImage == null) ||
            (fValueImageRT == null))
            return;

        int delta = fMax - fMin;
        float ratio = 1f;

        if (delta != 0)
        {
            ratio = (float)(fProgress - fMin) / (float)delta;
        }

        if ((!fUseMaxIndicate) ||
            ((fProgress != fMax) &&
             (fValueImage.color != fColor_Normal)))
        {
            fValueImage.color = fColor_Normal;
        }

        Vector3 size = fValueImageRT.localScale;
        size.x = ratio;
        fValueImageRT.localScale = size;
    }

	// Use this for initialization
	void Start () {
        fUnit = this.gameObject;
        Color_Normal = Default_Color_Normal;
        Color_Max = Default_Color_Max;
        BackgroundImage = Direct_BackgroundImage;
        ValueImage = Direct_ValueImage;
        Min = Default_Min;
        Max = Default_Max;
        Progress = Default_Progress;
        MaxIndicate_Period = Default_MaxIndicate_Period;
        MaxIndicate_Lifetime = Default_MaxIndicate_Lifetime;
        UseMaxIndicate = Default_UseMaxIndicate;
	}
	
	// Update is called once per frame
	void Update () {
        if ((fUseMaxIndicate) &&
            (fProgress == fMax) &&
            (fValueImage != null))
        {
            fMaxIndicate_Time += Time.deltaTime;

            switch (fIndicateState)
            {
                case FillingBar_IndicateState.Period:
                    if (fMaxIndicate_Time >= fMaxIndicate_Lifetime)
                    {
                        fMaxIndicate_Time = 0;
                        fIndicateState = FillingBar_IndicateState.Lifetime;
                        fValueImage.color = fColor_Normal;
                    }

                    break;
                case FillingBar_IndicateState.Lifetime:
                    if (fMaxIndicate_Time >= fMaxIndicate_Period)
                    {
                        fMaxIndicate_Time = 0;
                        fIndicateState = FillingBar_IndicateState.Period;
                        fValueImage.color = fColor_Max;
                    }

                    break;
            }
        }
	}

    private void OnValidate()
    {  
    }
}
