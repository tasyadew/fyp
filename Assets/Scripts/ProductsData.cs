using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ProductObject
{
    public string name;
    public string category;
}

[Serializable]
public struct NavigationObject
{
    public string category;
    public bool passed;
    public List<string> products;    
}

public class ProductsData : MonoBehaviour
{
    public static List<ProductObject> list;
}