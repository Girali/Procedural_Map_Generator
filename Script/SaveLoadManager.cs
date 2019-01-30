using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

static public class SaveLoadManager {
    public static void Save(float[,] map,string fileName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/"+ fileName+".sav", FileMode.Create);

        bf.Serialize(stream, map);
        stream.Close();
    }

    public static float[,] Load(string fileName)
    {
        if (File.Exists(Application.persistentDataPath + "/" + fileName + ".sav"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(Application.persistentDataPath + "/" + fileName + ".sav", FileMode.Open);

            float[,] res = bf.Deserialize(stream) as float[,];
            stream.Close();
            return res;
        }
        else
        {
            Debug.Log("Impossible de load");
            return null;
        }
    }
}