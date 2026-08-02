using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EditorJudgeLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Line Position")]
    [SerializeField] private float localYOffset = 0f;
    [SerializeField] private float leftX = -2f;
    [SerializeField] private float rightX = 2f;
    [SerializeField] private float zPosition = 0f;

    [Header("Line Visual")]
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color lineColor = Color.green;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        SetupLineRenderer();
    }

    private void LateUpdate()
    {
        UpdateLinePosition();
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        lineRenderer.sortingOrder = 100;

        if (lineRenderer.material == null)
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void UpdateLinePosition()
    {
        if (targetCamera == null)
            return;

        float judgeY = targetCamera.transform.position.y + localYOffset;

        lineRenderer.SetPosition(0, new Vector3(leftX, judgeY, zPosition));
        lineRenderer.SetPosition(1, new Vector3(rightX, judgeY, zPosition));
    }
}