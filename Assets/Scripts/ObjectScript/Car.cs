using UnityEngine;

public class Car : Entity<CarSO>
{

    public static Car Create(Vector3 worldPosition, CarSO carSO)
    {
        Car car = Create<Car>(worldPosition, carSO);
        return car;
    }

    public override string ToString()
    {
        return "Car: "  + GetColor().ToString() + ", " ;
    }
}