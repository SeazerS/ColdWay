using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSpeed = 1.0f;
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.1f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 90.0f;
        public float BottomClamp = -90.0f;

        private float _cinemachineTargetPitch;
        public static FirstPersonController Instance;
        public bool CanLook = true;

        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private float _footstepTimer = 0f;
        [Header("Audio Settings")]
        public float WalkStepRate = 0.5f;
        public float SprintStepRate = 0.3f;

        public KeyCode forwardKey = KeyCode.W;
        public KeyCode backwardKey = KeyCode.S;
        public KeyCode leftKey = KeyCode.A;
        public KeyCode rightKey = KeyCode.D;
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;


        // DİNAMİK YÜRÜME SESİ DEĞİŞKENİ (Artık Raycast bunu kendi güncelleyecek)
        [HideInInspector]
        public string currentFootstepSound = "Yurume_Sesi";

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            Instance = this;
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies.");
#endif
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            LoadKeys();

            RotationSpeed = PlayerPrefs.GetFloat("MouseSensitivity", 2.0f);
        }

        public void LoadKeys()
        {
            forwardKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Ileri", "W"));
            backwardKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Geri", "S"));
            leftKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Sol", "A"));
            rightKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Sag", "D"));
            jumpKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Ziplama", "Space"));
            sprintKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Kosma", "LeftShift"));
        }

        private void Update()
        {
            Vector2 customMove = Vector2.zero;
            if (Input.GetKey(rightKey)) customMove.x += 1f;
            if (Input.GetKey(leftKey)) customMove.x -= 1f;
            if (Input.GetKey(forwardKey)) customMove.y += 1f;
            if (Input.GetKey(backwardKey)) customMove.y -= 1f;

            _input.move = customMove.normalized;
            _input.sprint = Input.GetKey(sprintKey);
            _input.jump = Input.GetKey(jumpKey);

            JumpAndGravity();
            GroundedCheck();
            ZeminTipiKontrolu(); // Hangi zemine bastığımızı kontrol et
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        // --- YENİ EKLENEN: ZEMİN TİPİ KONTROLÜ ---
        private void ZeminTipiKontrolu()
        {
            if (!Grounded) return;

            // Ayakların biraz üstünden aşağıya doğru kısa bir ışın (Raycast) at
            Vector3 origin = transform.position + Vector3.up * 0.1f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 0.5f, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                string zeminTagi = hit.collider.tag;

                if (zeminTagi == "EvZemini")
                {
                    currentFootstepSound = "Ev_Yurume_Sesi";
                }
                else if (zeminTagi == "MagaraZemini")
                {
                    currentFootstepSound = "Magara_Yurume_Sesi";
                }
                else
                {
                    currentFootstepSound = "Yurume_Sesi"; // Dışarıdaki normal zemin (kar, toprak vb.)
                }
            }
        }

        private void CameraRotation()
        {
            if (!CanLook) return;

            if (_input.look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
            {
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // DİNAMİK YÜRÜME SESİ SİSTEMİ
            if (Grounded && _input.move != Vector2.zero && _speed > 0.1f)
            {
                float currentStepRate = _input.sprint ? SprintStepRate : WalkStepRate;
                _footstepTimer -= Time.deltaTime;

                if (_footstepTimer <= 0f)
                {
                    if (AudioManager.instance != null)
                    {
                        // 1. Önce olası tüm yürüme seslerini kesin olarak sustur (Üst üste binmeyi engeller)
                        AudioManager.instance.Stop("Yurume_Sesi");
                        AudioManager.instance.Stop("Magara_Yurume_Sesi");
                        AudioManager.instance.Stop("Ev_Yurume_Sesi");

                        // 2. Sadece o anki güncel zeminin sesini çal
                        AudioManager.instance.Play(currentFootstepSound);
                    }
                    _footstepTimer = currentStepRate;
                }
            }
            else
            {
                // OYUNCU DURDUĞUNDA TÜM SESLERİ KES
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Stop("Yurume_Sesi");
                    AudioManager.instance.Stop("Magara_Yurume_Sesi");
                    AudioManager.instance.Stop("Ev_Yurume_Sesi");
                }
                _footstepTimer = 0f;
            }
        }

            private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }

                if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;
                if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
            Gizmos.color = Grounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}