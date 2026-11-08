using UnityEngine;
using Kyoka.Core;

namespace Kyoka.UI
{
    /// <summary>
    /// Dedicated mobile touchscreen control system for Android and iOS.
    /// Provides responsive left virtual joystick, right camera swipe area,
    /// and dedicated touch action buttons for combat, abilities, jumping, sprinting, and vehicles.
    /// </summary>
    public class KyokaTouchControls : MonoBehaviour
    {
        public static KyokaTouchControls Instance { get; private set; }

        [Header("Display Settings")]
        public bool forceEnableOnDesktop = false;
        public bool isVisible = true;

        [Header("Virtual Joystick Configuration")]
        private Vector2 joystickCenter = new Vector2(160, 0); // Y calculated from bottom
        private float joystickRadius = 80f;
        private Vector2 currentHandlePos;
        private int joystickTouchId = -1;
        private bool isJoystickActive = false;

        [Header("Camera Swipe Configuration")]
        private int cameraTouchId = -1;
        private Vector2 lastCameraTouchPos;

        // Button Rectangles (Scaled to screen dimensions)
        private Rect jumpButtonRect;
        private Rect sprintButtonRect;
        private Rect attackButtonRect;
        private Rect ability1ButtonRect;
        private Rect ability2ButtonRect;
        private Rect interactButtonRect;
        private Rect cameraToggleRect;

        // UI Styles
        private GUIStyle buttonStyle;
        private GUIStyle activeSprintStyle;
        private bool sprintActive = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // On Android/iOS, always visible
            if (Application.isMobilePlatform) forceEnableOnDesktop = true;
        }

        private void Update()
        {
            if (!forceEnableOnDesktop && !Application.isMobilePlatform) return;

            UpdateTouchInputs();
        }

        private void UpdateTouchInputs()
        {
            float w = Screen.width;
            float h = Screen.height;

            // Define button rects on right side of screen
            jumpButtonRect = new Rect(w - 110, h - 130, 85, 85);
            sprintButtonRect = new Rect(w - 110, h - 235, 85, 85);
            attackButtonRect = new Rect(w - 215, h - 130, 90, 90);
            ability1ButtonRect = new Rect(w - 215, h - 235, 85, 85);
            ability2ButtonRect = new Rect(w - 180, h - 335, 80, 80);
            interactButtonRect = new Rect(w - 310, h - 130, 80, 80);
            cameraToggleRect = new Rect(w - 100, 70, 75, 50);

            var bridge = KyokaInputBridge.Instance;
            if (bridge == null) return;

            // Handle touch inputs on real device
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                Vector2 screenPos = new Vector2(t.position.x, h - t.position.y);

                // Left side: Joystick
                if (t.position.x < w * 0.45f)
                {
                    if (t.phase == TouchPhase.Began && joystickTouchId == -1)
                    {
                        joystickTouchId = t.fingerId;
                        joystickCenter = screenPos;
                        currentHandlePos = screenPos;
                        isJoystickActive = true;
                    }
                    else if (t.fingerId == joystickTouchId)
                    {
                        if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                        {
                            Vector2 offset = screenPos - joystickCenter;
                            offset = Vector2.ClampMagnitude(offset, joystickRadius);
                            currentHandlePos = joystickCenter + offset;

                            Vector2 moveVec = new Vector2(offset.x / joystickRadius, -offset.y / joystickRadius);
                            bridge.SetTouchMoveInput(moveVec);
                        }
                        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                        {
                            joystickTouchId = -1;
                            isJoystickActive = false;
                            bridge.SetTouchMoveInput(Vector2.zero);
                        }
                    }
                }
                // Right side: Camera swipe (if not clicking action button)
                else if (t.position.x >= w * 0.45f)
                {
                    bool isOverButton = jumpButtonRect.Contains(screenPos) || sprintButtonRect.Contains(screenPos) ||
                                       attackButtonRect.Contains(screenPos) || ability1ButtonRect.Contains(screenPos) ||
                                       ability2ButtonRect.Contains(screenPos) || interactButtonRect.Contains(screenPos) ||
                                       cameraToggleRect.Contains(screenPos);

                    if (!isOverButton)
                    {
                        if (t.phase == TouchPhase.Began && cameraTouchId == -1)
                        {
                            cameraTouchId = t.fingerId;
                            lastCameraTouchPos = screenPos;
                        }
                        else if (t.fingerId == cameraTouchId)
                        {
                            if (t.phase == TouchPhase.Moved)
                            {
                                Vector2 delta = screenPos - lastCameraTouchPos;
                                bridge.AddTouchLookDelta(new Vector2(delta.x * 0.4f, delta.y * 0.4f));
                                lastCameraTouchPos = screenPos;
                            }
                            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                            {
                                cameraTouchId = -1;
                            }
                        }
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!forceEnableOnDesktop && !Application.isMobilePlatform) return;
            if (!isVisible) return;

            InitializeGUIStyles();

            var bridge = KyokaInputBridge.Instance;
            if (bridge == null) return;

            float h = Screen.height;

            // Render Virtual Joystick
            if (!isJoystickActive)
            {
                joystickCenter = new Vector2(140, h - 140);
                currentHandlePos = joystickCenter;
            }

            // Joystick base circle
            GUI.Box(new Rect(joystickCenter.x - joystickRadius, joystickCenter.y - joystickRadius, joystickRadius * 2, joystickRadius * 2), "", buttonStyle);
            // Joystick thumb handle
            GUI.Box(new Rect(currentHandlePos.x - 28, currentHandlePos.y - 28, 56, 56), "MOVE", buttonStyle);

            // Render Right Side Action Buttons
            if (GUI.RepeatButton(jumpButtonRect, "JUMP", buttonStyle))
            {
                bridge.TriggerTouchJump();
            }

            if (GUI.Button(sprintButtonRect, "SPRINT", sprintActive ? activeSprintStyle : buttonStyle))
            {
                sprintActive = !sprintActive;
                bridge.SetTouchSprint(sprintActive);
            }

            if (GUI.RepeatButton(attackButtonRect, "ATTACK", buttonStyle))
            {
                bridge.TriggerTouchAttack();
            }

            if (GUI.Button(ability1ButtonRect, "ABILITY 1", buttonStyle))
            {
                bridge.TriggerTouchAbility1();
            }

            if (GUI.Button(ability2ButtonRect, "ABILITY 2", buttonStyle))
            {
                bridge.TriggerTouchAbility2();
            }

            if (GUI.Button(interactButtonRect, "INTERACT", buttonStyle))
            {
                bridge.TriggerTouchInteract();
            }

            if (GUI.Button(cameraToggleRect, "CAM 1P/3P", buttonStyle))
            {
                bridge.TriggerTouchCameraToggle();
            }
        }

        private void InitializeGUIStyles()
        {
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = 13;
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.alignment = TextAnchor.MiddleCenter;

                activeSprintStyle = new GUIStyle(buttonStyle);
                activeSprintStyle.normal.textColor = Color.yellow;
            }
        }
    }
}
