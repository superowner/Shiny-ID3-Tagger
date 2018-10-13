namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json.Linq;

	public interface TagsService
	{
		async Task<Id3> GetTags_QQ(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken);
    }
}