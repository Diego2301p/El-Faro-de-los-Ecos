namespace Faro
{
    // Lógica independiente de Unity: se puede probar sin una escena.
    public sealed class MissionState
    {
        public bool Powered { get; private set; }
        public int LensAngle { get; private set; } = 90;
        public bool Aligned { get; private set; }
        public bool Completed { get; private set; }

        public bool Energize()
        {
            if (Powered || Completed) return false;
            Powered = true;
            return true;
        }
        public bool RotateLens()
        {
            if (!Powered || Aligned || Completed) return false;
            LensAngle = (LensAngle + 90) % 360;
            Aligned = LensAngle == 0;
            return true;
        }
        public bool Finish()
        {
            if (!Powered || !Aligned || Completed) return false;
            Completed = true;
            return true;
        }
    }
}
