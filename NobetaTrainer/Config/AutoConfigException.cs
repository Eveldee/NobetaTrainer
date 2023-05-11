using System;
using JetBrains.Annotations;

namespace NobetaTrainer.Config;

public abstract class AutoConfigException : Exception
{
    protected AutoConfigException(string message) : base(message)
    {

    }
}