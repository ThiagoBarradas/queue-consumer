using RabbitMQ.Client;

namespace QueueConsumer.Queue
{
    public static class ChannelFactory
    {
        private static readonly object Lock = new object();

        private static IConnection Connection;

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

            return Connection.CreateModel();
        }

        public static void CloseConnection()
        {
            Connection.Close();
            Connection.Dispose();
            Connection = null;
        }
    }
}
