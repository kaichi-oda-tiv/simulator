/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Simulator.Database;
using Simulator.Database.Services;
using System;
using System.IO;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Simulator.Web.Modules
{
    public class TestResulListResponse
    {
        public long Id;
        public DateTime Created;
        public string Name;
        public long SimId;
        public string Status;
        public bool Success;

        public static TestResulListResponse Create(TestResultModel testResult)
        {
            return new TestResulListResponse()
            {
                Id = testResult.Id,
                Created = testResult.Created,
                Name = testResult.Name,
                SimId = testResult.Simulation,
                Status = testResult.Status,
                Success = testResult.Success
            };
        }
    }

    public class TestResulGetResponse
    {
        public long Id;
        public DateTime Created;
        public string Name;
        public string SimulationName;
        public string RuntimeTemplateType;
        public string Status;
        public bool Success;
        public long Iterations;
        public string Result;
        public string Error;

        public static TestResulGetResponse Create(TestResultModel testResult, SimulationModel simulation)
        {
            return new TestResulGetResponse()
            {
                Id = testResult.Id,
                Created = testResult.Created,
                Name = testResult.Name,
                SimulationName = simulation?.Name,
                RuntimeTemplateType = testResult.RuntimeTemplateType,
                Status = testResult.Status,
                Success = testResult.Success,
                Iterations = testResult.Iterations,
                Result = testResult.Status != "error" ? testResult.Result : null,
                Error = testResult.Status == "error" ? testResult.Result : null
            };
        }
    }
    public class TestResultsModule : NancyModule
    {
        public TestResultsModule(ITestResultService testResultService, ISimulationService simulationService) : base("testresults")
        {
            this.RequiresAuthentication();

            Get("/", x =>
            {
                Debug.Log($"Listing test results");
                try
                {
                    string filter = Request.Query["filter"];
                    int offset = Request.Query["offset"];
                    // TODO: Items per page should be read from personal user settings.
                    //       This value should be independent for each module: maps, vehicles, simulations and test results.
                    //       But for now 5 is just an arbitrary value to ensure that we don't try and Page a count of 0
                    int count = Request.Query["count"] > 0 ? Request.Query["count"] : Config.DefaultPageSize;
                    if (Request.Query["simId"]) {
                        int simId = Request.Query["simId"];
                        return testResultService.List(simId, offset, count, this.Context.CurrentUser.Identity.Name)
                            .Select(TestResulListResponse.Create)
                            .ToArray();
                    }

                    return testResultService.List(filter, offset, count, this.Context.CurrentUser.Identity.Name)
                        .Select(TestResulListResponse.Create)
                        .ToArray();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return Response.AsJson(new { error = $"Failed to list test results: {ex.Message}" }, HttpStatusCode.InternalServerError);
                }
            });

            Get("/{id:long}", x =>
            {
                long id = x.id;
                Debug.Log($"Getting test result with id {id}");
                try
                {
                    var testResult = testResultService.Get(id, this.Context.CurrentUser.Identity.Name);
                    SimulationModel simulation = null;
                    try
                    {
                        simulation = simulationService.Get(testResult.Simulation, this.Context.CurrentUser.Identity.Name);
                    }
                    catch (IndexOutOfRangeException)
                    {
                    }

                    return TestResulGetResponse.Create(testResult, simulation);
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.Log($"Test result with id {id} does not exist");
                    return Response.AsJson(new { error = $"Test result with id {id} does not exist" }, HttpStatusCode.NotFound);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return Response.AsJson(new { error = $"Failed to get test result with id {id}: {ex.Message}" }, HttpStatusCode.InternalServerError);
                }
            });

            Delete("/{id:long}", x =>
            {
                long id = x.id;
                Debug.Log($"Removing test result with id {id}");
                try
                {
                    TestResultModel testResult = testResultService.Get(id, this.Context.CurrentUser.Identity.Name);
                    int result = testResultService.Delete(id, testResult.Owner);
                    if (result > 1)
                    {
                        throw new Exception($"More than one test result has id {id}");
                    }
                    else if (result < 1)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    return new { };
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.Log($"Test result with id {id} does not exist");
                    return Response.AsJson(new { error = $"Test result with id {id} does not exist" }, HttpStatusCode.NotFound);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return Response.AsJson(new { error = $"Failed to remove test result with id {id}: {ex.Message}" }, HttpStatusCode.InternalServerError);
                }
            });
        }
    }
}
