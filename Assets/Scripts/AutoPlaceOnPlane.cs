using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class AutoPlaceOnPlane : MonoBehaviour
{
    [SerializeField] GameObject modelPrefab;
    [SerializeField] bool hidePlanesAfterPlacement = true;

    ARPlaneManager planeManager;
    bool placed;

    void Awake()
    {
        planeManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlanesChanged;
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (placed || modelPrefab == null)
        {
            return;
        }

        foreach (var plane in args.added)
        {
            if (plane.alignment != PlaneAlignment.HorizontalUp)
            {
                continue;
            }

            PlaceModel(plane);
            break;
        }
    }

    void PlaceModel(ARPlane plane)
    {
        placed = true;
        Instantiate(modelPrefab, plane.center, Quaternion.identity);

        if (!hidePlanesAfterPlacement)
        {
            return;
        }

        foreach (var trackedPlane in planeManager.trackables)
        {
            trackedPlane.gameObject.SetActive(false);
        }
        planeManager.enabled = false;
    }
}
