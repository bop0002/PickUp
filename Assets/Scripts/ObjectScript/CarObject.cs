using UnityEngine;
using static GridSystem;

public class CarObject : GridObject<Car,CarSO,CarObject>
{
    public void SetCar(Car placeCar)
    {
        SetObject(placeCar);
    }

    public Car GetCar()
    {
       return (Car)GetEntity();
    }
    public CarObject(Grid<CarObject> grid, int x, int z) : base(grid, x, z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }
}
