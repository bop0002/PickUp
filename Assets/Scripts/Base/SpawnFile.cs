//using System;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;

// KO BIET NEN DUNG STATIC CLASS NHU NAY HAY DUNG SINGLETON PATTERN

//public static class SpawnFile
//{
//    [SerializeField] private static List<CarSO> carSOList;
//    private static Dictionary<ECarType, CarSO> carSODict;
//    [SerializeField] private static List<PassengerSO> passengerSOList;
//    private static Dictionary<EColor, PassengerSO> passengerSODict;
//    private static string folderSpawnPath;
//    private static string fileSpawnPath;
//    private static string json;
//    private static SpawnData spawnData;
//    private static void InitDict()
//    {
//        carSODict = new Dictionary<ECarType, CarSO>();
//        foreach (var so in carSOList)
//        {
//            carSODict[so.GetCarType()] = so;
//        }
//        passengerSODict = new Dictionary<EColor, PassengerSO>();
//        foreach (var so in passengerSOList)
//        {
//            passengerSODict[so.GetColor()] = so;
//        }
//    }
//    static SpawnFile()
//    {
//        folderSpawnPath = Path.Combine(Application.streamingAssetsPath, "LevelPattern");
//        fileSpawnPath = Path.Combine(folderSpawnPath, "level1.json");
//        json = File.ReadAllText(fileSpawnPath);
//        spawnData = JsonUtility.FromJson<SpawnData>(json);
//        InitDict();
//    }
//    public static CarSO GetNextCarSO(int column)
//    {
//        SpawnDataColumn columnData = spawnData.carColumns[x];
//        if (columnData.indexCount >= columnData.pattern.Count)
//        {
//            Debug.Log("out of index in json car");
//            return null;
//        }
//        string enumString = columnData.pattern[spawnData.carColumns[x].indexCount++];
//        if (Enum.TryParse(enumString, out ECarType carType))
//        {
//            if (carSODict.TryGetValue(carType, out CarSO carSO))
//            {
//                return carSO;
//            }
//            else
//            {
//                Debug.Log("Cant find carSO");
//            }
//        }
//        else
//        {
//            Debug.Log("Cant parse enum");
//        }
//        return null;
//    }
//    private static PassengerSO GetNextPassengerSO(int x)
//    {
//        SpawnDataColumn columnData = spawnData.passengerColumns[x];
//        if (columnData.indexCount >= columnData.pattern.Count)
//        {
//            Debug.Log("out of index in json passenger");
//            return null;
//        }
//        string enumString = columnData.pattern[spawnData.passengerColumns[x].indexCount++];
//        if (Enum.TryParse(enumString, out EColor passengerColor))
//        {
//            if (passengerSODict.TryGetValue(passengerColor, out PassengerSO passengerSO))
//            {
//                return passengerSO;
//            }
//            else
//            {
//                Debug.Log("Cant find carSO");
//            }
//        }
//        else
//        {
//            Debug.Log("Cant parse enum");
//        }
//        return null;
//    }
//}