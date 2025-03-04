using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace DragnDrop
{
    public class DraggableItem : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private LayerMask snappingMask;
        [SerializeField, Range(0f, 0.1f)] private float smoothness = 0.03f;
        [SerializeField, Range(0f, 1f)] private float snappingRange = 0.25f;

        [Header("Animation")]
        [SerializeField, Range(1f, 2f)] private float grabScaleChange = 1.2f;
        [SerializeField, Range(0f, 1f)] private float grabAnimationDurationInSeconds = 0.2f;
        [SerializeField, Range(0f, 0.5f)] private float jumpPower = 0.03f;
        [SerializeField, Range(0, 5)] private int jumpCount = 1;
        [SerializeField, Range(0, 0.5f)] private float jumpAnimationDurationInSeconds = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip grabClip;
        [SerializeField] private AudioClip releaseClip;
        [SerializeField] private bool randomizePitch;
        [SerializeField, ShowIf(nameof(randomizePitch))] private Range pitchRange;

        private Pointer _pointer;

        private float _defaultPitch;

        private Vector3 _smoothingVelocity;
        
        private Vector3 _snappingPosition;
        private bool _snappingPositionFound;


        private void Awake()
        {
            _defaultPitch = audioSource.pitch;
        }


        private void Start()
        {
            _pointer = FindAnyObjectByType<Pointer>();
        }


        private void Update()
        {
            Snap();
        }


        private void Snap()
        {
            if (!_snappingPositionFound)
            {
                return;
            }
            
            transform.position = Vector3.SmoothDamp(transform.position, _snappingPosition, ref _smoothingVelocity, smoothness);

            if (Vector2.Distance(transform.position, _snappingPosition) < 0.01f)
            {
                transform.position = _snappingPosition;
                _snappingPositionFound = false;

                transform.DOJump(transform.position, jumpPower, jumpCount, jumpAnimationDurationInSeconds);
            }
        }


        private void FixedUpdate()
        {
            // If falling try find snapping position
            if (rigidBody.bodyType == RigidbodyType2D.Dynamic)
            {
                FindSnappingPosition();
            }
        }


        private void FindSnappingPosition()
        {
            Collider2D closestCollider = null;
            float distance = float.PositiveInfinity;
            Collider2D[] colliders = new Collider2D[8];
            ContactFilter2D filter = new ContactFilter2D
                {useTriggers = true, useLayerMask = true, layerMask = snappingMask};
            int collidersAmount = Physics2D.OverlapCircle(transform.position, snappingRange, filter, colliders);
            for (int i = 0; i < collidersAmount; i++)
            {
                if (Vector2.Distance(transform.position, colliders[i].ClosestPoint(transform.position)) < distance)
                {
                    distance = Vector2.Distance(transform.position, colliders[i].ClosestPoint(transform.position));
                    closestCollider = colliders[i];
                }
            }

            // If no collider detected nearby - fall down
            if (closestCollider == null)
            {
                rigidBody.bodyType = RigidbodyType2D.Dynamic;
                return;
            }
            
            rigidBody.bodyType = RigidbodyType2D.Kinematic;
            rigidBody.linearVelocity = Vector2.zero;
            _snappingPositionFound = true;

            // If inside a collider - stay
            if (closestCollider.OverlapPoint(transform.position))
            {
                _snappingPosition = transform.position;
                return;
            }
            
            // If outside a collider - snap to its edge
            _snappingPosition = closestCollider.ClosestPoint(transform.position);
        }


        private void OnMouseDown()
        {
            transform.DOScale(grabScaleChange * Vector3.one, grabAnimationDurationInSeconds);
            
            PlaySound(grabClip);
        }


        private void PlaySound(AudioClip clip)
        {
            audioSource.pitch = randomizePitch ? pitchRange.Random() : _defaultPitch;
            audioSource.PlayOneShot(clip);
        }


        private void OnMouseDrag()
        {
            transform.position = Vector3.SmoothDamp(transform.position, PointerPosition(), ref _smoothingVelocity, smoothness);
            rigidBody.bodyType = RigidbodyType2D.Kinematic;
            _snappingPositionFound = false;
            _pointer.Busy = true;
        }


        private Vector3 PointerPosition()
        {
            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = 0f;
            return position;
        }


        private void OnMouseUp()
        {
            FindSnappingPosition();

            _pointer.Busy = false;
            
            transform.DOScale(Vector3.one, grabAnimationDurationInSeconds);
            
            PlaySound(releaseClip);
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, snappingRange);
            Gizmos.color = Color.white;
        }
    }
}
