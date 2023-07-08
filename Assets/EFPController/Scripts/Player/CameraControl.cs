using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EFPController.Utils;

namespace EFPController
{

	public static class CameraAnimNames
	{
		public const string Idle = "Idle";
		public const string Jump = "Jump";
		public const string Land = "Land";
		public const string Climb = "Climb";
	}

	[DefaultExecutionOrder(-12)]
	public class CameraControl : MonoBehaviour
	{

		public enum ScreenEffectProfileType
		{
			None,
			Fade,
			Gameplay,
			Teleport,
			Dash,
		}

		[System.Serializable]
		public class ScreenEffectProfile
		{
			public ScreenEffectProfileType type;
			public PostProcessProfile profile;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public Camera mainCamera;
		public PostProcessVolume mainFilter;
		public PostProcessVolume effectsFilter;
		public List<ScreenEffectProfile> effectProfiles = new List<ScreenEffectProfile>();
		[Tooltip("Speed to smooth the camera angles.")]
		public float camSmoothSpeed = 0.075f;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[HideInInspector]
		public Vector3 CameraAnglesAnim = Vector3.zero; // DONT RENAME! USES IN ANIMATIONS. these values are modified by animations and added to camera angles

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		public bool smooth { get; set; } = true;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// to move gun and view down slightly on contact with ground
		private bool  landState = false;
		private float landStartTime = 0f;
		private float landTime = 0.35f;

		// camera roll angle smoothing amounts
		private float rollAmt;
		private float lookRollAmt;

		private Player player;
		private Transform playerTransform;
		private Animator animator;
		private Quaternion tempCamAngles;
		private float deltaAmt;
		private float returnSpeedAmt = 4f; // speed that camera angles return to neutral
		private Vector3 dampVel;
		private float lerpSpeedAmt;
		private float movingTime; // for controlling delay in lerp of camera position
		private Vector3 targetPos = Vector3.one; // point to orbit camera around in third person mode
		private Vector3 camPos; // camera position
		private Vector3 tempLerpPos = Vector3.one;
		private float dampVelocity;
		private float dampOrg; // smoothed vertical camera postion
		private PlayerMovement controller;
		private CameraBobAnims cameraBobAnims;
		private CameraRootAnimations cameraRootAnimations;

		void Awake()
		{
			mainCamera = GetComponent<Camera>();
			animator = GetComponent<Animator>();
			player = GetComponentInParent<Player>();
			playerTransform = player.transform;
			controller = player.controller;
			cameraBobAnims = player.cameraBobAnims;
			cameraRootAnimations = player.cameraRootAnimations;
		}

		void Start()
		{
			effectsFilter.weight = 0f;
			smooth = true;
		}
	
		public void Update()
		{
			if (!Player.inited || !Player.canControl) return;

			// make sure that animated camera angles zero-out when not playing an animation
			// this is necessary because sometimes the angle amounts did not return to zero
			// which resulted in the gun and camera angles becoming misaligned
			if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f)
			{
				CameraAnglesAnim = Vector3.zero;
			}
			
			// set up camera position with horizontal lean amount
			targetPos = playerTransform.position + (playerTransform.right * controller.leanPos);
			
			// if world has just been recentered, don't lerp camera position to prevent lagging behind player object position
			if (movingTime + 0.75f > Time.time)
			{
				lerpSpeedAmt = 0f;
			} else {
				lerpSpeedAmt = Mathf.MoveTowards(lerpSpeedAmt, camSmoothSpeed, Time.deltaTime); // gradually change lerpSpeedAmt for smoother lerp transition
			}

			// smooth player position before applying bob effects
			if (smooth)
			{
				tempLerpPos = Vector3.SmoothDamp(tempLerpPos, targetPos, ref dampVel, lerpSpeedAmt, Mathf.Infinity, Time.smoothDeltaTime);
			} else {
				tempLerpPos = targetPos;
			}

			// side to side bobbing/moving of camera (stored in the dampOriginX) needs to added to the right vector
			// of the transform so that the bobbing amount is correctly applied along the X and Z axis.
			// If added just to the x axis, like done with the vertical Y axis, the bobbing does not rotate
			// with camera/mouselook and only moves on the world axis.
			if (smooth)
			{
				dampOrg = Mathf.SmoothDamp(dampOrg, controller.midPos, ref dampVelocity, controller.camDampSpeed, Mathf.Infinity, Time.smoothDeltaTime);
			} else {
				dampOrg = controller.midPos;
			}

			camPos = tempLerpPos + (playerTransform.right * (cameraRootAnimations.camPosAnim.x * cameraBobAnims.camPositionBobAmt.x)) 
					+ new Vector3(0f, dampOrg + (cameraRootAnimations.camPosAnim.y * cameraBobAnims.camPositionBobAmt.y), 0f/*cameraRootAnimations.camPosAnim.z*/);
		
			transform.parent.transform.position = camPos;
			transform.position = camPos;

			// initialize camera position/angles quickly before fade out on level load
			if (Time.timeSinceLevelLoad < 0.5f || !smooth)
			{
				returnSpeedAmt = 64f;
			} else {
				if (controller.belowWater)
				{
					returnSpeedAmt = cameraBobAnims.rollReturnSpeedSwim;
				} else {
					returnSpeedAmt = cameraBobAnims.rollReturnSpeed;
				}
			}
				
			// caculate camera roll angle amounts
			if (controller.sprint || controller.dashActive)
			{
				rollAmt = cameraBobAnims.sprintStrafeRoll;	
				// view rolls more with horizontal looking during bullet time for dramatic effect
				lookRollAmt = -1000f * (1f - Time.timeScale);
			} else {
				rollAmt = cameraBobAnims.walkStrafeRoll;
				if (controller.belowWater)
				{
					lookRollAmt = -100f * cameraBobAnims.swimLookRoll;
				} else {
					if (Time.timeScale < 1f)
					{
						// view rolls more with horizontal looking during bullet time for dramatic effect
						lookRollAmt = -500f * (1f - Time.timeScale);
					} else {
						lookRollAmt = -100f * cameraBobAnims.lookRoll;	
					}
				}
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// Camera Angle Assignment
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////			

			// apply a force to the camera that returns it to neutral angles (Quaternion.identity) over time after being changed by code or by animations
			if (smooth)
			{
				transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * returnSpeedAmt);
			} else {
				transform.localRotation = Quaternion.identity;
			}

			deltaAmt = Mathf.Clamp01(Time.deltaTime) * 75f;

			Vector3 CameraAnglesAnimTemp = CameraAnglesAnim;
			// store camera angles in temporary Quaternion and add yaw and pitch from animations 
			tempCamAngles = Quaternion.Euler(transform.localEulerAngles.x - (cameraRootAnimations.camAngleAnim.y * cameraBobAnims.camAngleBobAmt.y * deltaAmt) 
				+ (CameraAnglesAnimTemp.x * deltaAmt), // camera pitch modifiers
				transform.localEulerAngles.y - (cameraRootAnimations.camAngleAnim.x * cameraBobAnims.camAngleBobAmt.x * deltaAmt) 
				+ (CameraAnglesAnimTemp.y * deltaAmt), // camera yaw modifiers
				transform.localEulerAngles.z - (cameraRootAnimations.camAngleAnim.z * cameraBobAnims.camAngleBobAmt.z * deltaAmt) 
				+ (CameraAnglesAnimTemp.z * deltaAmt) // camera roll modifiers 
				- (controller.leanAmt * 3f * Time.deltaTime * returnSpeedAmt) 
				- (controller.inputX * rollAmt * Time.deltaTime * returnSpeedAmt)
				- (cameraBobAnims.side * lookRollAmt * Time.deltaTime * returnSpeedAmt));

			// apply tempCamAngles to camera angles
			transform.localRotation = tempCamAngles;
		
			// Track time that player has landed from jump or fall for gun kicks
			if (controller.fallingDistance < 1.25f && !controller.jumping)
			{
				if (!landState)
				{
					// init timer amount
					landStartTime = Time.time;
					// set landState only once
					landState = true;
				}
			} else {
				if (landState)
				{
					// if land time has elapsed
					if (landStartTime + landTime < Time.time)
					{ 
						// reset landState
						landState = false;
					}
				}
			}
		}

		public void SetMainFilter(ScreenEffectProfileType effectType)
		{
			if (effectType == ScreenEffectProfileType.None) return;
			ScreenEffectProfile effectProfile = effectProfiles.FirstOrDefault(x => x.type == effectType);
			PostProcessProfile postProcProfile = effectProfile.profile;
			mainFilter.profile = postProcProfile;
		}

		private Coroutine effectCoroutine;

		public void SetEffectFilter(ScreenEffectProfileType effectType, float weight, float fadeOutTime, float fadeInTime = 0f, float effectTime = 0f)
		{
			if (effectType == ScreenEffectProfileType.None)
			{
				return;
			}

			ScreenEffectProfile effectProfile = effectProfiles.FirstOrDefault(x => x.type == effectType);

			if (effectProfile == null)
			{
				Debug.LogError("Can't find profile \"" + effectType.ToString() + "\" in effect profiles list");
				return;
			}

			PostProcessProfile postProcProfile = effectProfile.profile;

			if (postProcProfile == null) return;

			if (effectCoroutine != null) StopCoroutine(effectCoroutine);
			effectsFilter.profile = postProcProfile;
			effectsFilter.enabled = true;
			if (fadeInTime > 0f)
			{
				effectsFilter.weight = 0f;
				effectCoroutine = StartCoroutine(EffectFadeInCoroutine(fadeInTime, () => {
					this.WaitAndCall(effectTime, () => {
						effectCoroutine = StartCoroutine(EffectFadeOutCoroutine(fadeOutTime));
					});
				}));
			} else {
				effectsFilter.weight = weight;
				this.WaitAndCall(effectTime, () => {
					effectCoroutine = StartCoroutine(EffectFadeOutCoroutine(fadeOutTime));
				});
			}
		}

		public void RemoveEffectFilter()
		{
			if (effectCoroutine != null) StopCoroutine(effectCoroutine);
			effectsFilter.weight = 0f;
			effectsFilter.enabled = false;
		}

		private IEnumerator EffectFadeInCoroutine(float time, System.Action callback = null)
		{
			for (float i = effectsFilter.weight; i < 1f; i += Time.deltaTime / time)
			{
				effectsFilter.weight = i;
				yield return new WaitForEndOfFrame();
			}
			callback?.Invoke();
		}

		private IEnumerator EffectFadeOutCoroutine(float time)
		{
			for (float i = effectsFilter.weight; i > 0f; i -= Time.deltaTime / time)
			{
				effectsFilter.weight = i;
				yield return new WaitForEndOfFrame();
			}
			RemoveEffectFilter();
		}

	}

}