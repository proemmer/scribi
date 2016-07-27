using System;

namespace Scribi.Attributes
{
    public enum HubMethodType {
        Connected,  //Method will be called if an client was connected
        Disconnected, //Method will be called if an client was disconnected
        //Server, //Methode will be called from the client
        Client //Servercalls this Method on the client
    };

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HubMethodAttribute : Attribute
    {
        public HubMethodType Type { get; set; }

        public HubMethodAttribute(HubMethodType type)
        {
            Type = type;
        }
    }
}
