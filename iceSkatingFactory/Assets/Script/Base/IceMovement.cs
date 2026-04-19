using UnityEngine;
using UnityEngine.InputSystem;

public class IceMovement : MonoBehaviour
{
    [Header("移动参数")]
    public float acceleration = 15f;
    public float maxSpeed = 12f;
    public float turnSpeed = 2f;
    public float friction = 2f;
    public float driftFactor = 0.95f;
    
    private Rigidbody rb;
    private Vector2 moveInput;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
            
        rb.useGravity = false;
        rb.linearDamping = 0.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
    
    void Update()
    {
        // 新版输入系统获取WASD
        moveInput = Keyboard.current != null ? 
            new Vector2(
                (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
            ).normalized : Vector2.zero;
    }
    
    void FixedUpdate()
    {
        ApplyMovement();
        ApplyDrift();
    }
    
    void ApplyMovement()
    {
        ThirdPersonCamera cam = Camera.main.GetComponent<ThirdPersonCamera>();
        if (cam == null) return;
        
        Vector3 camForward = cam.GetCameraForward();
        Vector3 camRight = cam.GetCameraRight();
        
        Vector3 relativeInput = (camForward * moveInput.y + camRight * moveInput.x);
        
        if (relativeInput.magnitude > 0.1f)
        {
            rb.AddForce(relativeInput * acceleration, ForceMode.Acceleration);
            
            if (rb.linearVelocity.magnitude > 0.5f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, friction * Time.fixedDeltaTime);
        }
        
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
    
    void ApplyDrift()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        localVel.x *= driftFactor;
        rb.linearVelocity = transform.TransformDirection(localVel);
    }
}