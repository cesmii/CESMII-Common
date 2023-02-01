﻿using System;
using System.Collections.Generic;

using Opc.Ua.Cloud.Library.Client;
using CESMII.OpcUa.NodeSetImporter;

namespace CESMII.Common.CloudLibClient
{
    public interface ICloudLibWrapper
    {
        Task<GraphQlResult<Nodeset>> SearchAsync(int? limit, string cursor, bool pageBackwards, List<string> keywords, List<string> exclude, bool noTotalCount);
        Task<UANameSpace?> DownloadAsync(string id);
        Task<UANameSpace?> GetAsync(string modelUri, DateTime? publicationDate, bool exactMatch);
        Task<string> UploadAsync(UANameSpace uaNamespace);
    }
}
