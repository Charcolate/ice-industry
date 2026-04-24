using UnityEngine;
using System.Collections;

public class PlayerRespawnManager : MonoBehaviour
{
    [Header("重生设置")]
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private GameObject deathEffectPrefab;
    
    [Header("引用")]
    [SerializeField] private SnowmanController playerController;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FrostVisualManager frostVisualManager;
    
    private Vector3 currentCheckpoint;
    private Quaternion currentCheckpointRotation;
    private Vector3 defaultSpawnPoint;
    private Quaternion defaultSpawnRotation;
    private bool isRespawning = false;
    
    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<SnowmanController>();
        
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        
        if (frostVisualManager == null)
            frostVisualManager = FindAnyObjectByType<FrostVisualManager>();
        
        defaultSpawnPoint = transform.position;
        defaultSpawnRotation = transform.rotation;
        currentCheckpoint = defaultSpawnPoint;
        currentCheckpointRotation = defaultSpawnRotation;
    }
    
    void Update()
    {
        if (!isRespawning && playerController != null && !playerController.enabled)
        {
            StartCoroutine(RespawnPlayer());
        }
    }
    
    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        currentCheckpoint = position;
        currentCheckpointRotation = rotation;
    }
    
    IEnumerator RespawnPlayer()
    {
        isRespawning = true;
        
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(respawnDelay);
        
        characterController.enabled = false;
        transform.position = currentCheckpoint;
        transform.rotation = currentCheckpointRotation;
        characterController.enabled = true;
        
        playerController.enabled = true;
        playerController.ClearFrostStacks();
        
        if (frostVisualManager != null)
        {
            frostVisualManager.ResetAll();
        }
        
        isRespawning = false;
    }
    
    public void ForceRespawn()
    {
        if (!isRespawning)
        {
            playerController.enabled = false;
            StartCoroutine(RespawnPlayer());
        }
    }
}