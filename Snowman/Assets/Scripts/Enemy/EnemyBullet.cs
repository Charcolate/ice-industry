using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 5f;

    private Vector3 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetTarget(Vector3 targetPos)
    {
        direction = (targetPos - transform.position).normalized;
        transform.forward = direction;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SnowmanController player = other.GetComponent<SnowmanController>();
            if (player != null)
            {
                if (player.CurrentForm == SnowmanController.SnowmanForm.Ice)
                {
                    player.SpawnReflectBullet(transform.position);
                }
                else if (player.CurrentForm == SnowmanController.SnowmanForm.Powder)
                {
                    player.AddFrostStack();
                }
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}