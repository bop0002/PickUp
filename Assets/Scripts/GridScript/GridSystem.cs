using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using System;
using Unity.Mathematics;
using System.IO;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Collections;
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
    GameObject carGridParent;
    GameObject passengerGridParent;

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
        gridCarOrigin = new Vector3(-7, 0, -16.05f);

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
        InitPassengerFile();

        passengerColumns[0].transform.position = new Vector3(-2.7f, 0, 0);
        passengerColumns[1].transform.position = new Vector3(0.53f, 0, 0);
        passengerColumns[2].transform.position = new Vector3(3.85f, 0, 0);
    }
    //Hi HI
    public IEnumerator CheckDepartAndMove()
    {
        CarObject[,] carArray = gridCar.GetGridArray();
        PassengerObject[,] passengerArray = gridPassenger.GetGridArray();
        bool changed = true;
        bool breakFor;
        int passengerHeight = gridPassengerHeight - 1;
        int carHeight = gridCarHeight - 1;
        int carZ;
        int passengerZ;

        while (changed)
        {
            carArray = gridCar.GetGridArray();
            passengerArray = gridPassenger.GetGridArray();
            changed = false;
            breakFor = false;
            for (int i = 0; i < departIndexCar.Length; i++)
            {
                int countCarDepart = 0;
                for (int j = 0; j < departIndexPassenger.Length; j++)
                {
                    carZ = carHeight - countCarDepart;
                    int currentCarColumnIndex = departIndexCar[i];
                    int countPassengerDepart = 0;
                    int currentPassengerColumnIndex = departIndexPassenger[j];
                    passengerZ = passengerHeight - countPassengerDepart;
                    if (passengerArray[currentPassengerColumnIndex, passengerZ].GetPassenger() == null) continue;
                    if (carArray[currentCarColumnIndex, carZ].GetColor() == passengerArray[currentPassengerColumnIndex, passengerZ].GetColor())
                    {
                        while (carArray[currentCarColumnIndex, carZ].GetColor() == passengerArray[currentPassengerColumnIndex, passengerZ].GetColor())
                        {
                            yield return StartCoroutine(PassengerDepart(passengerArray[currentPassengerColumnIndex, passengerZ]));
                            countPassengerDepart++;
                            passengerZ = passengerHeight - countPassengerDepart;
                            carArray[currentCarColumnIndex, carZ].SeatDec();
                            if(carArray[currentCarColumnIndex,carZ].GetSeat()<=0)
                            {
                                countCarDepart++;
                                break;
                            }
                        }
                        if(countPassengerDepart > 0)
                        {
                            gridPassenger.ShiftColumnUp(currentPassengerColumnIndex,countPassengerDepart);
                            for(int k = 0;k<countPassengerDepart;k++)
                            {
                                PassengerSO nextPassengerSO = GetNextPassengerSO(currentPassengerColumnIndex);
                                if(nextPassengerSO!= null)
                                {
                                    Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(currentPassengerColumnIndex, k), nextPassengerSO);
                                    passengerArray[currentPassengerColumnIndex,k].SetPassenger(passenger);
                                }
                            }
                        }
                        if (carArray[currentCarColumnIndex, carZ].GetSeat() <= 0)
                        {
                            CarObject carObject = gridCar.GetGridObject(currentCarColumnIndex, carZ);
                            yield return StartCoroutine(CarDepart(carObject, currentCarColumnIndex, carZ));
                            gridCar.ShiftColumnUp(currentCarColumnIndex);
                        }
                        changed = true;
                        breakFor = true;
                    }
                    if (breakFor) break;
                }
                if(breakFor) break;
            }
        }

        gridCar.DebugPrintGridArray();
        gridPassenger.DebugPrintGridArray();
    }

    private IEnumerator PassengerDepart(PassengerObject passengerObject)
    {
        passengerObject.GetPassenger().TestAnimationOnDepart();
        yield return new WaitForSecondsRealtime(2);
        passengerObject.ClearObject();
    }

    private IEnumerator CarDepart(CarObject carObject, int column, int z)
    {
        carObject.GetCar().TestAnimationOnDepart();
        yield return new WaitForSecondsRealtime(2);
        CarSO nextCarSo = GetNextCarSO(column);
        if(nextCarSo != null)
        {
            carObject.ClearObject();
            Car car = Car.Create(gridCar.GetWorldPosition(column, z), nextCarSo);
            carObject.SetCar(car);
        }
    }
    //Passenger
    private void InitPassengerFile()
    {

        passengerGridParent = new GameObject("PassengerGrid");
        for (int x = 0; x < gridPassengerWidth; x++)
        {
            //int mirroredX = gridPassengerWidth - 1 - x;
            GameObject passengerColumn = new GameObject($"Column_{x}");
            for (int z = 0; z < gridPassengerHeight; z++)
            {
                int mirroredZ = gridPassengerHeight - 1 - z;
                passengerSO = GetNextPassengerSO(x);
                if (passengerSO != null)
                {
                    Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(x, mirroredZ), passengerSO);
                    passenger.name = $"({x},{z})";
                    PassengerObject passengerObject = gridPassenger.GetGridObject(x, z);
                    passenger.transform.SetParent(passengerColumn.transform, false);
                    passengerObject.SetPassenger(passenger);
                }
            }

            passengerColumns.Add(passengerColumn);
            passengerColumn.transform.SetParent(passengerGridParent.transform, false);
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
        string enumString = columnData.pattern[spawnData.passengerColumns[x].indexCount++];
        if (Enum.TryParse(enumString, out EColor passengerColor))
        {
            if (passengerSODict.TryGetValue(passengerColor, out PassengerSO passengerSO))
            {
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
    private void CreatePassenger(int column)
    {
        for (int z = 0; z < gridPassengerHeight; z++)
        {
            int mirroredZ = gridPassengerHeight - 1 - z;
            passengerSO = GetNextPassengerSO(column);
            if (passengerSO != null)
            {
                Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(column, mirroredZ), passengerSO);
                passenger.name = $"({column},{z})";
                PassengerObject passengerObject = gridPassenger.GetGridObject(column, z);
                passenger.transform.SetParent(passengerColumns[column].transform, false);
                passengerObject.SetPassenger(passenger);
            }
        }
    }
    private void GridPassenger_OnGridColumnVisualChanged(object sender, Grid<PassengerObject>.OnGridColumnVisualChangedEventArgs e)
    {
        if (e.column < 0 || e.column >= gridPassengerWidth)
        {
            Debug.Log("out of index column");
            return;
        }
        UpdateVisualPassengerColumnAfterShift(e.column);
    }
    private void UpdateVisualPassengerColumnAfterShift(int column)
    {
        ClearColumnPassengerVisual(column);
        CreatePassenger(column);
    }
    private void ClearColumnPassengerVisual(int column)
    {
        for (int z = 0; z < gridPassengerHeight; z++)
        {
            PassengerObject passengerObject = gridPassenger.GetGridObject(column, z);
            if (passengerObject.GetPassenger() != null)
            {
                passengerObject.ClearObject();
            }
        }
    }
    //Car
    private void InitCarFile()
    {
        carGridParent = new GameObject("CarGrid");
        for (int z = 0; z < gridCarHeight; z++)
        {
            GameObject rowVisualParent = new GameObject($"Row_{z}");
            rowVisualParent.transform.SetParent(this.transform, false);
            GameObject centerVisualParent = new GameObject($"Row_{z}_Center");
            centerVisualParent.transform.SetParent(rowVisualParent.transform);
            for (int x = 0; x < gridCarWidth; x++)
            {
                CarSO carSO = GetNextCarSO(x);
                if (carSO != null)
                {
                    Car spawnnedCar = Car.Create(gridCar.GetWorldPosition(x, z), carSO);
                    spawnnedCar.name = $"({x},{z})";
                    spawnnedCar.transform.SetParent(centerVisualParent.transform, false);
                    CarObject gridObject = gridCar.GetGridObject(x, z);
                    gridObject.SetCar(spawnnedCar);
                }
            }
            rowVisualParent.transform.SetParent(carGridParent.transform, false);
            rowParentGameObject.Add(new RowVisualGroup(z, centerVisualParent.transform, carGridTotalWidth, this));
            
        }
        //carGridParent.transform.position = new Vector3(0, 0, -3.55f);
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
            spawnedCar.name = $"({x},{row})";
            int tempSlotSeat = carObject.GetSeat();
            carObject.ClearObject();
            carObject.SetCar(spawnedCar,tempSlotSeat);
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
