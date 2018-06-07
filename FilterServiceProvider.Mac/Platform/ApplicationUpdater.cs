using System;
using System.Threading.Tasks;
using FilterServiceProvider.Common.Platform.Abstractions;

namespace FilterServiceProvider.Mac.Platform
{
    public class ApplicationUpdater : IApplicationUpdater
    {
        public ApplicationUpdater()
        {
        }

        public async Task<IApplicationUpdate> CheckForUpdate(string updateChannel = null)
        {
            // TODO: Implement when that time comes.
            return null;
        }
    }
}
