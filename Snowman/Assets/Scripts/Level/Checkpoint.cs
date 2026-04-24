using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("视觉设置")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    
    [Header("粒子效果")]
    [SerializeField] private ParticleSystem activateParticles;
    
    private Renderer rend;
    private bool isActivated = false;
    
    void Awake()
    {
        // 获取 Renderer
        rend = GetComponent<Renderer>();
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();
        
        // 立即设置材质，不等 Start
        if (inactiveMaterial != null && rend != null)
        {
            rend.material = inactiveMaterial;
        }
    }
    
    void Start()
    {
        // 如果 Awake 没设置成功，这里再试一次
        if (rend != null && inactiveMaterial != null && rend.material != inactiveMaterial)
        {
            rend.material = inactiveMaterial;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;
        
        if (other.CompareTag("Player"))
        {
            ActivateCheckpoint(other.gameObject);
        }
    }
    
    void ActivateCheckpoint(GameObject player)
    {
        isActivated = true;
        
        // 切换材质
        if (activeMaterial != null && rend != null)
        {
            rend.material = activeMaterial;
        }
        
        // 通知玩家
        SnowmanController playerController = player.GetComponent<SnowmanController>();
        if (playerController != null)
        {
            playerController.OnReachCheckpoint();
        }
        
        // 通知重生管理器
        PlayerRespawnManager respawnManager = player.GetComponent<PlayerRespawnManager>();
        if (respawnManager != null)
        {
            respawnManager.SetCheckpoint(transform.position, transform.rotation);
        }
        
        // 播放粒子
        if (activateParticles != null)
            activateParticles.Play();
        
        // 触碰动画
        StartCoroutine(ActivateAnimation());
    }
    
    System.Collections.IEnumerator ActivateAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
}