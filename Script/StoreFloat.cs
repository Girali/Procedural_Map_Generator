using UnityEngine;
using UnityEditor;
using System.IO;


public class StoreFloat{

    static string WriteFloatToFile(float[,] array)
    {
        string toWrite = "";
        int length = array.GetLength(0);
        for(int y = 0; y < length; y++)
        {
            for (int x = 0; x < length; x++)
            {
                toWrite = toWrite + array[x,y] + " ";
            }
            toWrite = toWrite + "-";
        }
        return toWrite;
    }

    [MenuItem("Tools/Write file")]
    static void WriteString(string text)
    {
        string path = "Assets/Resources/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
    }

    [MenuItem("Tools/Read file")]
    static string ReadString()
    {
        string path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        reader.Close();
        return reader.ReadToEnd();
    }

    static float[,] WriteFileToFloat(string text,int lenght)
    {
        float[,] array = new float[lenght, lenght];
        string floatS = "";
        int x = 0;
        int y = 0;

        for(int i = 0;i < text.Length; i++)
        {
            do
            {
                floatS = floatS + text[i];
                i++;
            } while (text[i].ToString() != " " && text[i].ToString() != "-");
            float convert = float.Parse(floatS);
            array[x, y] = convert;
            i++;
            x++;
            if (text[i].ToString() != "-")
            {
                i++;
                y++;
            }
        }

        return array;
    }
}
