using UnityEngine;

public class PassengerObject : GridObject<Passenger, PassengerSO, PassengerObject>
{
    public void SetPassenger(Passenger passenger)
    {
        SetObject(passenger);
    }
    public Passenger GetPassenger()
    {
        return (Passenger) GetEntity();
    }
    public PassengerObject(Grid<PassengerObject> grid, int x, int z) : base(grid, x, z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }
}
