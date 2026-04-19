using UnityEngine;
using System.Collections.Generic;

public class IceTrailManager : MonoBehaviour
{
    [Header("轨迹设置")]
    public float pointSpacing = 0.5f;
    public float trailLifetime = 10f;
    
    [Header("视觉设置")]
    public float trailWidth = 0.08f;
    public Color trailColor = new Color(0.7f, 0.9f, 1f, 0.6f);
    
    private List<TrailPoint> currentTrail = new List<TrailPoint>();
    private LineRenderer currentLineRenderer;
    private Vector3 lastPointPosition;
    private bool isDrawing = false;
    
    [System.Serializable]
    public class TrailPoint
    {
        public Vector3 position;
        public float timestamp;
        
        public TrailPoint(Vector3 pos, float time)
        {
            position = pos;
            timestamp = time;
        }
    }
    
    void Start()
    {
    CreateLineRenderer();
    
    // 强制测试：直接开始绘制
    isDrawing = true;
    currentTrail.Clear();
    lastPointPosition = transform.position;
    currentTrail.Add(new TrailPoint(transform.position, Time.time));
    Debug.Log("强制开始绘制，isDrawing = " + isDrawing);
    }
    
    void Update()
    {
        CleanupOldPoints();
        UpdateLineRenderer();
        CheckDrawingState();
    }
    
    void CreateLineRenderer()
    {
    GameObject lineObj = new GameObject("CurrentTrail");
    lineObj.transform.parent = null;
    currentLineRenderer = lineObj.AddComponent<LineRenderer>();
    
    currentLineRenderer.startWidth = trailWidth;
    currentLineRenderer.endWidth = trailWidth;
    
    // 尝试多种材质，哪个不粉用哪个
    Material mat = null;
    
    // 方案1：Unlit/Color（最不容易出问题）
    mat = new Material(Shader.Find("Unlit/Color"));
    if (mat != null)
    {
        mat.color = trailColor;
    }
    
    // 如果还不行，用 Legacy Shaders/Transparent/Diffuse
    if (mat == null || mat.shader == null)
    {
        mat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        if (mat != null)
        {
            mat.color = trailColor;
        }
    }
    
    // 最后备选：直接改颜色属性
    currentLineRenderer.material = mat;
    currentLineRenderer.startColor = trailColor;
    currentLineRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0.3f);
    currentLineRenderer.useWorldSpace = true;
    currentLineRenderer.positionCount = 0;
    currentLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    currentLineRenderer.receiveShadows = false;
    }
    
    void CheckDrawingState()
    {
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb == null) return;
    
    bool isMoving = rb.linearVelocity.magnitude > 0.1f;
    
    // 每帧都输出状态
    Debug.Log($"速度: {rb.linearVelocity.magnitude:F2}, isMoving: {isMoving}, isDrawing: {isDrawing}");
    
    if (isMoving && !isDrawing)
    {
        Debug.Log(">>> 开始绘制轨迹 <<<");
        StartDrawing();
    }
    else if (!isMoving && isDrawing)
    {
        Debug.Log(">>> 停止绘制轨迹 <<<");
        StopDrawing();
    }
    
    if (isDrawing)
    {
        AddTrailPoint();
        // 确认点在增加
        Debug.Log($"当前轨迹点数: {currentTrail.Count}");
    }
    }
    
    void StartDrawing()
    {
        isDrawing = true;
        currentTrail.Clear();
        
        // 重新创建 LineRenderer 开始新轨迹
        if (currentLineRenderer != null)
            Destroy(currentLineRenderer.gameObject);
            
        CreateLineRenderer();
        
        lastPointPosition = transform.position;
        currentTrail.Add(new TrailPoint(transform.position, Time.time));
    }
    
    void StopDrawing()
    {
        isDrawing = false;
        // 轨迹保留，等待自然过期
    }
    
    void AddTrailPoint()
    {
        float distance = Vector3.Distance(transform.position, lastPointPosition);
        
        if (distance >= pointSpacing)
        {
            currentTrail.Add(new TrailPoint(transform.position, Time.time));
            lastPointPosition = transform.position;
        }
    }
    
    void CleanupOldPoints()
    {
        float currentTime = Time.time;
        currentTrail.RemoveAll(p => currentTime - p.timestamp > trailLifetime);
        
        if (currentTrail.Count < 2 && currentLineRenderer != null)
        {
            currentLineRenderer.positionCount = 0;
        }
    }
    
    void UpdateLineRenderer()
    {
        if (currentLineRenderer == null || currentTrail.Count < 2)
            return;
            
        currentLineRenderer.positionCount = currentTrail.Count;
        for (int i = 0; i < currentTrail.Count; i++)
        {
            // 让轨迹稍微抬高一点，避免和冰面重叠闪烁
            Vector3 pos = currentTrail[i].position + Vector3.up * 0.5f;
            currentLineRenderer.SetPosition(i, pos);
        }
    }
    
    public List<TrailPoint> GetCurrentTrail()
    {
        return new List<TrailPoint>(currentTrail);
    }
    
    public void ClearAllTrails()
    {
        currentTrail.Clear();
        if (currentLineRenderer != null)
        {
            currentLineRenderer.positionCount = 0;
        }
    }
    
    public bool IsDrawing()
    {
        return isDrawing;
    }
}