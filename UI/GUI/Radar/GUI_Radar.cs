using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GUI_Radar_ActivateState
{
    None,
    Activate,
    Deactivate
}

[RequireComponent(typeof(Animator))]
public class GUI_Radar : MonoBehaviour {
    public Image Direct_Image = null;

    public string ActivatingAnimationName = "none";
    public string DeactivatingAnimationName = "none";

    [SerializeField]
    protected GameObject fUnit = null;
    [SerializeField]
    protected Image fImage = null;
    [SerializeField]
    protected Animator fAnimator = null;
    [SerializeField]
    protected Texture2D fCanvas = null;
    [SerializeField]
    protected Sprite fCanvasSprite = null;
    [SerializeField]
    protected GUI_Radar_ActivateState fPerformActivateState = GUI_Radar_ActivateState.None;
    [SerializeField]
    protected GUI_Radar_ActivateState fProcessingActivateState = GUI_Radar_ActivateState.None;
    [SerializeField]
    protected bool fCanvasUpdating = false;
    [SerializeField]
    protected float fCanvasUpdateTime = 0f;

    [SerializeField]
    protected bool fActive = false;



    public bool Active
    {
        get
        {
            return fActive;
        }

        set
        {
            if (value)
                fPerformActivateState = GUI_Radar_ActivateState.Activate;
            else
                fPerformActivateState = GUI_Radar_ActivateState.Deactivate;

            if (fProcessingActivateState == GUI_Radar_ActivateState.None)
            {
                ProcessState();
            }
        }
    }

    public Image Image
    {
        get
        {
            return fImage;
        }

        set
        {
            if (fImage != null)
                fImage.sprite = null;

            fImage = value;

            if ((fImage != null) &&
                (fCanvasSprite != null))
                fImage.sprite = fCanvasSprite;
        }
    }

    public bool Visible
    {
        get
        {
            return fUnit.activeSelf;
        }

        set
        {
            fUnit.SetActive(value);
        }
    }

    public void Event_OnMapResize(Map_Script sender, int width, int height)
    {
        UpdateTextureSize();
    }

    public void Initialize()
    {
        GlobalCollector.Instance.Current_Map.OnResize.AddListener(Event_OnMapResize);
        UpdateTextureSize();
    }

    protected void UpdateTextureSize()
    {
        Vector2Int size = GlobalCollector.Instance.Current_Map.GetMapSize();
        fCanvas.Resize(size.x, size.y);

        if (fCanvasSprite != null)
            Destroy(fCanvasSprite);
        
        fCanvasSprite = Sprite.Create(fCanvas, new Rect(0, 0, size.x, size.y), Vector2.zero, 1);

        if (fImage != null)
            fImage.sprite = fCanvasSprite;
    }

    public void FillCanvasFromMap()
    {
        Color[] pixels = fCanvas.GetPixels();

        for (int i=0; i<pixels.Length; i++)
        {
            int x = i % fCanvas.width;
            int y = i / fCanvas.width;

            pixels[i] = GlobalCollector.Instance.Current_Map.GetMapCell(x, fCanvas.height - y - 1).DynamicCellColor();
        }

        fCanvas.SetPixels(pixels);
        fCanvas.Apply();
    }

    public void Event_OnActivated()
    {
        Debug.Log("Activated anim finished");
        ProcessState();
    }

    public void Event_OnDeactivated()
    {
        Debug.Log("Deactivated anim finished");
        ProcessState();
    }
    
    protected void ProcessState()
    {
        switch (fProcessingActivateState)
        {
            case GUI_Radar_ActivateState.Activate:
                fActive = true;
                break;
            case GUI_Radar_ActivateState.Deactivate:
                fActive = false;
                break;
        }

        fProcessingActivateState = fPerformActivateState;
        fPerformActivateState = GUI_Radar_ActivateState.None;

        switch (fProcessingActivateState)
        {
            case GUI_Radar_ActivateState.Activate:
                if (fActive)
                {
                    fProcessingActivateState = GUI_Radar_ActivateState.None; 
                    break;
                }

                fCanvasUpdating = true;
                fCanvasUpdateTime = GlobalCollector.GUI_Radar_UpdateRate;
                fAnimator.Play(ActivatingAnimationName);
                break;
            case GUI_Radar_ActivateState.Deactivate:
                if (!fActive)
                {
                    fProcessingActivateState = GUI_Radar_ActivateState.None;
                    break;
                }

                fCanvasUpdating = false;
                fAnimator.Play(DeactivatingAnimationName);
                break;
        }
    }


	// Use this for initialization
	void Start () {
        fAnimator = this.GetComponent<Animator>();
        fCanvas = new Texture2D(1, 1);
        fCanvas.filterMode = FilterMode.Point;
        fUnit = gameObject;
        Image = Direct_Image;
	}
	
	// Update is called once per frame
	void Update () {
        if (fCanvasUpdating)
        {
            fCanvasUpdateTime += Time.deltaTime;

            if (fCanvasUpdateTime >= GlobalCollector.GUI_Radar_UpdateRate)
            {
                fCanvasUpdateTime = 0f;
                FillCanvasFromMap();
            }
        }
	}
}
