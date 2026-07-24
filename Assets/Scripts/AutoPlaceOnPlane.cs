using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class AutoPlaceOnPlane : MonoBehaviour
{
    [SerializeField] GameObject modelPrefab;
    [SerializeField] bool hidePlanesAfterPlacement = true;

    ARPlaneManager planeManager;
    GameObject placedInstance;
    bool placed;

    void Awake()
    {
        planeManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        planeManager.planesChanged += OnPlanesChanged;
        ARSession.stateChanged += OnSessionStateChanged;
        Debug.Log($"[ARDEBUG] OnEnable. subsystem={planeManager.subsystem}, running={planeManager.subsystem?.running}, descriptorSupportsHorizontal={planeManager.descriptor?.supportsHorizontalPlaneDetection}, sessionState={ARSession.state}");
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlanesChanged;
        ARSession.stateChanged -= OnSessionStateChanged;
    }

    void OnSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        Debug.Log($"[ARDEBUG] Session state changed: {args.state}");
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        Debug.Log($"[ARDEBUG] OnPlanesChanged added={args.added.Count} updated={args.updated.Count} removed={args.removed.Count}");
        foreach (var plane in args.added)
        {
            Debug.Log($"[ARDEBUG] Added plane id={plane.trackableId} alignment={plane.alignment} center={plane.center}");
        }

        if (placed)
        {
            if (hidePlanesAfterPlacement)
            {
                foreach (var plane in args.added)
                {
                    plane.gameObject.SetActive(false);
                }
            }
            return;
        }

        if (modelPrefab == null)
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
        placedInstance = Instantiate(modelPrefab, plane.center, Quaternion.identity);

        if (!hidePlanesAfterPlacement)
        {
            return;
        }

        foreach (var trackedPlane in planeManager.trackables)
        {
            trackedPlane.gameObject.SetActive(false);
        }
    }

    public void ResetPlacement()
    {
        if (placedInstance != null)
        {
            Destroy(placedInstance);
            placedInstance = null;
        }

        placed = false;

        foreach (var trackedPlane in planeManager.trackables)
        {
            trackedPlane.gameObject.SetActive(true);
        }

        foreach (var trackedPlane in planeManager.trackables)
        {
            if (trackedPlane.alignment == PlaneAlignment.HorizontalUp)
            {
                PlaceModel(trackedPlane);
                return;
            }
        }
    }
}
