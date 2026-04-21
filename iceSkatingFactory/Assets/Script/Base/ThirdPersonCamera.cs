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
    public LayerMask collisionMask = ~0;  // 默认检测所有层
    public float collisionOffset = 0.3f;  // 摄像机离障碍物的最小距离
    
    private float currentX = 0f;
    private float currentY = 20f;
    private Vector3 smoothVelocity = Vector3.zero;
    private float currentDistance;
    
    private Vector2 lookInput;
    private float scrollInput;
    
    void Start()
    {
        currentDistance = distance;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 如果没有设置碰撞遮罩，默认忽略 Player 层
        if (collisionMask.value == ~0)
        {
            collisionMask = ~LayerMask.GetMask("Player");
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
    }
    
    void HandleInput()
    {
        lookInput = Mouse.current.delta.ReadValue();
        scrollInput = Mouse.current.scroll.ReadValue().y;
        
        currentX += lookInput.x * rotationSpeedX * 0.1f;
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
        
        // 碰撞检测，忽略 Player 层
        Vector3 direction = (desiredPosition - targetPosition).normalized;
        float checkDistance = Vector3.Distance(targetPosition, desiredPosition);
        
        if (Physics.Linecast(targetPosition, desiredPosition, out RaycastHit hit, collisionMask))
        {
            // 把摄像机放在碰撞点稍前的位置
            desiredPosition = hit.point - direction * collisionOffset;
            
            // 确保不会比最小距离更近
            float hitDistance = Vector3.Distance(targetPosition, desiredPosition);
            if (hitDistance < minDistance)
            {
                desiredPosition = targetPosition + direction * minDistance;
            }
        }
        
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, smoothTime);
        transform.LookAt(targetPosition);
    }
    
    //public Vector3 GetCameraForward()
    //{
        //Vector3 forward = transform.forward;
        //forward.y = 0;
        //return forward.normalized;
    //}
    
    //public Vector3 GetCameraRight()
    //{
        //Vector3 right = transform.right;
        //right.y = 0;
        //return right.normalized;
    //}
}