using UnityEngine;

public class SensorAvatarController : MonoBehaviour
{
    [SerializeField] float maxMoveSpeed = 0.3f;
    [SerializeField] float maxTurnSpeed = 90f;
    [SerializeField] float walkThreshold = 5f;
    [SerializeField] float smoothing = 8f;

    [Header("Sensor Calibration")]
    [Tooltip("指をまっすぐ伸ばした時のセンサー値")]
    [SerializeField] float restValue = 45f;
    [Tooltip("指をしっかり曲げた時のセンサー値")]
    [SerializeField] float bentValue = 25f;

    [Tooltip("待機中のAnimator再生速度。専用Idleモーションが無いモデルは0にして一時停止させる")]
    [SerializeField] float idleAnimatorSpeed = 1f;

    Animator animator;
    float smoothedLeft;
    float smoothedRight;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        var client = BendSensorClient.Instance;
        int left = 0;
        int right = 0;
        if (client != null)
        {
            left = client.Left;
            right = client.Right;
        }

        smoothedLeft = Mathf.Lerp(smoothedLeft, left, Time.deltaTime * smoothing);
        smoothedRight = Mathf.Lerp(smoothedRight, right, Time.deltaTime * smoothing);

        float calibratedLeft = Calibrate(smoothedLeft);
        float calibratedRight = Calibrate(smoothedRight);

        float throttle = (calibratedLeft + calibratedRight) / 2f;
        float turn = (calibratedRight - calibratedLeft) / 2f;

        float moveSpeed = (throttle / 100f) * maxMoveSpeed;
        float turnSpeed = (turn / 100f) * maxTurnSpeed;

        transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime, Space.World);
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);

        bool isWalking = throttle > walkThreshold;
        if (animator != null)
        {
            animator.SetBool("IsWalking", isWalking);
            animator.speed = isWalking ? Mathf.Lerp(0.8f, 1.5f, throttle / 100f) : idleAnimatorSpeed;
        }
    }

    float Calibrate(float raw)
    {
        return Mathf.InverseLerp(restValue, bentValue, raw) * 100f;
    }
}
