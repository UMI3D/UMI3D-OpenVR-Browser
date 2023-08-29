namespace com.inetum.addonEulen.common
{
    public static class EulenEndPoint
    {
        public const string baseUrl = "/eulen/";

        public const string getTest = baseUrl + "test";

        public const string postRecord = baseUrl + "post/record/:param";

        public const string getEndSession= baseUrl + "get/endSession";
    }
}
