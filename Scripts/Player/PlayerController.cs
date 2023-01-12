using Meta.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Local.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController instance;
        
        private CharacterController controller;
        private InputActionManager manager;
        private InputActionMap keyboard;
        private Transform PlayerCamera;

        private float speed = 5f;
        private float CameraPitch = 0f;
        private float VelocityY = 0.0f;
        [SerializeField] private float mouseSensitivity = 1.5f;
        [SerializeField] private float MovementSpeed = 6.0f;
        [SerializeField] private float Gravity = -13.0f;

        [SerializeField] [Range(0.0f, 0.5f)] private float MoveSmoothTime = 0.3f;
        [SerializeField] [Range(0.0f, 0.5f)] private float MouseSmoothTime = 0.03f;

        private Vector2 CurrentDirection = Vector2.zero;
        private Vector2 CurrentDirVelocity = Vector2.zero;

        private Vector2 CurrentMouseDelta = Vector2.zero;
        private Vector2 CurrentMouseDeltaVelocity = Vector2.zero;

        public bool Movement { get; set; }
        public bool canmove = true;

        private void Awake()
        {
            if(instance != null) Destroy(gameObject);
            else instance = this;
        }

        private void Start()
        {
            manager = GameObject.Find("Controller").GetComponent<InputActionManager>();
            keyboard = manager.actionAssets.Find(x => x.name == "Bindings").FindActionMap("Keyboard Bindings");
            controller = gameObject.AddComponent<CharacterController>();
            PlayerCamera = GetComponent<Metadata>().FindParam("Camera").parameter.transform;
            canmove = true;
        }

        private void Update()
        {
            Move();
            UpdateRotation();
        }

        private void Move()
        {
            if (!canmove)
            {
                return;
            }
            // Forward/backward makes player model move
            float translation = keyboard.FindAction("Vertical").ReadValue<float>();

            if (translation != 0)
            {
                // Translate object
                this.transform.Translate(0, 0, translation * Time.deltaTime * speed);

                // Check movement limits
                Vector3 pos = this.transform.position;

                Movement = true;
            }

            // Left/right makes player model rotate around own axis
            // float rotation = keyboard.FindAction("Horizontal").ReadValue<float>();

            // if (rotation != 0)
            // {
            //     this.transform.Rotate(Vector3.up, rotation * Time.deltaTime * speed);

            //     Movement = true;
            // }
        }

        void UpdateMovement()
        {
            if (!canmove)
            {
                return;
            }
            Vector2 TargetDirection = new(keyboard.FindAction("Horizontal").ReadValue<float>(), keyboard.FindAction("Vertical").ReadValue<float>());

            TargetDirection.Normalize();

            CurrentDirection = Vector2.SmoothDamp(CurrentDirection, TargetDirection, ref CurrentDirVelocity, MoveSmoothTime);

            if (controller.isGrounded)
            {
                VelocityY = 0.0f;
            }

            VelocityY += Gravity * Time.deltaTime;

            Vector3 Velocity = (transform.forward * CurrentDirection.y + transform.right * CurrentDirection.x) * MovementSpeed + Vector3.up * VelocityY;

            controller.Move(Velocity * Time.deltaTime);
        }

        private void UpdateRotation()
        {
            if (canmove)
            {
                Vector2 TargetMouseDelta = new(Mouse.current.delta.x.ReadValue(), Mouse.current.delta.y.ReadValue());

                CurrentMouseDelta = Vector2.SmoothDamp(CurrentMouseDelta, TargetMouseDelta, ref CurrentMouseDeltaVelocity, MouseSmoothTime);

                CameraPitch -= CurrentMouseDelta.y * mouseSensitivity;
                CameraPitch = Mathf.Clamp(CameraPitch, -75.0f, 75.0f);

                PlayerCamera.localEulerAngles = Vector3.right * CameraPitch;

                transform.Rotate(Vector3.up * CurrentMouseDelta.x * mouseSensitivity);
            }
        }
    }
}