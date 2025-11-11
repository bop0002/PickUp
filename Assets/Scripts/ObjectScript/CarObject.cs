using UnityEngine;
using static GridSystem;

public class CarObject : GridObject<Car,CarSO,CarObject>
{
    private int seatSlot;
    public void SetCar(Car placeCar)
    {
        SetObject(placeCar);
        seatSlot = placeCar.GetSO().GetSeatSlot();
    }
    public void SetCar(Car placeCar,int slotSeated)
    {
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
       return (Car)GetPlacedObject();
    }
    public override string ToString()
    {
        return placedObject.ToString() + "," + seatSlot.ToString();
    }
    public CarObject(Grid<CarObject> grid) : base(grid) { }
}
