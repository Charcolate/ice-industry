using UnityEngine;

public class ReflectBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public float hitRadius = 2f;

    private Vector3 direction;
    private SnowmanController playerRef;
    private Vector3 lastPosition;

    void Start()
    {
        Destroy(gameObject, lifetime);
        lastPosition = transform.position;
    }

    public void SetDirection(Vector3 dir, float bulletSpeed, SnowmanController player)
    {
        direction = dir.normalized;
        speed = bulletSpeed;
        playerRef = player;
        transform.forward = direction;
    }

    void Update()
    {
        lastPosition = transform.position;
        transform.position += direction * speed * Time.deltaTime;

        // 射线检测
        Vector3 rayDirection = transform.position - lastPosition;
        float distance = rayDirection.magnitude;
        
        if (distance > 0.01f)
        {
            RaycastHit[] hits = Physics.RaycastAll(lastPosition, rayDirection.normalized, distance);
            
            foreach (RaycastHit hit in hits)
            {
                EnemyTurret turret = hit.collider.GetComponent<EnemyTurret>();
                if (turret == null)
                    turret = hit.collider.GetComponentInParent<EnemyTurret>();
                
                if (turret != null)
                {
                    if (playerRef != null)
                        playerRef.OnReflectHit();
                    
                    Destroy(turret.gameObject);
                    Destroy(gameObject);
                    return;
                }
            }
        }

        // 距离检测 - 新版 API
        EnemyTurret[] allTurrets = Object.FindObjectsByType<EnemyTurret>(FindObjectsInactive.Exclude);
        foreach (EnemyTurret turret in allTurrets)
        {
            if (turret == null) continue;
            
            float dist = Vector3.Distance(transform.position, turret.transform.position);
            if (dist < hitRadius)
            {
                if (playerRef != null)
                    playerRef.OnReflectHit();
                
                Destroy(turret.gameObject);
                Destroy(gameObject);
                return;
            }
        }
    }
}