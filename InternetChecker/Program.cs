using InternetChecker.Core;
using System.Threading.Tasks;
using log4net.Repository;
using System.Reflection;
using System.IO;

namespace InternetChecker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ILoggerRepository repository = log4net.LogManager.GetRepository(Assembly.GetCallingAssembly());
            var fileInfo = new FileInfo(@"log4net.config");
            log4net.Config.XmlConfigurator.Configure(repository, fileInfo);

            var checker = new ConnectionChecker(new DumbStatusNotifier());
            await checker.Run();
        }


        class DumbStatusNotifier : IStatusChangeNotifier
        {
            public void Notify(bool isAllGood)
            {
            }
        }
    }
}
