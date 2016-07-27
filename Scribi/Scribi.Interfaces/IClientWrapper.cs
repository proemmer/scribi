using System.Collections.Generic;

namespace Scribi.Interfaces
{
    public interface IClientWrapper<TClient> where TClient : class
    {
        TClient All { get; }

        TClient AllExcept(params string[] excludeConnectionIds);
        TClient Client(string connectionId);
        TClient Clients(IList<string> connectionIds);
        TClient Group(string groupName, params string[] excludeConnectionIds);
        TClient Groups(IList<string> groupNames, params string[] excludeConnectionIds);
        TClient User(string userId);
        TClient Users(IList<string> userIds);
    }
}