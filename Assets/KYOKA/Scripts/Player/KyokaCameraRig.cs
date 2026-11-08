using UnityEngine;
using Kyoka.Core;

namespace Kyoka.Player
{
    public class KyokaCameraRig : MonoBehaviour
    {
        [Header("Distance & Offsets")]
        public float thirdPersonDistance = 4.5f;
        public float minDistance = 1.0f;
        public float maxDistance = 7.0f;
        public float firstPersonHeight = 1.6f;
        public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0);

        [Header("Look Controls")]
        public float pitchMin = -75f;
        public float pitchMax = 75f;
        public bool firstPerson = false;

        [Header("Collision & Smoothing")]
        public LayerMask collisionMask = ~((1 << 9) | (1 << 2)); // Ignore player layer (9) and IgnoreRaycast (2)
        public float sphereRadius = 0.25f;
        public float smoothSpeed = 15f;

        private Camera cam;
        private float pitch = 10f;
        private float currentDistance;

        private void Start()
        {
            cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("KYOKA_MainCamera");
                cam = go.AddComponent<Camera>();
                go.tag = "MainCamera";
            }

            currentDistance = thirdPersonDistance;

            // Ensure exactly one active audio listener in the scene
            AudioListener[] listeners = FindObjectsOfType<AudioListener>();
            bool keptListener = false;
            foreach (var l in listeners)
            {
                if (!l) continue;
                if (!keptListener) keptListener = true;
                else l.enabled = false;
            }
            if (cam.GetComponent<AudioListener>() == null && !keptListener)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }

            // On standalone PC, lock cursor
            if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            PositionCamera(true);
        }

        private void Update()
        {
            var input = KyokaInputBridge.Instance;
            if (input != null && input.CameraTogglePressed)
            {
                firstPerson = !firstPerson;
            }

            // Desktop cursor handling
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked && !Application.isMobilePlatform)
            {
                // Only relock if not clicking on a UI button
                if (!UnityEngine.EventSystems.EventSystem.current || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            // Read pitch delta from Input Bridge
            if (input != null)
            {
                pitch += input.LookDelta.y;
                pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            }
        }

        private void LateUpdate()
        {
            PositionCamera(false);
        }

        private void PositionCamera(bool instant)
        {
            if (!cam) return;

            Vector3 pivot = transform.position + (firstPerson ? Vector3.up * firstPersonHeight : thirdPersonOffset);
            Quaternion targetRotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0);

            if (firstPerson)
            {
                cam.transform.rotation = targetRotation;
                cam.transform.position = pivot;
                return;
            }

            // Third person: SphereCast backwards to prevent wall clipping
            Vector3 desiredDirection = -(targetRotation * Vector3.forward);
            float targetDist = thirdPersonDistance;

            if (Physics.SphereCast(pivot, sphereRadius, desiredDirection, out RaycastHit hit, thirdPersonDistance, collisionMask))
            {
                targetDist = Mathf.Clamp(hit.distance - 0.1f, minDistance, thirdPersonDistance);
            }

            currentDistance = instant ? targetDist : Mathf.Lerp(currentDistance, targetDist, smoothSpeed * Time.deltaTime);

            Vector3 finalPosition = pivot + desiredDirection * currentDistance;
            cam.transform.rotation = targetRotation;
            cam.transform.position = finalPosition;
        }

        public void SetFirstPerson(bool fp)
        {
            firstPerson = fp;
        }
    }
}
