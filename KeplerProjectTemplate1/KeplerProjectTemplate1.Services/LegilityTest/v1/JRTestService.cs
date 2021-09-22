using KeplerProjectTemplate1.Interfaces.LegilityTest.v1.Exceptions;
using KeplerProjectTemplate1.Interfaces.LegilityTest.v1.Models;
using Relativity.API;
using Relativity.API.Context;
using Relativity.Audit.Services.Interfaces.V1.Metrics;
using Relativity.Audit.Services.Interfaces.V1.Metrics.Models;
using Relativity.Audit.Services.Interfaces.V1.ReviewerStatistics;
using Relativity.Audit.Services.Interfaces.V1.ReviewerStatistics.Models;
using Relativity.Kepler.Exceptions;
using Relativity.Kepler.Logging;
using Relativity.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Relativity.Audit.Services.Interfaces.V1.UI;
using Relativity.Audit.Services.Interfaces.V1.UI.Models;
using Relativity.Audit.Services.Interfaces.V1.DataContracts;
using Newtonsoft.Json;

namespace KeplerProjectTemplate1.Interfaces.LegilityTest
{
    public class JRTestService : IJRTestService
    {
        private IHelper _helper;
        private ILog _logger;
        private const int ADMIN_WORKSPACE_ID = -1;
        public const string ISO8601_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";

        // Note: IHelper and ILog are dependency injected into the constructor every time the service is called.
        public JRTestService(IHelper helper, ILog logger)
        {
            // Note: Set the logging context to the current class.
            _logger = logger.ForContext<JRTestService>();
            _helper = helper;

        }

        public async Task<JRTestServiceModel> GetWorkspaceNameAsync(int workspaceID)
        {
            JRTestServiceModel model;

            try
            {
                // Use the dependency injected IHelper to get a database connection.
                // In this example a query is being made for the name of a workspace from the workspaceID.
                // Note: async/await and ConfigureAwait(false) is used when making calls external to the service.
                //       See https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
                //       See also https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.configureawait
                //       See also https://blogs.msdn.microsoft.com/benwilli/2017/02/09/an-alternative-to-configureawaitfalse-everywhere/
                //       See also https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
                //       Warning: Improper use of the tasks can cause deadlocks and performance issues within an application.
                string workspaceName = await _helper.GetDBContext(workspaceID).ExecuteScalarAsync<string>(new ContextQuery()
                {
                    SqlStatement = "SELECT [TextIdentifier] FROM [EDDSDBO].[Artifact] WHERE [ArtifactTypeID] = 8"
                }).ConfigureAwait(false);

                model = new JRTestServiceModel
                {
                    Name = workspaceName
                };
            }
            catch (Exception exception)
            {
                // Note: logging templates should never use interpolation! Doing so will cause memory leaks. 
                _logger.LogWarning(exception, "Could not read workspace {WorkspaceID}.", workspaceID);

                // Throwing a user defined exception with a 404 status code with an additional custom FaultSafe object.
                throw new JRTestServiceException($"Workspace {workspaceID} not found.")
                {
                    FaultSafeObject = new JRTestServiceException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceID}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
        }

        public async Task<List<JRTestServiceModel>> QueryWorkspaceByNameAsync(string queryString, int limit)
        {
            var models = new List<JRTestServiceModel>();

            // Create a Kepler service proxy to interact with other Kepler services.
            // Use the dependency injected IHelper to create a proxy to an external service.
            // This proxy will execute as the currently logged in user. (ExecutionIdentity.CurrentUser)
            // Note: If calling methods within the same service the proxy is not needed. It is doing so
            //       in this example only as a demonstration of how to call other services.
            var proxy = _helper.GetServicesManager().CreateProxy<IJRTestService>(ExecutionIdentity.CurrentUser);

            // Validate queryString and throw a ValidationException (HttpStatusCode 400) if the string does not meet the validation requirements.
            if (string.IsNullOrEmpty(queryString) || queryString.Length > 50)
            {
                // ValidationException is in the namespace Relativity.Services.Exceptions and found in the Relativity.Kepler.dll.
                throw new ValidationException($"{nameof(queryString)} cannot be empty or grater than 50 characters.");
            }

            try
            {
                // Use the dependency injected IHelper to get a database connection.
                // In this example a query is made for all workspaces that are like the query string.
                // Note: async/await and ConfigureAwait(false) is used when making calls external to the service.
                //       See https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
                //       See also https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.configureawait
                //       See also https://blogs.msdn.microsoft.com/benwilli/2017/02/09/an-alternative-to-configureawaitfalse-everywhere/
                //       See also https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
                //       Warning: Improper use of the tasks can cause deadlocks and performance issues within an application.
                var workspaceIDs = await _helper.GetDBContext(-1).ExecuteEnumerableAsync(
                    new ContextQuery
                    {
                        SqlStatement = @"SELECT TOP (@limit) [ArtifactID] FROM [Case] WHERE [ArtifactID] > 0 AND [Name] LIKE '%'+@workspaceName+'%'",
                        Parameters = new[]
                        {
                            new SqlParameter("@limit", limit),
                            new SqlParameter("@workspaceName", queryString)
                        }
                    }, (record, cancel) => Task.FromResult(record.GetInt32(0))).ConfigureAwait(false);

                foreach (int workspaceID in workspaceIDs)
                {
                    // Loop through the results and use the proxy to call another service for more information.
                    // Note: async/await and ConfigureAwait(false) is used when making calls external to the service.
                    //       See https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
                    //       See also https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.configureawait
                    //       See also https://blogs.msdn.microsoft.com/benwilli/2017/02/09/an-alternative-to-configureawaitfalse-everywhere/
                    //       See also https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
                    //       Warning: Improper use of the tasks can cause deadlocks and performance issues within an application.
                    JRTestServiceModel wsModel = await proxy.GetWorkspaceNameAsync(workspaceID).ConfigureAwait(false);
                    if (wsModel != null)
                    {
                        models.Add(wsModel);
                    }
                }
            }
            catch (Exception exception)
            {
                // Note: logging templates should never use interpolation! Doing so will cause memory leaks. 
                _logger.LogWarning(exception, "An exception occured during query for workspace(s) containing {QueryString}.", queryString);

                // Throwing a user defined exception with a 404 status code.
                throw new JRTestServiceException($"An exception occured during query for workspace(s) containing {queryString}.");
            }

            return models;
        }


        public async Task<ServiceResponse<List<string>>> GetApplications()
        {
            ServiceResponse<List<string>> serviceResponse = new ServiceResponse<List<string>>();
            serviceResponse.Message = "success";
            return serviceResponse;
            //using (ILibraryApplicationManager libraryApplicationManager = _helper.GetServicesManager().CreateProxy<ILibraryApplicationManager>(ExecutionIdentity.System))
            //{
                
            //    return await LibraryApplicationAPI.ReadAll(libraryApplicationManager);

            //}
        }

        public async Task<ServiceResponse<List<AuditMetricsAggregateResponse>>> GetAuditsForWorkspace(int workspaceID)
        {
            ServiceResponse<List<AuditMetricsAggregateResponse>> service = new ServiceResponse<List<AuditMetricsAggregateResponse>>();
            service.Data = new List<AuditMetricsAggregateResponse>();
            using (IAuditMetricsService auditMetricsService = _helper.GetServicesManager().CreateProxy<IAuditMetricsService>(ExecutionIdentity.System))
            {
                try
                {
                    AuditMetricsAggregateResponse workspaceMetrics = await auditMetricsService.GetWorkspaceAuditMetricsAsync(workspaceID);
                    service.Data.Add(workspaceMetrics);
                    return service;
                }
                catch(ServiceNotFoundException snfe)
                {
                    service.Exception = snfe;
                    service.Message = "ServiceNotInstalled";
                    service.Success = true;
                    service.StatusCode = 404;
                }
                catch(Exception ex)
                {
                    service.Exception = ex;
                    service.Message = "GetWorkspacAudit Failed";
                    service.Success = false;
                    service.StatusCode = 500;
                }
            }
            return service;
        }

        public async Task<ServiceResponse<long>> GetLastAuditIdForWorkspace(int workspaceID)
        {
            ServiceResponse<long> serviceResponse = new ServiceResponse<long>();
            try
            {
                using (var auditObjectManager = _helper.GetServicesManager().CreateProxy<IAuditObjectManagerUIService>(ExecutionIdentity.System))
                {
                    var request = new QueryRequest
                    {
                        Fields = new List<FieldRef>
                        {
                            new FieldRef{Name = "Audit ID"},
                        },
                        Condition = "",
                        RowCondition = "",
                        Sorts = new List<Sort>
                        {
                            new Sort
                            {
                                Direction = SortEnum.Descending,
                                FieldIdentifier = new FieldRef {Name = "Timestamp"} // Only support Timestamp and Execution Time (ms)
                            }
                        }

                    };
                    QueryResultSlim queryRequest = await auditObjectManager.QuerySlimAsync(workspaceID, request, 0, 10);
                    if (queryRequest != null && queryRequest.Objects.Count > 0)
                    {
                        RelativityObjectSlim relObject = queryRequest.Objects[0];
                        if (relObject.Values.Count > 0)
                        {
                            string value = relObject.Values[0].ToString();
                            long lastIdStr;
                            Int64.TryParse(value.Replace(workspaceID.ToString(), "").Replace("-", ""), out lastIdStr);
                            serviceResponse.Data = lastIdStr;
                        }
                    }
                    

                }
            }
            catch (ServiceNotFoundException snfe)
            {
                serviceResponse.Exception = snfe;
                serviceResponse.Message = "ServiceNotInstalled";
                serviceResponse.Success = true;
                serviceResponse.StatusCode = 404;
            }
            catch (Exception ex)
            {
                serviceResponse.Exception = ex;
                serviceResponse.Message = "GetWorkspacAudit Failed";
                serviceResponse.Success = false;
                serviceResponse.StatusCode = 500;
            }
            return serviceResponse;
        }


        public async Task<string> GetElasticSearchReviewerStatistics(long workspaceId, DateTime lastAuditActivity)
        {
            DateTime lastDbActivity = await GetLastAuditTimeFromApi((int)workspaceId);
            if (DateTime.Compare(lastDbActivity, lastAuditActivity) > 0)
            {
                IEnumerable<ReviewersStats> revStats = await GetReviewerStatisticsFromApi(workspaceId);

                string userStatsJson = PackageRevStats(revStats);

                return userStatsJson;

            }
            return null;
        }

        private string PackageRevStats(IEnumerable<ReviewersStats> toJson)
        {
            return JsonConvert.SerializeObject(toJson);
        }

        private async Task<DateTime> GetLastAuditTimeFromApi(int workspaceId)
        { //TODO: figure out how to query the last audit time
            throw new NotImplementedException();

        }

        private async Task<IEnumerable<ReviewersStats>> GetReviewerStatisticsFromApi(long workspaceId)
        {
            DateTimeOffset localOffset = new DateTimeOffset(DateTime.Now);
            DateTime endOfLastHour = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc);

            using (var reviewerStats = _helper.GetServicesManager().CreateProxy<IAuditReviewerStatisticsService>(ExecutionIdentity.CurrentUser))
            {
                ReviewerStatsDataRequest request = new ReviewerStatsDataRequest
                {
                    StartDate = endOfLastHour.AddHours(-1).ToString(ISO8601_FORMAT),
                    EndDate = endOfLastHour.ToString(ISO8601_FORMAT),
                    TimeZone = localOffset.Offset.TotalHours,
                    NonAdmin = false,
                    AdditionalActions = "Mass Edit and Propagation"
                };

                IEnumerable<ReviewersStats> reviewerMetrics = await reviewerStats.GetReviewerStatsAsync((int)workspaceId, request);



                return reviewerMetrics;


            }

        }


        /// <summary>
        /// All Kepler services must inherit from IDisposable.
        /// Use this dispose method to dispose of any unmanaged memory at this point.
        /// See https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose for examples of how to properly use the dispose pattern.
        /// </summary>
        public void Dispose()
        { }
    }
}
