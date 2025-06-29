using UnityEngine;
namespace aurw
{
    public class AURW_FreeCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 5f;
        public float sprintSpeed = 10f;
        public float lookSpeed = 2f;
        public float acceleration = 10f; // Suavizado de transición

        [Header("Key Bindings")]
        public KeyCode exitKey = KeyCode.Escape;
        public KeyCode unlockMouseKey = KeyCode.Tab;
        public KeyCode sprintKey = KeyCode.LeftShift;

        private float _rotationX = 0f;
        private float _rotationY = 0f;
        private bool _mouseLocked = true;
        private float _currentSpeed; // Velocidad actual (para suavizado)

        private void Start()
        {
            LockMouse(true);
            _currentSpeed = walkSpeed;
        }

        private void Update()
        {
            HandleExit();
            HandleMouseToggle();

            if (_mouseLocked)
            {
                HandleMovement();
                HandleRotation();
            }
        }

        private void HandleExit()
        {
            if (Input.GetKeyDown(exitKey))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
        }

        private void HandleMouseToggle()
        {
            if (Input.GetKeyDown(unlockMouseKey))
            {
                LockMouse(!_mouseLocked);
            }
        }

        private void HandleMovement()
        {
            // Determina la velocidad objetivo (sprint o walk)
            float targetSpeed = Input.GetKey(sprintKey) ? sprintSpeed : walkSpeed;

            // Suaviza el cambio de velocidad
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += transform.forward;
            if (Input.GetKey(KeyCode.S)) move -= transform.forward;
            if (Input.GetKey(KeyCode.D)) move += transform.right;
            if (Input.GetKey(KeyCode.A)) move -= transform.right;
            if (Input.GetKey(KeyCode.E)) move += Vector3.up;
            if (Input.GetKey(KeyCode.Q)) move -= Vector3.up;

            transform.position += move.normalized * _currentSpeed * Time.deltaTime;
        }

        private void HandleRotation()
        {
            _rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
            _rotationY += Input.GetAxis("Mouse X") * lookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);

            transform.localEulerAngles = new Vector3(_rotationX, _rotationY, 0f);
        }

        public void LockMouse(bool locked)
        {
            _mouseLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}