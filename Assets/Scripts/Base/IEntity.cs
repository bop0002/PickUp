using UnityEngine;

public interface IEntity<DataSO> where DataSO : ISO
{
    public static void Create(Vector3 worldPosition, DataSO dataSO)
    {

    }

}
