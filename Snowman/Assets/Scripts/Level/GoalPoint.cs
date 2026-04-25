using UnityEngine;

public class GoalPoint : MonoBehaviour
{
    [Header("视觉 - Renderer")]
    [SerializeField] private Renderer goalRenderer;           // 终点 Renderer
    [SerializeField] private Material idleMaterial;           // 未触碰时材质
    [SerializeField] private Material completeMaterial;       // 触碰后材质
    
    [Header("粒子")]
    [SerializeField] private ParticleSystem idleParticles;
    [SerializeField] private ParticleSystem completeParticles;
    
    [Header("通关 UI")]
    [SerializeField] private GameObject levelCompleteUI;
    [SerializeField] private float uiDelay = 0.5f;
    
    private bool isCompleted = false;
    
    void Start()
    {
        // 初始材质
        if (goalRenderer != null && idleMaterial != null)
        {
            goalRenderer.material = idleMaterial;
        }
        
        if (levelCompleteUI != null)
            levelCompleteUI.SetActive(false);
        
        if (idleParticles != null)
            idleParticles.Play();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCompleted) return;
        
        if (other.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }
    
    void CompleteLevel()
    {
        isCompleted = true;
        
        // 切换材质
        if (goalRenderer != null && completeMaterial != null)
        {
            goalRenderer.material = completeMaterial;
        }
        
        // 粒子
        if (idleParticles != null)
            idleParticles.Stop();
        
        if (completeParticles != null)
            completeParticles.Play();
        
        // 停止玩家
        SnowmanController player = FindAnyObjectByType<SnowmanController>();
        if (player != null)
        {
            player.enabled = false;
        }
        
        // 显示通关 UI
        StartCoroutine(ShowCompleteUI());
        
        Debug.Log("[GoalPoint] 关卡完成！");
    }
    
    System.Collections.IEnumerator ShowCompleteUI()
    {
        yield return new WaitForSeconds(uiDelay);
        
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = isCompleted ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 2, 1));
    }
}