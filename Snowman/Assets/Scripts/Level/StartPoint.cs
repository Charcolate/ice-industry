using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [Header("视觉 - Renderer")]
    [SerializeField] private Renderer startRenderer;
    [SerializeField] private Material activeMaterial;
    
    //[Header("粒子")]
    //[SerializeField] private ParticleSystem activateParticles;
    
    void Start()
    {
        // 初始材质
        if (startRenderer != null && activeMaterial != null)
            startRenderer.material = activeMaterial;
        
        // 把玩家传送到起点
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            
            player.transform.position = transform.position;
            player.transform.rotation = transform.rotation;
            
            if (cc != null) cc.enabled = true;
            
            PlayerRespawnManager respawn = player.GetComponent<PlayerRespawnManager>();
            if (respawn != null)
            {
                respawn.SetCheckpoint(transform.position, transform.rotation);
            }
        }
        
        //if (activateParticles != null)
            //activateParticles.Play();
    }
}