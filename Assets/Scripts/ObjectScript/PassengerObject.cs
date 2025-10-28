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
    public PassengerObject(Grid<PassengerObject> grid) : base(grid)
    {
    }
}
