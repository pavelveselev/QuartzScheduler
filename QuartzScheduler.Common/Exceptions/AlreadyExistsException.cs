﻿using System;

namespace QuartzScheduler.Common.Exceptions
{
    public class AlreadyExistsException : Exception
    {
        public AlreadyExistsException() : base() { }

        public AlreadyExistsException(string message) : base(message) { }
    }
}
