namespace NHessian.IO.Utils
{
    /// <summary>
    /// Hessian 2.0 has a dedicated representation for '0.0' and '1.0'.
    /// This class defines boxes for those two special cases for efficiency.
    /// </summary>
    internal static class DoubleBoxes
    {
        public static object One { get; } = 1.0d;
        public static object Zero { get; } = 0.0d;
    }
}