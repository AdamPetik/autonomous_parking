using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Component just for collecting components of particular type T.
/// </summary>
public class Collector<T> : MonoBehaviour where T : Component
{
    private List<T> items;

    public List<T> Items 
    {
        get {
            if (items == null) Collect();
            return new List<T>(items);
        }
    }
    
    private void Collect() 
    {
        items = new List<T>(gameObject.GetComponentsInChildren<T>());
    }
}


