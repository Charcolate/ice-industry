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
    
    private float currentX = 0f;
    private float currentY = 20f;
    private Vector3 smoothVelocity = Vector3.zero;
    private float currentDistance;
    
    // 新输入系统的输入值
    private Vector2 lookInput;
    private float scrollInput;
    
    void Start()
{
    currentDistance = distance;
}

void LateUpdate()
{
    if (target == null) return;
    
    HandleInput();
    UpdateCameraPosition();
}
    
    void HandleInput()
    {
        // 新版输入系统获取鼠标移动
        lookInput = Mouse.current.delta.ReadValue();
        //scrollInput = Mouse.current.scroll.ReadValue().y;
        
        currentX += lookInput.x * rotationSpeedX * 0.1f;  // 乘0.1是因为新系统delta值比旧系统大
        currentY -= lookInput.y * rotationSpeedY * 0.1f;
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        
        //currentDistance -= scrollInput * 0.5f;
        //currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }
    
    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 targetPosition = target.position + Vector3.up * targetHeight;
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * currentDistance);
        
        if (Physics.Linecast(targetPosition, desiredPosition, out RaycastHit hit))
        {
            desiredPosition = hit.point + (desiredPosition - targetPosition).normalized * 0.5f;
        }
        
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, smoothTime);
        transform.LookAt(targetPosition);
    }
    
    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    
    public Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        return right.normalized;
    }
}