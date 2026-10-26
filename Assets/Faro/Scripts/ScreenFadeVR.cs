using System.Collections;
using UnityEngine;
namespace Faro
{
    public class ScreenFadeVR : MonoBehaviour
    {
        public CanvasGroup panel;
        public float duration = .45f;
        private void Awake() { panel.alpha = 1; panel.blocksRaycasts = false; panel.interactable = false; }
        public IEnumerator To(float target)
        {
            float initial = panel.alpha;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                panel.alpha = Mathf.Lerp(initial, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            panel.alpha = target;
        }
    }
}
