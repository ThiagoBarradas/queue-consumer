using RabbitMQ.Client;

namespace QueueConsumer.Queue
{
    public static class ChannelFactory
    {
        private static readonly object Lock = new object();

        private static IConnection Connection;

        private static IModel Model;

        public static IModel Create(ConnectionFactory factory)
        {
            if (Connection == null)
            {
                lock (Lock)
                {
                    if (Connection == null)
                    {
                        Connection = factory.CreateConnection();
                    }
                }
            }

            if (Model == null)
            {
                lock (Lock)
                {
                    if (Model == null)
                    {
                        Model = Connection.CreateModel();
                    }
                }
            }

            return Model;
        }

        public static void CloseConnection()
        {
            if (Connection != null)
            {
                lock (Lock)
                {
                    if (Connection != null)
                    {
                        Connection.Close();
                        Connection.Dispose();
                        Connection = null;
                    }
                }
            }
        }
    }
}
