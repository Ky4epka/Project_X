using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridValueMap;
using UnityEngine.Assertions;

namespace ObjectGrid
{
    public class GridCellMap : GriddedValueMap<GridCell>
    {
        public override GridCell AllocCell()
        {
            Assert.IsNotNull(fOwner);
            GridCell cell = Object.Instantiate((fOwner as Grid).GridCellPrefab, fOwner.transform);
            cell.gameObject.SetActive(true);
            return cell;
        }

        public override void ReleaseCell(GridCell cell)
        {
            Assert.IsNotNull(fOwner);
            Object.Destroy(cell);
        }

        public override void AssignCell(GridCell source, GridCell destination)
        {
            Assert.IsNotNull(fOwner);
            destination.Assign(source);
        }

        public override void CellPositionChanged(GridCell cell, int x, int y)
        {
            Assert.IsNotNull(fOwner);
            cell.SetPosition(fOwner.CellToWorld(x, y));
            cell.SetSize(fOwner.cellSize);
        }
    }

    public class Grid : GridValueMap<GridCell>
    {
        public GridCell GridCellPrefab = null;

        private void Awake()
        {
            AttachedValueMap = new GridCellMap();
        }
    }

}