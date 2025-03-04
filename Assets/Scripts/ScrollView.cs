using UnityEngine;

namespace DragnDrop
{
    public class ScrollView : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Pointer pointer;
        
        [Header("Borders")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private Vector2 referenceResolution = new Vector2(16f, 9f);
        
        [Header("Scrolling")]
        [SerializeField, Min(0f)] private float scrollSpeed = 1f;
        [SerializeField, Range(0f, 0.1f)] private float smoothing;

        private Vector3 _smoothingVelocity;
        private Vector3 _desiredPosition;

        private Camera _camera;
        private float adjustedMinX;
        private float adjustedMaxX;


        private void Awake()
        {
            _desiredPosition = cameraTransform.position;

            _camera = cameraTransform.GetComponent<Camera>();

            AdjustBorders();
        }


        private void AdjustBorders()
        {
            float defaultHalfWidth = referenceResolution.x / referenceResolution.y * _camera.orthographicSize;
            float halfWidth = _camera.aspect * _camera.orthographicSize;
            float difference = defaultHalfWidth - halfWidth;
            adjustedMinX = minX - difference;
            adjustedMaxX = maxX + difference;
        }


        private void Update()
        {
#if UNITY_EDITOR
            AdjustBorders();
#endif
            CalculateDesiredPosition();
            
            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, _desiredPosition, ref _smoothingVelocity, smoothing);
        }


        private void CalculateDesiredPosition()
        {
            if (pointer.Busy)
            {
                return;
            }
#if UNITY_EDITOR
            if (!Input.GetMouseButton(0))
            {
                return;
            }
#else
            if (Input.touchCount <= 0)
            {
                return;
            }

            if (Input.GetTouch(0).phase != TouchPhase.Moved)
            {
                return;
            }
#endif

            _desiredPosition = cameraTransform.position;
            float deltaX = -Input.mousePositionDelta.x / Screen.width;
            _desiredPosition.x += deltaX * scrollSpeed;
            _desiredPosition.x = Mathf.Clamp(_desiredPosition.x, adjustedMinX, adjustedMaxX);
        }
    }
}