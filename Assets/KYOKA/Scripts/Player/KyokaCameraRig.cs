using UnityEngine;

namespace Kyoka.Player
{
    public class KyokaCameraRig : MonoBehaviour
    {
        public float thirdPersonDistance = 5f;
        public float firstPersonHeight = 1.55f;
        public float lookSpeed = 2f;
        public bool firstPerson;
        private Camera cam;
        private float pitch;

        private void Start()
        {
            cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("KYOKA_MainCamera");
                cam = go.AddComponent<Camera>();
                go.tag = "MainCamera";
            }
            // Keep exactly one active audio listener in the runtime scene.
            var listeners = FindObjectsOfType<AudioListener>();
            bool keptListener = false;
            foreach (var listener in listeners)
            {
                if (!listener) continue;
                if (!keptListener) keptListener = true;
                else listener.enabled = false;
            }
            if (cam.GetComponent<AudioListener>() == null && !keptListener)
                cam.gameObject.AddComponent<AudioListener>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            PositionCamera();
        }

        private void Update()
        {
            if (!cam) return;
            if (Input.GetKeyDown(KeyCode.V)) firstPerson = !firstPerson;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
                pitch = Mathf.Clamp(pitch, -75f, 75f);
            }
            PositionCamera();
        }

        private void LateUpdate() => PositionCamera();

        private void PositionCamera()
        {
            if (!cam) return;
            Vector3 target = transform.position + Vector3.up * (firstPerson ? firstPersonHeight : 1.5f);
            cam.transform.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0);
            cam.transform.position = firstPerson
                ? target
                : target - cam.transform.forward * thirdPersonDistance;
        }
    }
}
