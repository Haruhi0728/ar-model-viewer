using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(AutoPlaceOnPlane))]
public class PinchToScaleController : MonoBehaviour
{
    [SerializeField] float minScale = 0.3f;
    [SerializeField] float maxScale = 3f;

    AutoPlaceOnPlane placer;
    float initialDistance;
    float initialScaleFactor;
    bool pinching;

    void Awake()
    {
        placer = GetComponent<AutoPlaceOnPlane>();
    }

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        var target = placer.PlacedInstance;
        var touches = Touch.activeTouches;

        if (target == null || touches.Count != 2)
        {
            pinching = false;
            return;
        }

        if (IsOverUI(touches[0].touchId) || IsOverUI(touches[1].touchId))
        {
            pinching = false;
            return;
        }

        var distance = Vector2.Distance(touches[0].screenPosition, touches[1].screenPosition);

        if (!pinching)
        {
            pinching = true;
            initialDistance = distance;
            initialScaleFactor = target.transform.localScale.x;
            return;
        }

        if (initialDistance < 0.01f)
        {
            return;
        }

        var ratio = distance / initialDistance;
        var newFactor = Mathf.Clamp(initialScaleFactor * ratio, minScale, maxScale);
        target.transform.localScale = Vector3.one * newFactor;
    }

    static bool IsOverUI(int touchId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touchId);
    }
}
