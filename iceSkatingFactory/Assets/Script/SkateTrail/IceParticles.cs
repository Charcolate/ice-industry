using UnityEngine;

public class IceParticles : MonoBehaviour
{
    public ParticleSystem iceDust;
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (iceDust == null)
            iceDust = GetComponentInChildren<ParticleSystem>();
    }
    
    void Update()
    {
        if (iceDust == null) return;
        
        float speed = rb.linearVelocity.magnitude;
        
        if (speed > 2f)
        {
            if (!iceDust.isPlaying)
                iceDust.Play();
                
            // 速度越快粒子越多
            var emission = iceDust.emission;
            emission.rateOverTime = Mathf.Lerp(5, 30, speed / 12f);
        }
        else
        {
            if (iceDust.isPlaying)
                iceDust.Stop();
        }
    }
}