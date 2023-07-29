using UnityEngine;
using System.Collections;

public class FastMenuRotateWithCamera : MonoBehaviour
{
    [SerializeField] private float _headOffset = 0.3f;
    [SerializeField] private float _movingDuration = 1f;

    public float RigDistance { get; set; } = 1.2f;

    private Vector3 oldPos;
    private Quaternion newRotation;
    private Vector3 newPosition;

    private IEnumerator coroutine;
    private bool windowMoving;

    private void Start()
    {
        coroutine = LerpWindow(transform.position, transform.rotation);
    }

    private void OnEnable()
    {
        OnAlignWindow();
    }

    private void OnDisable()
    {
        windowMoving = false;
    }

    void LateUpdate()
    {
        Transform rigCamera = Camera.main.transform;
        newRotation = Quaternion.Euler(rigCamera.rotation.eulerAngles.x, rigCamera.rotation.eulerAngles.y, 0f);
        newPosition = rigCamera.position + rigCamera.forward * RigDistance;

        // Get distance between head and window
        var distanceFromHead = Vector3.Distance(transform.position, newPosition);

        if (rigCamera.parent.position != oldPos)
        {
            // Stop lerp coroutine
            windowMoving = false;

            // Player moving - show always in front without lerp
            transform.rotation = newRotation;
            transform.position = newPosition;
        }
        else if ((distanceFromHead > _headOffset) && !windowMoving)
        {
            // Lerp window when turn head or change distance between head and window
            coroutine = LerpWindow(newPosition, newRotation);
            StartCoroutine(coroutine);
        }

        oldPos = rigCamera.parent.position;
    }
    private IEnumerator LerpWindow(Vector3 targetPosition, Quaternion targetRotation)
    {
        windowMoving = true;
        float k = 1f;

        while (Vector3.Distance(transform.position, newPosition) > 0.01f)
        {
            // Increase move speed when close to target
            k = -100f / 9f * Vector3.Distance(transform.position, newPosition) + 28f / 9f;
            if (k < 1f)
                k = 1f;

            transform.position = Vector3.Lerp(transform.position, newPosition, _movingDuration * k * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, _movingDuration * k * Time.deltaTime);

            yield return null;
        }

        windowMoving = false;
    }

    private void OnAlignWindow()
    {
        var rigCamera = Camera.main.transform;
        var cameraProjection = new Vector3(rigCamera.position.x, transform.position.y, rigCamera.position.z);
        var cameraVector = Vector3.ProjectOnPlane(rigCamera.forward, Vector3.up).normalized;
        var newPosition = cameraProjection + cameraVector * RigDistance;

        transform.rotation = new Quaternion(0f, rigCamera.rotation.y, 0f, rigCamera.rotation.w);
        transform.position = new Vector3(newPosition.x, rigCamera.position.y, newPosition.z);
    }
}
