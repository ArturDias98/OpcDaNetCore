using Opc.Da;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpcDaNetCore.Factory.Services
{
    internal partial class OpcDaService
    {
        private bool TryConnect()
        {
            try
            {
                LockServer().Connect();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Connect()
        {
            if (IsConnected)
            {
                throw new Exception("The system is already connected");
            }

            return TryConnect();
        }

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(TryConnect, cancellationToken);
        }

        public bool Disconnect()
        {
            if (!IsConnected)
            {
                throw new Exception("The system is not connected");
            }

            try
            {
                LockServer().Disconnect();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            foreach (Subscription item in LockServer().Subscriptions)
            {
                item.DataChanged -= Subscription_DataChanged;
                item.Dispose();
            }

            LockServer().Dispose();
        }
    }
}