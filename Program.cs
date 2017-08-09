using System.ServiceProcess;
using StructureMap;

// Enable log4net configuration file
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Log4Net.config", Watch = true)]

namespace MailImporter
{
    public class MyRegistry : Registry
    {
        public MyRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });
        }
    }

    static class Program
    {
        static void Main()
        {
            IContainer container = Container.For<MyRegistry>();
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                container.GetInstance<MailService>()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
