using UnityEngine;
using static GridSystem;
using static UnityEngine.EventSystems.EventTrigger;

public abstract class GridObject<TGridObject,TDataSO,TChild> where TDataSO : ISO where TChild : GridObject<TGridObject, TDataSO, TChild>
{
    protected Grid<TChild> grid;
    protected Entity<TDataSO> entity;

    public void SetObject(Entity<TDataSO> entity)
    {
        this.entity = entity;
        //grid.TriggerGridObjectChanged(x, z);
    }
    public Entity<TDataSO> GetEntity()
    {
        return this.entity;
    }
    public EColor GetColor()
    {
        return entity.GetColor();
    }
    public void ClearObject()
    {
        entity.SelfDestroy();
    }
    
    public bool CanBuild()
    {
        return this.entity == null;
    }

    public GridObject(Grid<TChild> grid)
    {
        this.grid = grid;
    }
    override
    public string ToString()
    {
        return entity.ToString()+ ",";
    }

}
