using System;
namespace Faro
{
    public sealed class DwellTimer
    {
        private int target;
        private float elapsed;
        private bool fired;
        public float Progress { get; private set; }
        public bool Tick(int targetId, float delta, float duration, bool manual)
        {
            if (targetId != target) { Reset(); target = targetId; }
            if (targetId == 0 || fired) return false;
            elapsed += Math.Max(0, delta);
            Progress = Math.Min(1, elapsed / Math.Max(.1f, duration));
            if (!manual && Progress < 1) return false;
            fired = true;
            Progress = 1;
            return true;
        }
        public void Reset() { target = 0; elapsed = 0; fired = false; Progress = 0; }
    }
}
