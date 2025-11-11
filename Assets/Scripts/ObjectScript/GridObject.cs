using UnityEngine;
using UnityEngine.LowLevelPhysics;
using static GridSystem;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEditor;
public abstract class GridObject<TGridObject,TDataSO,TChild> where TDataSO : ISO where TChild : GridObject<TGridObject, TDataSO, TChild>
{
    protected Grid<TChild> grid;
    protected PlacedObject<TDataSO> placedObject;

    public void SetObject(PlacedObject<TDataSO> placedObject)
    {
        this.placedObject = placedObject;
    }
    public PlacedObject<TDataSO> GetPlacedObject()
    {
        return this.placedObject;
    }
    public EColor GetColor()
    {
        return placedObject.GetColor();
    }
    public void ClearObject()
    {
        placedObject.SelfDestroy();
    }
    public void ClearObjectImmediate()
    {
        if (placedObject != null)
        {
            placedObject.SelfDestroy();
            placedObject = null;
        }
    }
    public GridObject(Grid<TChild> grid)
    {
        this.grid = grid;
    }
    override
    public string ToString()
    {
        return placedObject != null ? placedObject.ToString() + "," : "null,";
    }


}
