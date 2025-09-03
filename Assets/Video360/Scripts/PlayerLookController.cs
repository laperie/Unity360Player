using UnityEngine;

public class PlayerLookController : MonoBehaviour
{
    [Header("Look Settings")]
    [Tooltip("Mouse sensitivity")]
    [Range(0.1f, 10f)]
    public float mouseSensitivity = 3f;

    [Tooltip("Touch sensitivity")]
    [Range(0.1f, 10f)]
    public float touchSensitivity = 2f;

    [Tooltip("Gyroscope sensitivity")]
    [Range(0.1f, 5f)]
    public float gyroSensitivity = 1f;

    [Header("Look Constraints")]
    [Tooltip("Minimum pitch angle (looking down)")]
    [Range(-90f, 0f)]
    public float minPitch = -85f;

    [Tooltip("Maximum pitch angle (looking up)")]
    [Range(0f, 90f)]
    public float maxPitch = 85f;

    [Header("Input Options")]
    [Tooltip("Enable mouse look")]
    public bool enableMouseLook = true;

    [Tooltip("Enable touch look")]
    public bool enableTouchLook = true;

    [Tooltip("Enable gyroscope look")]
    public bool enableGyroscope = false;

    [Header("Smoothing")]
    [Tooltip("Smooth rotation transitions")]
    public bool enableSmoothing = true;

    [Tooltip("Rotation smoothing factor")]
    [Range(1f, 20f)]
    public float smoothingFactor = 5f;

    private float yaw = 0f;
    private float pitch = 0f;
    private Vector3 targetRotation;
    private Quaternion baselineRotation = Quaternion.identity;

    private bool isGyroInitialized = false;
    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        InitializeGyroscope();
    }

    void InitializeGyroscope()
    {
        if (enableGyroscope && SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            baselineRotation = Input.gyro.attitude;
            isGyroInitialized = true;
            Debug.Log("[PlayerLookController] Gyroscope initialized");
        }
    }

    public void CalibrateGyroscope()
    {
        if (isGyroInitialized)
        {
            baselineRotation = Input.gyro.attitude;
            Debug.Log("[PlayerLookController] Gyroscope recalibrated");
        }
    }

    void Update()
    {
        HandleInput();
        ApplyRotation();
    }

    void HandleInput()
    {
        Vector2 lookInput = Vector2.zero;

        if (enableMouseLook)
        {
            lookInput += HandleMouseInput();
        }

        if (enableTouchLook)
        {
            lookInput += HandleTouchInput();
        }

        if (enableGyroscope && isGyroInitialized)
        {
            HandleGyroscopeInput();
            return;
        }

        yaw += lookInput.x;
        pitch -= lookInput.y;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        targetRotation = new Vector3(pitch, yaw, 0f);
    }

    Vector2 HandleMouseInput()
    {
        Vector2 mouseInput = Vector2.zero;

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            mouseInput.x = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseInput.y = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        return mouseInput;
    }

    Vector2 HandleTouchInput()
    {
        Vector2 touchInput = Vector2.zero;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                touchInput.x = (touch.deltaPosition.x / Screen.width) * touchSensitivity * 100f;
                touchInput.y = (touch.deltaPosition.y / Screen.height) * touchSensitivity * 100f;
            }
        }

        return touchInput;
    }

    void HandleGyroscopeInput()
    {
        if (!isGyroInitialized) return;

        Quaternion gyroRotation = Input.gyro.attitude;
        Quaternion relativeRotation = Quaternion.Inverse(baselineRotation) * gyroRotation;

        relativeRotation = new Quaternion(-relativeRotation.x, -relativeRotation.z, relativeRotation.y, relativeRotation.w);

        Vector3 eulerAngles = relativeRotation.eulerAngles;
        
        float gyroPitch = eulerAngles.x;
        if (gyroPitch > 180f) gyroPitch -= 360f;
        float gyroYaw = eulerAngles.y;

        gyroPitch = Mathf.Clamp(gyroPitch * gyroSensitivity, minPitch, maxPitch);
        gyroYaw *= gyroSensitivity;

        targetRotation = new Vector3(gyroPitch, gyroYaw, 0f);
    }

    void ApplyRotation()
    {
        Quaternion targetQuaternion = Quaternion.Euler(targetRotation);

        if (enableSmoothing)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, Time.deltaTime * smoothingFactor);
        }
        else
        {
            transform.rotation = targetQuaternion;
        }
    }

    public void ResetRotation()
    {
        yaw = 0f;
        pitch = 0f;
        targetRotation = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        if (isGyroInitialized)
        {
            CalibrateGyroscope();
        }
    }

    public void SetRotation(float newYaw, float newPitch)
    {
        yaw = newYaw;
        pitch = Mathf.Clamp(newPitch, minPitch, maxPitch);
        targetRotation = new Vector3(pitch, yaw, 0f);
    }

    public Vector2 GetCurrentRotation()
    {
        return new Vector2(yaw, pitch);
    }

    void OnValidate()
    {
        minPitch = Mathf.Clamp(minPitch, -90f, 0f);
        maxPitch = Mathf.Clamp(maxPitch, 0f, 90f);
    }
}