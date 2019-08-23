using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public struct MapCellsParamsRepository_ParamStruct
{
    public string ParamName;
    public MapCell_SerializableParams CellParams;
}

[System.Serializable]
public struct MapCellsParamsRepository_DataStruct
{
    [SerializeField]
    public List<MapCellsParamsRepository_ParamStruct> Data;

    public MapCellsParamsRepository_DataStruct(List<MapCellsParamsRepository_ParamStruct> data)
    {
        Data = data;
    }

    public void Apply(MapCellsParamsRepository_DataStruct source)
    {
        Data.Clear();
        Data.AddRange(source.Data);
    }

    public void LoadFromStream(Stream stream)
    {
        BinaryFormatter fm = new BinaryFormatter();;
        fm.Serialize(stream, this);
    }

    public void SaveToStream(Stream stream)
    {
        BinaryFormatter fm = new BinaryFormatter();
        MapCellsParamsRepository_DataStruct ds = (MapCellsParamsRepository_DataStruct)fm.Deserialize(stream);
        Apply(ds);
    }

    public void LoadFromFile(string file_name)
    {
        Stream fs = File.Open(file_name, FileMode.Open);

        try
        {
            if (fs.CanRead)
            {
                LoadFromStream(fs);
            }
        }
        finally
        {
            fs.Close();
        }
    }

    public void SaveToFile(string file_name)
    {
        Stream fs = File.Open(file_name, FileMode.CreateNew);

        try
        {
            if (fs.CanWrite)
            {
                SaveToStream(fs);
            }
        }
        finally
        {
            fs.Close();
        }
    }
}

public class MapCellsParamsRepository : MonoBehaviour
{
    public List<MapCellsParamsRepository_ParamStruct> Default_Params = new List<MapCellsParamsRepository_ParamStruct>();

    protected MapCellsParamsRepository_DataStruct fData = new MapCellsParamsRepository_DataStruct(new List<MapCellsParamsRepository_ParamStruct>());


    public void AddParam(string param_name, MapCell_SerializableParams _params)
    {
        MapCellsParamsRepository_ParamStruct _struct = new MapCellsParamsRepository_ParamStruct();
        _struct.ParamName = param_name;
        _struct.CellParams = _params;
    }

    public void RemoveParam(int index)
    {
        if ((index < 0) ||
            (index >= fData.Data.Count))
            return;

        fData.Data.RemoveAt(index);
    }

    public void RemoveParam(string name)
    {
        RemoveParam(ParamIndex(name));
    }

    public int ParamIndex(string name)
    {
        for (int i=0; i< fData.Data.Count; i++)
        {
            if (fData.Data[i].ParamName.Equals(name))
                return i;
        }

        return -1;
    }


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
