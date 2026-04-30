using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class TutorialPanel : MonoBehaviour
{
    [Header("UI 设置")]
    [SerializeField] private GameObject tutorialPanel;          // 教学面板
    [SerializeField] private GameObject clickPrompt;            // "点击继续"提示
    
    [Header("图片列表 - 逐个显示的子物体")]
    [SerializeField] private List<GameObject> tutorialImages = new List<GameObject>();  // 图片 GameObject 列表
    
    [Header("设置")]
    [SerializeField] private bool pauseGameOnShow = true;      // 显示时暂停游戏
    
    private int currentIndex = 0;
    private bool panelActive = false;
    private SnowmanController playerController;
    private PlayerRespawnManager respawnManager;
    
    void Start()
    {
        // 隐藏面板
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        // 隐藏所有图片
        foreach (GameObject img in tutorialImages)
        {
            if (img != null)
                img.SetActive(false);
        }
        
        if (clickPrompt != null)
            clickPrompt.SetActive(false);
        
        // 找到玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<SnowmanController>();
            respawnManager = player.GetComponent<PlayerRespawnManager>();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!panelActive && other.CompareTag("Player"))
        {
            ShowPanel();
        }
    }
    
    void Update()
    {
        if (!panelActive) return;
        
        // 鼠标左键继续
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            NextImage();
        }
    }
    
    void ShowPanel()
    {
        panelActive = true;
        currentIndex = 0;
        
        // 暂停玩家
        if (playerController != null)
            playerController.enabled = false;
        
        if (respawnManager != null)
            respawnManager.enabled = false;
        
        // 暂停游戏
        if (pauseGameOnShow)
            Time.timeScale = 0f;
        
        // 显示面板
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
        
        // 隐藏所有图片
        foreach (GameObject img in tutorialImages)
        {
            if (img != null)
                img.SetActive(false);
        }
        
        // 显示点击提示
        if (clickPrompt != null)
            clickPrompt.SetActive(true);
        
        // 显示第一张图片
        ShowImage(0);
        
        // 显示光标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("[TutorialPanel] 教学开始");
    }
    
    void ShowImage(int index)
    {
        if (index < tutorialImages.Count && tutorialImages[index] != null)
        {
            tutorialImages[index].SetActive(true);
        }
    }
    
    void NextImage()
    {
        currentIndex++;
        
        // 如果还有更多图片
        if (currentIndex < tutorialImages.Count)
        {
            ShowImage(currentIndex);
        }
        else
        {
            // 所有图片看完了，关闭面板
            ClosePanel();
        }
    }
    
    void ClosePanel()
    {
        panelActive = false;
        
        // 恢复游戏
        if (pauseGameOnShow)
            Time.timeScale = 1f;
        
        // 恢复玩家
        if (playerController != null)
            playerController.enabled = true;
        
        if (respawnManager != null)
            respawnManager.enabled = true;
        
        // 隐藏面板
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        if (clickPrompt != null)
            clickPrompt.SetActive(false);
        
        // 隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("[TutorialPanel] 教学结束");
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}