using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalRService.Hubs
{
    /// <summary>
    /// ChatHub 继承自 Hub，从下面 Hub 的接口图可以看出：Hub 支持向发所有客户端(Clients(子属性Caller,Other,All))，特定组（Group) 推送消息。
    /// </summary>
    [HubName("ChatsHub")]
    //HubName属性提供给客户端调用的对象名称(调用时小写) 不设置则默认为该类名
    // HubName 这个特性是为了让客户端知道如何建立与服务器端对应服务的代理对象，如果没有设定该属性，则以服务器端的服务类名字作为 HubName 的缺省值
    public class ChatHub : Hub
    {
        public void TestSend(string clientName, string message)
        {
            Clients.All.addSomeMessage(clientName, message);
            //表示服务器端调用客户端的 addSomeMessage 方法，这是一个 Javascript 方法，从而给客户端推送消息。
        }
    }
}
