using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GUI_QueryItem : MonoBehaviour, IPointerClickHandler
{
    public NotifyEvent<GUI_QueryItem> OnClick = new NotifyEvent<GUI_QueryItem>();

    public FillingBar ProgressBar = null;
    public IconType Icon = null;

    protected GameObject fUnit = null;
    protected RectTransform fGUIBody = null;
    protected ProductionItem_ObjectType fQueryItem = null;
    protected bool fReady = false;
    protected bool fShowProgress = false;


    public void OnPointerClick(PointerEventData data)
    {
        OnClick.Invoke(this);
    }

    public ProductionItem_ObjectType QueryItem
    {
        get
        {
            return fQueryItem;
        }

        set
        {
            fQueryItem = value;
            UpdateUI();
        }
    }

    public GameObject Unit
    {
        get
        {
            if (fUnit == null)
                fUnit = gameObject;

            return fUnit;
        }
    }

    public RectTransform GUIBody
    {
        get
        {
            if (fGUIBody == null)
                fGUIBody = this.GetComponent<RectTransform>();

            return fGUIBody;
        }
    }

    public bool ShowProgress
    {
        get
        {
            return fShowProgress;
        }

        set
        {
            fShowProgress = value;

            if (!fReady)
                return;

            ProgressBar.Visible = value;
            UpdateUI();
        }
    }

    public virtual void UpdateUI()
    {
        if (!fReady)
            return;

        if (fQueryItem == null)
        {
            if (ShowProgress)
            {
                ProgressBar.Progress = 0;
                ProgressBar.Min = 0;
                ProgressBar.Max = 100;
            }

            Icon.ObjectType = null;
        }
        else
        {
            if (ShowProgress)
            {
                ProgressBar.Min = 0;
                ProgressBar.Max = 100;
                ProgressBar.Progress = Mathf.RoundToInt(fQueryItem.Progress * 100f);
            }

            Icon.ObjectType = fQueryItem.ObjectType;
        }        
    }

    IEnumerator UIInit()
    {
        yield return null;
        fReady = true;
        QueryItem = QueryItem;
        ShowProgress = ShowProgress;
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(UIInit());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
