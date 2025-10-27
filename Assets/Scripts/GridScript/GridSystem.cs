using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using System;
using Unity.Mathematics;
using System.IO;
using static UnityEngine.Rendering.DebugUI.Table;
public class GridSystem : MonoBehaviour
{
    [SerializeField] private List<CarSO> carSOList;
    private Dictionary<ECarType, CarSO> carSODict;
    [SerializeField] private List<PassengerSO> passengerSOList;
    private List<EColor> carTypeSOList;
    private List<RowVisualGroup> rowParentGameObject;
    private CarSO carSO;
    private PassengerSO passengerSO;
    private Grid<CarObject> gridCar;
    private Grid<PassengerObject> gridPassenger;
    private int gridCarWidth;
    private int gridCarHeight;
    private int gridPassengerWidth;
    private int gridPassengerHeight;
    private float gridCarCellSize;
    private float gridPassengerCellSize;
    private Vector3 gridCarOrigin;
    private Vector3 gridPassengerOrigin;
    private float carGridTotalWidth;
    private CarSpawnData carSpawnData;
    public event Action<int> OnCenterGroupChange;
    
    private void Awake()
    {
        InitDict();
        Init();
    }
    //Init base
    private void InitDict()
    {
        carSODict = new Dictionary<ECarType, CarSO>();
        foreach (var so in carSOList)
        {
            carSODict[so.GetCarType()] = so;
        }
    }
    private void Init()
    {
        gridCarWidth = 5;
        gridCarHeight = 3;
        gridCarCellSize = 3f;
        carGridTotalWidth = gridCarCellSize * gridCarWidth;
        gridCarOrigin = new Vector3(-7, 0, -12.5f);

        gridPassengerWidth = 3;
        gridPassengerHeight = 6;
        gridPassengerCellSize = 2.65f ;
        gridPassengerOrigin = new Vector3(-4f, 0, -3f);


        gridCar = new Grid<CarObject>(gridCarWidth, gridCarHeight, gridCarCellSize, gridCarOrigin, (Grid<CarObject> g) => new CarObject(g));
        gridPassenger = new Grid<PassengerObject> (gridPassengerWidth,gridPassengerHeight ,gridPassengerCellSize,gridPassengerOrigin,(Grid<PassengerObject> g) => new PassengerObject(g));
        carSO = carSOList[0];
        passengerSO = passengerSOList[0];
        rowParentGameObject = new List<RowVisualGroup>();
        

        gridCar.OnGridRowVisualChanged += Grid_OnGridRowVisualChanged;
        gridCar.OnGridColumnVisualChanged += GridCar_OnGridColumnVisualChanged;
        InitCarFile();
        InitPassenger();
    }
    private void InitCarFile()
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, "LevelPattern");
        string filePath = Path.Combine(folderPath, "level1.json");

        string json = File.ReadAllText(filePath);
        carSpawnData = JsonUtility.FromJson<CarSpawnData>(json);
        for (int z = 0; z < gridCarHeight; z++)
        {
            GameObject rowVisualParent = new GameObject($"Row_{z}");
            rowVisualParent.transform.SetParent(this.transform, false);
            GameObject centerVisualParent = new GameObject($"Row_{z}_Center");
            centerVisualParent.transform.SetParent(rowVisualParent.transform);
            for (int x = 0; x < gridCarWidth; x++)
            {
                string enumString = carSpawnData.columns[x].pattern[carSpawnData.columns[x].indexCount++];
                if (Enum.TryParse(enumString, out ECarType carType))
                {
                    if (carSODict.TryGetValue(carType, out CarSO carSO))
                    {
                        Debug.Log($"JSON content:\n{carSpawnData.columns[x].columnIndex}");
                        Car spawnnedCar = Car.Create(gridCar.GetWorldPosition(x, z), carSO);
                        spawnnedCar.transform.SetParent(centerVisualParent.transform, false);
                        CarObject gridObject = gridCar.GetGridObject(x, z);
                        gridObject.SetCar(spawnnedCar);
                    }
                    else
                    {
                        Debug.Log("Cant find carSO");
                    }
                }
                else
                {
                    Debug.Log("Cant parse enum");
                }
            }
            rowParentGameObject.Add(new RowVisualGroup(z, centerVisualParent.transform, carGridTotalWidth, this));
        }

    }
    private void InitPassenger()
    {
        GameObject passengerGrid = new GameObject("PassengerGrid");
        for (int x = 0; x < gridPassengerWidth; x++)
        {
            GameObject passengerColumn = new GameObject($"Coulmn_{x}");
            for (int z = 0; z < gridPassengerHeight; z++)
            {
                passengerSO = passengerSOList[UnityEngine.Random.Range(0, passengerSOList.Count)];
                Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(x, z), passengerSO);
                PassengerObject gridObject = gridPassenger.GetGridObject(x, z);
                passenger.transform.SetParent(passengerColumn.transform);
                gridObject.SetPassenger(passenger);
            }
            passengerColumn.transform.SetParent(passengerGrid.transform);
        }
    }
    //Visual manage
    public void TestSpawn(int x,int z)
    {
        CarObject gridObject = gridCar.GetGridObject(x, z);
        Car car = gridObject.GetCar();
        car.SelfDestroy();
        string enumString = carSpawnData.columns[x].pattern[carSpawnData.columns[x].indexCount++];
        if (Enum.TryParse(enumString, out ECarType carType))
        {
            if (carSODict.TryGetValue(carType, out CarSO carSO))
            {
                car = Car.Create(gridCar.GetWorldPosition(x, z), carSO);
                gridObject.SetCar(car);
                car.transform.SetParent(rowParentGameObject[z].GetCenterGroup().transform, false);
            }
            else
            {
                Debug.Log("Cant find carSO");
            }
        }
        else
        {
            Debug.Log("Cant parse enum");
        }

    }
    private void Grid_OnGridRowVisualChanged(object sender, Grid<CarObject>.OnGridRowVisualChangedEventArgs e)
    {
        if (e.row < 0 || e.row >= gridCarHeight)
        {
            Debug.Log("out of index");
            return;
        }
        UpdateVisualRowAfterShift(e.row);
        gridCar.DebugPrintGridArray();
    }
    private void GridCar_OnGridColumnVisualChanged(object sender, Grid<CarObject>.OnGridColumnVisualChangedEventArgs e) //co the refactor
    {
        if (e.column < 0 || e.column >= gridCarWidth)
        {
            Debug.Log("out of index column");
            return;
        }
        for(int i = 0; i < gridCarHeight; i++)
        {
            UpdateVisualRowAfterShift(i);
        }
        //UpdateVisualColumnAfterShift(e.column);
        gridCar.DebugPrintGridArray();
    }
    //private void UpdateVisualColumnAfterShift(int column)
    //{
    //    ClearColumnObjectVisual(column);
    //    for(int z = 0; z < gridCarHeight; z++)
    //    {
    //        CarObject gridObject = gridCar.GetGridObject(column,z);
    //        Car spawnedCar = Car.Create(gridCar.GetWorldPosition(column,z),gridObject.GetCar().GetSO());
    //        spawnedCar.transform.SetParent(rowParentGameObject[z].GetCenterGroup(),false);
    //        OnCenterGroupChange?.Invoke(z);
    //    }
    //}
    //private void ClearColumnObjectVisual(int column)
    //{
    //    for (int z = 0; z < gridCarHeight; z++)
    //    {
    //        CarObject gridObject = gridCar.GetGridObject(column, z);
    //        if (gridObject.GetCar() != null)
    //        {
    //            gridObject.GetCar().SelfDestroy();
    //        }
    //    }
    //}
    private void UpdateVisualRowAfterShift(int row)
    {
        ClearRowObjectVisual(row);
        for (int x = 0; x < gridCarWidth; x++)
        {
            CarObject gridObject = gridCar.GetGridObject(x, row);
            Car spawnedCar = Car.Create(gridCar.GetWorldPosition(x, row), gridObject.GetCar().GetSO());
            gridObject.SetCar(spawnedCar);
            spawnedCar.transform.SetParent(rowParentGameObject[row].GetCenterGroup(),false);
        }
        OnCenterGroupChange?.Invoke(row);

    }

    private void ClearRowObjectVisual(int row)
    {
        for (int x = 0; x < gridCarWidth; x++)
        {
            CarObject gridObject = gridCar.GetGridObject(x, row);
            if (gridObject.GetCar() != null)
            {
                gridObject.DestroyEntity();
            }
        }
    }
    // GetSet
    public RowVisualGroup GetRowVisualGroup(int row)
    {
        return rowParentGameObject[row];
    }

    public Grid<CarObject> GetGrid()
    {
        return this.gridCar;
    }
}
