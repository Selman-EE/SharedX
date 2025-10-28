namespace SharedX.Core.Constants
{
    /// <summary>
    /// Common constants used across microservices
    /// </summary>
    public static class CommonConstants
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string IsoDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        
        public static class Headers
        {
            public const string CorrelationId = "X-Correlation-ID";
            public const string RequestId = "X-Request-ID";
            public const string UserId = "X-User-ID";
        }

        public static class Environment
        {
            public const string Development = "Development";
            public const string Staging = "Staging";
            public const string Production = "Production";
        }
    }
}
