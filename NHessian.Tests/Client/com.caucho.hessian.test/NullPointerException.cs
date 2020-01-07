//-----------------------------------------------------------------------
// <copyright file="A2.cs" company="INFORM GmbH">
//     Copyright (c) INFORM GmbH. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using System;

namespace java.lang
{
    public class NullPointerException : Exception
    {
        public NullPointerException()
        {

        }

        public NullPointerException(string message)
            : base(message)
        {

        }

        public NullPointerException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
