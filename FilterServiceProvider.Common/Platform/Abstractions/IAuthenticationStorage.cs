using System;
namespace FilterServiceProvider.Common.Platform.Abstractions
{
    public interface IAuthenticationStorage
    {
        string AuthToken { get; set; }
        string UserEmail { get; set; }
    }
}
