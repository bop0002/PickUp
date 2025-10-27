using UnityEngine;

public class Car : Entity<CarSO>
{

    public static Car Create(Vector3 worldPosition, CarSO carSO)
    {
        return Create<Car>(worldPosition, carSO);
    }



}