using UnityEngine;
using UnityEngine.InputSystem;

public class SnowmanController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float waterMoveSpeed = 2f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 15f;

    [Header("跳跃设置")]
    [SerializeField] private float powderJumpHeight = 3.5f;
    [SerializeField] private float waterJumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("形态切换")]
    [SerializeField] private float switchConfirmTime = 0.3f;
    [SerializeField] private float waterFormTimeLimit = 4f;

    [Header("霜冻惩罚")]
    [SerializeField] private int maxFrostStacks = 4;
    [SerializeField] private float speedPenaltyPerStack = 0.2f;

    [Header("引用")]
    [SerializeField] private ThirdPersonCamera cameraController;
    [SerializeField] private Transform bodyTransform;      // 身体模型（改材质/缩放）
    [SerializeField] private Transform eyesTransform;      // 眼睛模型（只跟随缩放）

    [Header("形态材质")]
    [SerializeField] private Material powderMaterial;
    [SerializeField] private Material iceMaterial;
    [SerializeField] private Material waterMaterial;

    public enum SnowmanForm { Powder, Ice, Water }
    private SnowmanForm currentForm = SnowmanForm.Powder;
    private SnowmanForm pendingForm = SnowmanForm.Powder;
    private float formSwitchTimer = 0f;
    private bool isSwitching = false;

    private Vector2 moveInput;
    private Vector3 currentVelocity;
    private CharacterController characterController;

    private int frostStacks = 0;
    private float waterFormTimer = 0f;
    private bool isWaterFormActive = false;

    private bool jumpPressed = false;
    private float lastScrollValue = 0f;

    private Renderer bodyRenderer;
    private Vector3 originalBodyScale;      // 记录 Body 原始缩放（假设为1,1,1）
    private Vector3 originalEyesScale;      // 记录 Eyes 原始缩放

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            characterController = gameObject.AddComponent<CharacterController>();

        if (bodyTransform != null)
        {
            bodyRenderer = bodyTransform.GetComponent<Renderer>();
            originalBodyScale = bodyTransform.localScale;
        }
        if (eyesTransform != null)
            originalEyesScale = eyesTransform.localScale;
    }

    private void Start()
    {
        if (cameraController != null && cameraController.target == null)
            cameraController.target = transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateFormVisual();
    }

    private void Update()
    {
        HandleInput();
        HandleFormSwitch();
        HandleMovementAndGravity();
        HandleWaterTimer();
    }

    private void HandleInput()
    {
        // 移动输入
        moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;

        // 跳跃
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;

        // 滚轮切换形态
        float currentScroll = Mouse.current.scroll.ReadValue().y;
        if (!Mathf.Approximately(currentScroll, lastScrollValue))
        {
            float delta = currentScroll - lastScrollValue;
            lastScrollValue = currentScroll;

            if (Mathf.Abs(delta) > 0.01f)
            {
                int currentIndex = (int)currentForm;
                int pendingIndex = currentIndex + (delta > 0 ? 1 : -1);
                if (pendingIndex < 0) pendingIndex = 2;
                if (pendingIndex > 2) pendingIndex = 0;

                SnowmanForm newPending = (SnowmanForm)pendingIndex;

                if (newPending == currentForm)
                {
                    isSwitching = false;
                    formSwitchTimer = 0f;
                    pendingForm = currentForm;
                }
                else
                {
                    isSwitching = true;
                    formSwitchTimer = 0f;
                    pendingForm = newPending;
                }
            }
        }
    }

    private void HandleFormSwitch()
    {
        if (!isSwitching) return;

        formSwitchTimer += Time.deltaTime;
        if (formSwitchTimer >= switchConfirmTime)
        {
            ApplyFormSwitch(pendingForm);
            isSwitching = false;
            formSwitchTimer = 0f;
        }
    }

    private void ApplyFormSwitch(SnowmanForm newForm)
    {
        if (currentForm == SnowmanForm.Water)
        {
            isWaterFormActive = false;
            ReduceFrostStacks(1);
        }

        currentForm = newForm;
        pendingForm = newForm;

        switch (currentForm)
        {
            case SnowmanForm.Powder:
                break;
            case SnowmanForm.Ice:
                currentVelocity.x = 0;
                currentVelocity.z = 0;
                break;
            case SnowmanForm.Water:
                isWaterFormActive = true;
                waterFormTimer = waterFormTimeLimit;
                break;
        }

        UpdateFormVisualWithBottomAlignment();
    }

    private void HandleWaterTimer()
    {
        if (!isWaterFormActive) return;

        waterFormTimer -= Time.deltaTime;
        if (waterFormTimer <= 0f)
        {
            OnWaterTimeout();
        }
    }

    private void OnWaterTimeout()
    {
        Debug.Log("水形态超时，永远冻住 - 游戏失败");
        enabled = false;
    }

    private void HandleMovementAndGravity()
    {
        bool isGrounded = characterController.isGrounded;

        if (isGrounded && currentVelocity.y < 0)
            currentVelocity.y = -2f;

        Vector3 targetHorizontalVelocity = Vector3.zero;

        if (currentForm != SnowmanForm.Ice && moveInput.magnitude > 0.1f)
        {
            Vector3 forward = cameraController.GetCameraForward();
            Vector3 right = cameraController.GetCameraRight();
            Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

            float finalSpeed = GetCurrentMoveSpeed();
            targetHorizontalVelocity = moveDirection * finalSpeed;
        }

        float smoothFactor = targetHorizontalVelocity.magnitude > 0.1f ? acceleration : deceleration;
        Vector3 newHorizontalVelocity = Vector3.Lerp(
            new Vector3(currentVelocity.x, 0, currentVelocity.z),
            targetHorizontalVelocity,
            smoothFactor * Time.deltaTime
        );

        if (jumpPressed && isGrounded)
        {
            jumpPressed = false;
            if (currentForm == SnowmanForm.Powder)
                currentVelocity.y = Mathf.Sqrt(powderJumpHeight * -2f * gravity);
            else if (currentForm == SnowmanForm.Water)
                currentVelocity.y = Mathf.Sqrt(waterJumpHeight * -2f * gravity);
        }
        jumpPressed = false;

        currentVelocity.y += gravity * Time.deltaTime;
        currentVelocity.x = newHorizontalVelocity.x;
        currentVelocity.z = newHorizontalVelocity.z;

        characterController.Move(currentVelocity * Time.deltaTime);

        // 视觉朝向（身体和眼睛一起转）
        // 在 HandleMovementAndGravity 末尾
        if (moveInput.magnitude > 0.1f && currentForm != SnowmanForm.Ice)
        {
            Vector3 lookDirection = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            if (lookDirection.magnitude > 0.1f && bodyTransform != null)
            {
                 bodyTransform.rotation = Quaternion.LookRotation(lookDirection);
                    // Eyes 会自动跟随，无需手动设置
            }
        }
    }

    private float GetCurrentMoveSpeed()
    {
        float speed = baseMoveSpeed;
        if (currentForm == SnowmanForm.Water)
            speed = waterMoveSpeed;
        if (currentForm != SnowmanForm.Ice)
        {
            float penalty = 1f - (frostStacks * speedPenaltyPerStack);
            speed *= Mathf.Max(penalty, 0.2f);
        }
        return speed;
    }

    #region 形态视觉与底部对齐

    private void UpdateFormVisual()
    {
        UpdateFormVisualWithBottomAlignment();
    }

    private void UpdateFormVisualWithBottomAlignment()
{
    if (bodyTransform == null) return;

    // 记录切换前的 Body 底部 Y 坐标
    float oldBottomY = GetBodyBottomY();

    // 确定目标缩放
    Vector3 targetBodyScale = originalBodyScale;
    Material targetMaterial = powderMaterial;

    switch (currentForm)
    {
        case SnowmanForm.Powder:
            targetBodyScale = originalBodyScale;
            targetMaterial = powderMaterial;
            break;
        case SnowmanForm.Ice:
            targetBodyScale = new Vector3(originalBodyScale.x * 1.2f, originalBodyScale.y, originalBodyScale.z * 1.2f);
            targetMaterial = iceMaterial;
            break;
        case SnowmanForm.Water:
            targetBodyScale = new Vector3(originalBodyScale.x * 1.4f, originalBodyScale.y * 0.6f, originalBodyScale.z * 1.4f);
            targetMaterial = waterMaterial;
            break;
    }

    // 应用材质
    if (bodyRenderer != null && targetMaterial != null)
        bodyRenderer.material = targetMaterial;

    // 直接应用身体缩放（Eyes 作为子物体会自动跟随）
    bodyTransform.localScale = targetBodyScale;

    // 计算新的底部位置并补偿高度差
    float newBottomY = GetBodyBottomY();
    float deltaY = oldBottomY - newBottomY;

    if (!Mathf.Approximately(deltaY, 0f))
    {
        characterController.enabled = false;
        transform.position += Vector3.up * deltaY;
        characterController.enabled = true;
    }
}

    private float GetBodyBottomY()
    {
        if (bodyTransform == null) return transform.position.y;

        // 优先使用 Collider 底部
        Collider col = bodyTransform.GetComponent<Collider>();
        if (col != null)
            return col.bounds.min.y;

        // 其次使用 Renderer 底部
        if (bodyRenderer != null)
            return bodyRenderer.bounds.min.y;

        // 最后估算：假设模型 pivot 在中心，高度为 localScale.y
        return bodyTransform.position.y - bodyTransform.localScale.y * 0.5f;
    }

    #endregion

    #region 公共方法

    public void AddFrostStack()
    {
        if (currentForm == SnowmanForm.Ice) return;
        frostStacks = Mathf.Min(frostStacks + 1, maxFrostStacks);
        if (frostStacks >= maxFrostStacks) OnFullyFrozen();
    }

    public void ReduceFrostStacks(int amount)
    {
        frostStacks = Mathf.Max(frostStacks - amount, 0);
    }

    public void ClearFrostStacks()
    {
        frostStacks = 0;
    }

    private void OnFullyFrozen()
    {
        Debug.Log("被完全冻住 - 游戏失败");
        enabled = false;
    }

    public void OnReflectHit() => ReduceFrostStacks(1);

    public void OnHitByWater()
    {
        if (currentForm == SnowmanForm.Powder)
        {
            ApplyFormSwitch(SnowmanForm.Ice);
            AddFrostStack();
        }
    }

    public void OnReachCheckpoint() => ClearFrostStacks();

    #endregion

    #region 属性

    public SnowmanForm CurrentForm => currentForm;
    public int FrostStacks => frostStacks;
    public float WaterTimerNormalized => isWaterFormActive ? waterFormTimer / waterFormTimeLimit : 0f;

    #endregion
}