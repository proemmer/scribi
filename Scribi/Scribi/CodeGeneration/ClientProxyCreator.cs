using Scribi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scribi.CodeGeneration
{
    /// <summary>
    /// TODO How to call Signal R  Client methods from user code!!!???
    /// </summary>
    internal static class ClientProxyCreator
    {
        #region Controller Creation
        private const string HubTemplate =
        @"
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Threading.Tasks;
        using Microsoft.AspNetCore.SignalR;
        using Microsoft.AspNetCore.SignalR.Hubs;
        using Microsoft.Extensions.DependencyInjection;
        using Scribi.Interfaces;

        namespace Scribi.ProxyClient
        {{
            public class {0}ClientProxy : IClientProxy<{1}>
            {{
                private readonly IHubContext<{2},{1}> _obj;

                public {0}ClientProxy(IScriptCreatorService ccs)
                {{
                    //To get the selfe registred services, because asp.net 
                    //service provider did not update at runtime
                    _obj = ccs.ServiceProvider.GetRequiredService(typeof(IHubContext<{2},{1}>)) as IHubContext<{2},{1}>;
                }}


                public {1} All
                {{
                    get
                    {{
                        return _obj?.Clients.All;
                    }}
                }}

                public {1} AllExcept(params string[] excludeConnectionIds)
                {{
                    return _obj?.Clients.AllExcept(excludeConnectionIds);
                }}

                public {1} Client(string connectionId)
                {{
                    return _obj?.Clients.Client(connectionId);
                }}

                public {1} Clients(IList<string> connectionIds)
                {{
                    return _obj?.Clients.Clients(connectionIds);
                }}

                public {1} Group(string groupName, params string[] excludeConnectionIds)
                {{
                    return _obj?.Clients.Group(groupName, excludeConnectionIds);
                }}

                public {1} Groups(IList<string> groupNames, params string[] excludeConnectionIds)
                {{
                    return _obj?.Clients.Groups(groupNames, excludeConnectionIds);
                }}

                public {1} User(string userId)
                {{
                    return _obj?.Clients.User(userId);
                }}

                public {1} Users(IList<string> userIds)
                {{
                    return _obj?.Clients.Users(userIds);
                }}
            }}
        }}
        ";
        #endregion


        public static string Create(Type tHub, Type tClient)
        {
            return string.Format(HubTemplate, tHub.Name, tClient, tHub);
        }
    }
}

