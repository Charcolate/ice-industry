using UnityEngine;

public class DroneEnemy : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float hoverHeight = 2f;
    [SerializeField] private float stopDistance = 3f;

    [Header("射击设置")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float bulletSpeed = 6f;
    [SerializeField] private float detectionRange = 15f;

    [Header("浮动效果")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.3f;

    [Header("视觉")]
    [SerializeField] private GameObject droneBody;
    [SerializeField] private GameObject droneEye;

    [Header("激活设置")]
    [SerializeField] private bool requireActivation = true;          // 是否需要激活区域
    [SerializeField] private Collider activationCollider;            // 拖入场景中的触发器 Collider

    private Transform player;
    private float fireTimer;
    private float bobOffset;
    private bool isActivated = false;

    public bool IsActivated => isActivated;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);

        // 如果不需要激活区域，或者没有指定 Collider，则直接激活
        if (!requireActivation || activationCollider == null)
        {
            isActivated = true;
            Debug.Log($"[DroneEnemy] {gameObject.name} 无需激活区域，直接启动");
        }
        else
        {
            Debug.Log($"[DroneEnemy] {gameObject.name} 等待玩家进入激活区域");
        }
    }

    void Update()
    {
        // 未激活时，只检测玩家是否进入激活区域
        if (!isActivated)
        {
            CheckActivation();
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer(distanceToPlayer);
            TryFire();
        }

        ApplyHoverEffect();

        if (droneEye != null && player != null)
            droneEye.transform.LookAt(player.position + Vector3.up * 1f);
    }

    void CheckActivation()
    {
        if (player == null) return;

        // 使用激活 Collider 的包围盒检测玩家是否进入
        if (activationCollider != null && activationCollider.bounds.Contains(player.position))
        {
            Activate();
        }
    }

    void Activate()
    {
        isActivated = true;
        Debug.Log($"[DroneEnemy] {gameObject.name} 被激活！");
    }

    void MoveTowardsPlayer(float distance)
    {
        if (distance <= stopDistance) return;

        Vector3 targetPosition = player.position + Vector3.up * hoverHeight;
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.forward = direction;
    }

    void TryFire()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            Fire();
        }
    }

    void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Vector3 targetPos = player.position + Vector3.up * 1f;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.SetTarget(targetPos);
eb.speed = bulletSpeed;

        }
    }

    void ApplyHoverEffect()
    {
        if (droneBody != null)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobAmount;
            droneBody.transform.localPosition = Vector3.up * bob;
        }
    }

    void OnDrawGizmosSelected()
{
    // 探测范围
    Gizmos.color = new Color(1, 1, 0, 0.3f);
    Gizmos.DrawWireSphere(transform.position, detectionRange);

    // 停止距离
    Gizmos.color = new Color(1, 0, 0, 0.3f);
    Gizmos.DrawWireSphere(transform.position, stopDistance);

    // 激活区域（如果有）
    if (requireActivation && activationCollider != null)
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireCube(activationCollider.bounds.center, activationCollider.bounds.size);
    }
}

}