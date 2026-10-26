using UnityEngine;
using UnityEngine.InputSystem;
namespace Faro
{
    public class EditorMouseLook : MonoBehaviour
    {
        public Transform head;
        private float pitch;
        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Mouse.current == null || !Mouse.current.rightButton.isPressed) return;
            Vector2 d = Mouse.current.delta.ReadValue() * .12f;
            transform.RotateAround(head.position, Vector3.up, d.x);
            pitch = Mathf.Clamp(pitch - d.y, -75, 75);
            head.localRotation = Quaternion.Euler(pitch, 0, 0);
#endif
        }
        public void ResetPitch() { pitch = 0; head.localRotation = Quaternion.identity; }
    }
}
