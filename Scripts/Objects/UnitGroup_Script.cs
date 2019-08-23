using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class UnitGroup_Script : List<UnitObject_Script> {


    public UnitObject_Script FirstEntryByTypeName(string type_name)
    {
        for (int i = 0; i < Count; i++)
        {
            ObjectTypeInfo_Script ti = this[i].GetComponent<ObjectTypeInfo_Script>();

            if (ti.TypeInfo.TypeName.Equals(type_name))
            {
                return this[i];
            }
        }

        return null;
    }

    public void MatchUnitsByTypeName(string type_name, ref UnitGroup_Script result)
    {
        result.Clear();

        for (int i = 0; i < Count; i++)
        {
            ObjectTypeInfo_Script ti = this[i].GetComponent<ObjectTypeInfo_Script>();

            if (ti == null)
                continue;

            if (ti.TypeInfo.TypeName.Equals(type_name))
            {
                result.Add(this[i]);
            }
        }
    }

    public void UnitsInRadius(Vector3 pos, int range)
    {
        Clear();

        Vector2Int c_pos = GlobalCollector.Instance.Current_Map.WorldToMapCoord(pos);
        MapCell_Script cell;

        for (int i = -range; i < range + 1; i++)
        {
            for (int j = -range; j < range + 1; j++)
            {
                cell = GlobalCollector.Instance.Current_Map.GetMapCell(c_pos + new Vector2Int(j, i));

                if (cell == null)
                    continue;

                if (Vector3.Distance(cell.Body.position, pos) <= range * GlobalCollector.Cell_Size)
                {
                    foreach (GameObject _object in cell.ContainingObjects)
                    {
                        UnitObject_Script unit = _object.GetComponent<UnitObject_Script>();

                        if ((unit != null) && 
                            !Contains(unit) &&
                            unit.IsAlive())
                        {
                            Add(unit);
                        }
                    }
                }
            }
        }
    }

    public UnitObject_Script First
    {
        get
        {
            if (Count > 0)
                return this[0];

            return null;
        }
    }

    public UnitObject_Script Last
    {
        get
        {
            if (Count > 0)
                return this[Count - 1];

            return null;
        }
    }

    public UnitObject_Script FirstEnemy(Player_Script player)
    {
        for (int i=0; i<Count; i++)
        {
            if (player.IsEnemy(this[i].Owner))
                return this[i];                     
        }

        return null;
    }

    public void Order_Follow(Vector3 target)
    {
        foreach (UnitObject_Script unit in this)
        {
            unit.Order_Follow(target);
        }
    }

    public void Order_Follow(UnitObject_Script target)
    {
        foreach (UnitObject_Script unit in this)
        {
            unit.Order_Follow(target);
        }
    }

    public void Order_Attack(Vector3 target)
    {
        foreach (UnitObject_Script unit in this)
        {
            unit.Order_Attack(target);
        }
    }

    public void Order_Attack(UnitObject_Script target)
    {
        foreach (UnitObject_Script unit in this)
        {
            unit.Order_Attack(target);
        }
    }


    public void Order_Cancel()
    {
        foreach (UnitObject_Script unit in this)
        {
            unit.Order_Cancel();
        }
    }
	
}
