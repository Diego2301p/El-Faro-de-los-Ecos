using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Faro
{
    // Corrección de legibilidad para la escena existente. No hace falta añadirla
    // a objetos a mano ni reconstruir ElFaro.unity. Se aplica también al reiniciar.
    [DisallowMultipleComponent]
    public sealed class LegibilidadVR : MonoBehaviour
    {
        private Transform head;
        private CanvasGroup visibility;
        private Vector3 anchor;
        private const float CanvasWidth = 800f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var mission = Object.FindFirstObjectByType<MissionController>();
            if (!mission || !mission.head) return;

            foreach (var text in Object.FindObjectsByType<Text>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (text.gameObject.scene != scene) continue;
                var canvas = text.GetComponentInParent<Canvas>(true);
                if (!canvas) continue;

                if (canvas.name == "Objetivo")
                {
                    text.fontSize = 40;
                    text.fontStyle = FontStyle.Bold;
                    text.resizeTextForBestFit = false;
                    continue;
                }

                // Los diálogos y títulos grandes conservan su comportamiento.
                // Solo se corrigen los carteles pequeños del recorrido.
                string name = canvas.name;
                if (!IsRouteLabel(name) || canvas.GetComponent<LegibilidadVR>()) continue;
                var readable = canvas.gameObject.AddComponent<LegibilidadVR>();
                readable.Configure(text, canvas, mission.head);
            }
        }

        private static bool IsRouteLabel(string name)
        {
            return name == "Cartel_MUELLE" || name == "Cartel_CRISTAL" ||
                name == "Cartel_FARO" || name == "Cartel_LENTE" ||
                name == "Cartel_VENTANAL" || name == "Cartel_SEÑAL" ||
                name == "Cartel_ALINEA ARRIBA" || name == "Cartel_?" ||
                name == "Cartel_VOLVER A EMPEZAR" || name.StartsWith("Cartel_GIRO ");
        }

        private void Configure(Text text, Canvas canvas, Transform cameraHead)
        {
            head = cameraHead;
            anchor = transform.position;
            // Los nombres de los destinos quedan por encima del suelo.
            if (anchor.y < .8f) anchor.y = 1.05f;
            transform.position = anchor;

            var rect = (RectTransform)transform;
            rect.sizeDelta = new Vector2(CanvasWidth, 150);
            text.fontSize = 70;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(.96f, 1f, .98f, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(22, 10);
            text.rectTransform.offsetMax = new Vector2(-22, -10);

            var backgroundObject = new GameObject("Fondo_legible", typeof(RectTransform));
            backgroundObject.layer = 2;
            backgroundObject.transform.SetParent(transform, false);
            backgroundObject.transform.SetAsFirstSibling();
            var background = backgroundObject.AddComponent<Image>();
            background.color = new Color(.025f, .06f, .085f, .93f);
            background.raycastTarget = false;
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            visibility = canvas.gameObject.AddComponent<CanvasGroup>();
            visibility.interactable = false;
            visibility.blocksRaycasts = false;
            UpdateLabel();
        }

        private void LateUpdate() { UpdateLabel(); }

        private void UpdateLabel()
        {
            if (!head || !visibility) return;
            // Tamaño angular aproximadamente constante, con límites físicos.
            // El cartel permanece en el mundo: no se convierte en un HUD.
            float distance = Vector3.Distance(head.position, anchor);
            float width = Mathf.Clamp(distance * .34f, .8f, 3.8f);
            transform.localScale = Vector3.one * (width / CanvasWidth);
            Vector3 facing = anchor - head.position;
            if (facing.sqrMagnitude > .001f)
                transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
            visibility.alpha = 1f - Mathf.InverseLerp(12f, 16f, distance);
        }
    }
}
