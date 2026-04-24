using UnityEngine;

public class PatrolTurret : MonoBehaviour
{
    [Header("巡逻路线")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTimeAtWaypoint = 1f;
    
    [Header("射击设置")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float detectionRange = 12f;
    
    private Transform player;
    private float fireTimer;
    private float waitTimer;
    private int currentWaypointIndex = 0;
    private bool isMovingForward = true;
    private bool isWaiting = false;
    private bool playerDetected = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("[PatrolTurret] 未设置巡逻点！");
            enabled = false;
            return;
        }
    }
    
    void Update()
    {
        if (player == null || waypoints.Length == 0) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerDetected = distanceToPlayer <= detectionRange;
        
        if (playerDetected)
        {
            // 发现玩家：转向玩家并射击
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            dirToPlayer.y = 0;
            if (dirToPlayer != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dirToPlayer);
            }
            TryFire();
        }
        else
        {
            // 没发现玩家：继续巡逻
            Patrol();
        }
    }
    
    void Patrol()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                MoveToNextWaypoint();
            }
            return;
        }
        
        // 获取当前目标巡逻点
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if (targetWaypoint == null) return;
        
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        direction.y = 0;
        
        // 移动
        if (direction.magnitude > 0.1f)
        {
            transform.position += direction * patrolSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // 检测是否到达（忽略Y轴）
        Vector3 myPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 wayPos = new Vector3(targetWaypoint.position.x, 0, targetWaypoint.position.z);
        float distanceToWaypoint = Vector3.Distance(myPos, wayPos);
        
        if (distanceToWaypoint < 0.5f)
        {
            isWaiting = true;
            waitTimer = 0f;
            Debug.Log($"[PatrolTurret] 到达巡逻点 {currentWaypointIndex}，等待 {waitTimeAtWaypoint} 秒");
        }
    }
    
    void MoveToNextWaypoint()
    {
        if (isMovingForward)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = waypoints.Length - 2;
                isMovingForward = false;
            }
        }
        else
        {
            currentWaypointIndex--;
            if (currentWaypointIndex < 0)
            {
                currentWaypointIndex = 1;
                isMovingForward = true;
            }
        }
        
        Debug.Log($"[PatrolTurret] 前往下一个巡逻点: {currentWaypointIndex}");
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
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
                    if (i > 0 && waypoints[i-1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i-1].position, waypoints[i].position);
                    }
                }
            }
        }
    }
}