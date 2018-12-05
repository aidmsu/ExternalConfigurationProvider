namespace LUV.NetCoreServiceExample
{
    public class FakeMangoClient
    {
        public FakeMangoClient(string apiKey, string apiSignatureKey)
        {
            ApiKey = apiKey;
            ApiSignatureKey = apiSignatureKey;
        }

        public string ApiKey { get; }
        public string ApiSignatureKey { get; }
    }
}