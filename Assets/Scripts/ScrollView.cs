using UnityEngine;

namespace DragnDrop
{
    public class ScrollView : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Pointer pointer;
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField, Min(0f)] private float scrollSpeed = 1f;
        [SerializeField, Range(0f, 0.1f)] private float smoothing;

        private Vector3 _smoothingVelocity;
        private Vector3 _desiredPosition;


        private void Awake()
        {
            _desiredPosition = cameraTransform.position;
        }


        private void Update()
        {
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
            _desiredPosition.x += -Input.mousePositionDelta.x * Time.deltaTime * scrollSpeed;
            _desiredPosition.x = Mathf.Clamp(_desiredPosition.x, minX, maxX);
        }
    }
}