using UnityEngine;

public class PassengerObject : GridObject<Passenger, PassengerSO, PassengerObject>
{
    public void SetPassenger(Passenger passenger)
    {
        this.entity = passenger;
        //grid.TriggerGridObjectChanged(x, z, this);
    }
    public Passenger GetPassenger()
    {
        return (Passenger) GetEntity();
    }
    public PassengerObject(Grid<PassengerObject> grid) : base(grid)
    {
    }
}
