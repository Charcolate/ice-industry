using UnityEngine;
using System.Collections;
using TMPro;

public class WorldSpaceTextTrigger : MonoBehaviour
{
    [Header("文本对象")]
    [SerializeField] private GameObject textObject;          // 包含 TextMeshPro 的物体（初始隐藏）
    [SerializeField] private bool showOnce = true;           // 只显示一次
    
    [Header("动画")]
    [SerializeField] private bool playAnimation = true;      // 是否播放出现动画
    [SerializeField] private float bobHeight = 0.3f;        // 浮动高度
    [SerializeField] private float bobDuration = 0.5f;      // 单次浮动时长
    [SerializeField] private int bobCount = 2;              // 浮动次数
    
    private bool triggered = false;
    private Vector3 originalLocalPos;
    
    void Start()
    {
        if (textObject != null)
        {
            originalLocalPos = textObject.transform.localPosition;
            textObject.SetActive(false);                    // 初始隐藏
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (triggered && showOnce) return;
        if (!other.CompareTag("Player")) return;
        
        triggered = true;
        if (textObject != null)
        {
            textObject.SetActive(true);
            if (playAnimation)
                StartCoroutine(BobAnimation());
        }
    }
    
    IEnumerator BobAnimation()
    {
        Transform t = textObject.transform;
        for (int i = 0; i < bobCount; i++)
        {
            // 向上
            float elapsed = 0f;
            Vector3 startPos = originalLocalPos;
            Vector3 topPos = originalLocalPos + Vector3.up * bobHeight;
            
            while (elapsed < bobDuration)
            {
                elapsed += Time.deltaTime;
                t.localPosition = Vector3.Lerp(startPos, topPos, elapsed / bobDuration);
                yield return null;
            }
            // 向下
            elapsed = 0f;
            while (elapsed < bobDuration)
            {
                elapsed += Time.deltaTime;
                t.localPosition = Vector3.Lerp(topPos, startPos, elapsed / bobDuration);
                yield return null;
            }
        }
        t.localPosition = originalLocalPos;
    }
}