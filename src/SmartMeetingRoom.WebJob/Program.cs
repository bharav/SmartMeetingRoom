using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace SmartMeetingRoom.WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            JobHostConfiguration config = new JobHostConfiguration();
            config.UseServiceBus();
            config.DashboardConnectionString = ""; // performance optimization
#if DEBUG
            config.Queues.BatchSize = 1;
#endif
            // The following code ensures that the WebJob will be running continuously
            var host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
