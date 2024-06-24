using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NavigationManager : MonoBehaviour
{
    [SerializeField] List<ProductObject> allProducts;
    //[SerializeField] public static List<NavigationObject> allTargets;
    public TextMeshProUGUI myText;

    void Update()
    {
        myText.text = TargetHandler.myText;
    }

    // void Start()
    // {
    //     // Loop through all products and group them by category
    //     allProducts = ProductsData.list;
    //     List<NavigationObject> tempTargets = new List<NavigationObject>();
    //     foreach (ProductObject product in allProducts)
    //     {
    //         bool categoryExists = false;
    //         foreach (NavigationObject target in tempTargets)
    //         {
    //             if (target.category == product.category)
    //             {
    //                 target.products.Add(product.name);
    //                 categoryExists = true;
    //                 break;
    //             }
    //         }

    //         if (!categoryExists)
    //         {
    //             NavigationObject newTarget = new NavigationObject();
    //             newTarget.category = product.category;
    //             newTarget.products = new List<string>();
    //             newTarget.products.Add(product.name);
    //             tempTargets.Add(newTarget);
    //         }
    //     }
    //     allTargets = tempTargets;

    //     // Display all targets in the UI
    //     string tempString = "";
    //     foreach (NavigationObject target in allTargets)
    //     {
    //         tempString += "Category: " + target.category + " | Products: [";
    //         foreach (string product_name in target.products)
    //         {
    //             tempString += product_name + ", ";
    //         }
    //         tempString += "]\n";
    //     }
    //     myText.text = tempString;
    // }
}