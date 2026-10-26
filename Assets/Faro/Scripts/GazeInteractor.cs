using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Google.XR.Cardboard;
namespace Faro
{
    public class GazeInteractor : MonoBehaviour
    {
        public MissionController mission;
        public ReticleVisual reticle;
        public float dwellSeconds = 1.6f;
        public float maxDistance = 14;
        private GazeTarget current;
        private readonly DwellTimer timer = new DwellTimer();
        // Identificadores locales de la sesión: compatibles con Unity 6.0 y 6.5.
        // El mismo objetivo conserva su número después de animaciones y traslados.
        private readonly Dictionary<GazeTarget, int> targetIds = new Dictionary<GazeTarget, int>();
        // Tras una transición debe apartar la mirada antes de volver a activar el mismo objeto.
        private int suppressed;
        private void Update()
        {
            if (mission.Busy) { ClearFocus(); reticle.Show(false, 0); return; }
            GazeTarget found = null;
            // El primer collider bloquea el rayo, incluso si es una pared no interactiva.
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit,
                maxDistance, ~(1 << 2), QueryTriggerInteraction.Ignore))
                found = hit.collider.GetComponentInParent<GazeTarget>();
            if (found != current)
            {
                ClearFocus(); current = found;
                if (current) current.OnPointerEnter();
            }
            int id = GetTargetId(current);
            if (id != suppressed) suppressed = 0;
            bool manual = false;
#if UNITY_EDITOR || UNITY_STANDALONE
            manual = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#elif UNITY_ANDROID
            manual = Api.IsTriggerPressed;
#endif
            if (timer.Tick(id == suppressed ? 0 : id, Time.unscaledDeltaTime, dwellSeconds, manual))
            {
                suppressed = id;
                current.OnPointerClick();
            }
            reticle.Show(current != null, timer.Progress);
        }
        private int GetTargetId(GazeTarget target)
        {
            if (!target) return 0;
            if (!targetIds.TryGetValue(target, out int id))
            {
                id = targetIds.Count + 1;
                targetIds.Add(target, id);
            }
            return id;
        }
        private void ClearFocus()
        {
            if (current) current.OnPointerExit();
            current = null; timer.Reset();
        }
        private void OnDisable() { ClearFocus(); }
    }
}
