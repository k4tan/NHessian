using System;

namespace NHessian.IO
{
    /// <summary>
    /// Exception thrown if a tag (byte) was encountered
    /// that is not supported at that location.
    /// </summary>
    public class UnsupportedTagException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UnsupportedTagException"/>.
        /// </summary>
        /// <param name="location">
        /// The location at which the tag was encountered.
        /// </param>
        /// <param name="tag">
        /// The unsupported tag .
        /// </param>
        public UnsupportedTagException(string location, int tag)
            : base($"Unsupported tag 0x{tag:X2} ('{(char)tag}') received for {location}")
        {
        }
    }
}