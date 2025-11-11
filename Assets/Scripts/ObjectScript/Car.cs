using UnityEngine;

public class Car : PlacedObject<CarSO>
{

    public static Car Create(Vector3 worldPosition, CarSO carSO)
    {
        return Create<Car>(worldPosition, carSO);
    }

    public override string ToString()
    {
        return "Car: "  + GetColor().ToString() + ", " ;
    }
}