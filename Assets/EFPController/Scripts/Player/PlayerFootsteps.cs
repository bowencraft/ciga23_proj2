using UnityEngine;
using System.Collections;
using EFPController.Utils;

namespace EFPController
{

	public class PlayerFootsteps : MonoBehaviour 
	{

		[System.Serializable]
		public struct SurfaceDefinition 
		{
			public int surfaceIndex;
			public AudioClip[] footsteps;
			public AudioClip[] landing;
		}

		public AudioClip[] waterFootsteps;

		public SurfaceDefinition[] definedSurfaces;

		public bool allowAnimEvent = false;
		public int animationsInBlend = 1;
		public float distanceBetweenStepsWalk = 1.8f;
		public float distanceBetweenStepsSlow = 1.8f;
		public float distanceBetweenStepsSprint = 1.8f;
		public float distanceBetweenStepsDash = 1.8f;
		public int defaultSurfaceIndex = 0;
		[Tooltip("You need an audio source to play a footstep sound.")]
		public AudioSource audioSource;
		[Tooltip("Random volume between this limits")]
		public Vector2 volume = new Vector2(0.25f, 0.5f);
		public float landVolumeMult = 2f;
		public float silentVolumeMult = 0.5f;
		public float sprintVolumeMult = 1.2f;
		public float dashVolumeMult = 1.8f;

		private RaycastHit currentGroundInfo;
		private bool isGrounded;
		private bool previouslyGrounded;
		private bool firstGrounded = false;

		private float stepCycleProgress;
		private float distanceBetweenSteps;

		public bool isCrouch { get; set; } = false;
		public bool isSilent { get; set; } = false;
		public bool isSprint { get; set; } = false;
		public bool isDash { get; set; } = false;

		private bool inited = false;

		private float currentVolume
		{
			get {
				float temp = volume.Random();
				if (isSilent) temp *= silentVolumeMult;
				if (isSprint) temp *= sprintVolumeMult;
				if (isDash) temp *= dashVolumeMult;
				return temp;
			}
		}

		private Player player;

		private void Awake()
		{
			player = GetComponent<Player>();
		}

		IEnumerator Start()
		{
			yield return new WaitWhile(() => Player.instance == null);
			yield return new WaitForSeconds(1f);
			player.controller.OnWater += Controller_OnWater;
			inited = true;
		}

		private void Controller_OnWater(bool inWater)
		{
			if (inWater && player.controller.grounded && !player.controller.falling && !player.controller.jumping)
			{
				stepCycleProgress = 0f;
				PlayFootstep();
			}
		}

		void Update() 
		{
			if (!inited || player.controller.swimming) return;
			isSilent = player.controller.slowWalking;
			isCrouch = player.controller.crouched;
			isSprint = player.controller.sprint;
			isDash = player.controller.dashActive;
			CheckGround();
			if(isGrounded && previouslyGrounded && (player.controller.moving || isDash)) 
			{
				AdvanceStepCycle(player.controller.velocity.magnitude * Time.deltaTime);
			}
		}

		public void Footstep(AnimationEvent evt)
		{
			if (!allowAnimEvent) return;
			if (evt.animatorClipInfo.weight < 1f / animationsInBlend) return;
			if (isGrounded) PlayFootstep();
		}

		void AdvanceStepCycle(float increment) 
		{
			stepCycleProgress += increment;
			float dist = distanceBetweenStepsWalk;
			if (isCrouch) dist = distanceBetweenStepsSlow;
			if (isSprint) dist = distanceBetweenStepsSprint;
			if (isDash) dist = distanceBetweenStepsDash;

			distanceBetweenSteps = Mathf.Lerp(distanceBetweenSteps, dist, 5f * Time.deltaTime);

			if (stepCycleProgress > distanceBetweenSteps) 
			{
				stepCycleProgress = 0f;
				PlayFootstep();
			}
		}

		void PlayFootstep() 
		{
			AudioClip randomFootstep = GetFootstep(currentGroundInfo.collider, currentGroundInfo.point);
			if (randomFootstep) audioSource.PlayOneShot(randomFootstep, currentVolume);
		}

		void PlayLandSound()
		{
			AudioClip randomFootstep = GetFootstep(currentGroundInfo.collider, currentGroundInfo.point);
			if (randomFootstep) audioSource.PlayOneShot(randomFootstep, volume.Random() * landVolumeMult);
		}

		void CheckGround() 
		{
			previouslyGrounded = isGrounded;
			currentGroundInfo = player.controller.groundHitInfo;
			isGrounded = player.controller.grounded && !player.controller.jumping && !player.controller.falling && !player.controller.swimming;
			if (!previouslyGrounded && isGrounded) {
				if (firstGrounded)
				{
					PlayLandSound();
				} else {
					firstGrounded = true;
				}
			}
		}

		public AudioClip GetFootstep(Collider groundCollider, Vector3 worldPosition) 
		{
			if (player.controller.inWater)
			{
				return waterFootsteps.Random();
			}
			int surfaceIndex = SurfaceManager.instance.GetSurfaceIndex(groundCollider, worldPosition);
			if(surfaceIndex == -1 || definedSurfaces.Length <= surfaceIndex) surfaceIndex = defaultSurfaceIndex;
			return definedSurfaces[surfaceIndex].footsteps.Random();
		}

	}

}