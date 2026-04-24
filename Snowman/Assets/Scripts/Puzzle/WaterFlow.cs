using UnityEngine;
using System.Collections.Generic;

public class WaterFlow : MonoBehaviour
{
    [Header("水流设置")]
    [SerializeField] private float pushForce = 8f;              // 推力大小
    [SerializeField] private Vector3 flowDirection = Vector3.down; // 水流方向
    
    [Header("视觉")]
    [SerializeField] private Material waterMaterial;
    [SerializeField] private float scrollSpeed = 1f;
    
    [Header("碰撞体设置")]
    [SerializeField] private Vector3 colliderSize = new Vector3(2f, 3f, 1f);
    [SerializeField] private Vector3 colliderCenter = new Vector3(0, 1.5f, 0);
    
    private Renderer rend;
    private bool isActive = true;
    private Vector2 textureOffset;
    private BoxCollider waterCollider;
    private HashSet<CharacterController> playersInWater = new HashSet<CharacterController>();
    private HashSet<SnowmanController> playersToBlock = new HashSet<SnowmanController>();
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null && waterMaterial != null)
        {
            rend.material = waterMaterial;
        }
        
        // 获取或创建碰撞体（使用 Inspector 中的值）
        waterCollider = GetComponent<BoxCollider>();
        if (waterCollider == null)
        {
            waterCollider = gameObject.AddComponent<BoxCollider>();
        }
        waterCollider.isTrigger = true;
        waterCollider.size = colliderSize;
        waterCollider.center = colliderCenter;
    }
    
    void Update()
    {
        if (!isActive)
        {
            // 水流关闭时也要清理
            playersInWater.Clear();
            playersToBlock.Clear();
            return;
        }
        
        // 水流材质动画
        if (rend != null && rend.material != null)
        {
            textureOffset.y += scrollSpeed * Time.deltaTime;
            rend.material.SetTextureOffset("_MainTex", textureOffset);
        }
        
        // 持续推力
        Vector3 worldFlowDir = transform.TransformDirection(flowDirection);
        foreach (CharacterController cc in playersInWater)
        {
            if (cc != null && cc.enabled)
            {
                cc.Move(worldFlowDir * pushForce * Time.deltaTime);
            }
        }
    }
    
    public void SetWaterActive(bool active)
    {
        isActive = active;
        
        if (rend != null)
            rend.enabled = active;
        
        if (waterCollider != null)
            waterCollider.enabled = active;
        
        if (!active)
        {
            playersInWater.Clear();
            playersToBlock.Clear();
        }
        
        Debug.Log($"[WaterFlow] 水流: {(active ? "开启" : "关闭")}");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            SnowmanController player = other.GetComponent<SnowmanController>();
            CharacterController cc = other.GetComponent<CharacterController>();
            
            if (player != null)
            {
                // 粉雪碰到水 → 变成冰
                if (player.CurrentForm == SnowmanController.SnowmanForm.Powder)
                {
                    player.OnHitByWater();
                    playersToBlock.Add(player);
                    Debug.Log("[WaterFlow] 粉雪碰到水，变成蓝冰");
                }
                // 水形态 → 推力
                else if (player.CurrentForm == SnowmanController.SnowmanForm.Water && cc != null)
                {
                    playersInWater.Add(cc);
                    Debug.Log("[WaterFlow] 水形态进入水流");
                }
                // 蓝冰 → 推力
                else if (player.CurrentForm == SnowmanController.SnowmanForm.Ice && cc != null)
                {
                    playersInWater.Add(cc);
                    Debug.Log("[WaterFlow] 蓝冰进入水流");
                }
            }
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            SnowmanController player = other.GetComponent<SnowmanController>();
            
            if (player != null && player.CurrentForm == SnowmanController.SnowmanForm.Powder)
            {
                // 粉雪持续在水中 → 阻止
                if (!playersToBlock.Contains(player))
                {
                    player.OnHitByWater();
                    playersToBlock.Add(player);
                }
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            SnowmanController player = other.GetComponent<SnowmanController>();
            
            if (cc != null)
                playersInWater.Remove(cc);
            
            if (player != null)
                playersToBlock.Remove(player);
        }
    }
    
    // 在 Scene 视图中显示碰撞体范围
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.3f);
        
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(colliderCenter, colliderSize);
        Gizmos.matrix = oldMatrix;
        
        // 画水流方向箭头
        Gizmos.color = Color.cyan;
        Vector3 worldFlowDir = transform.TransformDirection(flowDirection);
        Gizmos.DrawRay(transform.position + colliderCenter, worldFlowDir * 2f);
    }
}