using UnityEngine;

public abstract class Entity<TDataSO> : MonoBehaviour where TDataSO : IHasPrefab
{
    protected TDataSO dataSO;
    public static T Create<T>(Vector3 worldPosition, TDataSO dataSO) where T : Entity<TDataSO>
    {
        Transform entityTransform = Instantiate(dataSO.GetPrefab().transform, worldPosition, Quaternion.identity);
        T entity = entityTransform.GetComponent<T>();
        entity.dataSO = dataSO;
        return entity;
    }

    public TDataSO GetSO()
    {
        return dataSO;
    }

    public void SelfDestroy() //hoac la delay 1 frame roi moi update sideVisual who know hoac la destyroy immediate
    {
        Destroy(this.gameObject);
    }

}
