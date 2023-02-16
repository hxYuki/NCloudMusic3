using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCloudMusic3.Helpers
{
    internal class DownTick
    {
        Action timeoutAction;
        int milliseconds;

        Task time = Task.CompletedTask;

        public DownTick(int milliseconds, Action timeoutAction)
        {
            this.milliseconds = milliseconds;
            this.timeoutAction = timeoutAction;
        }

        CancellationTokenSource ct;
        public void Touch()
        {
            if(!time.IsCompleted)
            {
                ct.Cancel(); ct.Dispose();
            }
            ct = new();
            time = Task.Delay(milliseconds, ct.Token).ContinueWith(t => timeoutAction(), ct.Token);
        }
    }
}
