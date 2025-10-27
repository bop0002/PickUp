using UnityEngine;
using static GridSystem;

public class CarObject : GridObject<Car,CarSO,CarObject>
{
    public void SetCar(Car placeCar)
    {
        this.entity = placeCar;
        //grid.TriggerGridObjectChanged(x, z, this);
    }

    public Car GetCar()
    {
       return (Car)GetEntity();
    }
    public CarObject(Grid<CarObject> grid) : base(grid) { }
}
