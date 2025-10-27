using UnityEngine;

public interface IEntity<DataSO> where DataSO : IHasPrefab
{
    public static void Create(Vector3 worldPosition, DataSO dataSO)
    {

    }

}
