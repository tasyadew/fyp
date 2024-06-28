using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TargetHandler : MonoBehaviour
{
    [SerializeField] private QrCodeRecenter qrCodeRecenter;
    [SerializeField] private NavigationController navigationController;
    [SerializeField] private TextAsset targetModelData;
    // [SerializeField] private TMP_Dropdown targetDataDropdown;

    [SerializeField] private GameObject targetObjectPrefab;
    [SerializeField] private Transform[] targetObjectsParentTransforms;

    public static List<TargetFacade> currentTargetItems = new List<TargetFacade>();
    private int currPosIndex = 0;
    public int currClosestIndex = 0;

    [SerializeField]
    public TMP_Text debugText;

    [SerializeField] List<ProductObject> allProducts;
    [SerializeField] List<NavigationObject> allTargets;

    public static string myText = "";

    private void Start()
    {
        allProducts = ProductsData.list;
        allTargets = new List<NavigationObject>();
        foreach (ProductObject product in allProducts)
        {
            bool categoryExists = false;
            foreach (NavigationObject target in allTargets)
            {
                if (target.category == product.category)
                {
                    target.products.Add(product.name);
                    categoryExists = true;
                    break;
                }
            }

            if (!categoryExists)
            {
                NavigationObject newTarget = new NavigationObject();
                newTarget.category = product.category;
                newTarget.products = new List<string>();
                newTarget.products.Add(product.name);
                allTargets.Add(newTarget);
            }
        }

        GenerateTargetItems();
        // FillDropdownWithTargetItems();
        currClosestIndex = FindClosestTargetIndex();

        if (currClosestIndex == -1)
        {
            myText = "All targets have been visited.";
        }
        else
        {
            string listOfProducts = allTargets.Find(x => x.category.Equals(currentTargetItems[currClosestIndex].Name)).products.Aggregate((i, j) => i + ", " + j); ;
            myText = "Destination:\n" + currentTargetItems[currClosestIndex].Name + "\n\nList of products:\n" + listOfProducts;
        }
    }

    public void setActivePathfinding()
    {
        SetSelectedTargetPositionWithDropdown(currClosestIndex);
        qrCodeRecenter.SetQrCodeRecenterTarget(currentTargetItems[currPosIndex].Name, false);
    }

    // OnNextButtonClicked, change the current target to passed, make it the currPosIndex and find the next closest target
    public void OnNextButtonClicked()
    {
        if (currClosestIndex == -1) return;
        if (!qrCodeRecenter.hasScanned) return;

        NavigationObject tempNavObj = allTargets[currClosestIndex - 1];
        tempNavObj.passed = true;
        allTargets[currClosestIndex - 1] = tempNavObj;
        currPosIndex = currClosestIndex;
        currClosestIndex = FindClosestTargetIndex();

        if (currClosestIndex == -1)
        {
            myText = "All targets have been visited.";
        }
        else
        {
            string listOfProducts = allTargets.Find(x => x.category.Equals(currentTargetItems[currClosestIndex].Name)).products.Aggregate((i, j) => i + ", " + j); ;
            myText = "Destination:\n" + currentTargetItems[currClosestIndex].Name + "\n\nList of products:\n" + listOfProducts;

            setActivePathfinding();
        }
    }

    public int FindClosestTargetIndex()
    {
        if (currentTargetItems.Count == 0)
        {
            Debug.LogWarning("No targets available.");
            return -1;
        }

        int closestIndex = -1;
        float closestDistance = 1000000; // Use a large number as the initial value

        for (int i = 1; i < currentTargetItems.Count; i++)
        {
            if (i == currPosIndex) continue; // Skip the current position
            if (allTargets[i - 1].passed == true) continue; // Skip this target if it has been passed

            float distance = Vector3.Distance(currentTargetItems[currPosIndex].transform.position, currentTargetItems[i].transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    public void onSwapStartPointButtonClicked(string targetName)
    {
        currentTargetItems[0] = CreateTargetFacade(GenerateTargetDataFromSource().FirstOrDefault(x => x.Name == targetName));
        debugText.text = System.DateTime.Now.ToString("HH:mm:ss") + " => " + targetName + "\n";
        // add all the currentTargetItem name for debugging
        int i = 0;
        foreach (TargetFacade target in currentTargetItems)
        {
            debugText.text += "[" + (i++) + "]:" + target.Name + ", ";
        }
    }

    private void GenerateTargetItems()
    {
        IEnumerable<Target> targets = GenerateTargetDataFromSource();
        foreach (Target target in targets)
        {
            if (target.Name == "StartPoint1")
            {
                currentTargetItems.Add(CreateTargetFacade(target));
                continue;
            }

            // Only add items that exists in the category of allTargets
            foreach (NavigationObject navObj in allTargets)
            {
                if (navObj.category.Equals(target.Name))
                {
                    currentTargetItems.Add(CreateTargetFacade(target));
                    break;
                }
            }
        }
    }

    public IEnumerable<Target> GenerateTargetDataFromSource()
    {
        return JsonUtility.FromJson<TargetWrapper>(targetModelData.text).TargetList;
    }

    public TargetFacade CreateTargetFacade(Target target)
    {
        GameObject targetObject = Instantiate(targetObjectPrefab, targetObjectsParentTransforms[0], false);
        targetObject.SetActive(true);
        targetObject.name = $"{target.Name}";
        targetObject.transform.localPosition = target.Position;
        targetObject.transform.localRotation = Quaternion.Euler(target.Rotation);

        TargetFacade targetData = targetObject.GetComponent<TargetFacade>();
        targetData.Name = target.Name;

        return targetData;
    }

    // private void FillDropdownWithTargetItems()
    // {
    //     List<TMP_Dropdown.OptionData> targetFacadeOptionData =
    //         currentTargetItems.Select(x => new TMP_Dropdown.OptionData
    //         {
    //             text = $"{x.Name}"
    //         }).ToList();

    //     targetDataDropdown.ClearOptions();
    //     targetDataDropdown.AddOptions(targetFacadeOptionData);
    // }

    public void SetSelectedTargetPositionWithDropdown(int selectedValue)
    {
        navigationController.TargetPosition = GetCurrentlySelectedTarget(selectedValue);
    }

    private Vector3 GetCurrentlySelectedTarget(int selectedValue)
    {
        if (selectedValue < 0 || selectedValue >= currentTargetItems.Count)
        {
            return Vector3.zero;
        }

        return currentTargetItems[selectedValue].transform.position;
    }

    public Vector3 GetNextTargetPosition()
    {
        return GetCurrentlySelectedTarget(currClosestIndex);
    }

    public TargetFacade GetCurrentTargetByTargetText(string targetText)
    {
        return currentTargetItems.Find(x =>
            x.Name.ToLower().Equals(targetText.ToLower()));
    }
}
