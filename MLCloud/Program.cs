using System.Threading.Tasks;

namespace MLCloud
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var azureClient = new AzureClient();
            await azureClient.ExecuteCall();
        }
    }
}
