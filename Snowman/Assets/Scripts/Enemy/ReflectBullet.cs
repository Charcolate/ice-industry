using UnityEngine;

public class ReflectBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public float hitRadius = 2f;

    private Vector3 moveDirection;
    private SnowmanController playerRef;
    private Vector3 lastPosition;

    void Start()
    {
        Destroy(gameObject, lifetime);
        lastPosition = transform.position;
    }

    public void SetDirection(Vector3 direction, float bulletSpeed, SnowmanController player)
    {
        moveDirection = direction.normalized;
        speed = bulletSpeed;
        playerRef = player;
        transform.forward = moveDirection;
    }

    void Update()
    {
        lastPosition = transform.position;
        transform.position += moveDirection * speed * Time.deltaTime;

        // 射线检测
        Vector3 rayDirection = transform.position - lastPosition;
        float distance = rayDirection.magnitude;

        if (distance > 0.01f)
        {
            RaycastHit[] hits = Physics.RaycastAll(lastPosition, rayDirection.normalized, distance);

            foreach (RaycastHit hit in hits)
            {
                if (IsEnemy(hit.collider))
                {
                    DestroyEnemy(hit.collider);
                    return;
                }
            }
        }

        // 距离检测
        CheckAllEnemies();
    }

    bool IsEnemy(Collider col)
    {
        if (col.GetComponent<EnemyTurret>() != null) return true;
        if (col.GetComponent<DroneEnemy>() != null) return true;
        if (col.GetComponent<PatrolTurret>() != null) return true;
        if (col.CompareTag("Enemy")) return true;

        if (col.transform.parent != null)
        {
            Transform parent = col.transform.parent;
            if (parent.GetComponent<EnemyTurret>() != null) return true;
            if (parent.GetComponent<DroneEnemy>() != null) return true;
            if (parent.GetComponent<PatrolTurret>() != null) return true;
            if (parent.CompareTag("Enemy")) return true;
        }

        return false;
    }

    GameObject FindEnemyRoot(Collider col)
    {
        Transform current = col.transform;

        if (current.GetComponent<EnemyTurret>() != null ||
            current.GetComponent<DroneEnemy>() != null ||
            current.GetComponent<PatrolTurret>() != null ||
            current.CompareTag("Enemy"))
        {
            return current.gameObject;
        }

        if (current.parent != null)
        {
            if (current.parent.GetComponent<EnemyTurret>() != null ||
                current.parent.GetComponent<DroneEnemy>() != null ||
                current.parent.GetComponent<PatrolTurret>() != null ||
                current.parent.CompareTag("Enemy"))
            {
                return current.parent.gameObject;
            }
        }

        EnemyTurret[] turrets = col.GetComponentsInChildren<EnemyTurret>();
        if (turrets.Length > 0) return turrets[0].gameObject;

        DroneEnemy[] drones = col.GetComponentsInChildren<DroneEnemy>();
        if (drones.Length > 0) return drones[0].gameObject;

        PatrolTurret[] patrols = col.GetComponentsInChildren<PatrolTurret>();
        if (patrols.Length > 0) return patrols[0].gameObject;

        return null;
    }

    void CheckAllEnemies()
    {
        EnemyTurret[] turrets = FindObjectsByType<EnemyTurret>();
        foreach (EnemyTurret turret in turrets)
        {
            if (turret == null) continue;
            float dist = Vector3.Distance(transform.position, turret.transform.position);
            if (dist < hitRadius)
            {
                DestroyEnemyObject(turret.gameObject);
                return;
            }
        }

        DroneEnemy[] drones = FindObjectsByType<DroneEnemy>();
        foreach (DroneEnemy drone in drones)
        {
            if (drone == null) continue;
            float dist = Vector3.Distance(transform.position, drone.transform.position);
            if (dist < hitRadius)
            {
                DestroyEnemyObject(drone.gameObject);
                return;
            }
        }

        PatrolTurret[] patrols = FindObjectsByType<PatrolTurret>();
        foreach (PatrolTurret patrol in patrols)
        {
            if (patrol == null) continue;
            float dist = Vector3.Distance(transform.position, patrol.transform.position);
            if (dist < hitRadius)
            {
                DestroyEnemyObject(patrol.gameObject);
                return;
            }
        }
    }

    void DestroyEnemy(Collider col)
    {
        GameObject enemyRoot = FindEnemyRoot(col);
        if (enemyRoot != null)
        {
            DestroyEnemyObject(enemyRoot);
        }
    }

    void DestroyEnemyObject(GameObject enemy)
    {
        Debug.Log($"[ReflectBullet] 击中敌人: {enemy.name}");

        if (playerRef != null)
            playerRef.OnReflectHit();

        Destroy(enemy);
        Destroy(gameObject);
    }

    // 只在 Scene 视图中显示（非运行模式）
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}