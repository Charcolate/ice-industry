using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class WaterFlow : MonoBehaviour
{
    [Header("水流设置")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private Vector3 flowDirection = Vector3.down;

    [Header("生成设置")]
    [SerializeField] private WaterStartState startState = WaterStartState.Active;
    [SerializeField] private float spawnDelay = 0f;

    public enum WaterStartState { Active, Inactive, Frozen }

    [Header("视觉 - Renderer")]
    [SerializeField] private Renderer waterRenderer;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material frozenMaterial;
    [SerializeField] private float scrollSpeed = 1f;

    [Header("冻结设置")]
    [SerializeField] private float frozenDuration = 5f;

    [Header("碰撞体设置")]
    [SerializeField] private Vector3 colliderSize = new Vector3(2f, 3f, 1f);
    [SerializeField] private Vector3 colliderCenter = new Vector3(0, 1.5f, 0);

    [Header("冻结触发设置")]
[SerializeField] private List<GameObject> freezeTriggerPrefabs = new List<GameObject>();  // 拖入能冻结水流的预制体

    private bool isActive = true;
    private bool isFrozen = false;
    private float frozenTimer = 0f;

    private Vector2 textureOffset;
    private BoxCollider waterCollider;
    private BoxCollider solidCollider;
    private HashSet<CharacterController> playersInWater = new HashSet<CharacterController>();
    private Keyboard keyboard;

    public bool IsFrozen => isFrozen;
    public bool IsActive => isActive;

    void Start()
    {
        keyboard = Keyboard.current;

        if (waterRenderer == null)
            waterRenderer = GetComponent<Renderer>();

        // 水流触发器
        waterCollider = GetComponent<BoxCollider>();
        if (waterCollider == null)
        {
            waterCollider = gameObject.AddComponent<BoxCollider>();
        }
        waterCollider.isTrigger = true;
        waterCollider.size = colliderSize;
        waterCollider.center = colliderCenter;

        // 冰冻实体碰撞体
        GameObject solidObj = new GameObject("SolidCollider");
        solidObj.transform.SetParent(transform);
        solidObj.transform.localPosition = colliderCenter;
        solidObj.transform.localRotation = Quaternion.identity;
        solidObj.layer = gameObject.layer;
        solidCollider = solidObj.AddComponent<BoxCollider>();
        solidCollider.size = colliderSize;
        solidCollider.isTrigger = false;
        solidCollider.enabled = false;

        // 初始隐藏
        if (waterRenderer != null)
            waterRenderer.enabled = false;
        waterCollider.enabled = false;
        solidCollider.enabled = false;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        if (spawnDelay > 0)
            yield return new WaitForSeconds(spawnDelay);

        switch (startState)
        {
            case WaterStartState.Active:
                SetWaterActive(true);
                break;
            case WaterStartState.Inactive:
                SetWaterActive(false);
                break;
            case WaterStartState.Frozen:
                SetWaterActive(true);
                Freeze();
                break;
        }

        if (waterRenderer != null)
            waterRenderer.enabled = true;
    }

    void Update()
    {
        if (isFrozen)
        {
            frozenTimer -= Time.deltaTime;
            if (frozenTimer <= 0f) Unfreeze();
            return;
        }

        if (!isActive) return;

        // 材质动画
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
                SnowmanController player = cc.GetComponent<SnowmanController>();

                // 水形态玩家按空格时不推，让其跳跃
                if (player != null && player.CurrentForm == SnowmanController.SnowmanForm.Water)
                {
                    if (keyboard != null && keyboard.spaceKey.isPressed)
                    {
                        continue;
                    }
                }

                cc.Move(worldFlowDir * pushForce * Time.deltaTime);
            }
        }
    }

    public void Freeze()
    {
        if (!isActive || isFrozen) return;
        isFrozen = true;
        frozenTimer = frozenDuration;

        if (waterRenderer != null && frozenMaterial != null)
            waterRenderer.material = frozenMaterial;

        waterCollider.enabled = false;
        solidCollider.enabled = true;
        playersInWater.Clear();

        Debug.Log($"[WaterFlow] 被冰冻！持续 {frozenDuration} 秒");
    }

    void Unfreeze()
    {
        isFrozen = false;

        if (waterRenderer != null)
            waterRenderer.material = isActive ? activeMaterial : inactiveMaterial;

        waterCollider.enabled = isActive;
        solidCollider.enabled = false;

        Debug.Log("[WaterFlow] 冰冻解除");
    }

    public void SetWaterActive(bool active)
    {
        if (isFrozen) return;
        isActive = active;

        if (!isFrozen && waterRenderer != null)
            waterRenderer.material = active ? activeMaterial : inactiveMaterial;

        if (waterCollider != null)
            waterCollider.enabled = active;

        if (!active)
            playersInWater.Clear();

        Debug.Log($"[WaterFlow] 水流: {(active ? "开启" : "关闭")}");
    }

    void OnTriggerEnter(Collider other)
{
    if (!isActive || isFrozen) return;

    // 不管三七二十一，只要碰到带 ReflectBullet 脚本的就冻结
    Component[] components = other.GetComponents<Component>();
    foreach (Component comp in components)
    {
        if (comp != null && comp.GetType().Name == "ReflectBullet")
        {
            Freeze();
            Destroy(other.gameObject);
            Debug.Log($"[WaterFlow] 被反击子弹击中，冻结！");
            return;
        }
    }

    // 玩家进入
    if (other.CompareTag("Player"))
    {
        SnowmanController player = other.GetComponent<SnowmanController>();
        CharacterController cc = other.GetComponent<CharacterController>();

        if (player != null && player.CurrentForm == SnowmanController.SnowmanForm.Powder)
        {
            player.OnHitByWater();
            if (cc != null && cc.enabled)
            {
                Vector3 worldFlowDir = transform.TransformDirection(flowDirection);
                cc.Move(worldFlowDir * pushForce * 0.5f);
            }
        }

        if (cc != null)
            playersInWater.Add(cc);
    }
}

bool ShouldFreeze(GameObject obj)
{
    // 直接检查名字
    if (obj.name.ToLower().Contains("reflect") || obj.name.ToLower().Contains("bullet"))
        return true;

    return false;
}

    void OnTriggerStay(Collider other)
    {
        if (!isActive || isFrozen) return;

        if (other.CompareTag("Player"))
        {
            SnowmanController player = other.GetComponent<SnowmanController>();
            CharacterController cc = other.GetComponent<CharacterController>();

            if (player != null && player.CurrentForm == SnowmanController.SnowmanForm.Powder)
            {
                player.OnHitByWater();
            }

            if (cc != null && !playersInWater.Contains(cc))
                playersInWater.Add(cc);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) playersInWater.Remove(cc);
        }
    }

    void OnDrawGizmos()
    {
        Color c;
        if (Application.isPlaying)
            c = isFrozen ? Color.cyan : (isActive ? new Color(0.3f, 0.7f, 1f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.2f));
        else
        {
            switch (startState)
            {
                case WaterStartState.Frozen: c = Color.cyan; break;
                case WaterStartState.Inactive: c = Color.gray; break;
                default: c = new Color(0.3f, 0.7f, 1f, 0.3f); break;
            }
        }
        Gizmos.color = c;
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(colliderCenter, colliderSize);
        Gizmos.matrix = old;
    }
}