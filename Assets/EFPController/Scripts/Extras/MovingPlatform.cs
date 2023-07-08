using UnityEngine;
using EFPController.Utils;

namespace EFPController.Extras
{

    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {

        public float speed;
        public Vector3 moveOffset;
        public Vector3 rotate;

        private Vector3 startPosition;
        private Vector3 targetPosition;

        private Vector3 position;

        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            startPosition = transform.position;
            targetPosition = startPosition + moveOffset;
        }

        void FixedUpdate()
        {
            if (speed > 0f && moveOffset.sqrMagnitude > 0f)
            {
                float moveTime = Vector3.Distance(startPosition, targetPosition) / Mathf.Max(speed, 0.0001f);
                float t = GameUtils.EaseInOut(Mathf.PingPong(Time.time, moveTime), moveTime);
                position = Vector3.Lerp(startPosition, targetPosition, t);
                rb.MovePosition(position);
            }
            if (rotate.sqrMagnitude > 0f)
            {
                Quaternion deltaRotation = Quaternion.Euler(rotate * Time.fixedDeltaTime);
                rb.MoveRotation(rb.rotation * deltaRotation);
            }
        }

    }

}