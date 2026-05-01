using UnityEngine;
using UnityEngine.InputSystem;

public class WaterValve : MonoBehaviour
{
    [Header("水阀设置")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private float turnAnimationTime = 0.5f;
    
    [Header("视觉 - 轮盘")]
    [SerializeField] private Transform valveWheel;
    [SerializeField] private Renderer wheelRenderer;
    [SerializeField] private float wheelRotationAmount = 720f;
    
    [Header("视觉 - 材质")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    
    [Header("视觉 - 其他部件")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Renderer pipeRenderer;
    
    [Header("UI 提示")]
    [SerializeField] private GameObject interactPrompt;
    
    [Header("关联水流")]
    [SerializeField] private WaterFlow[] waterFlows;
    
    private bool isClosed = false;
    private bool isTurning = false;
    private Transform player;
    private TMPro.TextMeshProUGUI promptText;
    
    public bool IsClosed => isClosed;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (interactPrompt != null)
        {
            promptText = interactPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            interactPrompt.SetActive(false);
        }
        
        UpdateAllVisuals();
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
                promptText.text = isClosed ? "Press E to open valve" : "Press E to close valve";
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
        
        // 旋转动画
        if (valveWheel != null)
        {
            float elapsed = 0f;
            Quaternion startRotation = valveWheel.localRotation;
            float direction = isClosed ? -1f : 1f;
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
        
        // 更新视觉
        UpdateAllVisuals();
        
        // 反转每个水流的当前状态
        UpdateWaterFlows();
        
        isTurning = false;
    }
    
    void UpdateAllVisuals()
    {
        Material targetMaterial = isClosed ? inactiveMaterial : activeMaterial;
        
        if (wheelRenderer != null && targetMaterial != null)
            wheelRenderer.material = targetMaterial;
        
        if (bodyRenderer != null && targetMaterial != null)
            bodyRenderer.material = targetMaterial;
        
        if (pipeRenderer != null && targetMaterial != null)
            pipeRenderer.material = targetMaterial;
    }
    
    void UpdateWaterFlows()
    {
        if (waterFlows == null) return;
        
        foreach (WaterFlow flow in waterFlows)
        {
            if (flow != null)
            {
                // 反转每个水流的当前状态
                flow.SetWaterActive(!flow.IsActive);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactRange);
        
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