using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GUI_ProductionItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Color NormalColor = Color.white;
    public Color HighlightColor = Color.yellow;
    public Color PressedColor = Color.gray;
    public Image Background = null;
    public Text NameValue = null;
    public Image Icon = null;
    public Text CostValue = null;
    public Text EnduranceValue = null;
    public Text SizeValue = null;

    public NotifyEvent<GUI_ProductionItem> OnClick = new NotifyEvent<GUI_ProductionItem>();

    [SerializeField]
    protected TypeObjectManager_Script_TypeInfo fTypeInfo = null;
    protected RectTransform fGUIBody = null;
    protected GameObject fUnit = null;


    public RectTransform GUIBody
    {
        get
        {
            if (fGUIBody == null)
                fGUIBody = this.GetComponent<RectTransform>();

            return fGUIBody;
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

    public TypeObjectManager_Script_TypeInfo TypeInfo
    {
        get
        {
            return fTypeInfo;
        }

        set
        {
            fTypeInfo = value;
            ApplyTypeInfo(fTypeInfo);
        }
    }

    public void OnPointerClick(PointerEventData data)
    {
        OnClick.Invoke(this);
    }

    public void OnPointerDown(PointerEventData data)
    {
        Background.color = PressedColor;
    }

    public void OnPointerUp(PointerEventData data)
    {
        Background.color = NormalColor;
    }

    public void OnPointerEnter(PointerEventData data)
    {
        Background.color = HighlightColor;
    }

    public void OnPointerExit(PointerEventData data)
    {
        Background.color = NormalColor;
    }

    protected void ApplyTypeInfo(TypeObjectManager_Script_TypeInfo type_info)
    {
        if (type_info == null)
        {
            NameValue.text = "";
            Icon.sprite = null;
            CostValue.text = "";
            EnduranceValue.text = "";
            SizeValue.text = "";
        }
        else
        {
            NameValue.text = type_info.TypeName;
            Icon.sprite = type_info.TypeData.TypeIcon;
            CostValue.text = Mathf.Round(type_info.TypeData.Base_Cost).ToString();
            EnduranceValue.text = Mathf.Round(type_info.TypeData.Base_Endurance).ToString();
            SizeValue.text = string.Concat(type_info.TypeData.Base_Size.x, "x" , type_info.TypeData.Base_Size.y);
        }
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
