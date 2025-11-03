using UnityEngine;

[CreateAssetMenu(menuName ="ScriptableObject/CarScriptableObject")] 
public class CarSO : ScriptableObject,ISO
{
    [SerializeField] private string objectName;
    [SerializeField] private int seat;
    [SerializeField] private EColor color;
    [SerializeField] private ECarType carType;
    [SerializeField] private GameObject prefab;

    public ECarType GetCarType() { return carType; }
    public int GetSeatSlot()
    {
        return seat;
    }
    public GameObject GetPrefab()
    {
        return prefab;
    }
    public EColor GetColor()
    {
        return color;
    }

}
