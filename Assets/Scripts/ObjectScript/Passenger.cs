using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Passenger : Entity<PassengerSO>
{
    public static Passenger Create(Vector3 worldPosition, PassengerSO passengerSO)
    {
        return Create<Passenger>(worldPosition, passengerSO);
    }
}
