using UnityEngine;

public class PatrolTurret : MonoBehaviour
{
    [Header("巡逻路线")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTimeAtWaypoint = 1f;
    [SerializeField] private float rotationSpeed = 2f;      // 旋转速度限制
    
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
    private Vector3 currentMoveDirection = Vector3.forward;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("[PatrolTurret] 未设置巡逻点！");
            enabled = false;
            return;
        }
        
        // 面向第一个巡逻点
        if (waypoints[0] != null)
        {
            Vector3 dir = (waypoints[0].position - transform.position).normalized;
            dir.y = 0;
            if (dir.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
                currentMoveDirection = dir;
            }
        }
        
        Debug.Log($"[PatrolTurret] 初始化完成");
    }
    
    void Update()
    {
        if (player == null || waypoints.Length == 0) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerDetected = distanceToPlayer <= detectionRange;
        
        if (playerDetected)
        {
            isWaiting = false;
            waitTimer = 0f;
            
            // 平滑转向玩家
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            dirToPlayer.y = 0;
            if (dirToPlayer.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
            TryFire();
        }
        else
        {
            Patrol();
        }
    }
    
    void Patrol()
    {
        if (waypoints.Length == 0 || currentWaypointIndex >= waypoints.Length) return;
        if (waypoints[currentWaypointIndex] == null) return;
        
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextWaypoint();
            }
            return;
        }
        
        Vector3 targetPos = waypoints[currentWaypointIndex].position;
        targetPos.y = transform.position.y;
        
        Vector3 direction = targetPos - transform.position;
        direction.y = 0;
        float distance = direction.magnitude;
        
        if (distance < 0.5f)
        {
            isWaiting = true;
            waitTimer = 0f;
            Debug.Log($"[PatrolTurret] 到达巡逻点 {currentWaypointIndex}");
        }
        else
        {
            direction.Normalize();
            
            // 平滑转向（关键修复）
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            
            // 移动
            transform.position += transform.forward * patrolSpeed * Time.deltaTime;
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
        
        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Length - 1);
        Debug.Log($"[PatrolTurret] 前往巡逻点 {currentWaypointIndex}");
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
            eb.Initialize(targetPos, bulletSpeed);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (waypoints != null && waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
                    
                    if (i > 0 && waypoints[i-1] != null)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(waypoints[i-1].position, waypoints[i].position);
                    }
                }
            }
        }
    }
}