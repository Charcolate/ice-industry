using UnityEngine;

public class ReflectBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public float hitRadius = 2f;

    private Vector3 moveDirection;
    private SnowmanController playerRef;
    private Vector3 lastPosition;
    private bool hasHit = false;

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
        if (hasHit) return;

        lastPosition = transform.position;
        transform.position += moveDirection * speed * Time.deltaTime;

        // 射线检测本帧移动路径
        Vector3 rayDir = transform.position - lastPosition;
        float distance = rayDir.magnitude;
        if (distance > 0)
        {
            RaycastHit[] hits = Physics.RaycastAll(lastPosition, rayDir.normalized, distance);
            foreach (RaycastHit hit in hits)
            {
                if (IsEnemy(hit.collider))
                {
                    DestroyEnemy(hit.collider);
                    return;
                }
            }
        }

        // 距离检测所有敌人（保底措施）
        CheckAllEnemies();
        // 水流检测
        CheckWaterFlows();
    }

    bool IsEnemy(Collider col)
    {
        if (col.GetComponent<EnemyTurret>() != null) return true;
        if (col.GetComponent<DroneEnemy>() != null) return true;
        if (col.GetComponent<PatrolTurret>() != null) return true;
        if (col.CompareTag("Enemy")) return true;
        // 检查父级
        if (col.transform.parent != null)
        {
            Transform p = col.transform.parent;
            if (p.GetComponent<EnemyTurret>() != null) return true;
            if (p.GetComponent<DroneEnemy>() != null) return true;
            if (p.GetComponent<PatrolTurret>() != null) return true;
            if (p.CompareTag("Enemy")) return true;
        }
        return false;
    }

    void DestroyEnemy(Collider col)
    {
        // 找到敌人根对象
        GameObject enemy = null;
        if (col.GetComponent<EnemyTurret>() != null) enemy = col.gameObject;
        else if (col.GetComponent<DroneEnemy>() != null) enemy = col.gameObject;
        else if (col.GetComponent<PatrolTurret>() != null) enemy = col.gameObject;
        else if (col.CompareTag("Enemy")) enemy = col.gameObject;
        else if (col.transform.parent != null)
        {
            Transform p = col.transform.parent;
            if (p.GetComponent<EnemyTurret>() != null) enemy = p.gameObject;
            else if (p.GetComponent<DroneEnemy>() != null) enemy = p.gameObject;
            else if (p.GetComponent<PatrolTurret>() != null) enemy = p.gameObject;
            else if (p.CompareTag("Enemy")) enemy = p.gameObject;
        }

        if (enemy != null)
        {
            hasHit = true;
            Debug.Log("[ReflectBullet] 摧毁敌人: " + enemy.name);
            if (playerRef != null) playerRef.OnReflectHit();
            Destroy(enemy);
            Destroy(gameObject);
        }
    }

    void CheckAllEnemies()
    {
        EnemyTurret[] turrets = FindObjectsByType<EnemyTurret>();
        foreach (var t in turrets)
        {
            if (t != null && Vector3.Distance(transform.position, t.transform.position) < hitRadius)
            {
                hasHit = true;
                if (playerRef != null) playerRef.OnReflectHit();
                Destroy(t.gameObject);
                Destroy(gameObject);
                return;
            }
        }
        DroneEnemy[] drones = FindObjectsByType<DroneEnemy>();
        foreach (var d in drones)
        {
            if (d != null && Vector3.Distance(transform.position, d.transform.position) < hitRadius)
            {
                hasHit = true;
                if (playerRef != null) playerRef.OnReflectHit();
                Destroy(d.gameObject);
                Destroy(gameObject);
                return;
            }
        }
        PatrolTurret[] patrols = FindObjectsByType<PatrolTurret>();
        foreach (var p in patrols)
        {
            if (p != null && Vector3.Distance(transform.position, p.transform.position) < hitRadius)
            {
                hasHit = true;
                if (playerRef != null) playerRef.OnReflectHit();
                Destroy(p.gameObject);
                Destroy(gameObject);
                return;
            }
        }
    }

    void CheckWaterFlows()
{
    WaterFlow[] waterFlows = FindObjectsByType<WaterFlow>();
    foreach (WaterFlow wf in waterFlows)
    {
        if (wf != null && !wf.IsFrozen && wf.IsActive)
        {
            float dist = Vector3.Distance(transform.position, wf.transform.position);
            if (dist < hitRadius)
            {
                wf.Freeze();
                Destroy(gameObject);
                return;
            }
        }
    }
}


    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}