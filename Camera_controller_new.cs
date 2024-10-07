using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform modelContainer;
    public float orbitSpeed = 300f;
    public float panSpeed = 0.01f;
    public float zoomSpeed = 0.5f;
    public float smoothness = 10f;
    public float minZoomDistance = 0.1f;
    public float maxZoomDistance = 1000f;
    public float maxPanDistance = 10f;

    private float currentX = 0f;
    private float currentY = 0f;
    private Vector3 lastMousePosition;
    private float targetZoomFactor = 0f;
    private float currentZoomFactor = 0f;
    private Bounds modelBounds;
    private Quaternion targetRotation;
    private Vector3 pivotPoint;
    private Vector3 panOffset;
    private Vector3 targetPanOffset;

    void Start()
    {
        ModelBoundsCalculator boundsCalculator = modelContainer.GetComponent<ModelBoundsCalculator>();
        if (boundsCalculator == null)
        {
            boundsCalculator = modelContainer.gameObject.AddComponent<ModelBoundsCalculator>();
        }
        modelBounds = boundsCalculator.CalculateBounds();

        pivotPoint = modelBounds.center;
        float initialDistance = modelBounds.extents.magnitude * 2f;
        transform.position = pivotPoint - transform.forward * initialDistance;
        targetRotation = transform.rotation;

        currentZoomFactor = targetZoomFactor = Mathf.Log(initialDistance);
        panOffset = targetPanOffset = Vector3.zero;
    }

    void LateUpdate()
    {
        if (modelContainer == null)
            return;

        // Orbit (Left mouse button)
        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            currentX += delta.x * orbitSpeed * Time.deltaTime;
            currentY -= delta.y * orbitSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, -80f, 80f);
            targetRotation = Quaternion.Euler(currentY, currentX, 0);
        }

        // Pan (Right mouse button or Middle mouse button)
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 panDirection = (transform.right * -delta.x + transform.up * -delta.y) * panSpeed;
            targetPanOffset += panDirection;

            // Constrain pan distance
            targetPanOffset = Vector3.ClampMagnitude(targetPanOffset, maxPanDistance);
        }

        // Zoom
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            targetZoomFactor -= scrollDelta * zoomSpeed;
            targetZoomFactor = Mathf.Clamp(targetZoomFactor, Mathf.Log(minZoomDistance), Mathf.Log(maxZoomDistance));
        }

        // Smooth interpolation
        currentZoomFactor = Mathf.Lerp(currentZoomFactor, targetZoomFactor, Time.deltaTime * smoothness);
        float currentDistance = Mathf.Exp(currentZoomFactor);
        panOffset = Vector3.Lerp(panOffset, targetPanOffset, Time.deltaTime * smoothness);

        // Calculate camera position based on pivot point, pan offset, and zoom
        Vector3 targetCameraPosition = pivotPoint + panOffset - targetRotation * Vector3.forward * currentDistance;
        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, Time.deltaTime * smoothness);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothness);

        // Ensure the camera is always looking at the pivot point plus pan offset
        transform.LookAt(pivotPoint + panOffset);

        lastMousePosition = Input.mousePosition;
    }
}