using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ProductionObject_Script_ProductionItem
{
    public NotifyEvent<ProductionObject_Script_ProductionItem> OnStart = new NotifyEvent<ProductionObject_Script_ProductionItem>();
    public NotifyEvent<ProductionObject_Script_ProductionItem> OnProduce = new NotifyEvent<ProductionObject_Script_ProductionItem>();
    public NotifyEvent<ProductionObject_Script_ProductionItem> OnCancel = new NotifyEvent<ProductionObject_Script_ProductionItem>();
    public NotifyEvent_2P<ProductionObject_Script_ProductionItem, float> OnProgress = new NotifyEvent_2P<ProductionObject_Script_ProductionItem, float>();

    protected float fProductionTime = 0f;
    protected float fTime = 0f;
    protected bool fStart = false;
    protected object fData = null;

    public virtual void Assign(ProductionObject_Script_ProductionItem source)
    {
        fProductionTime = source.fProductionTime;
        fTime = source.fTime;
        fStart = source.fStart;
    }

    public virtual ProductionObject_Script_ProductionItem Copy()
    {
        ProductionObject_Script_ProductionItem copy = new ProductionObject_Script_ProductionItem();
        copy.Assign(this);
        return copy;
    }

    public virtual float ProductionTime
    {
        get
        {
            return fProductionTime;
        }

        set
        {
            fProductionTime = value;
        }
    }

    public virtual float Progress
    {
        get
        {
            if (MathKit.NumbersEquals(fProductionTime, 0))
                return 1f;

            return fTime / fProductionTime;
        }
    }

    public virtual object Data
    {
        get
        {
            return fData;
        }

        set
        {
            fData = value;
        }
    }

    public ProductionObject_Script_ProductionItem()
    {

    }

    public virtual bool Start()
    {
        if (fStart)
            return false;

        fStart = true;
        fTime = 0;
        OnStart.Invoke(this);
        return true;
    }

    public virtual bool Cancel()
    {
        if (!fStart)
            return false;

        fStart = false;
        OnCancel.Invoke(this);
        return true;
    }
    
    public virtual void FinishImmediately()
    {
        if (!fStart)
            return;

        Product();
    }

    public virtual void Reset()
    {
        Cancel();
        fTime = 0f;
    }

    public virtual bool Process(float time_delta)
    {
        fTime += time_delta;

        if (fTime >= fProductionTime)
        {
            Product();
            fStart = false;
            return true;
        }

        return false;
    }

    protected virtual void Product()
    {
        OnProduce.Invoke(this);
    }
}

public class ProductionObject_Script_ProductionQuery
{
    public enum QueryState
    {
        None,
        Active,
        Paused
    }

    public NotifyEvent<ProductionObject_Script_ProductionQuery> OnActivate = new NotifyEvent<ProductionObject_Script_ProductionQuery>();
    public NotifyEvent<ProductionObject_Script_ProductionQuery> OnDeactivate = new NotifyEvent<ProductionObject_Script_ProductionQuery>();
    public NotifyEvent_2P<ProductionObject_Script_ProductionQuery, ProductionObject_Script_ProductionItem> OnItemChanged = new NotifyEvent_2P<ProductionObject_Script_ProductionQuery, ProductionObject_Script_ProductionItem>();
    public NotifyEvent_2P<ProductionObject_Script_ProductionQuery, ProductionObject_Script_ProductionItem> OnCurrentItemProduced = new NotifyEvent_2P<ProductionObject_Script_ProductionQuery, ProductionObject_Script_ProductionItem>();
    public NotifyEvent_3P<ProductionObject_Script_ProductionQuery, ProductionObject_Script_ProductionItem, float> OnCurrentItemProgress = new NotifyEvent_3P<ProductionObject_Script_ProductionQuery, ProductionObject_Script_ProductionItem, float>();
    public NotifyEvent<ProductionObject_Script_ProductionQuery> OnItemsChanged = new NotifyEvent<ProductionObject_Script_ProductionQuery>();

    protected bool fAutostart = true;
    protected List<ProductionObject_Script_ProductionItem> fItems = new List<ProductionObject_Script_ProductionItem>();
    protected QueryState fState = QueryState.None;
    protected ProductionObject_Script_ProductionItem fCurrentItem = null;

    public ProductionObject_Script_ProductionQuery()
    {
    }

    public List<ProductionObject_Script_ProductionItem> Items
    {
        get
        {
            return fItems;
        }
    }

    public bool Autostart
    {
        get
        {
            return fAutostart;
        }

        set
        {
            if (fAutostart == value)
                return;

            fAutostart = value;

            if (fAutostart)
                Start();
        }
    }

    public void AddItem(ProductionObject_Script_ProductionItem item, bool do_copy)
    {
        if (do_copy)
            fItems.Add(item.Copy());
        else
            fItems.Add(item);

        OnItemsChanged.Invoke(this);

        if (fAutostart)
            Start();
    }

    public void DeleteItem(int index)
    {
        if ((index < 0) ||
            (index >= fItems.Count))
            return;

        ProductionObject_Script_ProductionItem item = fItems[index];
        fItems.RemoveAt(index);
        OnItemsChanged.Invoke(this);

        if (item == fCurrentItem)
            CurrentItemNeeded();

        if (IsEmpty())
            Stop();
    }

    public void DeleteItem(ProductionObject_Script_ProductionItem item)
    {
        DeleteItem(fItems.IndexOf(item));
    }

    public void DeleteItem(string type_name)
    {
        DeleteItem(ItemIndex(type_name));
    }

    public int ItemIndex(string type_name)
    {
        for (int i=0; i<fItems.Count; i++)
        {
            if ((fItems[i] as ProductionItem_ObjectType).ObjectType.TypeInfo.TypeName.Equals(type_name))
                return i;
        }

        return -1;
    }
    
    public void Start()
    {
        if ((fState == QueryState.Active) ||
            IsEmpty())
            return;

        fState = QueryState.Active;
        CurrentItemNeeded();
        OnActivate.Invoke(this);
    }

    public void Pause()
    {
        fState = QueryState.Paused;
    }

    public void Stop()
    {
        if (fState == QueryState.None)
            return;

        fState = QueryState.None;
        fItems.Clear();
        OnDeactivate.Invoke(this);
    }

    public void CancelCurrent()
    {
        if (fCurrentItem != null)
        {
            fCurrentItem.Cancel();
            DeleteItem(fCurrentItem);
            CurrentItemNeeded();
        }
    }
    
    public bool IsEmpty()
    {
        return fItems.Count == 0;
    }

    public void Process(float time_delta)
    {
        if ((fState == QueryState.Active) && 
            (fCurrentItem != null))
        {
            if (fCurrentItem.Process(time_delta))
            {
                OnCurrentItemProduced.Invoke(this, fCurrentItem);
                DeleteItem(fCurrentItem);
            }
            else
            {
                OnCurrentItemProgress.Invoke(this, fCurrentItem, fCurrentItem.Progress);
            }
        }
    }

    protected bool CurrentItemNeeded()
    {
        if (IsEmpty())
        {
            fCurrentItem = null;
        }
        else
        {
            fCurrentItem = fItems[0];
            fCurrentItem.Start();
        }        

        OnItemChanged.Invoke(this, fCurrentItem);
        return IsEmpty();
    }
}

[RequireComponent(typeof(UnitObject_Script))]
public class ProductionObject_Script : MonoBehaviour
{
    public List<string> Default_ProductionTypeNameList = new List<string>();
    public NotifyEvent<ProductionObject_Script> OnProductionListChanged = new NotifyEvent<ProductionObject_Script>();

    protected UnitObject_Script fUnit = null;
    protected List<ProductionObject_Script_ProductionItem> fProductList = new List<ProductionObject_Script_ProductionItem>();
    protected ProductionObject_Script_ProductionQuery fProductionQuery = new ProductionObject_Script_ProductionQuery();
    private float fDeltaTime = 0f;


    public ProductionObject_Script_ProductionQuery ProductionQuery
    {
        get
        {
            return fProductionQuery;
        }
    }

    public void ItemsFromTypeNameList(List<string> type_names)
    {
        ClearProductionList();

        for (int i = 0; i < type_names.Count; i++)
            AddItem(type_names[i]);
    }

    public void AddItem(string type_name)
    {
        ObjectType_Script type = TypeObjectManager_Script.TypeInstance(type_name);

        if (type == null)
            return;

        ProductionItem_ObjectType item = new ProductionItem_ObjectType();
        item.ObjectType = type;
        item.TargetOwner = fUnit.Owner;
        AddItem(item);
    }

    public void AddItem(ProductionObject_Script_ProductionItem item)
    {
        fProductList.Add(item);
        OnProductionListChanged.Invoke(this);
    }

    public void RemoveItem(ProductionObject_Script_ProductionItem item)
    {
        fProductList.Remove(item);
        OnProductionListChanged.Invoke(this);
    }

    public void ProductItem(int index)
    {
        if ((index < 0) || 
            (index > fProductList.Count))
        {
            return;
        }

        fProductionQuery.AddItem(fProductList[index], true);
    }

    public void ProductItem(string type_name)
    {
        ProductItem(ItemIndex(type_name));
    }

    public void CancelProduct(int index)
    {
        fProductionQuery.DeleteItem(index);
    }

    public void CancelProduct(string type_name)
    {
        fProductionQuery.DeleteItem(type_name);
    }

    public int ItemIndex(string type_name)
    {
        for (int i=0; i<fProductList.Count; i++)
        {
            if ((fProductList[i] as ProductionItem_ObjectType).ObjectType.TypeInfo.TypeName.Equals(type_name))
                return i;
        }

        return -1;
    }

    public void ClearProductionList()
    {
        while (fProductList.Count > 0)
            RemoveItem(fProductList[0]);
    }

    public ProductionObject_Script_ProductionItem ItemAt(int index)
    {
        return fProductList[index];
    }

    public int ItemCount
    {
        get
        {
            return fProductList.Count;
        }
    }

    public bool IsValidIndex(int index)
    {
        return (index >= 0) && (index < fProductList.Count);
    }

    public UnitObject_Script Unit
    {
        get
        {
            return fUnit;
        }
    }

    IEnumerator DoInit()
    {
        yield return null;
        ItemsFromTypeNameList(Default_ProductionTypeNameList);
    }

    // Use this for initialization
    void Start()
    {
        fUnit = this.GetComponent<UnitObject_Script>();
        StartCoroutine(DoInit());
    }

    // Update is called once per frame
    void Update()
    {
        fDeltaTime += Time.fixedDeltaTime;

        if (Time.frameCount % GlobalCollector.UPDATE_TICKS_FOR_PRODUCTION_ITEM_PROCESS == 0)
        {
            fProductionQuery.Process(fDeltaTime);
            fDeltaTime = 0;
        }
    }
}
