using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Opc.Ua.Cloud.Library.Client;
using CESMII.OpcUa.NodeSetImporter;
using Microsoft.Extensions.Options;

namespace CESMII.Common.CloudLibClient
{
    public class CloudLibWrapper : ICloudLibWrapper
    {
        private readonly UACloudLibClient _client;
        private readonly ILogger<CloudLibWrapper> _logger;

        public CloudLibWrapper(IOptions<UACloudLibClient.Options> cloudLibOptions, ILogger<CloudLibWrapper> logger)
        {
            _logger = logger;

            //initialize cloud lib client
            _client = new UACloudLibClient(cloudLibOptions.Value);
        }

        public async Task<GraphQlResult<Nodeset>> SearchAsync(int? limit, string cursor, bool pageBackwards, List<string> keywords, List<string> exclude, bool noTotalCount = false)
        {
            GraphQlResult<Nodeset> result;
            if (!pageBackwards)
            {
                result = await _client.GetNodeSetsAsync(keywords: keywords?.ToArray(), after: cursor, first: limit, noTotalCount: noTotalCount, noRequiredModels: true, noMetadata: false);
            }
            else
            {
                result = await _client.GetNodeSetsAsync(keywords: keywords?.ToArray(), before: cursor, last: limit, noTotalCount: noTotalCount, noRequiredModels: true, noMetadata: false);
            }
            return result;
        }

        public async Task<UANameSpace?> DownloadAsync(string id)
        {
            var result = await _client.DownloadNodesetAsync(id).ConfigureAwait(false);
            return result;
        }

        public async Task<UANameSpace?> GetAsync(string modelUri, DateTime? publicationDate, bool exactMatch)
        {
            uint? id;
            var nodeSetResult = await _client.GetNodeSetsAsync(namespaceUri: modelUri, publicationDate: publicationDate);
            id = nodeSetResult.Edges?.FirstOrDefault()?.Node?.Identifier;

            if (id == null && !exactMatch)
            {
                nodeSetResult = await _client.GetNodeSetsAsync(namespaceUri: modelUri);
                id = nodeSetResult.Edges?.OrderByDescending(n => n.Node.PublicationDate).FirstOrDefault(n => n.Node.PublicationDate >= publicationDate)?.Node?.Identifier;
            }
            if (id == null)
            {
                return null;
            }

            var uaNamespace = await _client.DownloadNodesetAsync(id.ToString());
            return uaNamespace;
        }

        public async Task<string> UploadAsync(UANameSpace uaNamespace)
        {
            var result = await _client.UploadNodeSetAsync(uaNamespace);
            if (result.Status == System.Net.HttpStatusCode.OK)
            {
                #pragma warning disable 8603
                return null;
                #pragma warning restore 8603
            }
            return result.Message;
        }
    }
}
