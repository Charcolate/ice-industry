using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("按钮")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    
    [Header("设置")]
    [SerializeField] private string firstLevelName = "Level_1_1";  // 第一个关卡名
    [SerializeField] private int firstLevelIndex = 1;              // 或直接用 Build Index
    
    void Start()
    {
        // 解锁鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 绑定按钮
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    public void StartGame()
    {
        // 方法1：按场景名加载
        SceneManager.LoadScene(firstLevelName);
        
        // 方法2：按 Build Index 加载（二选一）
        // SceneManager.LoadScene(firstLevelIndex);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}