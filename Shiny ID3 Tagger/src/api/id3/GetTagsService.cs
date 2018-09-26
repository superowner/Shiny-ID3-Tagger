using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GlobalNamespace;

namespace GetTags
{
    public interface GetTagsService
    {
        Task<Id3> GetTags(
            HttpMessageInvoker client,
            string artist,
            string title,
            CancellationToken cancelToken);
    }
}