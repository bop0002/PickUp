//using System;
//using System.IO;
//using UnityEngine;

//public static class SpawnCarFile 
//{
//    private static CarSpawnData carSpawnData;
//    public static Car SpawnCar()
//    {
//        string folderPath = Path.Combine(Application.streamingAssetsPath, "LevelPattern");
//        string filePath = Path.Combine(folderPath, "level1.json");

//        string json = File.ReadAllText(filePath);
//        carSpawnData = JsonUtility.FromJson<CarSpawnData>(json);

//        string enumString = carSpawnData.columns[x].pattern[carSpawnData.columns[x].indexCount++];
//        if (Enum.TryParse(enumString, out ECarType carType))
//        {
//            if (carSODict.TryGetValue(carType, out CarSO carSO))
//            {
//                Debug.Log($"JSON content:\n{carSpawnData.columns[x].columnIndex}");
//                Car spawnnedCar = Car.Create(gridCar.GetWorldPosition(x, z), carSO);
//                spawnnedCar.transform.SetParent(centerVisualParent.transform, false);
//                CarObject gridObject = gridCar.GetGridObject(x, z);
//                gridObject.SetCar(spawnnedCar);
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
//    }
//}
