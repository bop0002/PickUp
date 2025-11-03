using UnityEngine;
using static GridSystem;

public class CarObject : GridObject<Car,CarSO,CarObject>
{
    private int seatSlot;
    public void SetCar(Car placeCar)
    {
        //this.entity = placeCar;
        ////grid.TriggerGridObjectChanged(x, z, this);
        SetObject(placeCar);
        seatSlot = placeCar.GetSO().GetSeatSlot();
    }
    public void SetCar(Car placeCar,int slotSeated)
    {
        //this.entity = placeCar;
        ////grid.TriggerGridObjectChanged(x, z, this);
        SetObject(placeCar);
        seatSlot = slotSeated;
    }
    public void SeatDec()
    {
        seatSlot--;
    }
    public int GetSeat()
    {
        return seatSlot;
    }
    public Car GetCar()
    {
       return (Car)GetEntity();
    }
    public override string ToString()
    {
        return entity.ToString() + "," + seatSlot.ToString();
    }
    public CarObject(Grid<CarObject> grid) : base(grid) { }
}
