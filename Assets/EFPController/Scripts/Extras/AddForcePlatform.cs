using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EFPController.Extras
{

    public class AddForcePlatform : MonoBehaviour
    {

        public ForceMode mode;
        [Tooltip("Force vector in local space")]
        public Vector3 force;
        [Tooltip("Add force while player stay in trigger. Otherwise add force once when player enter in trigger")]
        public bool continuous;
        [Tooltip("Add force smoothly. Only for continuous type")]
        public bool smooth;
        public float smoothSpeed = 1f;
        [Tooltip("Add force only if player move direction match to force direction")]
        public bool matchDirection;
        public float matchMaxAngle = 80f;

        public UnityEvent OnAddForce;

        private float currentSpeed = 0f;
        private bool eventFlag = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out PlayerMovement pm))
            {
                eventFlag = false;
                currentSpeed = 0f;
                if (!continuous && (!matchDirection || MatchDir(pm.rigidbody.velocity)))
                {
                    pm.rigidbody.AddForce(transform.TransformVector(force), mode);
                    OnAddForce?.Invoke();
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!continuous) return;
            if (other.gameObject.TryGetComponent(out PlayerMovement pm))
            {
                if (!matchDirection || MatchDir(pm.rigidbody.velocity))
                {
                    currentSpeed = Mathf.MoveTowards(currentSpeed, 1f, smoothSpeed * Time.deltaTime);
                    pm.rigidbody.AddForce(transform.TransformVector(force * currentSpeed), mode);
                    if (!eventFlag)
                    {
                        eventFlag = true;
                        OnAddForce?.Invoke();
                    }
                } else {
                    eventFlag = false;
                    currentSpeed = 0f;
                }
            }
        }

        private bool MatchDir(Vector3 dir)
        {
            return Vector3.Angle(dir.normalized, transform.TransformVector(force).normalized) < matchMaxAngle;
        }

    }

}