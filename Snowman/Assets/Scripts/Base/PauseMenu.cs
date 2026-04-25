using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    
    private bool isPaused = false;
    
    void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(ResumeGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }
    
    void Start()
    {
        pausePanel.SetActive(false);
        
        // 确保游戏开始时锁定光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("[PauseMenu] ESC 按下");
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    void PauseGame()
    {
        Debug.Log("[PauseMenu] PauseGame 调用");
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        
        // 强制显示光标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("[PauseMenu] 光标状态: lockState=" + Cursor.lockState + ", visible=" + Cursor.visible);
    }
    
    public void ResumeGame()
    {
        Debug.Log("[PauseMenu] ResumeGame 被调用！");
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void RestartGame()
    {
        Debug.Log("[PauseMenu] RestartGame 被调用！");
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
        
        PlayerRespawnManager respawn = FindAnyObjectByType<PlayerRespawnManager>();
        if (respawn != null)
        {
            respawn.ForceRespawn();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    
    public void ExitGame()
    {
        Debug.Log("[PauseMenu] ExitGame 被调用！");
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}