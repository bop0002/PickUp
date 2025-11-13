using System;
using System.Collections;
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
    //public override void TestAnimationOnDepart()
    //{
    //    this.transform.localPosition += new Vector3(0, 1.2f, 0);
    //}
    public override IEnumerator TestAnimation(Vector3 moveTarget)
    {
        while(Vector3.Distance(transform.position, moveTarget) > 0.1f)
        {
            Vector3 destination = Vector3.MoveTowards(transform.position,moveTarget,2 *Time.deltaTime);
            this.transform.position = destination;
            yield return null;
        }
    }
}
