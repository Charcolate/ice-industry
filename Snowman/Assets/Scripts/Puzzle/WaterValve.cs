using UnityEngine;
using UnityEngine.InputSystem;

public class WaterValve : MonoBehaviour
{
    [Header("水阀设置")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private float turnAnimationTime = 0.5f;
    
    [Header("视觉")]
    [SerializeField] private Transform valveWheel;
    [SerializeField] private float wheelRotationAmount = 720f;
    [SerializeField] private Material activeMaterial;         // 开启时材质（水流）
    [SerializeField] private Material inactiveMaterial;       // 关闭时材质（无水）
    
    [Header("UI 提示")]
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private string closeText = "Press E to close valve";
    [SerializeField] private string openText = "Press E to open valve";
    
    [Header("关联水流")]
    [SerializeField] private WaterFlow[] waterFlows;          // 这个水阀控制的所有水流
    
    private bool isClosed = false;
    private bool isTurning = false;
    private Transform player;
    private Renderer rend;
    private TMPro.TextMeshProUGUI promptText;
    
    public bool IsClosed => isClosed;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rend = GetComponent<Renderer>();
        
        if (interactPrompt != null)
        {
            promptText = interactPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            interactPrompt.SetActive(false);
        }
        
        // 初始状态：开启
        if (activeMaterial != null && rend != null)
            rend.material = activeMaterial;
        
        UpdateWaterFlows();
    }
    
    void Update()
    {
        if (player == null || isTurning) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactRange;
        
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(inRange);
            if (inRange && promptText != null)
            {
                promptText.text = isClosed ? openText : closeText;
            }
        }
        
        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(ToggleValve());
        }
    }
    
    System.Collections.IEnumerator ToggleValve()
    {
        isTurning = true;
        
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
        
        // 旋转阀门轮盘
        if (valveWheel != null)
        {
            float elapsed = 0f;
            Quaternion startRotation = valveWheel.localRotation;
            float direction = isClosed ? -1f : 1f;  // 关时正转，开时反转
            Quaternion targetRotation = startRotation * Quaternion.Euler(wheelRotationAmount * direction, 0, 0);
            
            while (elapsed < turnAnimationTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / turnAnimationTime;
                valveWheel.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(turnAnimationTime);
        }
        
        // 切换状态
        isClosed = !isClosed;
        
        // 更换材质
        if (rend != null)
        {
            rend.material = isClosed ? inactiveMaterial : activeMaterial;
        }
        
        // 更新水流
        UpdateWaterFlows();
        
        isTurning = false;
        
        Debug.Log($"[WaterValve] 水阀已{(isClosed ? "关闭" : "开启")}");
    }
    
    void UpdateWaterFlows()
    {
        if (waterFlows == null) return;
        
        foreach (WaterFlow flow in waterFlows)
        {
            if (flow != null)
            {
                flow.SetWaterActive(!isClosed);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactRange);
        
        // 画线连接到水流
        if (waterFlows != null)
        {
            Gizmos.color = Color.cyan;
            foreach (WaterFlow flow in waterFlows)
            {
                if (flow != null)
                {
                    Gizmos.DrawLine(transform.position, flow.transform.position);
                }
            }
        }
    }
}