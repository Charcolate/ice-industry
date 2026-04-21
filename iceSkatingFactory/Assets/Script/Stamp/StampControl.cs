using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

public class StampControl : MonoBehaviour
{
    [Header("印章形状")]
    public GameObject[] stampShapes;
    private int currentShapeIndex = 0;

    [Header("蓄力设置")]
    public float[] powerThresholds = { 0.2f, 0.5f, 1.0f };
    public GameObject[] powerIndicatorLights;
    public Color[] indicatorColors = { Color.white, Color.blue, Color.red };

    [Header("后坐力设置")]
    public float[] recoilDistances = { 2f, 3.5f, 5f };  // 1级、2级、3级对应的后退距离
    public float recoilDistance = 3f;        // 后退距离
    public float backDuration = 1.5f;        // 后退时长
    public float holdDuration = 1.0f;        // 停留时长
    public float returnDuration = 1.5f;      // 归位时长
    public float jumpHeight = 0.5f;          // 跳高度
    public float jumpPeakTime = 0.3f;        // 跳时长

    [Header("事件")]
    public UnityEvent<int, int> OnStamp;     // (形状索引, 蓄力等级)

    private float pressTimer = 0f;
    private bool isCharging = false;
    private int currentPowerLevel = 0;
    private bool isRecoiling = false;        // 是否正在后坐力中

    [Header("摄像机")]
    public ThirdPersonCamera cam;  

    private Mouse mouse;
    private Vector3 originalPosition;

    // ========== 公共属性 ==========
    public int GetCurrentShapeIndex() => currentShapeIndex;
    public int GetCurrentPowerLevel() => currentPowerLevel;
    public bool IsRecoiling() => isRecoiling;

    void Start()
    {
        mouse = Mouse.current;
        if (mouse == null) Debug.LogError("未检测到鼠标设备！");

        originalPosition = transform.position;
        UpdateStampShape(0);
        TurnOffAllLights();
    }

    void Update()
    {
        if (mouse == null) return;
        if (isRecoiling) return; // 后坐力期间禁止操作

        // 滚轮切换形状
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0)
        {
            int direction = scroll > 0 ? 1 : -1;
            SwitchShape(direction);
        }

        // 左键蓄力
        bool leftPressed = mouse.leftButton.isPressed;
        bool leftWasPressedThisFrame = mouse.leftButton.wasPressedThisFrame;
        bool leftWasReleasedThisFrame = mouse.leftButton.wasReleasedThisFrame;

        if (leftWasPressedThisFrame)
        {
            StartCharging();
        }

        if (isCharging && leftPressed)
        {
            pressTimer += Time.deltaTime;
            UpdatePowerLevel();
        }

        if (leftWasReleasedThisFrame && isCharging)
        {
            Stamp();
        }
    }

    // ========== 形状切换 ==========
    void SwitchShape(int direction)
    {
        currentShapeIndex = (currentShapeIndex + direction) % stampShapes.Length;
        if (currentShapeIndex < 0) currentShapeIndex += stampShapes.Length;
        UpdateStampShape(currentShapeIndex);
    }

    void UpdateStampShape(int index)
    {
        for (int i = 0; i < stampShapes.Length; i++)
        {
            if (stampShapes[i] != null)
                stampShapes[i].SetActive(i == index);
        }
    }

    // ========== 蓄力系统 ==========
    void StartCharging()
    {
        isCharging = true;
        pressTimer = 0f;
        currentPowerLevel = 0;
        TurnOffAllLights();
    }

    void UpdatePowerLevel()
    {
        int newLevel = 0;
        for (int i = 0; i < powerThresholds.Length; i++)
        {
            if (pressTimer >= powerThresholds[i])
                newLevel = i + 1;
        }

        if (newLevel != currentPowerLevel)
        {
            currentPowerLevel = newLevel;
            UpdateIndicatorLights(currentPowerLevel);
        }
    }

    void UpdateIndicatorLights(int level)
    {
        TurnOffAllLights();
        for (int i = 0; i < level; i++)
        {
            if (i < powerIndicatorLights.Length && powerIndicatorLights[i] != null)
            {
                powerIndicatorLights[i].SetActive(true);
                Renderer rend = powerIndicatorLights[i].GetComponent<Renderer>();
                if (rend != null && i < indicatorColors.Length)
                    rend.material.color = indicatorColors[i];
            }
        }
    }

    void TurnOffAllLights()
    {
        foreach (GameObject light in powerIndicatorLights)
        {
            if (light != null)
                light.SetActive(false);
        }
    }

    // ========== 盖章与后坐力 ==========
    void Stamp()
    {
        isCharging = false;
        int finalLevel = currentPowerLevel > 0 ? currentPowerLevel : 1;

        OnStamp?.Invoke(currentShapeIndex, finalLevel);

        pressTimer = 0f;
        currentPowerLevel = 0;
        TurnOffAllLights();

        // 根据蓄力等级获取后退距离
        float distance = recoilDistances[finalLevel - 1];
        StartCoroutine(RecoilRoutine(distance));
    }

    IEnumerator RecoilRoutine(float distance)
    {
    isRecoiling = true;
    if (cam != null) cam.SetSmoothing(false);

    Vector3 startPos = transform.position;
    Vector3 backPos = startPos - transform.forward * distance;

    // 安全钳：确保弹跳峰值时间不超过后退时长
    float peak = Mathf.Clamp(jumpPeakTime, 0.05f, backDuration - 0.05f);
    float jumpHeightSafe = Mathf.Max(0f, jumpHeight);

    float elapsed = 0f;
    while (elapsed < backDuration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / backDuration);
        Vector3 horizontalPos = Vector3.Lerp(startPos, backPos, t);

        float verticalOffset = 0f;
        if (elapsed <= peak)
        {
            float upT = elapsed / peak;
            verticalOffset = Mathf.Sin(upT * Mathf.PI * 0.5f) * jumpHeightSafe;
        }
        else
        {
            float downT = (elapsed - peak) / (backDuration - peak);
            verticalOffset = Mathf.Cos(downT * Mathf.PI * 0.5f) * jumpHeightSafe;
        }

        transform.position = horizontalPos + Vector3.up * verticalOffset;
        yield return null;
    }

    // 确保最终位置准确
    transform.position = backPos;

    yield return new WaitForSeconds(holdDuration);

    // 归位
    elapsed = 0f;
    Vector3 currentPos = transform.position;
    while (elapsed < returnDuration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / returnDuration;
        transform.position = Vector3.Lerp(currentPos, startPos, t);
        yield return null;
    }
    transform.position = startPos;

    if (cam != null) cam.SetSmoothing(true);
    isRecoiling = false;
    }
    }
