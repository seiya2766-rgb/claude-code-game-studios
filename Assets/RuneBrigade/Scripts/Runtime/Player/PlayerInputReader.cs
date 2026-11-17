using UnityEngine;
using UnityEngine.InputSystem;

namespace RuneBrigade
{
    /// <summary>
    /// Reads keyboard and mouse through the Input System package once per frame.
    /// Prototype only: replace with an Input Actions asset (rebinding, gamepad)
    /// before production. Keys: design/game-concept.md, "基本操作（PC）".
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        /// <summary>WASD as a vector, x = strafe, y = forward.</summary>
        public Vector2 Move { get; private set; }
        /// <summary>Mouse movement this frame in pixels.</summary>
        public Vector2 Look { get; private set; }
        /// <summary>Space pressed this frame.</summary>
        public bool JumpPressed { get; private set; }
        /// <summary>Shift pressed this frame.</summary>
        public bool DashPressed { get; private set; }
        /// <summary>Left mouse pressed this frame.</summary>
        public bool PrimaryPressed { get; private set; }
        /// <summary>Left mouse held.</summary>
        public bool PrimaryHeld { get; private set; }
        /// <summary>Right mouse pressed this frame.</summary>
        public bool SecondaryPressed { get; private set; }
        /// <summary>Right mouse held.</summary>
        public bool SecondaryHeld { get; private set; }
        /// <summary>Q pressed this frame.</summary>
        public bool Skill1Pressed { get; private set; }
        /// <summary>E pressed this frame.</summary>
        public bool Skill2Pressed { get; private set; }
        /// <summary>R pressed this frame.</summary>
        public bool UltimatePressed { get; private set; }
        /// <summary>0-3 for keys 1-4 pressed this frame, otherwise -1.</summary>
        public int RoleKeyPressed { get; private set; } = -1;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (keyboard == null || mouse == null)
            {
                Clear();
                return;
            }

            float x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
            float y = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            Move = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            Look = Cursor.lockState == CursorLockMode.Locked ? mouse.delta.ReadValue() : Vector2.zero;

            JumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            DashPressed = keyboard.leftShiftKey.wasPressedThisFrame;
            PrimaryPressed = mouse.leftButton.wasPressedThisFrame;
            PrimaryHeld = mouse.leftButton.isPressed;
            SecondaryPressed = mouse.rightButton.wasPressedThisFrame;
            SecondaryHeld = mouse.rightButton.isPressed;
            Skill1Pressed = keyboard.qKey.wasPressedThisFrame;
            Skill2Pressed = keyboard.eKey.wasPressedThisFrame;
            UltimatePressed = keyboard.rKey.wasPressedThisFrame;

            RoleKeyPressed = -1;
            if (keyboard.digit1Key.wasPressedThisFrame) RoleKeyPressed = 0;
            else if (keyboard.digit2Key.wasPressedThisFrame) RoleKeyPressed = 1;
            else if (keyboard.digit3Key.wasPressedThisFrame) RoleKeyPressed = 2;
            else if (keyboard.digit4Key.wasPressedThisFrame) RoleKeyPressed = 3;

            if (keyboard.escapeKey.wasPressedThisFrame) Cursor.lockState = CursorLockMode.None;
            else if (PrimaryPressed && Cursor.lockState != CursorLockMode.Locked) Cursor.lockState = CursorLockMode.Locked;
        }

        private void Clear()
        {
            Move = Look = Vector2.zero;
            JumpPressed = DashPressed = PrimaryPressed = PrimaryHeld = SecondaryPressed = SecondaryHeld = false;
            Skill1Pressed = Skill2Pressed = UltimatePressed = false;
            RoleKeyPressed = -1;
        }
    }
}
