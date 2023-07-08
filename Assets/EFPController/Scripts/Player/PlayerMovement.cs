using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using EFPController.Utils;
using EFPController.Extras;

namespace EFPController
{

	[DefaultExecutionOrder(-10)]
	public class PlayerMovement : MonoBehaviour
	{

		public static bool canMove = true;

		public bool debug = false;
		public CapsuleCollider capsule;
		[Tooltip("Mask of layers that player will detect collisions with.")]
		public LayerMask groundMask = Game.LayerMask.Default;

		public float inputLerpSpeed = 90f; // movement lerp speed
		public PhysicMaterial groundedPhysMaterial;
		public PhysicMaterial stayPhysMaterial;
		public PhysicMaterial flyPhysMaterial;
		public float gravityMultiplier = 2f;
		public float gravityMax = 30f;
		public float airSpeedMultiplier = 0.5f;
		[Tooltip("Speed that player moves when walking.")]
		public float walkSpeed = 4f;
		[Tooltip("Percentage to decrease movement speed when strafing diagonally.")]
		public float diagonalStrafeAmt = 0.7071f;
		[Tooltip("Percentage to decrease movement speed when moving backwards.")]
		public float backwardSpeedPercentage = 0.6f; // percentage to decrease movement speed while moving backwards
		[Tooltip("Percentage to decrease movement speed when strafing directly left or right.")]
		public float strafeSpeedPercentage = 0.8f; // percentage to decrease movement speed while strafing directly left or right
		[Tooltip("Angle of ground surface that player won't be allowed to move over."), Range(0f, 90f)]
		public int slopeLimit = 40; // the maximum allowed ground surface/normal angle that the player is allowed to climb
		public AnimationCurve slopeSpeed = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(1f, 0f) } };
		[Tooltip("Maximum height of step that will be climbed.")]
		public float maxStepHeight = 0.8f;
		public float stepHeightRadius = 1.6f;
		[Tooltip("Y position/height of camera when standing.")]
		public float standingCamHeight = 0.89f;
		[Tooltip("Height of player capsule while standing.")]
		public float standingCapsuleHeight = 2f;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Crouching
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Header("Crouching")]
		[Tooltip("True if player should be allowed to crouch.")]
		public bool allowCrouch = true;
		[Tooltip("Percentage to decrease movement speed when crouching.")]
		public float crouchSpeedMult = 0.55f; // percentage to decrease movement speed while crouchin
		public float crouchCooldown = 0.57f;
		[Tooltip("Y position/height of camera when crouched.")]
		public float crouchingCamHeight = 0.45f;
		[Tooltip("Height of player capsule while crouching.")]
		public float crouchCapsuleHeight = 1.25f; // height of capsule while crouching
		public float crouchCapsuleCastHeight = 0.80f; // height of capsule cast above player to check for obstacles before standing from crouch

		// true when player is crouching
		public bool crouched { get; set; }
		public bool crouchRisen { get; set; } // player has risen from crouch postition
		public bool crouchState { get; set; }
		public float playerHeightMod { get; private set; } = 0f; // amount to add to player height (proportionately increases player height, radius, and capsule cast/raycast heights)
	
		private float lastCrouchTime;
		private float crouchCapsuleCheckRadius = 0.5f;


		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Jumping
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Header("Jumping")]
		[Tooltip("True if player should be allowed to jump.")]
		public bool allowJumping = true;
		[Tooltip("Vertical speed of player jump.")]
		public float jumpSpeed = 4f;
		[Tooltip("True if player can Jump while sprinting and not cancel sprint.")]
		public bool jumpCancelsSprint;
		[Tooltip("Time in seconds allowed between player jumps.")]
		public float antiBunnyHopFactor = 0.1f;
	
		public bool jumpfxstate { get; set; } = true;
		public bool jumpBtn { get; set; } = true; // to control jump button behavior
		public bool _jumping { get; set; } // true when player is jumping
		public bool jumping
		{
			get { return _jumping; }
			set {
				_jumping = value;
				if (jumping) OnJump?.Invoke();
			}
		}

		private float jumpTimer = 0f; // track the time player began jump
		private float landStartTime = -999f; // time that player landed from jump

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Sprinting
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public enum sprintType
		{
			Hold = 0,
			Toggle = 1,
			Both = 2
		}

		[Header("Sprinting")]
		[Tooltip("True if player should be allowed to sprint.")]
		public bool allowSprinting = true;
		[Tooltip("Speed that player moves when sprinting.")]
		public float sprintSpeed = 9f;
		[Tooltip("User may set sprint mode to toggle, hold, or both (toggle on sprint button press, hold on sprint button hold).")]
		public sprintType sprintMode = sprintType.Both;
		public bool forwardSprintOnly = true;

		public bool sprintActive { get; set; } = true; // true when player is allowed to sprint
		public bool sprint { get; set; } // true when sprint button is ready
		public float sprintStopTime { get; set; } = 0f; // track when sprinting stopped for control of item pickup time in Player script
		public float sprintEnd { get; set; }

		private float sprintDelay = 0.4f;
		private float sprintStart = -2f;
		private bool sprintEndState;
		private bool sprintStartState;
		private bool sprintStopState = true;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Dash
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Header("Dash")]
		public bool allowDash = false;
		public float dashSpeed = 15f;
		public AnimationCurve dashSpeedCurve = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(1f, 0f) } };
		public float dashTime = 1f;
		public float dashActiveTime = 0.75f;
		public float dashCooldown = 1f;
		public AudioClip[] dashSounds;
		public float dashSoundVolume = 0.5f;

		public bool dashActive { get; private set; }

		private bool dashState = false;
		private float dashCurrentForce;
		private Vector3 dashDirection;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Slow Walking
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Header("Slow Walking")]
		public bool allowSlowWalking = true;
		[Tooltip("Multiplier speed that player moves when slow walking.")]
		public float slowWalkSpeedMult = 0.55f;

		public bool slowWalking { get; set; }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Swimming
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Header("Swimming")]
		[Tooltip("Speed that player moves when swimming.")]
		public float swimSpeed = 4f;
		[Tooltip("Amount of time before player starts drowning.")]
		public float holdBreathDuration = 15f;
		[Tooltip("Rate of damage to player while drowning.")]
		public float drownDamage = 7f;
		public float swimmingCamHeight = -0.2f;
		public float swimmingCapsuleHeight = 1.25f; // height of capsule while crouching
		public AudioSource underwaterAudioSource;
		public AudioSource swimmingAudioSource;
		public AudioClip swimOnSurfaceSound;
		public AudioClip swimUnderSurfaceSound;
		public AudioClip[] fallInWaterSounds;
		public AudioClip fallInDeepWaterSound;
		public AudioClip[] divingInSounds;
		public AudioClip[] divingOutSounds;

		// true when player is touching water collider/trigger
		private bool _inWater;
		public bool inWater { 
			get => _inWater;
			set {
				_inWater = value;
				OnWater?.Invoke(_inWater);
			}
		} 
		public bool holdingBreath { get; set; } // true when player view/camera is under the waterline
		// true when player is below water movement threshold and is not treading water (camera/view slightly above waterline)
		private bool _belowWater;
		public bool belowWater {
			get => _belowWater;
			set {
				_belowWater = value;
				if (swimmingAudioSource != null)
				{
					swimmingAudioSource.clip = _belowWater ? swimUnderSurfaceSound : swimOnSurfaceSound;
				}
			}
		}
		private bool _swimming;
		public bool swimming {
			get => _swimming;
			set {
				_swimming = value;
				if (_swimming)
				{
					rigidbody.useGravity = false;
					sprint = sprintActive = false;
				} else {
					if (swimmingAudioSource != null)
					{
						this.SmoothVolume(swimmingAudioSource, 0f, 0.25f, () => swimmingAudioSource.Pause());
					}
				}
			}
		}

		public bool canWaterJump { get; set; } = true; // to make player release and press jump button again to jump if surfacing from water by holding jump button
		public float swimStartTime { get; set; }
		public float diveStartTime { get; set; }
		public bool drowning { get; set; } // true when player has stayed under water for longer than holdBreathDuration

		private float drownStartTime = 0f;
		private Vector3 playerWaterInitVelocity;
		private float swimmingVerticalSpeed;

		private Collider currentWaterCollider;
		private List<Collider> allWaterColliders = new List<Collider>();
		private float enterWaterTime;
		private bool swimTimeState = false;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Leaning
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Header("Leaning")]
		[Tooltip("True if player should be allowed to lean.")]
		public bool allowLeaning = true;
		[Tooltip("Distance left or right the player can lean.")]
		public float leanDistance = 0.75f;
		[Tooltip("Pecentage the player can lean while standing.")]
		public float standLeanAmt = 1f;
		[Tooltip("Pecentage the player can lean while crouching.")]
		public float crouchLeanAmt = 1f;
		public float rotationLeanAmt = 20f;

		public float leanAmt { get; set; } = 0f;
		public float leanPos { get; set; }
		public bool leanState { get; set; }

		private float leanFactorAmt = 1f;
		private float leanVel;
		private Vector3 leanCheckPos;
		private Vector3 leanCheckPos2;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Climbing
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Header("Climbing")]
		public bool allowClimbing = true;
		[Tooltip("Speed that player moves vertically when climbing.")]
		public float climbingSpeed = 5.0f;
		public float angleToClimb = 45f;
		public float grabbingTime = 0.25f;
		public bool clampClimbingCameraRotate = true;
		public float clampClimbingCameraAngle = 45f;
		public bool rotateClibbingCamera = true;
		public bool climbOnInteract = true;
		public AudioSource climbingAudioSource;

		public enum ClimbingState
		{
			None,
			Grabbing,
			Grabbed,
			Releasing
		}

		public bool climbing => climbingState != ClimbingState.None; // true when playing is in contact with ladder trigger or edge climbing trigger

		private float climbingTargetVolume;
		private Ladder activeLadder;
		private float ladderPathPosition;
		private float ladderPathTargetPosition;
		private Vector3 ladderStartPosition;
		private Vector3 ladderTargetPosition;
		private Quaternion ladderStartRotation;
		private Quaternion ladderTargetRotation;
		private float ladderTime;
		public ClimbingState climbingState { get; set; } = ClimbingState.None;
		public bool climbInteract { get; set; }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Fall Damage
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Header("Fall Damage")]
		public bool allowFallDamage = true;
		[Tooltip("Number of units the player can fall before taking damage.")]
		public float fallDamageThreshold = 5.5f;
		[Tooltip("Multiplier of unit distance fallen to convert to hitpoint damage for the player.")]
		public float fallDamageMultiplier = 2f;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool grounded { get; private set; } // true when capsule cast hits ground surface
		public Vector3 velocity { get; private set; } = Vector3.zero; // total movement velocity vector
		public float airTime { get; private set; } = 0f; // total time that player is airborn
		public float fallStartLevel { get; private set; } // the y coordinate that the player lost grounding and started to fall
		public float fallingDistance { get; private set; } // total distance that player has fallen
		// true when player is losing altitude
		public bool _falling;
		public bool falling {
			get => _falling;
			set {
				_falling = value;
				fallingDistance = 0f;
			}
		}
		public float midPos { get; private set; } = 0.9f; // camera vertical position which is passed to VerticalBob and HorizontalBob
		public bool moving { get; private set; }
		public float speedMult { get; set; } = 1f;
		public float camDampSpeed { get; private set; } // variable damp speed for vertical camera smoothing for stance changes
		public Vector3 camPos { get; private set; }
		public Vector3 eyePos { get; private set; }
		public Vector3 moveDirection { get; private set; } = Vector3.zero; // movement velocity vector, modified by other speed factors like walk, zoom, and crouch states
		public Vector3 moveDirection2D { get; private set; } = Vector3.zero;
		public Vector3 playerBottomPos { get; private set; }
		public Vector3 playerMiddlePos { get; private set; }
		public Vector3 playerTopPos { get; private set; }
		public Vector3 lastOnGroundPosition { get; private set; }
		public RaycastHit groundHitInfo { get; private set; }
		public bool hoverClimbable { get; private set; }

		private bool canStep;
		private float yVelocity;
		private Vector3 playerAirVelocity;
		private float crouchSpeedAmt = 1f;
		private float speedAmtY = 1f; // current player speed per axis which is applied to rigidbody velocity vector
		private float speedAmtX = 1f;
		private float zoomSpeedAmt = 1f;
		private float speed; // combined axis speed of player movement
		private float limitStrafeSpeed = 0f;
		private bool airTimeState;
		private float moveSpeedMult = 1f;
		private float currSpeedMult = 1f;
		private int maxVelocityChange = 5; // maximum rate that player velocity can change
		private Vector3 velocityChange = Vector3.zero;
		private Vector3 futureDirection = Vector3.zero;
		private Vector3 groundNormal;
		private Vector3 groundPoint;
		private float slopeAngle;
		private float lastOnGroundPositionTime;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Player Input
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public float inputXSmoothed { get; private set; } = 0f; // inputs smoothed using lerps
		public float inputYSmoothed { get; private set; } = 0f;
		public float inputX { get; set; } = 0f; // 1 = button pressed 0 = button released
		public float inputY { get; set; } = 0f;

		private float inputYLerpSpeed;
		private float inputXLerpSpeed;

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// References
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public InputManager inputManager { get; set; }
		public new Rigidbody rigidbody { get; set; }
		public Player player { get; set; }
		public Transform mainCameraTransform { get; set; }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Events
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public event System.Action<bool> OnWater;
		public event System.Action OnJump;
		public event System.Action OnLand;
		public event System.Action<bool> OnCrouch;
		public event System.Action<int> OnFallDamage;

		void OnEnable()
		{
			rigidbody = GetComponent<Rigidbody>();
			rigidbody.freezeRotation = true;
		}

		void Start()
		{
			player = GetComponent<Player>();
			mainCameraTransform = player.playerCamera.transform;
			inputManager = InputManager.instance;

			// clamp movement modifier percentages
			Mathf.Clamp01(backwardSpeedPercentage);
			Mathf.Clamp01(crouchSpeedMult);
			Mathf.Clamp01(strafeSpeedPercentage);

			moveSpeedMult = 1f;

			capsule.height = standingCapsuleHeight;

			// add to reach distances if capsule cast height is larger than default
			playerHeightMod = standingCapsuleHeight - 2.24f;

			// initialize player height variables
			crouchCapsuleCheckRadius = capsule.radius * 0.9f;

			// initialize rayCast and capsule cast heights
			crouchCapsuleCastHeight = capsule.height * 0.45f;

			// set sprint mode to toggle, hold, or both, based on inspector setting
			switch (sprintMode)
			{
				case sprintType.Both:
					sprintDelay = 0.4f;
					break;
				case sprintType.Hold:
					sprintDelay = 0f;
					break;
				case sprintType.Toggle:
					sprintDelay = 999f; // time allowed between button down and release to activate toggle
					break;
			}

			dashActiveTime = Mathf.Min(dashActiveTime, dashTime);
		}

		public void Stop()
		{
			dashActive = false;
			moving = false;
			velocity = Vector3.zero;
			velocityChange = Vector3.zero;
			sprint = false;
		}

		void Update()
		{
			if (!Player.canControl) return;

			var t = Time.time;

			crouchCapsuleCheckRadius = capsule.radius * 0.9f;
			// set the vertical bounds of the capsule used to detect player collisions
			Vector3 p1 = transform.position - Vector3.up * ((transform.position.y - 0.1f) - (capsule.bounds.min.y + crouchCapsuleCheckRadius));
			// top point for capsule cast when standing
			Vector3 p2 = p1 + Vector3.up * ((standingCapsuleHeight - 0.1f) - (crouchCapsuleCheckRadius * 2f)); // subtract 2 radii because one is added for p1

			// set crouched variable that other scripts will access to check for crouching
			// do this in Update() instead of FixedUpdate to prevent missed button presses between fixed updates
			if (allowCrouch)
			{
				if (crouched && inputManager.jumpInputAction.WasPerformedThisFrame())
				{
					crouched = false;
					lastCrouchTime = t;
					OnCrouch?.Invoke(false);
				}
				if (inputManager.crouchInputAction.WasPerformedThisFrame() && !swimming && !climbing)
				{
					if (!crouchState)
					{
						if (lastCrouchTime + crouchCooldown < t)
						{
							if (!crouched && crouchRisen)
							{
								camDampSpeed = 0.1f; // faster moving down to crouch (not moving up against gravity)
								crouched = true;
								lastCrouchTime = t;
								sprint = false; // cancel sprint if crouch button is pressed
								OnCrouch?.Invoke(true);
							} else { // only uncrouch if the player has room above them to stand up
								if (!Physics.CheckCapsule(transform.position + transform.up * 0.75f, p2, crouchCapsuleCheckRadius * 0.9f, 
									groundMask.value, QueryTriggerInteraction.Ignore))
								{
									camDampSpeed = 0.2f;
									crouched = false;
									lastCrouchTime = t;
									OnCrouch?.Invoke(false);
								}
							}
							crouchState = true;
						}
					}
				} else {
					crouchState = false;
					if ((sprint || climbing || swimming || dashActive)
						&& !Physics.CheckCapsule(transform.position + transform.up * 0.75f, p2, crouchCapsuleCheckRadius * 0.9f, groundMask.value, QueryTriggerInteraction.Ignore)
					) {
						camDampSpeed = 0.2f;
						crouched = false; // cancel crouch if sprint button is pressed and there is room above the player to stand up
					}
				}
			}

			// determine if player has risen from crouch state
			if (mainCameraTransform.position.y - transform.position.y > (crouchingCamHeight * 0.5f))
			{
				if (!crouchRisen)
				{
					camDampSpeed = 0.1f;
					crouchRisen = true;
				}
			}

			// Determine if player is moving. This var is accessed by other scripts.
			if (Mathf.Abs(inputY) > 0.01f || Mathf.Abs(inputX) > 0.01f || dashActive)
			{
				moving = true;
			} else {
				moving = false;
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// Jumping
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			if (grounded)
			{
				if (jumping)
				{
					// play landing sound effect after landing from jump and reset jumpfxstate
					if (jumpTimer + 0.1f < t)
					{
						player.cameraAnimator.SetTrigger(CameraAnimNames.Land);
						OnLand?.Invoke();

						jumpfxstate = true;
						jumpBtn = false;
						jumping = false; // reset jumping var (this check must be before jumping var is set to true below)
					}
				} else {
					if (allowJumping)
					{
						// determine if player is jumping and set jumping variable
						if (inputManager.jumpInputAction.WasPerformedThisFrame()
							&& jumpBtn // check that jump button is not being held
							&& !belowWater
							&& canWaterJump
							&& landStartTime + antiBunnyHopFactor < t // check for bunnyhop delay before jumping
							&& (slopeAngle < slopeLimit || inWater)
						) { // do not jump if ground normal is greater than slopeLimit and not in water
							if (crouched)
							{
								crouched = false;
								lastCrouchTime = t;
							}

							jumpTimer = t;

							player.cameraAnimator.SetTrigger(CameraAnimNames.Jump);

							if (jumpfxstate)
							{
								// Play Audio jump sfx
								jumpfxstate = false;
							}
							yVelocity = jumpSpeed;
							// apply the jump velocity to the player rigidbody
							rigidbody.AddForce(Vector3.up * Mathf.Sqrt(2f * jumpSpeed * -Physics.gravity.y), ForceMode.VelocityChange);
							jumping = true;
						}
						// reset jumpBtn to prevent continuous jumping while holding jump button.
						if (!inputManager.jumpInputAction.IsPressed() && landStartTime + antiBunnyHopFactor < t)
						{
							jumpBtn = true;
						}
					}
				}
			} else {
				if (allowJumping && swimming && inputManager.jumpInputAction.WasPerformedThisFrame() && !belowWater && canWaterJump)
				{
					jumpTimer = t;
					player.cameraAnimator.SetTrigger(CameraAnimNames.Jump);
					if (jumpfxstate)
					{
						// Play Audio jump sfx
						jumpfxstate = false;
					}
					yVelocity = jumpSpeed;
					// apply the jump velocity to the player rigidbody
					rigidbody.AddForce(Vector3.up * Mathf.Sqrt(2f * jumpSpeed * -Physics.gravity.y), ForceMode.VelocityChange);
					jumping = true;
				}
			}

			if (climbInteract && !inputManager.interactInputAction.IsPressed()) climbInteract = false;

			if (climbing && climbingState != ClimbingState.Grabbed) Climbing();

			if (inWater)
			{
				// check if player is at wading depth in water (water line at chest) after wading into water
				if (capsule.bounds.center.y <= currentWaterCollider.bounds.max.y)
				{
					swimming = true;
					if (!swimTimeState)
					{
						swimStartTime = Time.time; // track time that swimming started
						swimTimeState = true;
					}
					// check if player is at treading water depth (water line at shoulders/neck) after surfacing from dive
					belowWater = camPos.y <= currentWaterCollider.bounds.max.y;
				} else {
					swimming = false;
				}
			
				// check if view height is under water line
				if (camPos.y <= currentWaterCollider.bounds.max.y)
				{
					if (!holdingBreath)
					{
						diveStartTime = Time.time;
						holdingBreath = true;
						if (enterWaterTime + 0.3f > Time.time && fallInDeepWaterSound != null)
						{
							player.effectsAudioSource.PlayOneShot(fallInDeepWaterSound);
						}
						if (underwaterAudioSource != null)
						{
							if (!underwaterAudioSource.isPlaying) underwaterAudioSource.Play();
							underwaterAudioSource.UnPause();
						}
						if (divingInSounds.Length > 0 && enterWaterTime > 1f)
						{
							AudioManager.CreateSFX(divingInSounds.Random(), camPos);
						}
					}
				} else {
					if (holdingBreath && divingOutSounds.Length > 0 && diveStartTime + 0.2f < Time.time)
					{
						AudioManager.CreateSFX(divingOutSounds.Random(), camPos);
					}
					if (underwaterAudioSource != null) underwaterAudioSource.Pause();
					holdingBreath = false;
				}

				if (!swimming && !climbing && grounded)
				{
					rigidbody.useGravity = true;
				}
			}

		}

		private void LateUpdate()
		{
			if (!Player.canControl) return;
			transform.eulerAngles = Vector3.up * player.smoothLook.transform.eulerAngles.y;
/*#if UNITY_2022_1_OR_NEWER
			transform.eulerAngles = Vector3.up * player.smoothLook.transform.eulerAngles.y;
#else
			transform.Rotate(Vector3.up * player.smoothLook.lookInput.x, Space.Self);
#endif*/
		}

		void FixedUpdate()
		{
			if (!Player.canControl) return;

			var t = Time.time;
			var fdt = Time.fixedDeltaTime;
			var dt = Time.deltaTime;

			if (swimming && jumping) jumping = false;

			playerBottomPos = GetPlayerBottomPosition();
			velocity = rigidbody.velocity;
			Vector2 move = inputManager.GetMovementInput();
			inputY = move.y;

			// manage the CapsuleCast size and distance for frontal collision check based on player stance
			// set the vertical bounds of the capsule used to detect player collisions
			playerMiddlePos = transform.position; // middle of player capsule
			playerTopPos = playerMiddlePos + transform.up * (capsule.height * 0.45f); // top of player capsule

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// Player Input
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			if (!swimming)
			{
				inputXLerpSpeed = inputYLerpSpeed = inputLerpSpeed;
			} else {
				inputXLerpSpeed = inputYLerpSpeed = 3f; // player accelerates and decelerates slower in water
			}

			if (sprint)
			{
				// only strafe when sprinting after pressing the joystick horizontally further than 0.4, for better control and less zig-zagging
				if (Mathf.Abs(move.x) > 0.4f)
				{
					inputX = Mathf.Sign(move.x);
				} else {
					inputX = 0f;
				}
			} else {
				// cancel horizontal movement if player is looking up while prone
				inputX = move.x;
			}

			// Smooth our movement inputs using Mathf.Lerp
			inputXSmoothed = Mathf.Lerp(inputXSmoothed, inputX, dt * inputXLerpSpeed);
			inputYSmoothed = Mathf.Lerp(inputYSmoothed, inputY, dt * inputYLerpSpeed);

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// Player Movement Speeds
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			// check that player can run and set speed 
			if (sprintActive)
			{
				// offset speeds by 0.1f to prevent small oscillation of speed value
				if (speed < sprintSpeed - 0.1f)
				{
					speed += 15f * dt; // gradually accelerate to run speed
				} else if (speed > sprintSpeed + 0.1f) {
					speed -= 15f * dt; // gradually decelerate to run speed
				}
			} else {
				if (!swimming)
				{
					if (speed > (walkSpeed * moveSpeedMult) + 0.1f)
					{
						speed -= 15f * dt; // gradually decelerate to walk speed
					} else if (speed < (walkSpeed * moveSpeedMult) - 0.1f) {
						speed += 15f * dt; // gradually accelerate to walk speed
					}
				}
			}

			if (swimming)
			{
				if (speed > swimSpeed + 0.1f)
				{
					speed -= 30f * dt; // gradually decelerate to swim speed
				} else if (speed < swimSpeed - 0.1f) {
					speed += 15f * dt; // gradually accelerate to swim speed
				}
			}

			float speedMod = playerHeightMod / 2.24f + 1f;

			// also lower to crouch position if player dies
			if (swimming)
			{
				midPos = swimmingCamHeight;
				// gradually increase capsule height
				var nh = Mathf.Min(capsule.height + 4f * (dt * speedMod), swimmingCapsuleHeight);
				capsule.height = nh;
			} else if (crouched) {
				midPos = crouchingCamHeight;
				// gradually decrease capsule height
				var nh = Mathf.Max(capsule.height - 4f * (dt * speedMod), crouchCapsuleHeight);
				capsule.height = nh;
			} else {
				midPos = standingCamHeight;
				// gradually increase capsule height
				var nh = Mathf.Min(capsule.height + 4f * (dt * (0.9f * speedMod)), standingCapsuleHeight);
				capsule.height = nh;
			}

			camPos = transform.position + transform.up * midPos;

			// This is the start of the large block that performs all movement actions while grounded	
			if (grounded)
			{
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// Landing
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				// reset airTimeState var so that airTime will only be set once when player looses grounding
				airTimeState = true;

				// reset falling state and perform actions if player has landed from a fall
				if (falling)
				{
					playerAirVelocity = Vector3.zero;
					landStartTime = t; // track the time when player landed

					if ((fallStartLevel - transform.position.y) > 2f)
					{
						player.cameraAnimator.SetTrigger(CameraAnimNames.Land);
						OnLand?.Invoke();
					}

					var dist = fallStartLevel - transform.position.y;

					// track the distance of the fall and apply damage if over falling threshold
					if (allowFallDamage && transform.position.y < fallStartLevel - fallDamageThreshold && !inWater)
					{
						CalculateFallingDamage(dist);
					}

					// active platform
					Collider currentGroundCollider = groundHitInfo.collider;
					if (currentGroundCollider != null)
					{
						Rigidbody groundRB = currentGroundCollider.attachedRigidbody;
						if (groundRB != null && !groundRB.isKinematic)
						{
							groundRB.AddForceAtPosition(Vector3.down * rigidbody.mass, groundPoint, ForceMode.Acceleration);
						}
					}

					falling = false;
				}

				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// Slow Walking
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				if (allowSlowWalking)
				{
					if (inputManager.slowInputAction.IsPressed())
					{
						inputManager.slowInputAction.Reset();
						if (crouched && !slowWalking)
						{
							return;
						}
						slowWalking = !slowWalking;
						if (slowWalking)
						{
							if (sprint) sprint = false;
						}
					}
				}

				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// Sprinting
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				if (allowSprinting)
				{
					// toggle or hold sprinting state by determining if sprint button is pressed or held
					if ((Mathf.Abs(inputY) > 0f && forwardSprintOnly) || (!forwardSprintOnly && (Mathf.Abs(inputX) > 0f) || Mathf.Abs(inputY) > 0f))
					{
						if ((inputManager.sprintInputAction.IsPressed() || inputManager.sprintInputAction.WasPerformedThisFrame()))
						{
							if (!sprintStartState)
							{
								sprintStart = t; // track time that sprint button was pressed
								sprintStartState = true; // perform these actions only once
								sprintEndState = false;
								// if button is tapped, toggle sprint state
								if (sprintEnd - sprintStart < sprintDelay * Time.timeScale)
								{
									if (!sprint)
									{
										// only allow sprint to start or cancel crouch if player is not under obstacle
										if (!sprint)
										{
											sprint = true;
										} else {
											sprint = false; // pressing sprint button again while sprinting stops sprint
										}
									} else {
										sprint = false;
									}
								}
							}
						} else if (!sprintEndState) {
							sprintEnd = t; // track time that sprint button was released
							sprintEndState = true;
							sprintStartState = false;
							// if releasing sprint button after holding it down, stop sprinting
							if (sprintEnd - sprintStart > sprintDelay * Time.timeScale)
							{
								sprint = false;
							}
						}
					} else {
						if (!inputManager.sprintInputAction.IsPressed()) sprint = false;
					}
				}

				if (dashActive) sprint = false;

				// cancel a sprint in certain situations
				if ((inputY <= 0f && Mathf.Abs(inputX) > 0f && forwardSprintOnly) // cancel sprint if player sprints into a wall and strafes left or right
					|| (move.y <= 0f && forwardSprintOnly) // cancel sprint if joystick is released
					|| (inputY < 0f && forwardSprintOnly) // cancel sprint if player moves backwards
					|| (jumping && jumpCancelsSprint)
					|| swimming
					|| climbing // cancel sprint if player runs out of breath
				) {
					sprint = false;
				}

				// determine if player can run
				if (((inputY > 0f && forwardSprintOnly) || (moving && !forwardSprintOnly))
					&& sprint
					&& !crouched
					&& grounded
				) {
					sprintActive = true;
					sprintStopState = true;
				} else {
					if (sprintStopState)
					{
						sprintStopTime = t;
						sprintStopState = false;
					}
					sprintActive = false;
				}

				// check that player can crouch and set speed
				// also check midpos because player can still be under obstacle when crouch button is released 
				if (crouched || midPos < standingCamHeight)
				{
					if (crouchSpeedAmt > crouchSpeedMult)
					{
						crouchSpeedAmt -= dt; // gradually decrease crouchSpeedAmt to crouch limit value
					}
				} else {
					if (crouchSpeedAmt < 1f)
					{
						crouchSpeedAmt += dt; // gradually increase crouchSpeedAmt to neutral
					}
				}

				// limit move speed if backpedaling
				if (inputY >= 0)
				{
					if (speedAmtY < 1f)
					{
						speedAmtY += dt; // gradually increase speedAmtY to neutral
					}
				} else {
					if (speedAmtY > backwardSpeedPercentage)
					{
						speedAmtY -= dt; // gradually decrease speedAmtY to backpedal limit value
					}
				}

				// allow limiting of move speed if strafing directly sideways and not diagonally
				if (inputX == 0f || inputY != 0f)
				{
					if (speedAmtX < 1f)
					{
						speedAmtX += dt; // gradually increase speedAmtX to neutral
					}
				} else {
					if (speedAmtX > strafeSpeedPercentage)
					{
						speedAmtX -= dt; // gradually decrease speedAmtX to strafe limit value
					}
				}
			} else { // Player is airborn
				// keep track of the time that player lost grounding for air manipulation and moving gun while jumping
				if (airTimeState)
				{
					airTime = t;
					airTimeState = false;
				}

				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// Falling
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				// subtract height we began falling from current position to get falling distance
				if (!swimming && !climbing) fallingDistance = fallStartLevel - transform.position.y; // this value referenced in other scripts

				if (!falling && !climbing && !swimming)
				{
					// playerAirInitVelocity = playerAirVelocity = new Vector3(velocity.x, 0f, velocity.z);
					falling = true;
					// start tracking altitude (y position) for fall check
					fallStartLevel = transform.position.y;
				}
			}

			if (grounded || climbing)
			{
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// Leaning
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				if (allowLeaning)
				{
					if (leanDistance > 0f)
					{
						if (Time.timeSinceLevelLoad > 0.5f
							&& !player.dead
							&& !sprint
							&& grounded
							&& Mathf.Abs(move.y) < 0.2f)
						{
							if (inputManager.leanLeftInputAction.IsPressed() && !inputManager.leanRightInputAction.IsPressed()) // lean left
							{
								// set up horizontal capsule cast to check if player would be leaning into an object
								if (!crouched)
								{
									// move up bottom of lean capsule check when standing to allow player to lean over short objects like boxes 
									leanCheckPos = transform.position + Vector3.up * crouchCapsuleCheckRadius;
									leanFactorAmt = standLeanAmt;
								} else {
									leanCheckPos = transform.position;
									leanFactorAmt = crouchLeanAmt;
								}
								// define upper point of capsule (1/2 height minus radius)
								leanCheckPos2 = transform.position + Vector3.up * ((capsule.height / 2f) - crouchCapsuleCheckRadius);
								if (!Physics.CapsuleCast(leanCheckPos, leanCheckPos2, crouchCapsuleCheckRadius * 0.8f, -transform.right, out _, leanDistance * leanFactorAmt, groundMask.value))
								{
									leanAmt = -leanDistance * leanFactorAmt;
									leanState = true;
								} else {
									leanAmt = 0f;
									leanState = false;
								}
							} else if (inputManager.leanRightInputAction.IsPressed() && !inputManager.leanLeftInputAction.IsPressed()) { // lean right
								// set up horizontal capsule cast to check if player would be leaning into an object
								if (!crouched)
								{
									// move up bottom of lean capsule check when standing to allow player to lean over short objects like boxes 
									leanCheckPos = transform.position + Vector3.up * crouchCapsuleCheckRadius;
									leanFactorAmt = standLeanAmt;
								} else {
									leanCheckPos = transform.position;
									leanFactorAmt = crouchLeanAmt;
								}
								// define upper point of capsule (1/2 height minus radius)
								leanCheckPos2 = transform.position + Vector3.up * ((capsule.height / 2f) - crouchCapsuleCheckRadius);
								if (!Physics.CapsuleCast(leanCheckPos, leanCheckPos2, crouchCapsuleCheckRadius * 0.5f, transform.right, out _, leanDistance * leanFactorAmt, groundMask.value))
								{
									leanAmt = leanDistance * leanFactorAmt;
									leanState = true;
								} else {
									leanAmt = 0f;
									leanState = false;
								}
							} else {
								leanAmt = 0f;
								leanState = false;
							}
						} else {
							leanAmt = 0f;
							leanState = false;
						}
						// smooth position between leanAmt values
						leanPos = Mathf.SmoothDamp(leanPos, leanAmt, ref leanVel, 0.1f, Mathf.Infinity, Time.deltaTime);
					}
				} else {
					leanState = false;
					leanAmt = 0f;
				}
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// Swimming
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
			if (holdingBreath)
			{
				// determine if player will gasp for air when surfacing
				if (t - diveStartTime > holdBreathDuration / 1.5f)
				{
					drowning = true;
				}
				// determine if player is drowning
				if (t - diveStartTime > holdBreathDuration)
				{
					if (drownStartTime < t)
					{
						if (debug) Debug.Log("ApplyDamage: " + drownDamage);
						drownStartTime = t + 1.75f;
					}
				}
			} else {
				// play gasping sound if player needed air when surfacing
				if (drowning)
				{
					// Play Audio gasp
					drowning = false;
				}
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// Player Velocity
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			// limit speed if strafing diagonally
			limitStrafeSpeed = (Mathf.Abs(inputX) > 0.5f && Mathf.Abs(inputY) > 0.5f) ? diagonalStrafeAmt : 1f;

			// We are grounded, so recalculate movedirection directly from axes	
			moveDirection = new Vector3(inputXSmoothed * limitStrafeSpeed, 0f, inputYSmoothed * limitStrafeSpeed);

			// realign moveDirection vector to world space
			if (swimming)
			{
				moveDirection = mainCameraTransform.TransformDirection(moveDirection);
			} else {
				moveDirection = transform.TransformDirection(moveDirection);
			}

			CheckGround(moveDirection.normalized);

			if (climbing)
			{
				if (climbingState == ClimbingState.Grabbed) Climbing();
			} else {
				capsule.material = grounded ? (moveDirection.magnitude > 0.1f ? groundedPhysMaterial : stayPhysMaterial) : flyPhysMaterial;
				canStep = CanStep(moveDirection.normalized);

				if (grounded)
				{
					if (!jumping && !swimming) yVelocity = 0f;
					if (!swimming) moveDirection = sprint ? futureDirection : Vector3.Project(moveDirection, futureDirection);
				} else {
					if (yVelocity != gravityMax && !swimming)
					{
						yVelocity = Mathf.Clamp(yVelocity + (Physics.gravity.y * gravityMultiplier) * fdt, -gravityMax, gravityMax);
					}
					if (inWater)
					{
						if (playerWaterInitVelocity.magnitude > 0.1f)
						{
							playerWaterInitVelocity = Vector3.Lerp(playerWaterInitVelocity, Vector3.zero, fdt);
							moveDirection += new Vector3(playerWaterInitVelocity.x, 0f, playerWaterInitVelocity.z) * fdt;
						} else {
							playerWaterInitVelocity = Vector3.zero;
						}
					} else {
						if (moveDirection.magnitude > 0f)
						{
							playerAirVelocity = moveDirection * airSpeedMultiplier;
						} else {
							playerAirVelocity = Vector3.Lerp(playerAirVelocity, Vector3.zero, 5f * fdt);
						}
						moveDirection = playerAirVelocity;
					}
				}

				if (belowWater)
				{
					if (Mathf.Abs(yVelocity) > 0.1f)
					{
						yVelocity = Mathf.Lerp(yVelocity, 0f, 5f * fdt);
					} else {
						yVelocity = 0f;
					}
				}

				currSpeedMult = 1f;

				if (grounded && !swimming && !canStep)
				{
					currSpeedMult = slopeSpeed.Evaluate(slopeAngle / slopeLimit);
					currSpeedMult *= CanMoveToSlope() ? 1f : 0f;
				}

				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// Dash
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				///
				if (allowDash && !dashState && inputManager.dashInputAction.IsPressed() && grounded && !swimming && !falling && !jumping 
					&& moving)
				{
					StopCoroutine(nameof(DashCoroutine));
					StartCoroutine(nameof(DashCoroutine));
				}

				// apply speed limits to moveDirection vector
				float currSpeed = speed * speedAmtX * speedAmtY * crouchSpeedAmt * zoomSpeedAmt * currSpeedMult * speedMult;

				if (slowWalking && !sprint && !crouched) currSpeed *= slowWalkSpeedMult;

				moveDirection = moveDirection * currSpeed;
				moveDirection += Vector3.up * yVelocity;
				moveDirection2D = new Vector3(moveDirection.x, 0f, moveDirection.y);

				// apply a force that attempts to reach target velocity
				velocityChange = moveDirection - velocity;

				// limit max speed
				velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

				// finally, add movement velocity to player rigidbody velocity
				if (!velocityChange.IsZero()) rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// Swimming
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				if (swimming)
				{
					// make player sink under surface for a short time if they jumped in deep water
					if (enterWaterTime + 0.2f > t)
					{
						// dont make player sink if they are close to bottom
						// make sure that player doesn't try to sink into the ground if wading into water
						if (landStartTime + 0.3f > t)
						{
							if (!Physics.CapsuleCast(playerMiddlePos, playerTopPos, crouchCapsuleCheckRadius * 0.9f, -transform.up, out _, crouchCapsuleCastHeight, groundMask.value))
							{
								// make player sink into water after jump
								rigidbody.AddForce(new Vector3(0f, swimmingVerticalSpeed, 0f), ForceMode.VelocityChange);
							}
						}
					} else {
						// make player rise to water surface if they hold the jump button
						if (inputManager.jumpInputAction.IsPressed())
						{
							if (belowWater)
							{
								swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, 3f, dt * 4f);
								if (holdingBreath) canWaterJump = false; // don't also jump if player just surfaced by holding jump button
							} else {
								swimmingVerticalSpeed = 0f;
							}
							// make player dive downwards if they hold the crouch button
						} else if (inputManager.crouchInputAction.IsPressed()) {
							swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, -3f, dt * 4f);
						} else {
							// make player sink slowly when underwater due to the weight of their gear
							if (belowWater)
							{
								swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, -0.1f, dt * 4f);
							} else {
								swimmingVerticalSpeed = 0f;
							}
						}
						// allow jumping when treading water if player has released the jump button after surfacing 
						// by holding jump button down to prevent player from surfacing and immediately jumping
						if (!belowWater && !inputManager.jumpInputAction.IsPressed())
						{
							canWaterJump = true;
						}

						if (swimmingAudioSource != null)
						{
							if (moving)
							{
								if (swimmingAudioSource != null)
								{
									if (!swimmingAudioSource.isPlaying) swimmingAudioSource.Play();
									swimmingAudioSource.UnPause();
									this.SmoothVolume(swimmingAudioSource, 1f, 0.25f);
								}
							} else {
								if (swimmingAudioSource != null) 
								{
									this.SmoothVolume(swimmingAudioSource, 0f, 0.25f, () => swimmingAudioSource.Pause());
								}
							}
						}

						// apply the vertical swimming speed to the player rigidbody
						rigidbody.AddForce(new Vector3(0f, swimmingVerticalSpeed, 0f), ForceMode.VelocityChange);
					}
				}

				if (grounded && Vector3.Angle(groundNormal, Vector3.up) > slopeLimit && rigidbody.velocity.y <= 0f && !belowWater)
				{
					rigidbody.velocity += (transform.up * Physics.gravity.y) * (30f * fdt);
				}

				if (grounded && !swimming && !jumping && groundHitInfo.rigidbody != null && groundHitInfo.rigidbody.isKinematic)
				{
					rigidbody.velocity += groundHitInfo.rigidbody.GetVelocityAtPoint(groundPoint);
				}

				if (dashActive)
				{
					moveDirection = (moveDirection.IsZero() ? dashDirection : moveDirection.normalized) * (currSpeed + dashCurrentForce);
					moveDirection += Vector3.up * yVelocity;
					velocityChange = moveDirection - velocity;
					if (!velocityChange.IsZero()) rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
				}
			}

			hoverClimbable = false;

		}

		private IEnumerator DashCoroutine()
		{
			dashActive = true;
			dashState = true;

			sprint = false;
			slowWalking = false;
			dashDirection = moveDirection.normalized;

			yield return new WaitForEndOfFrame();

			if (dashSounds.Length > 0) player.effectsAudioSource.PlayOneShot(dashSounds.Random(), dashSoundVolume);
			player.cameraControl.SetEffectFilter(CameraControl.ScreenEffectProfileType.Dash, 1f, dashTime - 0.2f, 0.2f);

			float elapsed = 0f;
			float time = dashTime;
			while (elapsed < time)
			{
				float curveTime = elapsed / time;
				dashCurrentForce = dashSpeed * dashSpeedCurve.Evaluate(curveTime);
				elapsed += Time.deltaTime;
				if (elapsed > dashActiveTime) dashActive = false;
				yield return null;
			}

			dashCurrentForce = 0f;
			dashActive = false;

			yield return new WaitForSeconds(dashCooldown);
			dashState = false;
		
		}

		public void CheckGround(Vector3 direction)
		{
			var r = capsule.radius;
			var origin = playerBottomPos + Vector3.up * (r + 0.2f);

			slopeAngle = 0f;
			grounded = false;

			if (belowWater) return;

			if (Physics.SphereCast(origin, r, -transform.up, out RaycastHit hit,
				jumping || falling ? 0.3f : 0.75f, groundMask.value, QueryTriggerInteraction.Ignore))
			{
				grounded = true;
				groundHitInfo = hit;
				groundNormal = hit.normal;
				groundPoint = hit.point;
				if (lastOnGroundPositionTime < Time.time) 
				{
					lastOnGroundPosition = groundPoint;
					lastOnGroundPositionTime = Time.time + 1f;
				}
				CheckSlope(direction);
				if (playerBottomPos.y - hit.point.y > 0.01f && rigidbody.velocity.y <= 0.01f && !belowWater && !climbing)
				{
					Vector3 newForce = 30f * Physics.gravity.y * Time.fixedDeltaTime * transform.up;
					if (!newForce.IsZero()) rigidbody.AddForce(newForce, ForceMode.VelocityChange);
				}
			}
		}

		private void CheckSlope(Vector3 direction)
		{
			futureDirection = direction.magnitude > 0f ? GetMoveDir(direction).normalized : Vector3.zero;
			slopeAngle = Vector3.Angle(futureDirection, transform.up);
			if (direction.magnitude > 0f) slopeAngle = 90f - slopeAngle;
			if (debug) Debug.DrawLine(playerBottomPos, playerBottomPos + futureDirection, Color.blue, 5f);
		}

		public Vector3 GetMoveDir(Vector3 direction)
		{
			float r = capsule.radius * 2f;
			float dist = r + 0.2f;
			Vector3 origin = playerBottomPos + Vector3.up * dist;
			Vector3 newGroundNormal = groundNormal;

			bool groundIsStatic = true;

			if (Physics.SphereCast(origin, r, -transform.up, out RaycastHit hit, dist + 0.5f, groundMask.value, QueryTriggerInteraction.Ignore))
			{
				Rigidbody rb = hit.collider.gameObject.GetComponent<Rigidbody>();
				groundIsStatic = rb == null || rb.isKinematic;
				newGroundNormal = hit.normal;
			}

			if (!groundIsStatic)
			{
				return direction;
			} else if (Vector3.Angle(Vector3.up, newGroundNormal) > 89f) {
				return direction + newGroundNormal;
			}

			Vector3 dir = -Vector3.Cross(direction, newGroundNormal).normalized;
			dir = Vector3.Cross(dir, newGroundNormal).normalized;

			return dir;
		}

		public Vector3 GetPlayerBottomPosition()
		{
			return transform.position + -transform.up * (capsule.height * 0.5f);
		}

		public bool CanMoveToSlope()
		{
			if (swimming) return true;
			if (futureDirection.magnitude > 0f && futureDirection.y <= 0f) return true;
			if (debug) Debug.DrawLine(groundPoint + (Vector3.up * 0.1f), groundPoint + (Vector3.up * 0.1f) + (-Vector3.up * 0.2f), Color.magenta, 10f);
			if (Physics.Raycast(groundPoint + (Vector3.up * 0.1f), -Vector3.up, out RaycastHit rh, 0.2f, groundMask.value, QueryTriggerInteraction.Ignore))
			{
				if (rh.point.y < playerBottomPos.y) return true;
				return Vector3.Angle(rh.normal, Vector3.up) < slopeLimit;
			}
			return true;
		}

		private bool CanStep(Vector3 direction)
		{
			if (swimming) return true;
			if (grounded)
			{
				float radius = capsule.radius;
				float stepDist = radius + radius * stepHeightRadius;
				Vector3 maxStepPos = playerBottomPos + (direction * -0.1f) + (Vector3.up * (maxStepHeight + radius));
				if (!Physics.SphereCast(maxStepPos, radius, direction.normalized, out _, stepDist, groundMask.value, QueryTriggerInteraction.Ignore))
				{
					Vector3 slopeOrigin = maxStepPos + (direction * stepDist);
					if (Physics.Raycast(slopeOrigin, -Vector3.up, out RaycastHit rh, (maxStepPos.y - playerBottomPos.y) * 2f, groundMask.value, QueryTriggerInteraction.Ignore))
					{
						if (Vector3.Angle(rh.normal, Vector3.up) < slopeLimit)
						{
							return true;
						}
					} else {
						return true;
					}
				}
			}
			return false;
		}

		void CalculateFallingDamage(float fallDistance)
		{
			if (fallDamageMultiplier >= 1f)
			{
				int dmg = (int)(fallDistance * fallDamageMultiplier);
				OnFallDamage?.Invoke(dmg);
			}
		}

		public void Climb(Ladder ladder)
		{
			if (climbing) return;
			ladderTime = 0f;
			activeLadder = ladder;
			climbingState = ClimbingState.Grabbing;

			Transform closestPoint = ladder.GetClosestPointByPosition(playerBottomPos);
			Vector3 dir = ladder.transform.up;
			if (closestPoint == ladder.topPoint) dir = -ladder.transform.up;

			ladderStartPosition = transform.position;
			ladderTargetPosition = (activeLadder.ClosestPointOnPath(playerBottomPos, out ladderPathPosition) + dir * 0.4f) + transform.up * (capsule.height * 0.5f);

			ladderStartRotation = player.smoothLook.transform.rotation;
			ladderTargetRotation = closestPoint.rotation;

			rigidbody.useGravity = false;
			if (clampClimbingCameraRotate) player.smoothLook.maxXAngle = clampClimbingCameraAngle;
			if (rotateClibbingCamera) player.smoothLook.enabled = false;

			jumping = false;
			falling = false;
			sprint = sprintActive = false;
			swimming = false;

			if (player.cameraAnimator.HasParameter(CameraAnimNames.Climb))
			{
				player.cameraAnimator.SetTrigger(CameraAnimNames.Climb);
			}

			if (climbingAudioSource != null && ladder.climbSound != null)
			{
				climbingAudioSource.volume = 1f;
				climbingAudioSource.PlayOneShot(ladder.climbSound, Random.Range(0.5f, 1f));
			}

			Stop();
		}

		public void StopClimbing()
		{
			if (climbingState != ClimbingState.Grabbed) return;
			climbingState = ClimbingState.None;
			Unclimb();
		}

		private void Unclimb()
		{
			rigidbody.useGravity = true;
			if (clampClimbingCameraRotate) player.smoothLook.maxXAngle = player.smoothLook.maxXAngleDef;
			if (rotateClibbingCamera) player.smoothLook.enabled = true;
			if (climbingAudioSource != null && activeLadder.climbingSound != null)
			{
				this.SmoothVolume(climbingAudioSource, 0f, 0.5f, () => climbingAudioSource.Stop());
			}
		}

		private void TryClimbing(Ladder ladder)
		{
			Vector3 ladderPos = ladder.centerPoint.position;
			ladderPos.y = 0f;
			Vector3 playerPos = transform.position;
			playerPos.y = 0f;
			Vector3 dirToLadder = (ladderPos - playerPos).normalized;
			Vector3 forwardPlayer = transform.forward;
			float angle = Vector3.Angle(forwardPlayer, dirToLadder);
			if (angle < angleToClimb)
			{
				if (climbOnInteract) hoverClimbable = true;
				if (!climbOnInteract || inputManager.interactInputAction.IsPressed()) Climb(ladder);
			}
		}

		private void Climbing()
		{
			switch (climbingState)
			{
				case ClimbingState.Releasing:
					ladderTime += Time.deltaTime;
					if (ladderTime < grabbingTime)
					{
						transform.position = Vector3.Lerp(ladderStartPosition, ladderTargetPosition, ladderTime / grabbingTime);
					} else {
						rigidbody.isKinematic = false;
						ladderTime = 0f;
						climbingState = ClimbingState.None;
						Unclimb();
					}
					break;
				case ClimbingState.Grabbing:
					ladderTime += Time.deltaTime;
					if (ladderTime < grabbingTime)
					{
						if (rotateClibbingCamera)
						{
							player.smoothLook.transform.rotation = Quaternion.Lerp(ladderStartRotation, ladderTargetRotation, ladderTime / grabbingTime);
						}
						transform.position = Vector3.Lerp(ladderStartPosition, ladderTargetPosition, ladderTime / grabbingTime);
					} else {
						rigidbody.isKinematic = false;
						if (rotateClibbingCamera)
						{
							player.Rotate(ladderTargetRotation);
							player.smoothLook.enabled = true;
						}
						ladderTime = 0f;
						climbingState = ClimbingState.Grabbed;
						if (climbingAudioSource != null && activeLadder.climbingSound != null)
						{
							climbingAudioSource.clip = activeLadder.climbingSound;
							climbingAudioSource.loop = true;
							climbingAudioSource.Play();
							climbingAudioSource.Pause();
							climbingTargetVolume = 0f;
							climbingAudioSource.volume = 0f;
						}
					}
					break;
				case ClimbingState.Grabbed:
					// Get the path position from character's current position
					Vector3 p1 = activeLadder.ClosestPointOnPath(playerBottomPos, out ladderPathPosition);
					Transform closestPoint = activeLadder.GetClosestPointByPosition(playerBottomPos);
					Vector3 p2 = activeLadder.ClosestPointOnPath(closestPoint.position, out ladderPathTargetPosition);
					float dist = Vector3.Distance(p1, p2);
					if (dist > 0.2f)
					{
						// Move the character along the ladder path
						rigidbody.velocity = activeLadder.transform.up * inputY * climbingSpeed * Time.fixedDeltaTime;
						if (climbingAudioSource != null && activeLadder.climbingSound != null)
						{
							if (inputY.IsZero())
							{
								climbingTargetVolume = 0f;
							} else {
								climbingTargetVolume = 1f;
							}
							climbingAudioSource.volume = Mathf.Lerp(climbingAudioSource.volume, climbingTargetVolume, 10f * Time.deltaTime);
							if (climbingAudioSource.volume.IsZero())
							{
								climbingAudioSource.Pause();
							} else {
								climbingAudioSource.UnPause();
							}
						}
					} else {
						// If reached on of the ladder path extremes, change to releasing phase
						ladderTime = 0f;
						climbingState = ClimbingState.Releasing;
						Transform currPoint = activeLadder.GetClosestPointByPosition(playerBottomPos);
						ladderStartPosition = transform.position;
						ladderTargetPosition = currPoint.position + transform.up * (capsule.height * 0.5f);
					}
				
					break;
			}
		}

		private void OnTriggerStay(Collider other)
		{
			if (other.gameObject.CompareTag(Game.Tags.Climbable)) // Climbing
			{
				if (climbingState == ClimbingState.Grabbed)
				{
					if (!climbInteract && (inputManager.interactInputAction.IsPressed() || inputManager.jumpInputAction.IsPressed()))
					{
						climbInteract = true;
						StopClimbing();
					}
				} else if (!climbing && climbOnInteract && !climbInteract) {
					Ladder ladder = other.gameObject.GetComponent<Ladder>();
					if (ladder != null) TryClimbing(ladder);
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (allowClimbing && !climbing && !climbOnInteract && other.gameObject.CompareTag(Game.Tags.Climbable)) // Climbing
			{
				Ladder ladder = other.gameObject.GetComponent<Ladder>();
				if (ladder != null) TryClimbing(ladder);
			} else if (other.gameObject.IsLayer(Game.Layer.Water)) { // Water
				if (!allWaterColliders.Contains(other)) allWaterColliders.Add(other);

				if (falling && fallingDistance > 1f && !inWater)
				{
					if (fallInWaterSounds.Length > 0) AudioManager.CreateSFX(fallInWaterSounds.Random(), transform.position);
				}

				enterWaterTime = Time.time;
				currentWaterCollider = other;
				inWater = true;
				falling = false;
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.IsLayer(Game.Layer.Water)) // Water
			{
				if (allWaterColliders.Contains(other)) allWaterColliders.Remove(other);
				if (other == currentWaterCollider)
				{
					if (allWaterColliders.Count > 0)
					{
						currentWaterCollider = allWaterColliders.First();
					} else {
						swimTimeState = false;
						inWater = false;
						swimming = false;
						belowWater = false;
						canWaterJump = true;
						holdingBreath = false;
						if (underwaterAudioSource != null) underwaterAudioSource.Pause();
					}
				}
			}
		}

	}

}