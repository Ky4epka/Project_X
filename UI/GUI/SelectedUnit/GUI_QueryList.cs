using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUI_QueryList : MonoBehaviour
{
    public NotifyEvent_2P<GUI_QueryList, GUI_QueryItem> OnItemClicked = new NotifyEvent_2P<GUI_QueryList, GUI_QueryItem>();

    public GUI_QueryItem Item_Sample = null;
    public GameObject QueryContainerUnit = null;

    protected RectTransform fGUIBody = null;
    protected List<GUI_QueryItem> fItems = new List<GUI_QueryItem>();
    protected ProductionObject_Script fProductionObject = null;



    public ProductionObject_Script ProductionObject
    {
        get
        {
            return fProductionObject;
        }

        set
        {
            Clear();

            if (fProductionObject != null)
            {
                fProductionObject.ProductionQuery.OnItemsChanged.RemoveListener(Event_OnItemsChanged);
                fProductionObject.ProductionQuery.OnCurrentItemProduced.RemoveListener(Event_OnCurrentItemProduced);
                fProductionObject.ProductionQuery.OnCurrentItemProgress.RemoveListener(Event_OnCurrentItemProgress);
            }

            fProductionObject = value;

            if (fProductionObject != null)
            {
                fProductionObject.ProductionQuery.OnItemsChanged.AddListener(Event_OnItemsChanged);
                fProductionObject.ProductionQuery.OnCurrentItemProduced.AddListener(Event_OnCurrentItemProduced);
                fProductionObject.ProductionQuery.OnCurrentItemProgress.AddListener(Event_OnCurrentItemProgress);
                TakeItemsFromQuery(fProductionObject);
            }

            UpdateUI();
        }
    }

    public void Event_OnCurrentItemProduced(ProductionObject_Script_ProductionQuery sender, ProductionObject_Script_ProductionItem item)
    {
        UpdateUI();
    }

    public void Event_OnCurrentItemProgress(ProductionObject_Script_ProductionQuery sender, ProductionObject_Script_ProductionItem item, float progress)
    {
        UpdateUI();
    }

    public void Event_OnItemsChanged(ProductionObject_Script_ProductionQuery sender)
    {
        TakeItemsFromQuery(fProductionObject);
    }

    public void Event_OnItemClicked(GUI_QueryItem sender)
    {
        OnItemClicked.Invoke(this, sender);
    }

    protected void TakeItemsFromQuery(ProductionObject_Script _object)
    {
        Clear();

        if (_object == null)
            return;

        for (int i=0; i<_object.ProductionQuery.Items.Count; i++)
        {
            if (!(_object.ProductionQuery.Items[i] is ProductionItem_ObjectType))
                continue;

            AddItem(_object.ProductionQuery.Items[i] as ProductionItem_ObjectType);
        }

        UpdateUI();
    }

    protected void AddItem(ProductionItem_ObjectType object_item)
    {
        GUI_QueryItem item = GUI_QueryItem.Instantiate(Item_Sample, GUIBody);
        item.GUIBody.localPosition = new Vector3(fItems.Count * item.GUIBody.sizeDelta.x, 0, 0);
        item.QueryItem = object_item;
        item.ShowProgress = false;
        item.OnClick.AddListener(Event_OnItemClicked);
        fItems.Add(item);
        UpdateUI();
    }

    protected bool RemoveItem(int index)
    {
        if ((index < 0) ||
            (index >= fItems.Count))
            return false;

        fItems[index].OnClick.RemoveListener(Event_OnItemClicked);
        Destroy(fItems[index].Unit);
        fItems.RemoveAt(index);
        UpdateUI();
        return true;
    }

    protected void Clear()
    {
        while (fItems.Count > 0)
            RemoveItem(0);
    }

    public bool Visible
    {
        get
        {
            return QueryContainerUnit.activeSelf;
        }

        set
        {
            QueryContainerUnit.SetActive(value);
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

    protected void UpdateUI()
    {
        if (fItems.Count > 0)
        {
            GUI_QueryItem item = fItems[0];
            item.ShowProgress = true;
            item.UpdateUI();
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
