using System;

public partial class ConnectionController
{
    [Serializable]
    private struct SerializableConnection
    {
        public string from;
        public string to;
        public ConnectionType type;

        public static implicit operator Connection(SerializableConnection serializable)
        {
            return new Connection
            {
                from = new Guid(serializable.from),
                to = new Guid(serializable.to),
                type = serializable.type
            };
        }

        public static implicit operator SerializableConnection(Connection connection)
        {
            return new SerializableConnection
            {
                from = connection.from.ToString(),
                to = connection.to.ToString(),
                type = connection.type
            };
        }
    }
}
