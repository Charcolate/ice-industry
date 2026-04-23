using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrostVisualManager : MonoBehaviour
{
    [Header("分层物体 - 每层显示不同的 GameObject")]
    [SerializeField] private GameObject frostMistObject;
    [SerializeField] private GameObject frostIceObject;
    [SerializeField] private GameObject frostCrackObject;
    [SerializeField] private GameObject frostShatterObject;
    
    [Header("下雪粒子")]
    [SerializeField] private ParticleSystem snowParticles;
    [SerializeField] private float snowEmissionMin = 5f;
    [SerializeField] private float snowEmissionMax = 50f;
    
    [Header("视野变暗")]
    [SerializeField] private GameObject darkOverlayObject;
    [SerializeField] private float darkMaxAlpha = 0.6f;
    private Image darkOverlayImage;
    
    [Header("玩家身体效果")]
    [SerializeField] private GameObject iceCrystalsOnBody;
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Color frostBodyColor = new Color(0.5f, 0.7f, 1f);
    
    [Header("碎屏动画")]
    [SerializeField] private float shatterDuration = 1.5f;
    
    [Header("引用")]
    [SerializeField] private SnowmanController player;
    
    private int currentStacks = 0;
    private Color originalBodyColor;
    private Coroutine shatterCoroutine;
    private Image shatterImage;
    private Image crackImage;
    
    // 保存每张图片的原始颜色
    private Color originalMistColor = Color.white;
    private Color originalIceColor = Color.white;
    private Color originalCrackColor = Color.white;
    private Color originalShatterColor = Color.white;
    
    private void Start()
    {
        if (player == null)
            player = FindAnyObjectByType<SnowmanController>();
        
        if (bodyRenderer != null)
            originalBodyColor = bodyRenderer.material.color;
        
        if (darkOverlayObject != null)
            darkOverlayImage = darkOverlayObject.GetComponent<Image>();
        
        if (frostShatterObject != null)
            shatterImage = frostShatterObject.GetComponent<Image>();
        
        if (frostCrackObject != null)
            crackImage = frostCrackObject.GetComponent<Image>();
        
        // 保存原始颜色
        SaveOriginalColors();
        
        // 初始化：隐藏所有效果
        HideAllObjects();
        
        if (snowParticles != null)
        {
            var emission = snowParticles.emission;
            emission.rateOverTime = snowEmissionMin;
            snowParticles.Play();
        }
    }
    
    private void SaveOriginalColors()
    {
        if (frostMistObject != null)
        {
            Image img = frostMistObject.GetComponent<Image>();
            if (img != null) originalMistColor = img.color;
        }
        if (frostIceObject != null)
        {
            Image img = frostIceObject.GetComponent<Image>();
            if (img != null) originalIceColor = img.color;
        }
        if (frostCrackObject != null)
        {
            Image img = frostCrackObject.GetComponent<Image>();
            if (img != null) originalCrackColor = img.color;
        }
        if (frostShatterObject != null)
        {
            Image img = frostShatterObject.GetComponent<Image>();
            if (img != null) originalShatterColor = img.color;
        }
    }
    
    private void Update()
    {
        if (player == null) return;
        
        int stacks = player.FrostStacks;
        
        if (stacks != currentStacks)
        {
            currentStacks = stacks;
            UpdateLayerObjects(stacks);
            UpdateSnowParticles(stacks);
            UpdateDarkOverlay(stacks);
            UpdateBodyEffect(stacks);
        }
        
        // 第3层时冰裂脉冲（保持原有透明度结构，只微调）
        if (stacks == 3 && crackImage != null)
        {
            float pulse = Mathf.Sin(Time.time * 3f) * 0.1f + 0.9f;
            Color c = originalCrackColor;
            crackImage.color = new Color(c.r, c.g, c.b, c.a * pulse);
        }
    }
    
    private void HideAllObjects()
    {
        if (frostMistObject != null) frostMistObject.SetActive(false);
        if (frostIceObject != null) frostIceObject.SetActive(false);
        if (frostCrackObject != null) frostCrackObject.SetActive(false);
        if (frostShatterObject != null) frostShatterObject.SetActive(false);
        
        if (darkOverlayObject != null) darkOverlayObject.SetActive(false);
        if (iceCrystalsOnBody != null) iceCrystalsOnBody.SetActive(false);
    }
    
    private void UpdateLayerObjects(int stacks)
    {
        if (shatterCoroutine != null)
        {
            StopCoroutine(shatterCoroutine);
            shatterCoroutine = null;
        }
        
        HideAllObjects();
        
        switch (stacks)
        {
            case 0:
                break;
                
            case 1:
                if (frostMistObject != null)
                {
                    frostMistObject.SetActive(true);
                    ResetImageToOriginal(frostMistObject, originalMistColor);
                }
                break;
                
            case 2:
                if (frostMistObject != null)
                {
                    frostMistObject.SetActive(true);
                    ResetImageToOriginal(frostMistObject, originalMistColor);
                }
                if (frostIceObject != null)
                {
                    frostIceObject.SetActive(true);
                    ResetImageToOriginal(frostIceObject, originalIceColor);
                }
                break;
                
            case 3:
                if (frostMistObject != null)
                {
                    frostMistObject.SetActive(true);
                    ResetImageToOriginal(frostMistObject, originalMistColor);
                }
                if (frostIceObject != null)
                {
                    frostIceObject.SetActive(true);
                    ResetImageToOriginal(frostIceObject, originalIceColor);
                }
                if (frostCrackObject != null)
                {
                    frostCrackObject.SetActive(true);
                    ResetImageToOriginal(frostCrackObject, originalCrackColor);
                }
                break;
                
            case 4:
                if (frostMistObject != null)
                {
                    frostMistObject.SetActive(true);
                    ResetImageToOriginal(frostMistObject, originalMistColor);
                }
                if (frostIceObject != null)
                {
                    frostIceObject.SetActive(true);
                    ResetImageToOriginal(frostIceObject, originalIceColor);
                }
                if (frostCrackObject != null)
                {
                    frostCrackObject.SetActive(true);
                    ResetImageToOriginal(frostCrackObject, originalCrackColor);
                }
                if (frostShatterObject != null)
                {
                    frostShatterObject.SetActive(true);
                    ResetImageToOriginal(frostShatterObject, originalShatterColor);
                    shatterCoroutine = StartCoroutine(ShatterAnimation());
                }
                break;
        }
    }
    
    private void ResetImageToOriginal(GameObject obj, Color originalColor)
    {
        if (obj == null) return;
        Image img = obj.GetComponent<Image>();
        if (img != null)
        {
            img.color = originalColor;
        }
    }
    
    private void UpdateSnowParticles(int stacks)
    {
        if (snowParticles == null) return;
        
        var emission = snowParticles.emission;
        float t = stacks / 4f;
        float rate = Mathf.Lerp(snowEmissionMin, snowEmissionMax, t);
        emission.rateOverTime = rate;
        
        var main = snowParticles.main;
        main.startSize = Mathf.Lerp(0.1f, 0.3f, t);
    }
    
    private void UpdateDarkOverlay(int stacks)
    {
        if (darkOverlayObject == null) return;
        
        float alpha = 0;
        if (stacks >= 2) alpha = darkMaxAlpha * 0.3f;
        if (stacks >= 3) alpha = darkMaxAlpha * 0.6f;
        if (stacks >= 4) alpha = darkMaxAlpha;
        
        if (alpha > 0)
        {
            darkOverlayObject.SetActive(true);
            if (darkOverlayImage != null)
                darkOverlayImage.color = new Color(0, 0, 0, alpha);
        }
        else
        {
            darkOverlayObject.SetActive(false);
        }
    }
    
    private void UpdateBodyEffect(int stacks)
    {
        if (bodyRenderer != null)
        {
            float t = stacks / 4f;
            bodyRenderer.material.color = Color.Lerp(originalBodyColor, frostBodyColor, t);
        }
        
        if (iceCrystalsOnBody != null)
        {
            iceCrystalsOnBody.SetActive(stacks > 0);
            
            if (stacks > 0)
            {
                float scale = 0.5f + (stacks / 4f) * 1f;
                iceCrystalsOnBody.transform.localScale = Vector3.one * scale;
            }
        }
    }
    
    private IEnumerator ShatterAnimation()
    {
        if (shatterImage == null) yield break;
        
        Color originalColor = originalShatterColor;
        float elapsed = 0f;
        
        while (elapsed < shatterDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shatterDuration;
            
            // 只改变透明度，保持原有 RGB
            float alpha = Mathf.Lerp(0f, originalColor.a, t);
            shatterImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            if (darkOverlayObject != null && darkOverlayImage != null)
            {
                darkOverlayObject.SetActive(true);
                darkOverlayImage.color = new Color(0, 0, 0, Mathf.Lerp(darkMaxAlpha * 0.6f, darkMaxAlpha, t));
            }
            
            yield return null;
        }
        
        shatterImage.color = originalColor;
    }
    
    public void ResetAll()
    {
        HideAllObjects();
        currentStacks = 0;
        
        if (snowParticles != null)
        {
            var emission = snowParticles.emission;
            emission.rateOverTime = snowEmissionMin;
        }
        
        if (bodyRenderer != null)
            bodyRenderer.material.color = originalBodyColor;
    }
}