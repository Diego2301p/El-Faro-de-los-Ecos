using UnityEngine;
namespace Faro
{
    public enum TargetAction { Crystal, Lens, Teleport, Beacon, Restart, Help }
    public class GazeTarget : MonoBehaviour
    {
        public TargetAction action;
        public MissionController mission;
        public Transform destination;
        public bool requiresPower;
        public int zone;
        [TextArea] public string hint;
        public Renderer highlight;
        private MaterialPropertyBlock block;
        private Color original;
        private Vector3 scale;
        private bool focused;
        private void Awake()
        {
            scale = transform.localScale;
            block = new MaterialPropertyBlock();
            if (highlight) original = highlight.sharedMaterial.color;
        }
        public void OnPointerEnter()
        {
            focused = true;
            if (highlight) { block.SetColor("_Color", new Color(.5f, 1f, .9f)); highlight.SetPropertyBlock(block); }
            if (mission) mission.ShowHint(hint);
        }
        public void OnPointerExit()
        {
            focused = false;
            transform.localScale = scale;
            if (highlight) { block.SetColor("_Color", original); highlight.SetPropertyBlock(block); }
        }
        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scale * (focused ? 1.045f : 1f), Time.deltaTime * 10);
        }
        public void OnPointerClick() { if (mission) mission.Activate(this); }
        private void OnDisable() { if (block != null) OnPointerExit(); }
    }
}
