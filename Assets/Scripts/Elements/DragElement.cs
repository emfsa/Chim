using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class DragElement : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;

    private bool _isDragging = false;
    private Vector3 _offset;

    private void Awake()
    {
        EnsureCameraReference();
    }

    private void Update()
    {
        Drag();
    }

    private void Drag()
    {
        if (Pointer.current == null) return;

        EnsureCameraReference();
        if (_mainCamera == null) return;

        bool isPressed = Pointer.current.press.isPressed;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos);

            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                _isDragging = true;
                _offset = transform.position - mouseWorldPos;
            }
        }

        if (_isDragging && isPressed)
        {
            Vector3 targetPosition = GetMouseWorldPosition() + _offset;
            targetPosition.z = transform.position.z; 
            transform.position = targetPosition;
        }

        if (Pointer.current.press.wasReleasedThisFrame)
        {
            _isDragging = false;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = Pointer.current.position.ReadValue();

        float distanceToCamera = transform.position.z - _mainCamera.transform.position.z;

        Vector3 point = new Vector3(mouseScreenPos.x, mouseScreenPos.y, distanceToCamera);
        return _mainCamera.ScreenToWorldPoint(point);
    }

    private void EnsureCameraReference()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                _mainCamera = FindAnyObjectByType<Camera>();
            }
        }
    }

    public void InitCamera(Camera cam)
    {
        _mainCamera = cam;
    }
}