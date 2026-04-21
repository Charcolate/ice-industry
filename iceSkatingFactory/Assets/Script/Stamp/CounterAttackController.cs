using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

public class CounterAttackController : MonoBehaviour
{
    [Header("蓄力设置")]
    public float[] powerThresholds = { 0.2f, 0.5f, 1.0f };
    public GameObject[] powerIndicatorLights;      // 反击专用的指示灯（也可以复用，但建议独立）
    public Color[] indicatorColors = { Color.white, Color.blue, Color.red };

    [Header("后坐力设置")]
    public float[] recoilDistances = { 2f, 3.5f, 5f };  // 对应1/2/3级
    public float backDuration = 1.5f;
    public float holdDuration = 1.0f;
    public float returnDuration = 1.5f;
    public float jumpHeight = 0.5f;
    public float jumpPeakTime = 0.3f;

    [Header("摄像机")]
    public ThirdPersonCamera cam;

    [Header("事件")]
    public UnityEvent<int> OnCounter;   // 反击盖章事件，参数：蓄力等级

    private float pressTimer = 0f;
    private bool isCharging = false;
    private int currentPowerLevel = 0;
    private bool isRecoiling = false;

    private Mouse mouse;
    private StampControl stampControl;   // 引用左键脚本，用于检查是否后坐力中

    void Start()
    {
        mouse = Mouse.current;
        stampControl = GetComponent<StampControl>();

        TurnOffAllLights();
    }

    void Update()
    {
        if (mouse == null) return;
        if (isRecoiling) return;

        // 如果左键脚本正在后坐力中，禁止反击操作
        if (stampControl != null && stampControl.IsRecoiling()) return;

        // 右键蓄力
        bool rightPressed = mouse.rightButton.isPressed;
        bool rightWasPressedThisFrame = mouse.rightButton.wasPressedThisFrame;
        bool rightWasReleasedThisFrame = mouse.rightButton.wasReleasedThisFrame;

        if (rightWasPressedThisFrame)
        {
            StartCharging();
        }

        if (isCharging && rightPressed)
        {
            pressTimer += Time.deltaTime;
            UpdatePowerLevel();
        }

        if (rightWasReleasedThisFrame && isCharging)
        {
            Counter();
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

    // ========== 反击 ==========
    void Counter()
    {
        isCharging = false;
        int finalLevel = currentPowerLevel > 0 ? currentPowerLevel : 1;

        OnCounter?.Invoke(finalLevel);

        pressTimer = 0f;
        currentPowerLevel = 0;
        TurnOffAllLights();

        float distance = recoilDistances[finalLevel - 1];
        StartCoroutine(RecoilRoutine(distance));
    }

    IEnumerator RecoilRoutine(float distance)
    {
        isRecoiling = true;
        if (cam != null) cam.SetSmoothing(false);

        Vector3 startPos = transform.position;
        Vector3 backPos = startPos - transform.forward * distance;

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

        transform.position = backPos;
        yield return new WaitForSeconds(holdDuration);

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

    public bool IsRecoiling() => isRecoiling;
    public int GetCurrentPowerLevel() => currentPowerLevel;
}