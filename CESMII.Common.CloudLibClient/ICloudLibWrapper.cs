using Opc.Ua.Cloud.Library.Client;
using System.Runtime.Serialization;

namespace CESMII.Common.CloudLibClient
{
    public interface ICloudLibWrapper
    {
        Task<GraphQlResult<Nodeset>> SearchAsync(int? limit, string cursor, bool pageBackwards, List<string> keywords, List<string> exclude, bool noTotalCount);
        Task<GraphQlResult<Nodeset>> GetManyAsync(List<string> identifiers);
        Task<UANameSpace?> DownloadAsync(string id);
        Task<UANameSpace?> GetAsync(string identifier);
        Task<UANameSpace?> GetAsync(string modelUri, DateTime? publicationDate, bool exactMatch);
        Task<string> UploadAsync(UANameSpace uaNamespace);
        Task<GraphQlResult<Nodeset>> GetNodeSetsPendingApprovalAsync(int? limit, string cursor, bool pageBackwards, bool noTotalCount = false, UAProperty? prop = null);
        Task<UANameSpace?> UpdateApprovalStatusAsync(string nodeSetId, string newStatus, string statusInfo, UAProperty? additionalProperty = null);

    }

    [Serializable]
    public class UploadException : Exception
    {
        public UploadException()
        {
        }

        public UploadException(string? message) : base(message)
        {
        }

        public UploadException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UploadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
