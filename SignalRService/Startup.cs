using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Owin;
using Microsoft.Owin;
[assembly: OwinStartup(typeof(SignalRService.Startup))]
namespace SignalRService
{
   public class Startup
    {
       public void Configuration(IAppBuilder app)
       {
           // Any connection or hub wire up and configuration should go here
           app.MapSignalR();
       }
    }
}
