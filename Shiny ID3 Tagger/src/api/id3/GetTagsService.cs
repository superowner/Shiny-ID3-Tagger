namespace GetTags
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using GlobalVariables;

    public interface IGetTagsService
    {
        Task<Id3> GetTags(
            HttpMessageInvoker client,
            string artist,
            string title,
            CancellationToken cancelToken);
    }
}