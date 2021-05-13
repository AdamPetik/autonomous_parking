using System.Collections.Generic;
using UnityEngine;

public static class Extends {
    public static T[] FindComponentsInChildrenWithTag<T>(this GameObject parent, string tag, bool forceActive = false) where T : Component
    {
        if(parent == null) { throw new System.ArgumentNullException(); }
        if(string.IsNullOrEmpty(tag) == true) { throw new System.ArgumentNullException(); }
        List<T> list = new List<T>(parent.GetComponentsInChildren<T>(forceActive));
        if(list.Count == 0) { return null; }

        for(int i = list.Count - 1; i >= 0; i--) 
        {
            if (list[i].CompareTag(tag) == false)
            {
                list.RemoveAt(i);
            }
        }
        return list.ToArray();
    }

    public static T SelectRandom<T>(this List<T> obj) where T : Object
    {   
        if(obj == null) { throw new System.ArgumentNullException(); }
        if(obj.Count == 0) { return null; }

        int index = Random.Range(0, obj.Count);
        return obj[index];
    }

    public static List<U> Map<T, U>(this List<T> list, System.Func<T, U> mapFunction) where T : Object where U : Object
    {   
        if(list == null) { throw new System.ArgumentNullException(); }
        if(mapFunction == null) { throw new System.ArgumentNullException(); }
        if(list.Count == 0) { return new List<U>(); }
        
        List<U> mapped = new List<U>();

        list.ForEach(delegate (T item)
        {
            mapped.Add(mapFunction(item));
        });

        return mapped;
    }

    public static List<T> SelectRandomItemsWithoutReplacement<T>(this List<T> obj, int itemsCount = -1) where T : Object
    {   
        if(obj == null) { throw new System.ArgumentNullException(); }
        if(itemsCount > obj.Count) { throw new System.ArgumentException("Argument itemsCount is greater than obj.Count"); }
        if(obj.Count == 0) { return new List<T>(); }
        
        List<T> copy = new List<T>(obj);
        List<T> selected = new List<T>();

        if (itemsCount == -1)
        {
            itemsCount = Random.Range(0, obj.Count + 1);
        }

        for(int i = 0; i < itemsCount; i++)
        {
            int removeAt = Random.Range(0, copy.Count);
            selected.Add(copy[removeAt]);
            copy.RemoveAt(removeAt);
        }

        return selected;
    }
 }