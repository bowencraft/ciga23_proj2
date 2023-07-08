using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EFPController
{

    public class PlayerCameraBob : MonoBehaviour
    {

        [Tooltip("Frequency at which the weapon will move around in the screen when the player is in movement")]
        public float bobFrequency = 9f;
        [Tooltip("How fast the weapon bob is applied, the bigger value the fastest")]
        public float bobSharpness = 10f;
        [Tooltip("Distance the weapon bobs when not aiming")]
        public float bobAmount = 0.03f;
        public float bobFactor { get; set; }

        public PlayerMovement playerController;

        private Vector3 offset;
        private Vector3 newPos;

        private void Start()
        {
            offset = transform.localPosition;
        }

        private void Update()
        {
            Bob();
        }

        private void Bob()
        {
            float characterMovementFactor = 0f;

            if (playerController.grounded)
            {
                characterMovementFactor = Mathf.Clamp01(playerController.velocity.magnitude / playerController.sprintSpeed);
            }

            bobFactor = Mathf.Lerp(bobFactor, characterMovementFactor, bobSharpness * Time.deltaTime);

            float hBobValue = Mathf.Sin(Time.time * bobFrequency) * bobAmount * bobFactor;
            float vBobValue = ((Mathf.Sin(Time.time * bobFrequency * 2f) * 0.5f) + 0.5f) * bobAmount * bobFactor;

            newPos = new Vector3(hBobValue, Mathf.Abs(vBobValue), 0f);

            transform.localPosition = offset + newPos;
        }

    }

}