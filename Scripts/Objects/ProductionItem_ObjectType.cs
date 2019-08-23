using UnityEngine;
using System.Collections;

public class ProductionItem_ObjectType : ProductionObject_Script_ProductionItem
{
    protected Player_Script fTargetOwner = null;
    protected float fPaidSpice = 0;
    protected float fTargetSpice = 0;
    protected float fSpiceSpeed = 0;
    protected ObjectType_Script fObjectType = null;
    protected ObjectTypeInfo_Script fProducedObject = null;

    public Player_Script TargetOwner
    {
        get
        {
            return fTargetOwner;
        }

        set
        {
            if (fTargetOwner != null)
            {
                fTargetOwner.ParametersValues.CurrentSpice.Add(fPaidSpice);
                fPaidSpice = 0;
            }

            fTargetOwner = value;
        }
    }

    public ObjectType_Script ObjectType
    {
        get
        {
            return fObjectType;
        }

        set
        {
            fObjectType = value;

            if (fObjectType != null)
            {
                ProductionTime = fObjectType.TypeInfo.TypeData.Base_BuildTime;

                if (MathKit.NumbersEquals(ProductionTime, 0))
                    ProductionTime = 0.1f;

                fTargetSpice = fObjectType.TypeInfo.TypeData.Base_Cost;

                if (MathKit.NumbersEquals(fTargetSpice, 0))
                    fTargetSpice = 1;

                fSpiceSpeed = fTargetSpice / ProductionTime;
            }
            else
            {
                ProductionTime = 1;
                fPaidSpice = 0;
                fTargetSpice = 1;
                fSpiceSpeed = 1;
            }
        }
    }

    public ObjectTypeInfo_Script ProducedObject
    {
        get
        {
            return fProducedObject;
        }
    }

    public override float Progress
    {
        get
        {
            if (fTargetOwner == null)
                return base.Progress;

            if (MathKit.NumbersEquals(fTargetSpice, 0))
                return 1f;

            return fPaidSpice / fTargetSpice;
        }
    }

    public override void Assign(ProductionObject_Script_ProductionItem source)
    {
        base.Assign(source);

        if (source is ProductionItem_ObjectType)
        {
            ProductionItem_ObjectType item = source as ProductionItem_ObjectType;
            ObjectType = item.ObjectType;
            TargetOwner = item.TargetOwner;
        }
    }

    public override ProductionObject_Script_ProductionItem Copy()
    {
        ProductionItem_ObjectType item = new ProductionItem_ObjectType();
        item.Assign(this);
        return item;
    }

    public override bool Start()
    {
        if (base.Start())
        {
            fPaidSpice = 0f;
            return true;
        }

        return false;
    }

    public override bool Cancel()
    {
        if (base.Cancel())
        {
            if (fTargetOwner != null)
                fTargetOwner.ParametersValues.CurrentSpice.Add(fPaidSpice);

            fPaidSpice = 0f;
            return true;
        }

        return false;
    }

    public override bool Process(float time_delta)
    {
        if ((fObjectType == null) || (fTargetOwner == null))
            return base.Process(time_delta);

        float spice = fSpiceSpeed * time_delta;

        spice = fTargetOwner.ParametersValues.CurrentSpice.Sub(spice);

        if (!MathKit.NumbersEquals(spice, 0))
        {
            fPaidSpice += spice;
        }

        if (fPaidSpice > fTargetSpice)
        {
            Product();
            fStart = false;
            return true;
        }

        return false;
    }
    
    protected override void Product()
    {
        fProducedObject = fObjectType.CreateInstance();
        UnitObject_Script unit = fProducedObject.GetComponent<UnitObject_Script>();

        if (unit != null)
        {
            unit.Default_Owner = fTargetOwner;
        }

        base.Product();
    }

}
