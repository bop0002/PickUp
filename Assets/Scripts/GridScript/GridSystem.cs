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
    private Dictionary<EColor, PassengerSO> passengerSODict;
    private List<GameObject> passengerColumns;
    private List<EColor> carTypeSOList;
    private List<RowVisualGroup> rowParentGameObject;
    private int[] departIndexPassenger = { 0, 1, 2 };
    private int[] departIndexCar = { 0, 2, 4 };
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
    private SpawnData spawnData;
    public event Action<int> OnCenterGroupChange;

    string folderSpawnPath;
    string fileSpawnPath;
    string json;
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
        passengerSODict = new Dictionary<EColor, PassengerSO>();
        foreach(var so in passengerSOList)
        {
            passengerSODict[so.GetColor()] = so;
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
        gridPassengerCellSize = 2.65f;
        gridPassengerOrigin = new Vector3(-4f, 0, -3f);


        gridCar = new Grid<CarObject>(gridCarWidth, gridCarHeight, gridCarCellSize, gridCarOrigin, (Grid<CarObject> g) => new CarObject(g));
        gridPassenger = new Grid<PassengerObject>(gridPassengerWidth, gridPassengerHeight, gridPassengerCellSize, gridPassengerOrigin, (Grid<PassengerObject> g) => new PassengerObject(g));
        carSO = carSOList[0];
        passengerSO = passengerSOList[0];
        rowParentGameObject = new List<RowVisualGroup>();
        passengerColumns = new List<GameObject>();

        folderSpawnPath = Path.Combine(Application.streamingAssetsPath, "LevelPattern");
        fileSpawnPath = Path.Combine(folderSpawnPath, "level1.json");
        json = File.ReadAllText(fileSpawnPath);
        spawnData = JsonUtility.FromJson<SpawnData>(json);

        gridCar.OnGridRowVisualChanged += Grid_OnGridRowVisualChanged;
        gridCar.OnGridColumnVisualChanged += GridCar_OnGridColumnVisualChanged;
        gridPassenger.OnGridColumnVisualChanged += GridPassenger_OnGridColumnVisualChanged;

        InitCarFile();
        InitPassenger();
    }


    private void InitCarFile()
    {
        for (int z = 0; z < gridCarHeight; z++)
        {
            GameObject rowVisualParent = new GameObject($"Row_{z}");
            rowVisualParent.transform.SetParent(this.transform, false);
            GameObject centerVisualParent = new GameObject($"Row_{z}_Center");
            centerVisualParent.transform.SetParent(rowVisualParent.transform);
            for (int x = 0; x < gridCarWidth; x++)
            {
                CarSO carSO = GetNextCarSO(x);
                if(carSO!= null)
                {
                    Car spawnnedCar = Car.Create(gridCar.GetWorldPosition(x, z), carSO);
                    spawnnedCar.transform.SetParent(centerVisualParent.transform, false);
                    CarObject gridObject = gridCar.GetGridObject(x, z);
                    gridObject.SetCar(spawnnedCar);
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
                passengerSO = GetNextPassengerSO(x);
                if(passengerSO!= null)
                {
                    Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(x, z), passengerSO);
                    PassengerObject passengerObject = gridPassenger.GetGridObject(x, z);
                    passenger.transform.SetParent(passengerColumn.transform, false);
                    passengerObject.SetPassenger(passenger);
                }
                else
                {
                    Debug.Log("Error init passengerSO = null Init");
                }
            }
            passengerColumns.Add(passengerColumn);
            passengerColumn.transform.SetParent(passengerGrid.transform,false);
        }
    }
    private PassengerSO GetNextPassengerSO(int x)
    {
        SpawnDataColumn columnData = spawnData.passengerColumns[x];
        if (columnData.indexCount >= columnData.pattern.Count)
        {
            Debug.Log("out of index in json passenger");
            return null;
        }
        string enumString = columnData.pattern[spawnData.carColumns[x].indexCount++];
        if (Enum.TryParse(enumString, out EColor passengerColor))
        {
            if (passengerSODict.TryGetValue(passengerColor, out PassengerSO passengerSO))
            {
                Debug.Log($"JSON content Passenger:\n{columnData.columnIndex}");
                return passengerSO;
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
        return null;
    }
    private CarSO GetNextCarSO(int x)
    {
        SpawnDataColumn columnData = spawnData.carColumns[x];
        if (columnData.indexCount >= columnData.pattern.Count)
        {
            Debug.Log("out of index in json car");
            return null;
        }
        string enumString = columnData.pattern[spawnData.carColumns[x].indexCount++];
        if (Enum.TryParse(enumString, out ECarType carType))
        {
            if (carSODict.TryGetValue(carType, out CarSO carSO))
            {
                Debug.Log($"JSON content Car:\n{columnData.columnIndex}");
                return carSO;
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
        return null;

    }
    //Visual manage
    //public void TestSpawn(int x, int z)
    //{
    //    CarObject carObject = gridCar.GetGridObject(x, z);
    //    carObject.ClearObject();
    //    CarSO carSO = GetNextCarSO(x);
    //    if (carSO != null)
    //    {
    //        Car car = Car.Create(gridCar.GetWorldPosition(x, z), carSO);
    //        carObject.SetCar(car);
    //        car.transform.SetParent(rowParentGameObject[z].GetCenterGroup().transform, false); Debug.Log("Cant parse enum");
    //    }
    //}
    public void TestSpawn()
    {
        CarObject[,] carArray = gridCar.GetGridArray();
        PassengerObject[,] passengerArray = gridPassenger.GetGridArray();
        int countCarDepart;
        int countPassengerDepart;
        int passengerHeight = gridPassengerHeight - 1;
        int carHeight = gridCarHeight - 1;
        int carZ;
        int passengerZ;
        for (int i = 0; i < departIndexPassenger.Length; i++)
        {
            for (int j = 0; j < departIndexCar.Length; j++)
            {
                countCarDepart = 0;
                countPassengerDepart = 0;
                carZ = carHeight - countCarDepart;
                passengerZ = passengerHeight - countPassengerDepart;
                if (carArray[departIndexCar[j], carZ].GetColor() == passengerArray[departIndexPassenger[i], passengerZ].GetColor())
                {
                    while (carArray[departIndexCar[j], carZ].GetColor() == passengerArray[departIndexPassenger[i], passengerZ].GetColor())
                    {
                        Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(departIndexPassenger[i], passengerZ), GetNextPassengerSO(departIndexPassenger[i]));
                        PassengerObject passengerObject = gridPassenger.GetGridObject(departIndexPassenger[i], passengerZ);
                        passengerObject.ClearObject();
                        passengerObject.SetPassenger(passenger);
                        countPassengerDepart++;
                        passengerZ = passengerHeight - countPassengerDepart;
                    }
                    Car car = Car.Create(gridCar.GetWorldPosition(departIndexCar[j], carZ), GetNextCarSO(departIndexCar[j]));
                    CarObject carObject = gridCar.GetGridObject(departIndexCar[j], carZ);
                    carObject.ClearObject();
                    carObject.SetCar(car);
                    countCarDepart++;
                    carZ = carHeight - countCarDepart;
                }
                if (countCarDepart > 0)
                {
                    gridCar.ShiftColumnUp(departIndexCar[j],countCarDepart);
                }
                if(countPassengerDepart > 0)
                {
                    gridPassenger.ShiftColumnUp(departIndexPassenger[i],countPassengerDepart);
                }
            }
        }
    }
    //Passenger
    private void GridPassenger_OnGridColumnVisualChanged(object sender, Grid<PassengerObject>.OnGridColumnVisualChangedEventArgs e)
    {
        if (e.column < 0 || e.column >= gridPassengerWidth)
        {
            Debug.Log("out of index column");
            return;
        }
        for (int i = 0; i < gridPassengerHeight; i++)
        {
            UpdateVisualPassengerRowAfterShift(i);
        }
        gridPassenger.DebugPrintGridArray();
    }
    private void UpdateVisualPassengerRowAfterShift(int row)
    {
        ClearRowPassengerVisual(row);
        for (int x = 0; x < gridPassengerWidth; x++)
        {
            PassengerObject passengerObject = gridPassenger.GetGridObject(x, row);
            Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(x, row), passengerObject.GetPassenger().GetSO());
            passenger.transform.SetParent(passengerColumns[x].transform, false);
            passengerObject.SetObject(passenger);
        }
        

    }
    private void ClearRowPassengerVisual(int row)
    {
        for (int x = 0; x < gridPassengerWidth; x++)
        {
            PassengerObject passengerObject = gridPassenger.GetGridObject(x, row);
            if (passengerObject.GetPassenger() != null)
            {
                passengerObject.ClearObject();
            }
        }
    }
    //Car

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
    private void UpdateVisualRowAfterShift(int row)
    {
        ClearRowObjectVisual(row);
        for (int x = 0; x < gridCarWidth; x++)
        {
            CarObject carObject = gridCar.GetGridObject(x, row);
            Car spawnedCar = Car.Create(gridCar.GetWorldPosition(x, row), carObject.GetCar().GetSO());
            carObject.SetCar(spawnedCar);
            spawnedCar.transform.SetParent(rowParentGameObject[row].GetCenterGroup(),false);
        }
        OnCenterGroupChange?.Invoke(row);

    }

    private void ClearRowObjectVisual(int row)
    {
        for (int x = 0; x < gridCarWidth; x++)
        {
            CarObject carObject = gridCar.GetGridObject(x, row);
            if (carObject.GetCar() != null)
            {
                carObject.ClearObject();
            }
        }
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
