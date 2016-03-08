
namespace OrderProcessing.WorkNode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using OrderProcessing.Domain.Request;
    using OrderProcessing.DataAccessor;
    using OrderProcessing.Logger;
    using OrderProcessing.Domain;
    /// <summary>
    /// This class is used for simulate a user to periodcally submit create request
    /// </summary>
    public class RequestSpawner :BaseDisposable
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private const int SPAWN_COUNT_EACH_TIME = 20;
        private const int SPAWN_TIME_INTERVAL_SECONDS = 5;
        public void Start()
        {
            Task.Factory.StartNew(() => SpawnRequests(cancellationTokenSource), cancellationTokenSource.Token);
        }

        private void SpawnRequests(CancellationTokenSource cancellationTokenSource)
        {
            while (!this.cancellationTokenSource.IsCancellationRequested)
            {
                for(int i=0; i<SPAWN_COUNT_EACH_TIME; ++i)
                {
                    if (this.cancellationTokenSource.IsCancellationRequested) return;
                    OrderCreationRequest request = new OrderCreationRequest(){OrderDetail = "", TrackingId = Guid.NewGuid().ToString()};
                    DataAccessor.OrderRepository.CreateOrderProcessingInfo(request);
                }
                cancellationTokenSource.Token.WaitHandle.WaitOne(SPAWN_TIME_INTERVAL_SECONDS* 1000);
            }
        }

        public void Stop()
        {
            Logger.LogInformation("RequestSpawner Stop...");
            cancellationTokenSource.Cancel();
        }

        protected override void Disposing()
        {
            this.Stop();
            cancellationTokenSource.Dispose();
        }
    }
}
