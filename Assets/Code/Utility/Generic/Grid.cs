using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T1, T2> where T1 : class where T2 : class
{
    public Grid(Vector3Int size)
    {
        SetSize(size);
    }

    public int Length => Array.Length;
    public int Count => GetAllTypeOneValues().Length + GetAllTypeTwoValues().Length;

    public object[,,] Array { get; private set; }

    public void Clear() => SetAllValues(null);

    public void SetSize(Vector3Int size)
    {
        Array = new object[size.x, size.y, size.z];
    }

    public void SetAllValues(T2 value)
    {
        for (int x = 0; x < Array.GetLength(0); x++)
            for (int y = 0; y < Array.GetLength(1); y++)
                for (int z = 0; z < Array.GetLength(2); z++)
                    Array[x, y, z] = value;
    }

    public void SetValue(T1 value, int x, int y, int z) => Array[x, y, z] = value;
    public void SetValue(T2 value, int x, int y, int z) => Array[x, y, z] = value;

    public object GetValue(int x, int y, int z) => Array[x, y, z];

    public GridObject<object>[] GetAllValues()
    {
        List<GridObject<object>> allValues = new List<GridObject<object>>();

        for (int x = 0; x < Array.GetLength(0); x++)
            for (int y = 0; y < Array.GetLength(1); y++)
                for (int z = 0; z < Array.GetLength(2); z++)
                    if (Array[x, y, z] != null)
                        allValues.Add(new GridObject<object>(Array[x, y, z], x, y, z));

        return allValues.ToArray();
    }
    public GridObject<T1>[] GetAllTypeOneValues()
    {
        List<GridObject<T1>> allValues = new List<GridObject<T1>>();

        for (int x = 0; x < Array.GetLength(0); x++)
            for (int y = 0; y < Array.GetLength(1); y++)
                for (int z = 0; z < Array.GetLength(2); z++)
                    if (Array[x, y, z] != null && Array[x, y, z] is T1)
                        allValues.Add(new GridObject<T1>(Array[x, y, z] as T1, x, y, z));

        return allValues.ToArray();
    }
    public GridObject<T2>[] GetAllTypeTwoValues()
    {
        List<GridObject<T2>> allValues = new List<GridObject<T2>>();

        for (int x = 0; x < Array.GetLength(0); x++)
            for (int y = 0; y < Array.GetLength(1); y++)
                for (int z = 0; z < Array.GetLength(2); z++)
                    if (Array[x, y, z] != null && Array[x, y, z] is T2)
                        allValues.Add(new GridObject<T2>(Array[x, y, z] as T2, x, y, z));

        return allValues.ToArray();
    }
}
public struct GridObject<T>
{
    public GridObject(T value, int x, int y, int z)
    {
        Object = value;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public T Object;
    public int x, y, z;
}