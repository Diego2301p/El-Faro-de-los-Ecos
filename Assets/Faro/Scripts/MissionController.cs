using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Faro
{
    public class MissionController : MonoBehaviour
    {
        public Transform playerRoot, head, crystal, socket, lensPivot, beaconRotor, boat;
        public ScreenFadeVR fade;
        public WorldDialogue dockDialogue, towerDialogue;
        public GameObject[] powerVisuals;
        public GameObject beam, finalPanel;
        public Text objective, lensReadout;
        public AudioSource effects, dockAmbient, towerAmbient;
        public AudioClip powerClip, tickClip, successClip;
        public bool Busy { get; private set; }
        public MissionState State { get; private set; } = new MissionState();
        private int zone;
        private float timePlaying;
        private bool travellingBoat;
        private bool gameStarted;
        private GameObject completionMenu;
        private bool MenuOpen => completionMenu && completionMenu.activeSelf;
        private IEnumerator Start()
        {
            Busy = true;
            beam.SetActive(false); finalPanel.SetActive(false);
            foreach (var g in powerVisuals) g.SetActive(false);
            lensPivot.localRotation = Quaternion.Euler(0, 0, State.LensAngle);
            dockAmbient.Play(); towerAmbient.Play(); towerAmbient.volume = 0;
            var intro = gameObject.AddComponent<MenuInicioVR>();
            yield return intro.Present(this);
            Destroy(intro);
            UpdateObjective();
            yield return fade.To(0);
            gameStarted = true;
            Busy = false;
            Help();
        }
        private void Update()
        {
            if (gameStarted && !State.Completed) timePlaying += Time.deltaTime;
            if (State.Aligned) beaconRotor.Rotate(0, 22 * Time.deltaTime, 0, Space.World);
            if (travellingBoat) boat.position = Vector3.MoveTowards(boat.position,
                new Vector3(52, -.2f, 1), Time.deltaTime * 1.1f);
        }
        public void ShowHint(string message)
        {
            // El cierre tiene instrucciones propias. Evita superponer las burbujas.
            if (MenuOpen) return;
            if (!string.IsNullOrEmpty(message)) ActiveDialogue().Show(message, 5);
        }
        private WorldDialogue ActiveDialogue() { return zone == 0 ? dockDialogue : towerDialogue; }
        public void Activate(GazeTarget target)
        {
            if (Busy) return;
            switch (target.action)
            {
                case TargetAction.Crystal:
                    if (State.Energize()) StartCoroutine(InstallCrystal(target));
                    else ShowHint("El muelle ya tiene energía. Sigue el círculo ámbar hacia el faro.");
                    break;
                case TargetAction.Lens:
                    if (!State.Powered) { ShowHint("Falta energía: instala el cristal del muelle."); break; }
                    if (State.RotateLens()) StartCoroutine(RotateLens());
                    else ShowHint("Lente alineada. Mira el pulsador SEÑAL para guiar al barco.");
                    break;
                case TargetAction.Teleport:
                    if (target.requiresPower && !State.Powered)
                    { ShowHint("El ascensor está sin energía. Mira el cristal turquesa del muelle."); break; }
                    if (target.destination) StartCoroutine(Teleport(target.destination, target.zone));
                    break;
                case TargetAction.Beacon:
                    if (State.Completed) { ShowCompletionMenu(); }
                    else if (State.Finish()) { Finish(); }
                    else ShowHint(State.Aligned ? "La señal ya fue enviada." : "Gira la lente hasta alinear la aguja turquesa con la marca dorada.");
                    break;
                case TargetAction.Restart: StartCoroutine(Restart()); break;
                case TargetAction.Help: Help(); break;
            }
        }
        private IEnumerator InstallCrystal(GazeTarget target)
        {
            Busy = true;
            target.OnPointerExit();
            Vector3 start = crystal.position;
            for (float t = 0; t < 1; t += Time.deltaTime)
            {
                crystal.position = Vector3.Lerp(start, socket.position, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }
            crystal.position = socket.position;
            foreach (var g in powerVisuals) g.SetActive(true);
            effects.PlayOneShot(powerClip, .65f);
            UpdateObjective();
            dockDialogue.Show("¡Energía restaurada! El cristal alimenta las balizas.\nMira el círculo ámbar FARO para subir a la sala de la lente.");
            Busy = false;
        }
        private IEnumerator RotateLens()
        {
            Busy = true;
            Quaternion start = lensPivot.localRotation;
            Quaternion end = Quaternion.Euler(0, 0, State.LensAngle);
            effects.PlayOneShot(tickClip, .6f);
            for (float t = 0; t < 1; t += Time.deltaTime * 2)
            { lensPivot.localRotation = Quaternion.Slerp(start, end, t); yield return null; }
            lensPivot.localRotation = end;
            lensReadout.text = State.Aligned ? "LENTE ALINEADA" : "GIRO " + State.LensAngle + "°";
            if (State.Aligned)
            {
                beam.SetActive(true);
                effects.PlayOneShot(successClip, .55f);
                towerDialogue.Show("La lente está alineada. El haz vuelve a recorrer el mar.\nMira SEÑAL para avisar al barco y terminar el rescate.");
            }
            else towerDialogue.Show("La aguja aún no apunta a la marca dorada de arriba.\nAparta la mirada y vuelve a mirar la rueda para girar otro cuarto de vuelta.");
            UpdateObjective(); Busy = false;
        }
        private IEnumerator Teleport(Transform destination, int newZone)
        {
            Busy = true;
            yield return fade.To(1);
            if (completionMenu) completionMenu.SetActive(false);
            objective.gameObject.SetActive(true);
            dockDialogue.gameObject.SetActive(true);
            towerDialogue.gameObject.SetActive(true);
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            float yaw = Vector3.SignedAngle(forward, destination.forward, Vector3.up);
            playerRoot.Rotate(0, yaw, 0, Space.World);
            Vector3 offset = head.position - playerRoot.position; offset.y = 0;
            playerRoot.position = destination.position - offset;
#if UNITY_EDITOR || UNITY_STANDALONE
            var mouseLook = playerRoot.GetComponent<EditorMouseLook>();
            if (mouseLook) mouseLook.ResetPitch();
#endif
            zone = newZone;
            dockAmbient.volume = zone == 0 ? .3f : .06f;
            towerAmbient.volume = zone == 1 ? .22f : 0;
            dockDialogue.group.alpha = towerDialogue.group.alpha = 0;
            yield return null;
            yield return fade.To(0);
            Busy = false;
            if (State.Completed)
                ActiveDialogue().Show("Rescate completado. Puedes seguir recorriendo el faro.\nMira ? para abrir el cierre y reiniciar la misión.", 14);
            else Help();
        }
        private void Finish()
        {
            travellingBoat = true;
            // Sustituye el cartel fijo, que podía quedar de lado o detrás del jugador.
            finalPanel.SetActive(false);
            effects.PlayOneShot(successClip, .75f);
            UpdateObjective();
            ShowCompletionMenu();
            Debug.Log("FARO: misión completada en " + Mathf.RoundToInt(timePlaying) + " segundos.");
        }
        private IEnumerator Restart()
        {
            Busy = true; yield return fade.To(1);
            // En el editor también funciona si la escena aún no figura en el build.
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                gameObject.scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(gameObject.scene.buildIndex);
#endif
        }
        private void ShowCompletionMenu()
        {
            if (!completionMenu) BuildCompletionMenu();
            dockDialogue.gameObject.SetActive(false);
            towerDialogue.gameObject.SetActive(false);
            objective.gameObject.SetActive(false);
            // Se coloca una vez en el mundo. No persigue la cabeza en VR.
            completionMenu.transform.SetPositionAndRotation(
                head.position + head.forward * 1.8f,
                Quaternion.LookRotation(head.forward, head.up));
            completionMenu.SetActive(true);
            Physics.SyncTransforms();
        }
        private void BuildCompletionMenu()
        {
            completionMenu = new GameObject("Pantalla_de_cierre", typeof(RectTransform));
            completionMenu.SetActive(false);
            completionMenu.transform.SetParent(transform, false);
            completionMenu.transform.localScale = Vector3.one * .0018f;
            var rect = (RectTransform)completionMenu.transform;
            rect.sizeDelta = new Vector2(1000, 650);
            var canvas = completionMenu.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = head.GetComponent<Camera>();
            var background = completionMenu.AddComponent<Image>();
            background.color = new Color(.018f, .045f, .065f, 1);
            background.raycastTarget = false;
            // El fondo impide activar la rueda o la señal a través del menú.
            var blocker = completionMenu.AddComponent<BoxCollider>();
            blocker.size = new Vector3(1000, 650, 2);
            blocker.center = new Vector3(0, 0, 4);
            MenuText(rect, "MISIÓN COMPLETADA", new Vector2(0, 225),
                new Vector2(930, 100), 62, new Color(.35f, 1f, .83f));
            MenuText(rect, "¡El barco llegó a salvo!\nRestauraste la energía y alineaste la lente.",
                new Vector2(0, 100), new Vector2(920, 125), 38, Color.white);
            MenuText(rect, "Tiempo: " + Mathf.RoundToInt(timePlaying) + " segundos\nMira una opción durante 1,6 segundos.",
                new Vector2(0, -25), new Vector2(920, 100), 32, Color.white);
            MenuButton("REINICIAR\nMISIÓN", new Vector2(-235, -190), TargetAction.Restart, null);
            // Reutiliza el destino real del muelle, sin depender del nombre del objeto.
            Transform dockDestination = null;
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                foreach (var target in root.GetComponentsInChildren<GazeTarget>(true))
                    if (target.mission == this && target.action == TargetAction.Teleport &&
                        target.zone == 0 && target.destination)
                    { dockDestination = target.destination; break; }
                if (dockDestination) break;
            }
            if (dockDestination)
                MenuButton("VOLVER\nAL MUELLE", new Vector2(235, -190), TargetAction.Teleport, dockDestination);
            else Debug.LogError("FARO: no se encontró un destino de teletransporte al muelle.");
        }
        private void MenuButton(string label, Vector2 position, TargetAction action, Transform destination)
        {
            var button = new GameObject("Opcion_" + action, typeof(RectTransform));
            button.layer = 8;
            var rect = (RectTransform)button.transform;
            rect.SetParent(completionMenu.transform, false);
            rect.sizeDelta = new Vector2(430, 150);
            rect.localPosition = new Vector3(position.x, position.y, -8);
            var image = button.AddComponent<Image>();
            image.color = new Color(.035f, .30f, .36f, 1);
            image.raycastTarget = false;
            var collider = button.AddComponent<BoxCollider>();
            collider.size = new Vector3(430, 150, 4);
            var target = button.AddComponent<GazeTarget>();
            target.action = action; target.mission = this;
            target.destination = destination; target.zone = 0;
            target.hint = "";
            MenuText(rect, label, Vector2.zero, new Vector2(410, 140), 38, Color.white);
        }
        private void MenuText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var go = new GameObject("Texto_cierre", typeof(RectTransform));
            go.layer = 2;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            rect.localPosition = new Vector3(position.x, position.y, -2);
            var text = go.AddComponent<Text>();
            text.font = objective.font;
            text.text = value; text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold; text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
        }
        private void UpdateObjective()
        {
            objective.text = State.Completed ? "RESCATE COMPLETADO" : !State.Powered ?
                "1 / 2   RESTAURA LA ENERGÍA" : !State.Aligned ? "2 / 2   ALINEA LA LENTE" : "ENVÍA LA SEÑAL";
        }
        private void Help()
        {
            if (State.Completed) { ShowCompletionMenu(); return; }
            if (zone == 0)
                dockDialogue.Show(State.Powered ? "Las balizas ya tienen energía. Mira FARO para subir.\nLos círculos del suelo permiten viajar sin caminar." :
                    "Un barco está perdido en la niebla. Devuelve la luz al faro.\nMira el cristal turquesa durante 1,6 segundos para instalarlo.\nLos círculos permiten teletransportarte; ? repite la ayuda.", 22);
            else towerDialogue.Show(State.Aligned ? "Mira SEÑAL para guiar al barco. El regreso al muelle sigue disponible." :
                "La lente está desviada. Mira la rueda para girarla 90°.\nAlinea la aguja turquesa con la marca dorada de arriba.\nEntre giros, aparta la mirada para volver a cargar.", 22);
        }
    }
}
