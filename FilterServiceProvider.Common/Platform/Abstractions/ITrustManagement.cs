using System;
namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public interface ITrustManagement
    {
        /// <summary>
        /// This should do anything necessary to establish trust with all programs on the computer.
        /// 
        /// </summary>
        void EstablishTrust();
    }
}
