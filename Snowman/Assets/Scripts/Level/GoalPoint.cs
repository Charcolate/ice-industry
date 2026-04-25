using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoalPoint : MonoBehaviour
{
    [Header("视觉")]
    [SerializeField] private Renderer goalRenderer;
    [SerializeField] private Material idleMaterial;
    [SerializeField] private Material completeMaterial;
    [SerializeField] private ParticleSystem idleParticles;
    [SerializeField] private ParticleSystem completeParticles;
    
    [Header("通关 UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("场景")]
    [SerializeField] private string nextSceneName;
    
    private bool isCompleted = false;
    
    void Start()
    {
        levelCompletePanel.SetActive(false);
        goalRenderer.material = idleMaterial;
        if (idleParticles) idleParticles.Play();
        
        // 绑定按钮
        restartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        nextLevelButton.onClick.AddListener(() => {
            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
        });
        mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCompleted) return;
        if (other.CompareTag("Player"))
        {
            isCompleted = true;
            
            // 禁用重生系统，防止自动传送回起点
            PlayerRespawnManager respawn = other.GetComponent<PlayerRespawnManager>();
            if (respawn != null)
                respawn.enabled = false;
            
            // 冻结玩家
            SnowmanController player = other.GetComponent<SnowmanController>();
            if (player != null)
                player.enabled = false;
            
            goalRenderer.material = completeMaterial;
            if (idleParticles) idleParticles.Stop();
            if (completeParticles) completeParticles.Play();
            
            levelCompletePanel.SetActive(true);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // 持续强制光标可见
            StartCoroutine(ForceCursorVisible());
        }
    }
    
    System.Collections.IEnumerator ForceCursorVisible()
    {
        while (levelCompletePanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}