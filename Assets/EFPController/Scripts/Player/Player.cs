using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using EFPController.Utils;
using EFPController.Extras;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EFPController
{

    public class Player : MonoBehaviour
    {

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // static
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
        public static bool canControl { get; private set; }
        public static Player instance;
        public static bool inited;
        public static event UnityAction OnPlayerInited;

        private static int controlBlockers = 0;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public LayerMask interactLayerMask = Game.LayerMask.Default;
        [Tooltip("Distance that player can pickup and activate items.")]
        public float interactDistance = 2.1f;
        public float interactCastRadius = 0.05f;

        public GameObject weaponRoot;
        public GameObject cameraRoot;
        public Transform audioSources;
        public Animator cameraAnimator;
        public Camera playerCamera;
        public AudioListener audioListener;
        public PlayerMovement controller;
        public CameraBobAnims cameraBobAnims;
        public SmoothLook smoothLook;
        public CameraRootAnimations cameraRootAnimations;
        public CameraControl cameraControl;
        public PlayerFootsteps footsteps;
        public GameObject playerUIPrefab;
        public float deadlyHeight = -100f;
        public AudioSource effectsAudioSource;
        public float returnToGroundAltitude = -100f;
        [SerializeField]
        public List<Collider> colliders = new List<Collider>();

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public new Rigidbody rigidbody { get; private set; }
        public InputManager inputManager { get; private set; }
        public CapsuleCollider capsule { get; private set; }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // for net sync etc...
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool dead { get; set; }
        public bool hidden { get; set; }
        public float leaning => controller.leanAmt / controller.leanDistance;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    #if UNITY_EDITOR
        protected virtual void Reset()
        {
            FindColliders();
        }

        private void FindColliders()
        {
            colliders.Clear();
            colliders.AddRange(GetComponentsInChildren<Collider>());
        }
    #endif

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            inputManager = InputManager.instance;
        }

        private void Start()
        {
            instance = this;
            Instantiate(playerUIPrefab);
            SetControl(false, true);
            OnPlayerInited?.Invoke();
            cameraBobAnims.PlayIdleAnim();
            cameraControl.SetMainFilter(CameraControl.ScreenEffectProfileType.Gameplay);
            cameraControl.RemoveEffectFilter();
            cameraControl.SetEffectFilter(CameraControl.ScreenEffectProfileType.Fade, 1f, 1f);
            cameraControl.enabled = true;
            controller.speedMult = 1f;
            controller.allowCrouch = true;
            audioSources.SetParent(cameraRoot.transform);
            inited = true;
            this.WaitAndCall(0.5f, () => {
                SetControl(true, true);
                Teleport(transform.position, transform.rotation);
            });
        }

        void Update()
        {
            if (controller.falling && transform.position.y < deadlyHeight)
            {
                //Kill();
                return;
            }

            if (!canControl) return;

            if (transform.position.y < returnToGroundAltitude)
            {
                ReturnToLastGroundPosition();
            }

            Interactable();
        }

        private void Interactable()
        {
            GameObject interactable = null;

            RaycastHit hitUsable;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            Physics.Raycast(ray, out hitUsable, interactDistance, interactLayerMask, QueryTriggerInteraction.Ignore);

            if (hitUsable.collider != null && hitUsable.collider.gameObject.tag == Game.Tags.Interactable)
            {
                interactable = hitUsable.collider.gameObject;
            } else {
                RaycastHit hitUsable2;
                Physics.SphereCast(ray.origin, interactCastRadius, ray.direction, out hitUsable2, interactDistance, interactLayerMask, QueryTriggerInteraction.Ignore);
                if (hitUsable2.collider != null)
                {
                    Vector3 center = hitUsable.collider != null ? GameUtils.GetClosestPointOnLine(hitUsable2.point, ray.origin, hitUsable.point) : 
                        GameUtils.GetClosestPointOnRay(hitUsable2.point, ray.origin, ray.direction);
                    Collider[] usables = Physics.OverlapSphere(center, interactCastRadius, interactLayerMask, QueryTriggerInteraction.Ignore);
                    interactable = usables.Where(x => x.gameObject.tag == Game.Tags.Interactable).
                        OrderBy(x => Vector3.Distance(x.ClosestPoint(center), center)).Select(x => x.gameObject).FirstOrDefault();
                }
            }

            if (interactable != null)
            {
                InteractReceiver interactReceiver = interactable.GetComponent<InteractReceiver>();
                if (interactReceiver != null && interactReceiver.enabled)
                {
                    if (inputManager.interactInputAction.WasReleasedThisFrame())
                    {
                        interactReceiver.InteractRequest();
                    } else {
                        interactReceiver.HoverRequest();
                    }
                }
            }
        }

        public void ReturnToLastGroundPosition()
        {
            Vector3 lastPosOnNavmesh = controller.lastOnGroundPosition;
            if (NavMesh.SamplePosition(lastPosOnNavmesh, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                lastPosOnNavmesh = hit.position;
            }
            lastPosOnNavmesh += Vector3.up * (controller.standingCamHeight + 0.5f);
            cameraControl.SetEffectFilter(CameraControl.ScreenEffectProfileType.Fade, 1f, 0.75f);
            Teleport(lastPosOnNavmesh);
        }

    
        public static void SetControl(bool value, bool force = false)
        {
            controlBlockers = Mathf.Max(value ? (force ? 0 : controlBlockers - 1) : (force ? 1 : controlBlockers + 1), 0);
            canControl = controlBlockers == 0;
            if (!canControl)
            {
                instance.controller.Stop();
                instance.rigidbody.Sleep();
                instance.cameraBobAnims.PlayIdleAnim();
            }
        }

        void Kill()
        {
            // disable player control and sprinting on death
            controller.inputX = 0f;
            controller.inputY = 0f;

            controller.rigidbody.velocity = Vector3.zero;

            cameraBobAnims.PlayIdleAnim();

            SetControl(false, true);
        }

        private void OnDestroy()
        {
            OnPlayerInited = null;
            instance = null;
            inited = false;
        }

        public void Teleport(Vector3 position, Quaternion? rotation = null)
        {
            cameraControl.smooth = false;
            smoothLook.smooth = false;
            controller.falling = false;
            transform.position = position;

            if (rotation != null) Rotate((Quaternion)rotation);

            this.WaitAndCall(1f, () => {
                cameraControl.smooth = true;
                smoothLook.smooth = true;
            });
        }

        public void Rotate(Quaternion rotation)
        {
            Quaternion newRotation = rotation;

            Vector3 playerEulerAngles = new Vector3(0f, newRotation.eulerAngles.y, 0f);
            transform.eulerAngles = playerEulerAngles;

            Vector3 tempEulerAngles2 = new Vector3(0f, newRotation.eulerAngles.y, 0f);

            smoothLook.rotationX = 0f;
            smoothLook.rotationY = 0f;
            smoothLook.rotationZ = 0f;
            smoothLook.recoilY = 0f;
            smoothLook.recoilX = 0f;
            smoothLook.inputY = 0f;

            smoothLook.originalRotation = Quaternion.Euler(tempEulerAngles2);
        }

    }

}