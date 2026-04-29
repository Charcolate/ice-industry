using UnityEngine;
using System.Collections;   

public class AppearingGuide : MonoBehaviour
{
     [Header("箭头设置")]
    [SerializeField] private GameObject arrowObject;          // 箭头模型（初始可隐藏）
    [SerializeField] private float bobHeight = 0.5f;          // 每次浮动高度
    [SerializeField] private float bobDuration = 0.4f;        // 单次浮动时长
    [SerializeField] private int bobCount = 3;                // 浮动次数
    
    [Header("触发设置")]
    [SerializeField] private bool triggerOnce = true;         // 是否只触发一次
    
    private Vector3 originalLocalPos;
    private bool triggered = false;
    
    void Start()
    {
        if (arrowObject != null)
        {
            originalLocalPos = arrowObject.transform.localPosition;
            arrowObject.SetActive(false);                     // 初始隐藏
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (triggered && triggerOnce) return;
        if (!other.CompareTag("Player")) return;
        
        triggered = true;
        if (arrowObject != null)
        {
            arrowObject.SetActive(true);
            StartCoroutine(BobAnimation());
        }
    }
    
    IEnumerator BobAnimation()
    {
        Transform arrowTransform = arrowObject.transform;
        
        // 来回浮动 bobCount 次
        for (int i = 0; i < bobCount; i++)
        {
            // 向上
            float elapsed = 0f;
            Vector3 startPos = originalLocalPos;
            Vector3 topPos = originalLocalPos + Vector3.up * bobHeight;
            
            while (elapsed < bobDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bobDuration;
                arrowTransform.localPosition = Vector3.Lerp(startPos, topPos, t);
                yield return null;
            }
            
            // 向下
            elapsed = 0f;
            while (elapsed < bobDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bobDuration;
                arrowTransform.localPosition = Vector3.Lerp(topPos, startPos, t);
                yield return null;
            }
        }
        
        // 最后停在原位
        arrowTransform.localPosition = originalLocalPos;
    }
}
