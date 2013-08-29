using System;
﻿using FubuTransportation.ErrorHandling;

namespace FubuTransportation.Runtime.Invocation
{
    public interface IMessageCallback
    {
        void MarkSuccessful();
        void MarkFailed();

        void MoveToDelayedUntil(DateTime time);
        void MoveToErrors(ErrorReport report);
        void Requeue();
    }
}