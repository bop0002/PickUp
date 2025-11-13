using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;
using System;
using UnityEngine.Networking;
using Unity.Mathematics;
using System.IO;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Collections;
using UnityEngine.UIElements;
public class GridSystem : MonoBehaviour
{
    [SerializeField] private List<CarSO> carSOList;
    private Dictionary<ECarType, CarSO> carSODict;
    [SerializeField] private List<PassengerSO> passengerSOList;
    private Dictionary<EColor, PassengerSO> passengerSODict;
    private List<GameObject> passengerColumns;
    private List<EColor> carTypeSOList;
    private List<RowVisualGroup> rowParentGameObject;
    private readonly int[] departIndexPassenger = { 0, 1, 2 };
    private readonly int[] departIndexCar = { 0, 2, 4 };
    private readonly Vector3[] departPositions = { new Vector3(-6.86f, 0, -6.6f), new Vector3(-0.95f, 0, -6.6f), new Vector3(5.2f, 0, -6.6f) }; //Position dang loi!!!
    //private readonly Vector3[] departPositions = { new Vector3(-4f, 0, -6.6f), new Vector3(-1.35f, 0, -6.6f), new Vector3(1.3f, 0, -6.6f) };
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
    public event Action OnLoadDataSuccess;
    GameObject carGridParent;
    GameObject passengerGridParent;
    public bool isProcessing;
    string fileURL;
    string json;
    private void Awake()
    {
        InitDict();
        StartCoroutine(InitAfterLoad());
        // Init();
    }
    IEnumerator InitAfterLoad()
    {
        yield return StartCoroutine(LoadData());
        Init();
        yield return null;
        OnLoadDataSuccess?.Invoke();
    }
    IEnumerator LoadData()
    {
        string fileURL = Path.Combine(Application.streamingAssetsPath, "LevelPattern/level1.json");
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityWebRequest www = UnityWebRequest.Get(fileURL);
        yield return www.SendWebRequest();
        if(www.result!=UnityWebRequest.Result.Success)
        {
            Debug.Log("Faild load json webgl");
            yield break;
        }
        json = www.downloadHandler.text;
#else
        json = File.ReadAllText(fileURL);
        yield return null;
#endif
        spawnData = JsonUtility.FromJson<SpawnData>(json);
        Debug.Log("Load data succesful");
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
        foreach (var so in passengerSOList)
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

        // fileURL = Path.Combine(Application.streamingAssetsPath, "LevelPattern/level1.json");
        // json = File.ReadAllText(fileURL);
        // spawnData = JsonUtility.FromJson<SpawnData>(json);

        gridCar.OnGridRowVisualChanged += GridCar_OnGridRowVisualChanged;
        gridCar.OnGridColumnVisualChanged += GridCar_OnGridColumnVisualChanged;
        gridPassenger.OnGridColumnVisualChanged += GridPassenger_OnGridColumnVisualChanged;

        InitCarFile();
        InitPassengerFile();

        isProcessing = false;

        passengerColumns[0].transform.position = new Vector3(-2.7f, 0, 0);
        passengerColumns[1].transform.position = new Vector3(0.53f, 0, 0);
        passengerColumns[2].transform.position = new Vector3(3.85f, 0, 0);
    }
    //Hi HI
    public IEnumerator CheckDepartAndMove()
    {
        isProcessing = true;
        CarObject[,] carArray = gridCar.GetGridArray();
        PassengerObject[,] passengerArray = gridPassenger.GetGridArray();
        int passengerHeight = gridPassengerHeight - 1;
        int carHeight = gridCarHeight - 1;
        int carZ;
        int passengerZ;

        for (int i = 0; i < departIndexPassenger.Length; i++)
        {
            carArray = gridCar.GetGridArray();
            passengerArray = gridPassenger.GetGridArray();
            int currentCarColumnIndex = departIndexCar[i];
            int currentPassengerColumnIndex = departIndexPassenger[i];
            int countCarDepart = 0;
            carZ = carHeight - countCarDepart;
            int countPassengerDepart = 0;
            passengerZ = passengerHeight - countPassengerDepart;
            //if (passengerArray[i, passengerZ].GetPassenger() == null) continue;
            while (carArray[currentCarColumnIndex, carZ].GetColor() == passengerArray[currentPassengerColumnIndex, passengerZ].GetColor())
            {
                yield return StartCoroutine(PassengerDepart(passengerArray[currentPassengerColumnIndex, passengerZ], departPositions[i]));
                countPassengerDepart++;
                passengerZ = passengerHeight - countPassengerDepart;
                carArray[currentCarColumnIndex, carZ].SeatDec();
                if (carArray[currentCarColumnIndex, carZ].GetSeat() <= 0)
                {
                    countCarDepart++;
                    yield return StartCoroutine(CarDepart(carArray[currentCarColumnIndex, carZ], departPositions[i]));
                    break;
                }
            }
            if (countPassengerDepart > 0)
            {
                gridPassenger.ShiftColumnUp(currentPassengerColumnIndex, countPassengerDepart);
                for (int k = 0; k < countPassengerDepart; k++)
                {
                    PassengerSO nextPassengerSO = GetNextPassengerSO(currentPassengerColumnIndex);
                    if (nextPassengerSO != null)
                    {
                        Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(currentPassengerColumnIndex, gridPassengerHeight - 1 - k), nextPassengerSO);
                        passenger.name = $"({currentPassengerColumnIndex},{k})";
                        passengerArray[currentPassengerColumnIndex, k].SetPassenger(passenger);
                        passenger.transform.SetParent(passengerColumns[currentPassengerColumnIndex].transform, false);
                    }
                }
            }
            if (countCarDepart > 0)
            {
                gridCar.ShiftColumnUp(currentCarColumnIndex, countCarDepart);
                for (int k = 0; k < countCarDepart; k++)
                {
                    CarSO nextCarSO = GetNextCarSO(currentCarColumnIndex);
                    if (nextCarSO != null)
                    {
                        Car car = Car.Create(gridCar.GetWorldPosition(currentCarColumnIndex, k), nextCarSO);
                        car.name = $"({currentCarColumnIndex},{k})";
                        carArray[currentCarColumnIndex, k].SetCar(car);
                        car.transform.SetParent(rowParentGameObject[k].GetCenterGroup().transform, false);
                    }
                }
                i--;
                continue;
            }
        }

        gridCar.DebugPrintGridArray();
        gridPassenger.DebugPrintGridArray();
        isProcessing = false;
    }
    //public IEnumerator CheckDepartAndMove()
    //{
    //    isProcessing = true;
    //    CarObject[,] carArray = gridCar.GetGridArray();
    //    PassengerObject[,] passengerArray = gridPassenger.GetGridArray();
    //    bool changed = true;
    //    bool breakFor;
    //    int passengerHeight = gridPassengerHeight - 1;
    //    int carHeight = gridCarHeight - 1;
    //    int carZ;
    //    int passengerZ;

    //    while (changed)
    //    {
    //        carArray = gridCar.GetGridArray();
    //        passengerArray = gridPassenger.GetGridArray();
    //        changed = false;
    //        breakFor = false;
    //        for (int i = 0; i < departIndexCar.Length; i++)
    //        {
    //            int countCarDepart = 0;
    //            for (int j = 0; j < departIndexPassenger.Length; j++)
    //            {
    //                carZ = carHeight - countCarDepart;
    //                int currentCarColumnIndex = departIndexCar[i];
    //                int countPassengerDepart = 0;
    //                int currentPassengerColumnIndex = departIndexPassenger[j];
    //                passengerZ = passengerHeight - countPassengerDepart;
    //                if (passengerArray[currentPassengerColumnIndex, passengerZ].GetPassenger() == null) continue;
    //                if (carArray[currentCarColumnIndex, carZ].GetColor() == passengerArray[currentPassengerColumnIndex, passengerZ].GetColor())
    //                {
    //                    while (carArray[currentCarColumnIndex, carZ].GetColor() == passengerArray[currentPassengerColumnIndex, passengerZ].GetColor())
    //                    {
    //                        yield return StartCoroutine(PassengerDepart(passengerArray[currentPassengerColumnIndex, passengerZ]));
    //                        countPassengerDepart++;
    //                        passengerZ = passengerHeight - countPassengerDepart;
    //                        carArray[currentCarColumnIndex, carZ].SeatDec();
    //                        if (carArray[currentCarColumnIndex, carZ].GetSeat() <= 0)
    //                        {
    //                            countCarDepart++;
    //                            yield return StartCoroutine(CarDepart(carArray[currentCarColumnIndex, carZ]));
    //                            break;
    //                        }
    //                    }
    //                    if (countPassengerDepart > 0)
    //                    {
    //                        gridPassenger.ShiftColumnUp(currentPassengerColumnIndex, countPassengerDepart);
    //                        for (int k = 0; k < countPassengerDepart; k++)
    //                        {
    //                            PassengerSO nextPassengerSO = GetNextPassengerSO(currentPassengerColumnIndex);
    //                            if (nextPassengerSO != null)
    //                            {
    //                                Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(currentPassengerColumnIndex, gridPassengerHeight - 1 - k), nextPassengerSO);
    //                                passenger.name = $"({currentPassengerColumnIndex},{k})";
    //                                passengerArray[currentPassengerColumnIndex, k].SetPassenger(passenger);
    //                                passenger.transform.SetParent(passengerColumns[currentPassengerColumnIndex].transform, false);
    //                            }
    //                        }
    //                    }
    //                    if (countCarDepart > 0)
    //                    {
    //                        gridCar.ShiftColumnUp(currentCarColumnIndex, countCarDepart);
    //                        for (int k = 0; k < countCarDepart; k++)
    //                        {
    //                            CarSO nextCarSO = GetNextCarSO(currentCarColumnIndex);
    //                            if (nextCarSO != null)
    //                            {
    //                                Car car = Car.Create(gridCar.GetWorldPosition(currentCarColumnIndex, k), nextCarSO);
    //                                car.name = $"({currentCarColumnIndex},{k})";
    //                                carArray[currentCarColumnIndex, k].SetCar(car);
    //                                car.transform.SetParent(rowParentGameObject[k].GetCenterGroup().transform, false);
    //                            }
    //                        }
    //                    }
    //                    changed = true;
    //                    breakFor = true;
    //                }
    //                if (breakFor) break;
    //            }
    //            if (breakFor) break;
    //        }
    //    }

    //    gridCar.DebugPrintGridArray();
    //    gridPassenger.DebugPrintGridArray();
    //    isProcessing = false;
    //}


    private IEnumerator PassengerDepart(PassengerObject passengerObject,Vector3 departPos)
    {
        yield return StartCoroutine(passengerObject.GetPassenger().TestAnimation(departPos));
        yield return new WaitForSecondsRealtime(1);
        passengerObject.ClearObject();
        yield return null;
    }

    private IEnumerator CarDepart(CarObject carObject,Vector3 departPos)
    {
        yield return StartCoroutine(carObject.GetCar().TestAnimation(departPos));
        yield return new WaitForSecondsRealtime(1);
        carObject.ClearObject();
        yield return null;
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
    private void UpdatePassenger(int column)
    {
        for (int z = 0; z < gridPassengerHeight; z++)
        {
            int mirroredZ = gridPassengerHeight - 1 - z;
            PassengerObject passengerObject = gridPassenger.GetGridObject(column, z);
            if (passengerObject.GetPassenger() != null & passengerObject.GetPassenger().GetSO() != null)
            {
                Passenger passenger = Passenger.Create(gridPassenger.GetWorldPosition(column, mirroredZ), passengerObject.GetPassenger().GetSO());
                passenger.name = $"({column},{z})";
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
        UpdatePassenger(column);

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
    private void GridCar_OnGridRowVisualChanged(object sender, Grid<CarObject>.OnGridRowVisualChangedEventArgs e)
    {
        if (e.row < 0 || e.row >= gridCarHeight)
        {
            Debug.Log("out of index row shift");
            return;
        }
        UpdateVisualRowAfterShift(e.row);
    }
    private void GridCar_OnGridColumnVisualChanged(object sender, Grid<CarObject>.OnGridColumnVisualChangedEventArgs e) //co the refactor
    {
        if (e.column < 0 || e.column >= gridCarWidth)
        {
            Debug.Log("out of index column shift");
            return;
        }
        for (int i = 0; i < gridCarHeight; i++)
        {
            UpdateVisualRowAfterShift(i);
        }
        //Co the dung cai o duoi nhung ko dong bo cho side visual
        //UpdateVisualColumnAfterShift(e.column);
    }
    // code ngu nhung thanh tinh nang ?
    private void UpdateVisualRowAfterShift(int row)
    {

        ClearRowCarVisual(row);
        UpdateCarRow(row);
    }
    //private void UpdateVisualColumnAfterShift(int column)
    //{
    //    ClearColumnCarVisual(column);
    //    UpdateCarColumn(column);
    //}
    //private void ClearColumnCarVisual(int column)
    //{
    //    for(int z= 0; z < gridCarHeight; z++)
    //    {
    //        CarObject carObject = gridCar.GetGridObject(column, z);
    //        if (carObject.GetCar() != null)
    //        {
    //            carObject.ClearObject();
    //        }
    //    }
    //}
    //private void UpdateCarColumn(int column)
    //{
    //    for(int z = 0; z < gridCarHeight; z++)
    //    {
    //        CarObject carObject = gridCar.GetGridObject(column, z);
    //        if (carObject.GetCar() != null & carObject.GetCar().GetSO() != null & carObject.GetSeat() > 0)
    //        {
    //            Car spawnedCar = Car.Create(gridCar.GetWorldPosition(column, z), carObject.GetCar().GetSO());
    //            spawnedCar.name = $"({column},{z})";
    //            int tempSlotSeat = carObject.GetSeat();
    //            carObject.SetCar(spawnedCar, tempSlotSeat);
    //            spawnedCar.transform.SetParent(rowParentGameObject[z].GetCenterGroup(), false);
    //        }
    //    }
    //}
    private void UpdateCarRow(int row)
    {
        for (int x = 0; x < gridCarWidth; x++)
        {
            CarObject carObject = gridCar.GetGridObject(x, row);
            if (carObject.GetCar() != null & carObject.GetCar().GetSO() != null & carObject.GetSeat() > 0)
            {
                Car spawnedCar = Car.Create(gridCar.GetWorldPosition(x, row), carObject.GetCar().GetSO());
                spawnedCar.name = $"({x},{row})";
                int tempSlotSeat = carObject.GetSeat();
                carObject.SetCar(spawnedCar, tempSlotSeat);
                spawnedCar.transform.SetParent(rowParentGameObject[row].GetCenterGroup(), false);
            }
        }
        OnCenterGroupChange?.Invoke(row);
    }
    private void ClearRowCarVisual(int row)
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
    // GetSet
    public RowVisualGroup GetRowVisualGroup(int row)
    {
        return rowParentGameObject[row];
    }

    public Grid<CarObject> GetCarGrid()
    {
        return this.gridCar;
    }
}
