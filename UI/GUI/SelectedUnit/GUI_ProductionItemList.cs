using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUI_ProductionItemList : MonoBehaviour {
    
    public GameObject ProductionContainerUnit = null;
    public GUI_ProductionItem Item_Sample = null;

    public NotifyEvent_2P<GUI_ProductionItemList, GUI_ProductionItem> OnItemClick = new NotifyEvent_2P<GUI_ProductionItemList, GUI_ProductionItem>();

    protected Transform fBody = null;
    protected RectTransform fGUIBody = null;
    protected List<GUI_ProductionItem> fItems = new List<GUI_ProductionItem>();


    public void ItemsFromBuildingProductionObject(ProductionObject_Script _object)
    {
        Clear();

        if (_object == null)
            return;

        for (int i = 0; i < _object.ItemCount; i++)
        {
            if (!(_object.ItemAt(i) is ProductionItem_ObjectType))
                continue;

            ProductionItem_ObjectType pitem = _object.ItemAt(i) as ProductionItem_ObjectType;
            AddItemFromTypeInfo(pitem.ObjectType.TypeInfo);
        }
    }

    public void AddItems(List<TypeObjectManager_Script_TypeInfo> type_info_list)
    {
        for (int i = 0; i < type_info_list.Count; i++)
            AddItemFromTypeInfo(type_info_list[i]);
    }

    public void AddItemFromTypeInfo(TypeObjectManager_Script_TypeInfo type_info)
    {
        GUI_ProductionItem item = GUI_ProductionItem.Instantiate(Item_Sample, fBody);
        item.GUIBody.localPosition = new Vector3(0, fItems.Count * Item_Sample.GUIBody.sizeDelta.y, 0);
        item.TypeInfo = type_info;
        item.OnClick.AddListener(Event_OnItemClicked);
        fItems.Add(item);
    }

    public bool RemoveItem(int index)
    {
        if ((index < 0) ||
            (index >= fItems.Count))
            return false;

        fItems[index].OnClick.RemoveListener(Event_OnItemClicked);
        Destroy(fItems[index].Unit);
        fItems.RemoveAt(index);
        return true;
    }

    public void Clear()
    {
        while (fItems.Count > 0)
            RemoveItem(0);
    }

    public List<GUI_ProductionItem> Items
    {
        get
        {
            return fItems;
        }
    }

    public bool Visible
    {
        get
        {
            return ProductionContainerUnit.activeSelf;
        }

        set
        {
            ProductionContainerUnit.SetActive(value);
        }
    }

    public void Event_OnItemClicked(GUI_ProductionItem sender)
    {
        OnItemClick.Invoke(this, sender);
    }

	// Use this for initialization
	void Start () {
        fBody = transform;
        fGUIBody = this.GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
