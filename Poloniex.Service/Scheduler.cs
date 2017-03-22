using Poloniex.Service.Framework;
using System.ServiceProcess;

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
            SchedulerHelper.Start();
        }

        protected override void OnStop()
        {
            SchedulerHelper.Stop();
        }
    }
}
