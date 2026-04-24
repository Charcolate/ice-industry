using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public IronGate gate;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SnowmanController player = other.GetComponent<SnowmanController>();
            if (player != null && gate != null)
            {
                gate.OnPlayerEnter(player, other);
            }
        }
    }
}