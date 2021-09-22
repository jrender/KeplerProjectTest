using KeplerProjectTemplate1.Interfaces.LegilityTest.v1.Models;
using Relativity.Kepler.Services;
using Relativity.Environment.V1.LibraryApplication.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Audit.Services.Interfaces.V1.Metrics.Models;

namespace KeplerProjectTemplate1.Interfaces.LegilityTest
{
    /// <summary>
    /// MyService Service Interface.
    /// </summary>
    [WebService("JRTestService Service")]
    [ServiceAudience(Audience.Public)]
    [RoutePrefix("JRTestService")]
    public interface IJRTestService : IDisposable
    {
        /// <summary>
        /// Get workspace name.
        /// </summary>
        /// <param name="workspaceID">Workspace ArtifactID.</param>
        /// <returns><see cref="JRTestServiceModel"/> with the name of the workspace.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [GET] /Relativity.REST/api/LegilityTest/v1/JRTestService/workspace/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpGet]
        [Route("workspace/{workspaceID:int}")]
        Task<JRTestServiceModel> GetWorkspaceNameAsync(int workspaceID);

        /// <summary>
        /// Query for a workspace by name
        /// </summary>
        /// <param name="queryString">Partial name of a workspace to query for.</param>
        /// <param name="limit">Limit the number of results via a query string parameter. (Default 10)</param>
        /// <returns>Collection of <see cref="JRTestServiceModel"/> containing workspace names that match the query string.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [POST] /Relativity.REST/api/LegilityTest/v1/JRTestService/workspace?limit=2
        ///   { "queryString":"a" }
        /// Example REST response:
        ///   [{"Name":"New Case Template"},{"Name":"Relativity Starter Template"}]
        /// </remarks>
        [HttpPost]
        [Route("workspace?{limit}")]
        Task<List<JRTestServiceModel>> QueryWorkspaceByNameAsync(string queryString, int limit = 10);

        [HttpGet]
        [Route("application")]
        Task<ServiceResponse<List<string>>> GetApplications();

        [HttpGet]
        [Route("audit/{workspaceID:int}")]
        Task<ServiceResponse<List<AuditMetricsAggregateResponse>>> GetAuditsForWorkspace(int workspaceID);

        [HttpGet]
        [Route("audit/query/{workspaceID:int}")]
        Task<ServiceResponse<long>> GetLastAuditIdForWorkspace(int workspaceID);

        //[HttpGet]
        //[Route("application/{appGuid:Guid}")]
        //Task<ServiceResponse<LibraryApplicationResponse>> GetApplication(Guid appGuid);
    }
}
