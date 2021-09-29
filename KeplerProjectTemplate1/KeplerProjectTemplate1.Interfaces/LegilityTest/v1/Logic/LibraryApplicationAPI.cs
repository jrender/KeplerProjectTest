using KeplerProjectTemplate1.Interfaces.LegilityTest.v1.Models;
using Relativity.Environment.V1.LibraryApplication;
using Relativity.Environment.V1.LibraryApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeplerProjectTemplate1.Interfaces.LegilityTest.v1.Logic
{
    public static class LibraryApplicationAPI
    {
        private const int ADMIN_WORKSPACE_ID = -1;

        //public static async Task<ServiceResponse<LibraryApplicationResponse>> Read(ILibraryApplicationManager libraryApplicationManager, int artifactID)
        //{
        //    ServiceResponse<LibraryApplicationResponse> serviceResponse = new ServiceResponse<LibraryApplicationResponse>();
        //    try
        //    {
        //        serviceResponse.Data = await libraryApplicationManager.ReadAsync(ADMIN_WORKSPACE_ID, artifactID);
        //        return serviceResponse;
        //    }
        //    catch (Exception ex)
        //    {
        //        serviceResponse.Message = $"An error occurred: {ex.Message}";
        //        serviceResponse.Exception = ex;
        //        serviceResponse.Success = false;
        //        return serviceResponse;
        //    }
        //}
        public static async Task<ServiceResponse<List<LibraryApplicationResponse>>> ReadAll(ILibraryApplicationManager libraryApplicationManager)
        {
            ServiceResponse<List<LibraryApplicationResponse>> serviceResponse = new ServiceResponse<List<LibraryApplicationResponse>>();
            try
            {
                serviceResponse.Data = await libraryApplicationManager.ReadAllAsync(ADMIN_WORKSPACE_ID);
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = $"An error occurred: {ex.Message}";
                serviceResponse.Exception = ex;
                serviceResponse.Success = false;
                return serviceResponse;
            }
        }
    }
}
