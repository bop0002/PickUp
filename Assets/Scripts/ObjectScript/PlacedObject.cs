using System;
using System.Collections;
using UnityEngine;

public abstract class PlacedObject<TDataSO> : MonoBehaviour where TDataSO : ISO
{
    private TDataSO dataSO;
    public static T Create<T>(Vector3 worldPosition, TDataSO dataSO) where T : PlacedObject<TDataSO>
    {
        Transform placedObjectTransform = Instantiate(dataSO.GetPrefab().transform, worldPosition, Quaternion.identity);
        T placedObject = placedObjectTransform.GetComponent<T>();
        placedObject.dataSO = dataSO;
        return placedObject;
    }
    public override string ToString()
    {
        return dataSO.GetColor().ToString(); 
    }
    public TDataSO GetSO()
    {
        return dataSO;
    }
    public EColor GetColor()
    {
        return dataSO.GetColor();
    }
    public abstract IEnumerator TestAnimation(Vector3 moveTarget);
    public void SelfDestroy() //hoac la delay 1 frame roi moi update sideVisual who know hoac la destyroy immediate
    {
        Destroy(this.gameObject);
    }

}
