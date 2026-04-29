using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 5f;

    private Vector3 moveDirection;
    private bool isInitialized = false;

    public void Initialize(Vector3 targetPos, float bulletSpeed)
    {
        speed = bulletSpeed;
        moveDirection = (targetPos - transform.position).normalized;
        transform.forward = moveDirection;
        isInitialized = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!isInitialized) return;

        Vector3 startPos = transform.position;
        float moveDistance = speed * Time.deltaTime;

        // 射线检测，忽略子弹自己的碰撞体
        int layerMask = ~0; // 所有层，或者可以排除子弹层
        RaycastHit hit;
        if (Physics.Raycast(startPos, moveDirection, out hit, moveDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            // 检查是否碰到玩家
            if (hit.collider.CompareTag("Player"))
            {
                SnowmanController player = hit.collider.GetComponent<SnowmanController>();
                if (player != null)
                {
                    // 水形态：子弹穿过，不销毁
                    if (player.CurrentForm == SnowmanController.SnowmanForm.Water)
                    {
                        // 移动剩余距离，穿过玩家
                        float distanceToPlayer = hit.distance;
                        float remaining = moveDistance - distanceToPlayer;
                        if (remaining > 0)
                        {
                            // 移动到玩家后面继续检测
                            transform.position = hit.point + moveDirection * remaining;
                        }
                        else
                        {
                            transform.position = hit.point;
                        }
                        return; // 不销毁
                    }
                    // 冰形态：反射
                    else if (player.CurrentForm == SnowmanController.SnowmanForm.Ice)
                    {
                        player.SpawnReflectBullet(transform.position);
                    }
                    // 粉雪：伤害
                    else if (player.CurrentForm == SnowmanController.SnowmanForm.Powder)
                    {
                        player.AddFrostStack();
                    }
                }
                Destroy(gameObject); // 非水形态都销毁
            }
            else
            {
                // 碰到墙壁/地面等其他物体，直接销毁
                Destroy(gameObject);
            }
        }
        else
        {
            // 什么都没碰到，正常移动
            transform.position += moveDirection * moveDistance;
        }
    }
}