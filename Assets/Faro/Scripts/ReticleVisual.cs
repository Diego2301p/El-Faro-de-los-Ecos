using UnityEngine;
namespace Faro
{
    public class ReticleVisual : MonoBehaviour
    {
        public LineRenderer rim;
        public LineRenderer progress;
        public void Show(bool active, float amount)
        {
            Color c = active ? new Color(.35f, 1f, .8f) : Color.white;
            rim.startColor = rim.endColor = c;
            int count = Mathf.Clamp(Mathf.CeilToInt(amount * 48), 0, 48);
            progress.enabled = active && count > 0;
            progress.positionCount = count + 1;
            for (int i = 0; i <= count; i++)
            {
                float a = i / 48f * Mathf.PI * 2;
                progress.SetPosition(i, new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0) * .024f);
            }
        }
    }
}
