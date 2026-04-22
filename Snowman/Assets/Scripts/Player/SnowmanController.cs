using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;

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

    [Header("反击")]
    [SerializeField] private GameObject reflectBulletPrefab;
    [SerializeField] private float reflectBulletSpeed = 20f;

    [Header("瞄准线")]
    [SerializeField] private LineRenderer aimLineRenderer;
    [SerializeField] private float maxAimDistance = 30f;
    [SerializeField] private Color aimLineColor = Color.cyan;

    [Header("瞄准辅助")]
    [SerializeField] private bool enableAimAssist = true;
    [SerializeField] private float aimAssistRadius = 30f;
    [SerializeField] private float aimAssistStrength = 0.8f;

    [Header("敌人列表")]
    [SerializeField] private List<GameObject> enemyList = new List<GameObject>();

    [Header("引用")]
    [SerializeField] private ThirdPersonCamera cameraController;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform eyesTransform;

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
    private float lastReflectTime = -1f;

    private Renderer bodyRenderer;
    private Vector3 originalBodyScale;
    private Vector3 originalEyesScale;

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

        // 自动创建瞄准线
        if (aimLineRenderer == null)
        {
            GameObject lineObj = new GameObject("AimLine");
            lineObj.transform.SetParent(transform);
            aimLineRenderer = lineObj.AddComponent<LineRenderer>();
            aimLineRenderer.startWidth = 0.1f;
            aimLineRenderer.endWidth = 0.1f;
            aimLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            aimLineRenderer.startColor = aimLineColor;
            aimLineRenderer.endColor = aimLineColor;
            aimLineRenderer.positionCount = 2;
            aimLineRenderer.enabled = false;
        }
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
        // 保持鼠标锁定
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        HandleInput();
        HandleFormSwitch();
        HandleMovementAndGravity();
        HandleWaterTimer();
        UpdateAimLine();
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

        if (moveInput.magnitude > 0.1f && currentForm != SnowmanForm.Ice)
        {
            Vector3 lookDirection = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            if (lookDirection.magnitude > 0.1f && bodyTransform != null)
            {
                bodyTransform.rotation = Quaternion.LookRotation(lookDirection);
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

    private void UpdateFormVisualWithBottomAlignment()
    {
        if (bodyTransform == null) return;

        float oldBottomY = GetBodyBottomY();

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

        if (bodyRenderer != null && targetMaterial != null)
            bodyRenderer.material = targetMaterial;

        bodyTransform.localScale = targetBodyScale;

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

        Collider col = bodyTransform.GetComponent<Collider>();
        if (col != null)
            return col.bounds.min.y;

        if (bodyRenderer != null)
            return bodyRenderer.bounds.min.y;

        return bodyTransform.position.y - bodyTransform.localScale.y * 0.5f;
    }

    private void UpdateFormVisual()
    {
        UpdateFormVisualWithBottomAlignment();
    }

    private void UpdateAimLine()
    {
        if (currentForm == SnowmanForm.Ice && aimLineRenderer != null)
        {
            aimLineRenderer.enabled = true;

            float bodyHeight = 0.6f;
            Vector3 startPoint = transform.position + Vector3.up * bodyHeight;

            Vector3 aimDirection = GetHorizontalAimDirection();

            if (enableAimAssist)
            {
                aimDirection = ApplyAimAssist(startPoint, aimDirection);
            }

            Vector3 endPoint = startPoint + aimDirection * maxAimDistance;

            aimLineRenderer.SetPosition(0, startPoint);
            aimLineRenderer.SetPosition(1, endPoint);

            if (HasEnemyInSights(startPoint, aimDirection))
            {
                aimLineRenderer.startColor = Color.red;
                aimLineRenderer.endColor = Color.red;
            }
            else
            {
                aimLineRenderer.startColor = aimLineColor;
                aimLineRenderer.endColor = aimLineColor;
            }
        }
        else
        {
            if (aimLineRenderer != null)
                aimLineRenderer.enabled = false;
        }
    }

    private Vector3 GetHorizontalAimDirection()
    {
        if (Camera.main == null)
            return transform.forward;

        float bodyHeight = 0.6f;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane horizontalPlane = new Plane(Vector3.up, transform.position + Vector3.up * bodyHeight);

        if (horizontalPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = (hitPoint - (transform.position + Vector3.up * bodyHeight)).normalized;
            direction.y = 0;

            if (direction.magnitude < 0.1f)
            {
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0;
                return camForward.normalized;
            }

            return direction.normalized;
        }

        Vector3 fallbackDir = Camera.main.transform.forward;
        fallbackDir.y = 0;
        return fallbackDir.normalized;
    }

    private Vector3 ApplyAimAssist(Vector3 startPoint, Vector3 originalDirection)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            EnemyTurret[] turrets = FindObjectsByType<EnemyTurret>(FindObjectsSortMode.None);
            enemies = turrets.Select(t => t.gameObject).ToArray();
        }

        Transform nearestEnemy = null;
        float nearestAngle = aimAssistRadius;

        foreach (GameObject enemy in enemies)
        {
            Vector3 directionToEnemy = (enemy.transform.position - startPoint).normalized;
            directionToEnemy.y = 0;

            float angle = Vector3.Angle(originalDirection, directionToEnemy);
            float distance = Vector3.Distance(startPoint, enemy.transform.position);

            if (angle < aimAssistRadius && distance < maxAimDistance)
            {
                if (angle < nearestAngle)
                {
                    nearestAngle = angle;
                    nearestEnemy = enemy.transform;
                }
            }
        }

        if (nearestEnemy != null)
        {
            Vector3 targetDirection = (nearestEnemy.position - startPoint).normalized;
            targetDirection.y = 0;
            return Vector3.Lerp(originalDirection, targetDirection, aimAssistStrength).normalized;
        }

        return originalDirection;
    }

    private bool HasEnemyInSights(Vector3 startPoint, Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(startPoint, direction, out hit, maxAimDistance))
        {
            return hit.collider.CompareTag("Enemy") || hit.collider.GetComponent<EnemyTurret>() != null;
        }
        return false;
    }

    // 获取当前瞄准线瞄准的敌人
    private GameObject GetAimedEnemy()
    {
        float bodyHeight = 0.6f;
        Vector3 startPoint = transform.position + Vector3.up * bodyHeight;
        Vector3 aimDirection = GetHorizontalAimDirection();

        if (enableAimAssist)
        {
            aimDirection = ApplyAimAssist(startPoint, aimDirection);
        }

        RaycastHit hit;
        if (Physics.Raycast(startPoint, aimDirection, out hit, maxAimDistance))
        {
            // 检查是否击中敌人
            if (hit.collider.CompareTag("Enemy") || hit.collider.GetComponent<EnemyTurret>() != null)
            {
                return hit.collider.gameObject;
            }

            // 检查父物体
            if (hit.collider.transform.parent != null)
            {
                Transform parent = hit.collider.transform.parent;
                if (parent.CompareTag("Enemy") || parent.GetComponent<EnemyTurret>() != null)
                {
                    return parent.gameObject;
                }
            }
        }

        return null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (Camera.main == null)
            return transform.position + transform.forward * 5f;

        float bodyHeight = 0.6f;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane horizontalPlane = new Plane(Vector3.up, transform.position + Vector3.up * bodyHeight);

        if (horizontalPlane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return transform.position + Camera.main.transform.forward * 5f;
    }

    public void SpawnReflectBullet(Vector3 spawnPosition)
{
    if (reflectBulletPrefab == null) return;

    if (Time.time - lastReflectTime < 0.1f) return;
    lastReflectTime = Time.time;

    float bodyHeight = 0.6f;
    Vector3 startPoint = transform.position + Vector3.up * bodyHeight;
    Vector3 aimDirection = GetHorizontalAimDirection();

    GameObject bullet = Instantiate(reflectBulletPrefab, spawnPosition, Quaternion.LookRotation(aimDirection));
    ReflectBullet rb = bullet.GetComponent<ReflectBullet>();
    if (rb != null)
    {
        rb.SetDirection(aimDirection, reflectBulletSpeed, this);
    }
}


    public void AddFrostStack()
    {
        if (currentForm == SnowmanForm.Ice) return;
        frostStacks = Mathf.Min(frostStacks + 1, maxFrostStacks);
        if (frostStacks >= maxFrostStacks) OnFullyFrozen();
        Debug.Log($"[SnowmanController] 霜冻层数: {frostStacks}");
    }

    public void ReduceFrostStacks(int amount)
    {
        frostStacks = Mathf.Max(frostStacks - amount, 0);
        Debug.Log($"[SnowmanController] 霜冻减少，当前: {frostStacks}");
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

    public void OnReflectHit()
    {
        ReduceFrostStacks(1);
    }

    public void OnHitByWater()
    {
        if (currentForm == SnowmanForm.Powder)
        {
            ApplyFormSwitch(SnowmanForm.Ice);
            AddFrostStack();
        }
    }

    public void OnReachCheckpoint()
    {
        ClearFrostStacks();
    }

    public SnowmanForm CurrentForm => currentForm;
    public int FrostStacks => frostStacks;
    public float WaterTimerNormalized => isWaterFormActive ? waterFormTimer / waterFormTimeLimit : 0f;
}