using UnityEngine;

public class IronGate : MonoBehaviour
{
    [Header("碰撞体引用")]
    [SerializeField] private BoxCollider solidCollider;
    [SerializeField] private Collider triggerZone;
    
    void Start()
    {
        if (triggerZone != null)
        {
            GateTrigger gateTrigger = triggerZone.GetComponent<GateTrigger>();
            if (gateTrigger == null)
                gateTrigger = triggerZone.gameObject.AddComponent<GateTrigger>();
            
            gateTrigger.gate = this;
        }
    }
    
    public void OnPlayerEnter(SnowmanController player, Collider playerCollider)
    {
        if (player.CurrentForm == SnowmanController.SnowmanForm.Water)
        {
            // 只忽略实体碰撞体
            if (solidCollider != null)
            {
                Physics.IgnoreCollision(solidCollider, playerCollider, true);
            }
            
            Debug.Log("[IronGate] 水形态穿过栅栏");
            StartCoroutine(RestoreCollision(playerCollider));
        }
    }
    
    System.Collections.IEnumerator RestoreCollision(Collider playerCollider)
    {
        yield return new WaitForSeconds(2f);
        
        if (playerCollider != null && solidCollider != null)
        {
            Physics.IgnoreCollision(solidCollider, playerCollider, false);
        }
    }
}