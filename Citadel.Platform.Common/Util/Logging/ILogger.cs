using System;
namespace Citadel.Platform.Common.Util
{
    public interface IAppLogger
    {
        void Debug(string message);
        void Debug(string message, params object[] args);

        void Info(string message);
        void Info(string message, params object[] args);

        void Warn(string message);

        void Error(string message);
        void Error(Exception exception);
        void Error(string message, params object[] args);
    }
}
