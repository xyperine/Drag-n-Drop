using System;
using UnityEngine;

namespace DragnDrop
{
    public class DraggableItem : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.1f)] private float smoothness;
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField, Range(0f, 1f)] private float snappingRange;
        
        private Vector3 _smoothingVelocity;
        
        private Vector3 _snappingPosition;
        private bool _positionFound;


        private void Update()
        {
            if (_positionFound)
            {
                transform.position = Vector3.SmoothDamp(transform.position, _snappingPosition, ref _smoothingVelocity, smoothness);

                if (Vector2.Distance(transform.position, _snappingPosition) < 0.01f)
                {
                    transform.position = _snappingPosition;
                    _positionFound = false;
                }
            }


        }


        private void FixedUpdate()
        {
            // If we are falling perform sweep tests
            if (rigidBody.bodyType == RigidbodyType2D.Dynamic)
            {
                FindSnappingPosition();
            }
        }


        private void FindSnappingPosition()
        {
            Collider2D[] colliders = new Collider2D[8];
            int collidersAmount = Physics2D.OverlapCircle(transform.position, snappingRange, new ContactFilter2D{useTriggers = true}, colliders);
            Collider2D closestCollider = null;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < collidersAmount; i++)
            {
                if (!colliders[i] || colliders[i] == GetComponent<Collider2D>())
                {
                    continue;
                }

                if (Vector2.Distance(transform.position, colliders[i].ClosestPoint(transform.position)) < distance)
                {
                    distance = Vector2.Distance(transform.position, colliders[i].ClosestPoint(transform.position));
                    closestCollider = colliders[i];
                }
            }

            // If no or self we fall
            if (closestCollider == null)
            {
                rigidBody.bodyType = RigidbodyType2D.Dynamic;
                return;
            }
            
            rigidBody.bodyType = RigidbodyType2D.Kinematic;
            rigidBody.linearVelocity = Vector2.zero;
            _positionFound = true;

            // If inside we stay
            if (closestCollider.OverlapPoint(transform.position))
            {
                _snappingPosition = transform.position;
                return;
            }
            
            // If outside we snap to the edge
            _snappingPosition = closestCollider.ClosestPoint(transform.position);
        }


        private void OnMouseDrag()
        {
            transform.position = Vector3.SmoothDamp(transform.position, PointerPosition(), ref _smoothingVelocity, smoothness);
            rigidBody.bodyType = RigidbodyType2D.Kinematic;
            _positionFound = false;
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
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, snappingRange);
            Gizmos.color = Color.white;
        }
    }
}
