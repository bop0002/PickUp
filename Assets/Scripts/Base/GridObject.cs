using UnityEngine;
using static GridSystem;
using static UnityEngine.EventSystems.EventTrigger;

public abstract class GridObject<TGridObject,TDataSO,TChild> where TDataSO : IHasPrefab where TChild : GridObject<TGridObject, TDataSO, TChild>
{
    protected Grid<TChild> grid;
    protected int x;
    protected int z;
    Entity<TDataSO> entity;

    public void SetObject(Entity<TDataSO> entity)
    {
        this.entity = entity;
        grid.TriggerGridObjectChanged(x, z);
    }

    public Entity<TDataSO> GetEntity()
    {
        return this.entity;
    }

    public void ClearObject()
    {
        entity = null;
        grid.TriggerGridObjectChanged(x, z);
    }

    public bool CanBuild()
    {
        return this.entity == null;
    }

    public GridObject(Grid<TChild> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }
    override
    public string ToString()
    {
        return x + "," + z;
    }

}
