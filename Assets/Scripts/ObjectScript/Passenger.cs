using UnityEngine;
public class Passenger : PlacedObject<PassengerSO>
{
    public static Passenger Create(Vector3 worldPosition, PassengerSO passengerSO)
    {
        return Create<Passenger>(worldPosition, passengerSO);
    }

    public override string ToString()
    {
        return "Passenger: " + GetColor().ToString();
    }
}
