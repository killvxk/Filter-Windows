using System;
using System.Threading.Tasks;

namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public interface IApplicationUpdater
    {
        Task<IApplicationUpdate> CheckForUpdate(string updateChannel = null);
    }
}
