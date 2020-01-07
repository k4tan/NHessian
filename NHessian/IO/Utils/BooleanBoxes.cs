namespace NHessian.IO.Utils
{
    internal static class BooleanBoxes
    {
        public static object FalseBox { get; } = false;
        public static object TrueBox { get; } = true;

        public static object Box(bool value) => value ? TrueBox : FalseBox;
    }
}