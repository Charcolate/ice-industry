using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("目标")]
    public Transform target;
    public float targetHeight = 1.5f;
    
    [Header("距离")]
    public float distance = 8f;
    public float minDistance = 3f;
    public float maxDistance = 15f;
    
    [Header("旋转速度")]
    public float rotationSpeedX = 3f;
    public float rotationSpeedY = 2f;
    
    [Header("垂直角度限制")]
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 70f;
    
    [Header("平滑")]
    public float smoothTime = 0.15f;
    
    [Header("碰撞")]
    public LayerMask collisionMask = ~0;
    public float collisionOffset = 0.3f;

    // 后坐力时禁用平滑
    private bool useSmoothing = true;
    private Vector3 smoothVelocity = Vector3.zero;
    
    private float currentX = 0f;
    private float currentY = 20f;
    private float currentDistance;

    void Start()
    {
        currentDistance = distance;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (collisionMask.value == ~0)
            collisionMask = ~LayerMask.GetMask("Player");
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        Vector2 lookInput = Mouse.current.delta.ReadValue();
        float scrollInput = Mouse.current.scroll.ReadValue().y;
        
        currentX += lookInput.x * rotationSpeedX * 0.1f;
        currentY -= lookInput.y * rotationSpeedY * 0.1f;
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        
        currentDistance -= scrollInput * 0.5f;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 targetPosition = target.position + Vector3.up * targetHeight;
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * currentDistance);
        
        // 碰撞检测
        Vector3 direction = (desiredPosition - targetPosition).normalized;
        float checkDistance = Vector3.Distance(targetPosition, desiredPosition);
        if (Physics.Linecast(targetPosition, desiredPosition, out RaycastHit hit, collisionMask))
        {
            desiredPosition = hit.point - direction * collisionOffset;
            float hitDistance = Vector3.Distance(targetPosition, desiredPosition);
            if (hitDistance < minDistance)
                desiredPosition = targetPosition + direction * minDistance;
        }

        // 根据开关决定是否平滑
        if (useSmoothing)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, smoothTime);
        }
        else
        {
            transform.position = desiredPosition;
        }

        transform.LookAt(targetPosition);
    }

    // 公共方法：后坐力期间禁用平滑
    public void SetSmoothing(bool enabled)
    {
        useSmoothing = enabled;
        if (!enabled) smoothVelocity = Vector3.zero;
    }
}