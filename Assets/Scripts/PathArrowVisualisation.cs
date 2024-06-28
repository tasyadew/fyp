using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PathArrowVisualisation : MonoBehaviour
{

    [SerializeField]
    private NavigationController navigationController;
    [SerializeField]
    private GameObject arrow;
    [SerializeField]
    private Slider navigationYOffset;
    [SerializeField]
    private float moveOnDistance;

    [SerializeField]
    private TargetHandler targetHandler;
    private float offsetValue = -1;

    private NavMeshPath path;
    private float currentDistance;
    private Vector3[] pathOffset;
    private Vector3 nextNavigationPoint = Vector3.zero;

    private void Start()
    {
        // Find object IndoorNavigation and get component targetHandler
        targetHandler = GameObject.Find("IndoorNavigation").GetComponent<TargetHandler>();
    }

    private void Update()
    {
        path = navigationController.CalculatedPath;

        AddOffsetToPath();
        SelectNextNavigationPoint();
        //AddArrowOffset();

        // If the targetHandler has a target and the LineVis mode is arrow
        if (targetHandler.currClosestIndex != -1 && SwitchPathVisualisation.visualisationCounter == 1)
        {
            arrow.SetActive(true);
            arrow.transform.LookAt(nextNavigationPoint);
        }
        else
        {
            arrow.SetActive(false);
        }
    }

    private void AddOffsetToPath()
    {
        pathOffset = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++)
        {
            pathOffset[i] = new Vector3(path.corners[i].x, transform.position.y, path.corners[i].z);
        }
    }

    private void SelectNextNavigationPoint()
    {
        //nextNavigationPoint = SelectNextNavigationPointWithinDistance();
        nextNavigationPoint = targetHandler.GetNextTargetPosition();
    }

    private Vector3 SelectNextNavigationPointWithinDistance()
    {
        for (int i = 0; i < pathOffset.Length; i++)
        {
            currentDistance = Vector3.Distance(transform.position, pathOffset[i]);
            if (currentDistance > moveOnDistance)
            {
                return pathOffset[i];
            }
        }
        return navigationController.TargetPosition;
    }

    private void AddArrowOffset()
    {
        // if (navigationYOffset.value != 0) {
        arrow.transform.position = new Vector3(arrow.transform.position.x, offsetValue, arrow.transform.position.z);
        // }
    }
}
