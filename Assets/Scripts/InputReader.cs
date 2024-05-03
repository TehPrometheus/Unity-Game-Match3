using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Match3
{
    public class InputReader: MonoBehaviour 
    {
        PlayerInput playerInput;
        InputAction selectAction;
        InputAction fireAction;

        [SerializeField]
        private InputActionReference fireTest;
        public event Action Fire;
        public Vector2 Selected => selectAction.ReadValue<Vector2>(); // This is pretty cool

        private void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            selectAction = playerInput.actions["Select"];
            fireAction = playerInput.actions["Fire"];

            fireTest.action.performed += OnFire;
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            Fire?.Invoke();
        }
    }
}