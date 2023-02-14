using Microsoft.Extensions.Logging;

using Opc.Ua.Cloud.Library.Client;
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

        public async Task<UANameSpace?> GetAsync(string identifier)
        {
            GraphQlResult<Nodeset> result;
            result = await _client.GetNodeSetsAsync(identifier: identifier, noRequiredModels: true, noTotalCount: true);
            return result?.Nodes?.FirstOrDefault()?.Metadata;
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
            if (result.Status != System.Net.HttpStatusCode.OK)
            {
                throw new UploadException(result.Message);
            }
            return result.Message;
        }

        public async Task<GraphQlResult<Nodeset>> GetNodeSetsPendingApprovalAsync(int? limit, string cursor, bool pageBackwards, bool noTotalCount = false, UAProperty? prop = null)
        {
            GraphQlResult<Nodeset> result;
            if (!pageBackwards)
            {
                result = await _client.GetNodeSetsPendingApprovalAsync(after: cursor, first: limit, noTotalCount: noTotalCount, noRequiredModels: true, noMetadata: false, additionalProperty: prop);
            }
            else
            {
                result = await _client.GetNodeSetsPendingApprovalAsync(before: cursor, last: limit, noTotalCount: noTotalCount, noRequiredModels: true, noMetadata: false, additionalProperty: prop);
            }
            return result;
        }

        public Task<UANameSpace?> UpdateApprovalStatusAsync(string nodeSetId, string newStatus, string statusInfo, UAProperty? additionalProperty = null)
        {
            return _client.UpdateApprovalStatusAsync(nodeSetId, newStatus, statusInfo, additionalProperty);
        }
    }

}
