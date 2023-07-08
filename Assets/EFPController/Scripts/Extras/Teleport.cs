using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EFPController.Utils;

namespace EFPController.Extras
{

    public class Teleport : MonoBehaviour
    {

        public bool playerOnly = true;

        public GameObject target;
        public bool rotateToTargetDir = true;
        public AudioClip outClip;
        public AudioClip inClip;

        public Transform exit;

        [HideInInspector]
        public Collider waitForColliderExit;

        private bool NotSelfGameObject(GameObject go)
        {
            return go != gameObject;
        }

        private bool NotNullGameObject(GameObject go)
        {
            return go != null;
        }

        private void Awake()
        {
            if (!NotNullGameObject(target))
            {
                Debug.LogWarning("Target can't be null", this);
                gameObject.SetActive(false);
                return;
            }
            if (!NotSelfGameObject(target)) Debug.LogError("Target can't be itself", this);
        }

        private void TeleportTo(GameObject go, Transform target)
        {
            go.transform.position = target.transform.position;
            if (rotateToTargetDir) go.transform.rotation = target.transform.rotation;
            DoFXOut();
        }

        private void TeleportTo(Collider collider, Teleport target)
        {
            target.waitForColliderExit = collider;
            TeleportTo(collider.gameObject, target.exit.transform);
        }

        public void DoFXOut()
        {
            if (outClip != null) AudioManager.CreateSFX(outClip, transform.position, rolloffDistanceMax: 10f, volume: 0.5f);
        }

        public void DoFXIn()
        {
            if (inClip != null) AudioManager.CreateSFX(inClip, transform.position, rolloffDistanceMax: 10f, volume: 0.5f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.isStatic || other.attachedRigidbody == null || other.attachedRigidbody.isKinematic == true) return;
            if (waitForColliderExit != null && other == waitForColliderExit) return;
            bool isPlayer = other.gameObject.IsLayer(Game.Layer.Player);
            bool isLocalPlayer = Player.instance.gameObject == other.gameObject;
            if (isPlayer && !isLocalPlayer) return;
            if (playerOnly && !isPlayer) return;
            Teleport targetTeleport = target.GetComponent<Teleport>();
            if (targetTeleport != null)
            {
                if (isPlayer)
                {
                    targetTeleport.waitForColliderExit = other;
                    targetTeleport.DoFXIn();
                    Player.instance.cameraControl.SetEffectFilter(CameraControl.ScreenEffectProfileType.Teleport, 1f, 0.75f);
                    Transform target = targetTeleport.exit == null ? targetTeleport.transform : targetTeleport.exit;
                    if (rotateToTargetDir)
                    {
                        Player.instance.Teleport(target.position, target.rotation);
                    } else {
                        Player.instance.Teleport(target.position);
                    }
                    DoFXOut();
                } else {
                    TeleportTo(other, targetTeleport);
                }
            } else {
                if (isPlayer)
                {
                    AudioManager.CreateSFX(inClip, target.transform.position, rolloffDistanceMax: 10f, volume: 0.5f);
                    Player.instance.cameraControl.SetEffectFilter(CameraControl.ScreenEffectProfileType.Teleport, 1f, 0.75f);
                    if (rotateToTargetDir)
                    {
                        Player.instance.Teleport(target.transform.position, target.transform.rotation);
                    } else {
                        Player.instance.Teleport(target.transform.position);
                    }
                    DoFXOut();
                } else {
                    TeleportTo(other.gameObject, target.transform);
                }
            }        
        }

        private void OnTriggerExit(Collider other)
        {
            if (waitForColliderExit != null && other == waitForColliderExit)
            {
                waitForColliderExit = null;
            }
        }

    }

}