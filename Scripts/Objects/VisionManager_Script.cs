using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionManager_Script : MonoBehaviour {

    public int Default_VisionRange = 0;
    private int Default_FrontRange = 0;
    private int Default_SideRange = 0;
    private int Default_BackRange = 0;
    
    protected ObjectComponentsCollection_Script fUnit_Comp = null;
    protected int fVisionRange = 0;
    protected int fFrontRange = 0;
    protected int fSideRange = 0;
    protected int fBackRange = 0;
    protected float fUpdateTime = 0f;
    protected bool fUse = true;


    public int VisionRange
    {
        get
        {
            return fVisionRange;
        }

        set
        {
            fVisionRange = Default_VisionRange;
        }
    }

    public int FrontRange
    {
        get
        {
            return fFrontRange;
        }

        set
        {
            fFrontRange = value;
        }
    }

    public int SideRange
    {
        get
        {
            return fSideRange;
        }

        set
        {
            fSideRange = value;
        }
    }

    public int BackRange
    {
        get
        {
            return fBackRange;
        }

        set
        {
            fBackRange = value;
        }
    }

    public bool Use
    {
        get
        {
            return fUse;
        }

        set
        {
            fUse = value;
        }
    }

    // Use this for initialization
    void Start () {
        fUnit_Comp = this.GetComponent<ObjectComponentsCollection_Script>();
        VisionRange = fVisionRange;
        FrontRange = Default_FrontRange;
        SideRange = Default_SideRange;
        BackRange = Default_BackRange;
	}
	
	// Update is called once per frame
	void Update () {
        if (!fUse || 
            fUnit_Comp.Unit_Comp.Owner.IsEnemy(GlobalCollector.Instance.LocalPlayer) ||
            (fUnit_Comp.Unit_Comp.Owner != GlobalCollector.Instance.LocalPlayer))
            return;

		if (fUpdateTime >= GlobalCollector.Vision_UpdatePeriod)
        {
            fUpdateTime = 0;
            Vector3 pos = fUnit_Comp.GameObject_Comp.Body.position;

            Vector2Int bl_coord = GlobalCollector.Instance.Current_Map.WorldToMapCoord(pos);
            Vector2Int cur_cell_pos;

            MapCell_Script cell;

            for (int i = -fVisionRange; i<fVisionRange + 1; i++)
            {
                for (int j = -fVisionRange; j<fVisionRange+ 1; j++)
                {
                    cur_cell_pos = bl_coord + new Vector2Int(j, i);
                    cell = GlobalCollector.Instance.Current_Map.GetMapCell(cur_cell_pos);

                    if (GlobalCollector.Instance.Current_Map.IsValidMapCoords(cur_cell_pos) &&
                        (Vector3.Distance(cell.Body.position, pos) <= fVisionRange * GlobalCollector.Cell_Size))
                    {
                        cell.FOW_ShowCell();
                    }
                }
            }

        }
        else
        {
            fUpdateTime += Time.deltaTime;
        }
	}
}
