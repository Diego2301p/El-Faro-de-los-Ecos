using UnityEngine;
using UnityEngine.UI;
namespace Faro
{
    public class WorldDialogue : MonoBehaviour
    {
        public Text text;
        public CanvasGroup group;
        public Transform head;
        private float remaining;
        public void Show(string message, float seconds = 14)
        {
            // Se coloca una vez en el mundo al mostrar el mensaje; no sigue la cabeza.
            // Así se lee desde cualquiera de los puntos de teletransporte.
            if (head)
            {
                Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
                if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
                transform.position = head.position + forward * 4 + Vector3.up * 1.45f;
            }
            text.text = message; remaining = seconds; group.alpha = 1;
        }
        private void LateUpdate()
        {
            if (head)
            {
                Vector3 facing = transform.position - head.position;
                facing.y = 0;
                if (facing.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(facing);
            }
            remaining -= Time.unscaledDeltaTime;
            group.alpha = Mathf.MoveTowards(group.alpha, remaining > 0 ? 1 : 0, Time.unscaledDeltaTime * 3);
        }
    }
}
