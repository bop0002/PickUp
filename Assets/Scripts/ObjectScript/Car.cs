using UnityEngine;
using System;
using System.Collections;
using UnityEngine;
using TMPro;
public class Car : PlacedObject<CarSO>
{
    [SerializeField] TMP_Text tempText;
    public static Car Create(Vector3 worldPosition, CarSO carSO)
    {
        return Create<Car>(worldPosition, carSO);
    }
    public override string ToString()
    {
        return "Car: "  + GetColor().ToString() + ", " ;
    }
    public void UpdateText(int seat)
    {
        tempText.text = seat.ToString();
    }
    //public override void TestAnimationOnDepart()
    //{
    //    this.transform.localPosition += new Vector3(0, 1.2f, 0);
    //}
    public override IEnumerator TestAnimation(Vector3 moveTarget)
    {
        while (Vector3.Distance(transform.position, moveTarget) > 0.1f)
        {
            Vector3 destination = Vector3.MoveTowards(transform.position, moveTarget, 2f * Time.deltaTime);
            this.transform.position = destination;
            yield return null;
        }
    }
}