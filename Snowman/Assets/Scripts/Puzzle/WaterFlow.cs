using UnityEngine;
using System.Collections.Generic;

public class WaterFlow : MonoBehaviour
{
    [Header("水流设置")]
    [SerializeField] private float pushForce = 8f;
    [SerializeField] private Vector3 flowDirection = Vector3.down;
    
    [Header("视觉 - Renderer")]
    [SerializeField] private Renderer waterRenderer;        // 水流 Renderer
    [SerializeField] private Material activeMaterial;       // 水流开启时材质
    [SerializeField] private Material inactiveMaterial;     // 水流关闭时材质
    [SerializeField] private float scrollSpeed = 1f;
    
    [Header("碰撞体设置")]
    [SerializeField] private Vector3 colliderSize = new Vector3(2f, 3f, 1f);
    [SerializeField] private Vector3 colliderCenter = new Vector3(0, 1.5f, 0);
    
    private bool isActive = true;
    private Vector2 textureOffset;
    private BoxCollider waterCollider;
    private HashSet<CharacterController> playersInWater = new HashSet<CharacterController>();
    private HashSet<SnowmanController> playersToBlock = new HashSet<SnowmanController>();
    
    void Start()
    {
        if (waterRenderer == null)
            waterRenderer = GetComponent<Renderer>();
        
        if (waterRenderer != null && activeMaterial != null)
        {
            waterRenderer.material = activeMaterial;
        }
        
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
            playersInWater.Clear();
            playersToBlock.Clear();
            return;
        }
        
        // 材质动画（仅激活时滚动）
        if (waterRenderer != null && waterRenderer.material != null)
        {
            textureOffset.y += scrollSpeed * Time.deltaTime;
            waterRenderer.material.SetTextureOffset("_MainTex", textureOffset);
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
        
        // 切换材质
        if (waterRenderer != null)
        {
            waterRenderer.material = active ? activeMaterial : inactiveMaterial;
        }
        
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
            
            if (player != null && player.CurrentForm == SnowmanController.SnowmanForm.Powder)
            {
                player.OnHitByWater();
                playersToBlock.Add(player);
            }
            else if (cc != null)
            {
                playersInWater.Add(cc);
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
    
    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? new Color(0.3f, 0.7f, 1f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);
        
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(colliderCenter, colliderSize);
        Gizmos.matrix = oldMatrix;
    }
}