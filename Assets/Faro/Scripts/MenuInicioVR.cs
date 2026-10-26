using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Google.XR.Cardboard;

namespace Faro
{
    // Escenario de bienvenida independiente. Se crea al iniciar la escena y
    // se retira antes de jugar; no modifica el barco del rescate ni la misión.
    public sealed class MenuInicioVR : MonoBehaviour
    {
        private MissionController mission;
        private GazeInteractor gameGaze;
        private GameObject stage, menu;
        private Transform menuBoat;
        private Quaternion boatHeading;
        private Vector3 boatAnchor;
        private readonly List<Material> materials = new List<Material>();
        private readonly List<Transform> wake = new List<Transform>();
        private Collider startButton, exitButton;
        private Image startImage, exitImage;
        private readonly DwellTimer dwell = new DwellTimer();
        private bool ready, startRequested, storyVisible, waitForLookAway;
        private float animationTime;
        private Color idle = new Color(.025f, .20f, .27f, .98f);
        private Color focused = new Color(.06f, .42f, .45f, 1f);

        public IEnumerator Present(MissionController controller)
        {
            mission = controller;
            gameGaze = mission.head.GetComponent<GazeInteractor>();
            gameGaze.enabled = false;
            mission.objective.gameObject.SetActive(false);
            mission.dockDialogue.gameObject.SetActive(false);
            mission.towerDialogue.gameObject.SetActive(false);
            Vector3 savedPosition = mission.playerRoot.position;
            Quaternion savedRotation = mission.playerRoot.rotation;
            mission.playerRoot.SetPositionAndRotation(new Vector3(-40, 0, -40), Quaternion.identity);
            // Espera una actualización del seguimiento antes de orientar la bienvenida.
            yield return null;
            Vector3 view = Vector3.ProjectOnPlane(mission.head.forward, Vector3.up);
            if (view.sqrMagnitude > .001f)
                mission.playerRoot.Rotate(0, Vector3.SignedAngle(view, Vector3.forward, Vector3.up), 0, Space.World);
            BuildBackdrop();
            BuildMenu();
            Physics.SyncTransforms();
            yield return mission.fade.To(0);
            ready = true;
            while (!startRequested) yield return null;
            ready = false;
            yield return mission.fade.To(1);
            Cleanup();
            mission.playerRoot.SetPositionAndRotation(savedPosition, savedRotation);
#if UNITY_EDITOR || UNITY_STANDALONE
            var look = mission.playerRoot.GetComponent<EditorMouseLook>();
            if (look) look.ResetPitch();
#endif
            mission.objective.gameObject.SetActive(true);
            mission.dockDialogue.gameObject.SetActive(true);
            mission.towerDialogue.gameObject.SetActive(true);
            gameGaze.enabled = true;
            // MissionController revela el muelle, inicia el tiempo y muestra la misión.
        }

        private void Update()
        {
            AnimateBoat();
            if (!mission || !gameGaze) return;
            if (!ready) { gameGaze.reticle.Show(false, 0); return; }
            int id = 0;
            if (Physics.Raycast(mission.head.position, mission.head.forward, out RaycastHit hit,
                6, 1 << 8, QueryTriggerInteraction.Ignore))
                id = hit.collider == startButton ? 1 : hit.collider == exitButton ? 2 : 0;
            startImage.color = id == 1 ? focused : idle;
            if (exitImage) exitImage.color = id == 2 ? focused : idle;
            // Al cambiar de pantalla hay que apartar la mirada del botón nuevo.
            // Así la mirada que eligió INICIAR no puede saltarse la historia.
            if (waitForLookAway)
            {
                dwell.Reset();
                gameGaze.reticle.Show(false, 0);
                if (id == 0) waitForLookAway = false;
                return;
            }
            bool manual = false;
#if UNITY_EDITOR || UNITY_STANDALONE
            manual = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#elif UNITY_ANDROID
            manual = Api.IsTriggerPressed;
#endif
            bool activate = dwell.Tick(id, Time.unscaledDeltaTime, gameGaze.dwellSeconds, manual);
            gameGaze.reticle.Show(id != 0, dwell.Progress);
            if (!activate) return;
            if (id == 1 && !storyVisible)
            {
                ShowStory();
                return;
            }
            ready = false;
            if (id == 1) startRequested = true;
            else if (id == 2) StartCoroutine(Exit());
        }

        private IEnumerator Exit()
        {
            yield return mission.fade.To(1);
#if UNITY_EDITOR
            // En el editor se termina Play; no se cierra Unity ni se pierden cambios.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void BuildBackdrop()
        {
            stage = new GameObject("Escenario_menu_inicial");
            stage.transform.position = new Vector3(-40, 0, -40);
            Material litSource = mission.boat.GetComponentInChildren<Renderer>().sharedMaterial;
            Material glowSource = mission.crystal.GetComponentInChildren<Renderer>().sharedMaterial;
            Material ivory = MaterialFrom(litSource, new Color(.88f, .88f, .78f));
            Material turquoise = MaterialFrom(litSource, new Color(.055f, .43f, .48f));
            Material rock = MaterialFrom(litSource, new Color(.22f, .30f, .34f));
            Material roof = MaterialFrom(litSource, new Color(.12f, .20f, .26f));
            Material light = MaterialFrom(glowSource, new Color(1f, .88f, .52f));
            Material foam = MaterialFrom(glowSource, new Color(.25f, .65f, .68f));
            Primitive("Isla_del_faro", PrimitiveType.Sphere, new Vector3(3, -1, 15), new Vector3(8, 2.5f, 7), rock);
            Primitive("Torre", PrimitiveType.Cylinder, new Vector3(3, 3.3f, 15), new Vector3(2.5f, 3.3f, 2.5f), ivory);
            for (int i = 1; i <= 3; i++)
                Primitive("Franja_" + i, PrimitiveType.Cylinder, new Vector3(3, i * 1.6f, 15), new Vector3(2.54f, .16f, 2.54f), turquoise);
            Primitive("Balcon", PrimitiveType.Cylinder, new Vector3(3, 6.65f, 15), new Vector3(3.1f, .16f, 3.1f), roof);
            Primitive("Linterna", PrimitiveType.Cylinder, new Vector3(3, 7.2f, 15), new Vector3(1.7f, .45f, 1.7f), light);
            Primitive("Techo", PrimitiveType.Sphere, new Vector3(3, 7.8f, 15), new Vector3(3, .8f, 3), roof);
            Primitive("Puerta", PrimitiveType.Cube, new Vector3(3, .7f, 13.72f), new Vector3(.65f, 1.4f, .06f), roof);
            menuBoat = Instantiate(mission.boat.gameObject, stage.transform).transform;
            menuBoat.name = "Barco_animado_del_menu";
            boatAnchor = new Vector3(-3.3f, -.8f, 8.5f);
            boatHeading = Quaternion.LookRotation(new Vector3(6.3f, 0, 6.5f));
            menuBoat.localPosition = boatAnchor;
            menuBoat.localRotation = boatHeading;
            menuBoat.localScale = Vector3.one * .65f;
            foreach (var collider in menuBoat.GetComponentsInChildren<Collider>()) Destroy(collider);
            for (int i = 0; i < 4; i++)
                wake.Add(Primitive("Estela_" + i, PrimitiveType.Sphere,
                    boatAnchor, new Vector3(1, .025f, .10f), foam).transform);
        }

        private void AnimateBoat()
        {
            if (!menuBoat) return;
            animationTime += Time.unscaledDeltaTime;
            float t = animationTime;
            // X y Z permanecen fijas: solo sube y baja y oscila sobre las olas.
            menuBoat.localPosition = boatAnchor + Vector3.up * (Mathf.Sin(t * 1.8f) * .075f);
            menuBoat.localRotation = boatHeading * Quaternion.Euler(Mathf.Sin(t * 1.3f) * 2, 0, Mathf.Sin(t * 1.6f) * 3);
            for (int i = 0; i < wake.Count; i++)
            {
                float phase = Mathf.Repeat(t * .65f + i * .65f, 2.6f);
                var trail = wake[i];
                trail.localPosition = boatAnchor + boatHeading * new Vector3(0, -.23f, -1.7f - phase);
                trail.localRotation = boatHeading;
                float width = Mathf.Sin(phase / 2.6f * Mathf.PI) * (1.1f + phase * .35f);
                trail.localScale = new Vector3(Mathf.Max(.01f, width), .025f, .10f);
            }
        }

        private void BuildMenu()
        {
            menu = new GameObject("Menu_Iniciar_Salir", typeof(RectTransform));
            menu.transform.position = mission.head.position + Vector3.forward * 2f;
            menu.transform.rotation = Quaternion.identity;
            menu.transform.localScale = Vector3.one * .002f;
            ((RectTransform)menu.transform).sizeDelta = new Vector2(1200, 1000);
            var canvas = menu.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mission.head.GetComponent<Camera>();
            TextLabel(menu.transform, "EL FARO\nDE LOS ECOS", new Vector2(-260, 300), new Vector2(650, 180), 64);
            TextLabel(menu.transform, "Una luz puede traerlos de vuelta", new Vector2(-250, 180), new Vector2(660, 60), 27);
            startButton = Button("INICIAR", new Vector2(-220, -265), out startImage);
            exitButton = Button("SALIR", new Vector2(220, -265), out exitImage);
            TextLabel(menu.transform, "Mira una opción hasta completar el aro", new Vector2(0, -365), new Vector2(1000, 65), 28);
        }

        private void ShowStory()
        {
            // Se conserva el faro y el barco animado detrás de la tarjeta.
            foreach (Transform child in menu.transform)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
            exitButton = null;
            exitImage = null;
            menu.name = "Historia_antes_de_la_mision";
            var panel = new GameObject("Fondo_historia", typeof(RectTransform));
            panel.layer = 2;
            var rect = (RectTransform)panel.transform;
            rect.SetParent(menu.transform, false);
            rect.localPosition = new Vector3(0, 0, 5);
            rect.sizeDelta = new Vector2(1000, 720);
            var background = panel.AddComponent<Image>();
            background.color = new Color(.015f, .055f, .085f, .97f);
            background.raycastTarget = false;
            TextLabel(menu.transform, "UNA LUZ EN LA NIEBLA", new Vector2(0, 275), new Vector2(900, 75), 42);
            TextLabel(menu.transform,
                "La niebla ha cubierto la costa y un barco espera mar adentro, sin encontrar el camino al muelle.\n\n" +
                "El faro se ha apagado. Recupera su energía y orienta la luz para guiar a la tripulación a salvo.",
                new Vector2(0, 65), new Vector2(860, 330), 37);
            startButton = Button("COMENZAR MISIÓN", new Vector2(0, -200), out startImage, 700, 38);
            TextLabel(menu.transform, "Lee la historia y luego mira COMENZAR MISIÓN", new Vector2(0, -300), new Vector2(940, 55), 25);
            storyVisible = true;
            waitForLookAway = true;
            dwell.Reset();
            gameGaze.reticle.Show(false, 0);
            Physics.SyncTransforms();
        }

        private Collider Button(string label, Vector2 at, out Image image, float width = 390, int fontSize = 46)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.layer = 8;
            var rect = (RectTransform)go.transform;
            rect.SetParent(menu.transform, false);
            rect.localPosition = new Vector3(at.x, at.y, 0);
            rect.sizeDelta = new Vector2(width, 115);
            image = go.AddComponent<Image>(); image.color = idle; image.raycastTarget = false;
            TextLabel(rect, label, Vector2.zero, new Vector2(width - 10, 105), fontSize);
            var collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(width, 115, 4);
            return collider;
        }

        private void TextLabel(Transform parent, string value, Vector2 at, Vector2 size, int fontSize)
        {
            var go = new GameObject("Texto_menu", typeof(RectTransform)); go.layer = 2;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.localPosition = new Vector3(at.x, at.y, -3);
            rect.sizeDelta = size;
            var text = go.AddComponent<Text>();
            text.font = mission.objective.font; text.text = value;
            text.fontSize = fontSize; text.fontStyle = FontStyle.Bold;
            text.color = Color.white; text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate; text.raycastTarget = false;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(.02f, .06f, .09f, .9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private Material MaterialFrom(Material source, Color color)
        {
            var material = new Material(source); material.color = color;
            materials.Add(material); return material;
        }
        private GameObject Primitive(string label, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type); go.name = label; go.layer = 2;
            go.transform.SetParent(stage.transform, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(go.GetComponent<Collider>()); return go;
        }
        private void Cleanup()
        {
            if (stage) { stage.SetActive(false); Destroy(stage); stage = null; }
            if (menu) { menu.SetActive(false); Destroy(menu); menu = null; }
            foreach (var material in materials) if (material) Destroy(material);
            materials.Clear(); menuBoat = null;
        }
        private void OnDestroy() { Cleanup(); }
    }
}
