using Poloniex.Service.Framework;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Poloniex.Service
{
    public partial class Scheduler : ServiceBase
    {
        public Scheduler()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Task.Run(() =>
            {
                SchedulerHelper.Start();
            });
        }

        protected override void OnStop()
        {
            SchedulerHelper.Stop();
        }
    }
}
