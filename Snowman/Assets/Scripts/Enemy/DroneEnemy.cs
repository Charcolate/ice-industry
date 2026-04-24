using UnityEngine;

public class DroneEnemy : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float hoverHeight = 2f;
    [SerializeField] private float stopDistance = 3f;        // 靠近玩家多远时停下
    
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
    [SerializeField] private GameObject droneEye;               // 眼睛始终看向玩家
    
    private Transform player;
    private float fireTimer;
    private Vector3 startPosition;
    private float bobOffset;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPosition = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 只在探测范围内活动
        if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer(distanceToPlayer);
            TryFire(distanceToPlayer);
        }
        
        // 浮动效果
        ApplyHoverEffect();
        
        // 眼睛看向玩家
        if (droneEye != null && player != null)
        {
            droneEye.transform.LookAt(player.position + Vector3.up * 1f);
        }
    }
    
    void MoveTowardsPlayer(float distance)
    {
        if (distance <= stopDistance) return;
        
        Vector3 targetPosition = player.position + Vector3.up * hoverHeight;
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.forward = direction;
    }
    
    void TryFire(float distance)
    {
        if (distance > detectionRange) return;
        
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 停止距离
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}