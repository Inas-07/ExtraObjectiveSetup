namespace ExtraObjectiveSetup.Utils
{
    public class EOSColor
    {
        public float r { get; set; }

        public float g { get; set; }

        public float b { get; set; }

        public float a { get; set; } = 1.0f;

        public UnityEngine.Color ToUnityColor() => new UnityEngine.Color(r, g, b, a);

        public EOSColor() { }
    }
}
