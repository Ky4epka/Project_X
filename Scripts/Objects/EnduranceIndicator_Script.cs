using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnduranceIndicator_Script : MonoBehaviour {

    public EnduranceObject_Script EnduranceObject = null;
    public ObjectAttaching_Script AttachingObject = null;

    public float Default_Spacing = 1f;
    public float Default_WidthScalling = 1f;
    public int PixelHeight = 0;

    public Transform Body = null;
    public Transform Indicator_Parent = null;

    public SpriteRenderer Filler_Renderer = null;
    public Transform Border_Body = null;
    public Transform Filler_Body = null;
    public Transform Background_Body = null;

    public Sprite Filler_MoreThan75pc = null;
    public Sprite Filler_MoreThan50pc = null;
    public Sprite Filler_MoreThan25pc = null;
    public Sprite Filler_LessThan25pc = null;


    private float fSpacing = 0f;
    private float fWidthScalling = 0f;
    private bool fVisible = true;


    public void SetSpacing(float value)
    {
        fSpacing = value;
        if (AttachingObject != null)
            AttachingObject.ChangeAttachOffset(name, new Vector3(0, value + PixelHeight / 2, 0));
        UpdateState();
    }

    public float GetSpacing()
    {
        return fSpacing;
    }

    public float Spacing
    {
        get
        {
            return GetSpacing();
        }

        set
        {
            SetSpacing(value);
        }
    }

    public void SetWidthScalling(float value)
    {
        fWidthScalling = value;
        UpdateState();
    }

    public float GetWidthScalling()
    {
        return fWidthScalling;
    }

    public float WidthScalling
    {
        get
        {
            return GetWidthScalling();
        }

        set
        {
            SetWidthScalling(value);
        }
    }

    public bool Visible
    {
        get
        {
            return fVisible;
        }

        set
        {
            if (fVisible == value)
                return;

            fVisible = value;
            Border_Body.gameObject.SetActive(value);
            Filler_Body.gameObject.SetActive(value);
            Background_Body.gameObject.SetActive(value);
        }
    }

    public void OnEnduranceChanged(EnduranceObject_Script sender, float value, float new_value)
    {
        UpdateState();
    }


    void UpdateState()
    {
        float x_sz = fWidthScalling;
        Vector3 sz = new Vector3(x_sz, 1, 1);
        Border_Body.localScale = sz;
        Background_Body.localScale = sz;
        float end_ratio = EnduranceObject.GetEnduranceToMaxRatio();
        float filler_x_sz = x_sz * end_ratio;
        Filler_Body.localScale = new Vector3(filler_x_sz, 1, 1);
        Filler_Body.localPosition = new Vector3(-(x_sz * (1f - end_ratio) * GlobalCollector.Cell_Size) / 2f, 0, 0); 
        Sprite filler;

        if (end_ratio >= 0.75)
            filler = Filler_MoreThan75pc;
        else if (end_ratio >= 0.50)
            filler = Filler_MoreThan50pc;
        else if (end_ratio >= 0.25)
            filler = Filler_MoreThan25pc;
        else
            filler = Filler_LessThan25pc;

        Filler_Renderer.sprite = filler;
    }

    public void Initialize()
    {
        Body.parent = Indicator_Parent;
        EnduranceObject.OnEnduranceChanged.AddListener(OnEnduranceChanged);
        AttachingObject.AddAttach(Body, name, ObjectAttaching_Script.ATTACH_TYPE_ABSOLUTE_VECTOR, new Vector3(0, Default_Spacing, 0));
        SetSpacing(Default_Spacing);
        SetWidthScalling(Default_WidthScalling);
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
}
