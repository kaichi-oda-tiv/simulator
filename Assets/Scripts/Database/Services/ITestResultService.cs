/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System.Collections.Generic;

namespace Simulator.Database.Services
{
    public interface ITestResultService
    {
        IEnumerable<TestResultModel> List(string filter, int offset, int count, string owner);
        IEnumerable<TestResultModel> List(long simId, int offset, int count, string owner);
        TestResultModel Get(long id, string owner);
        long Count(long simId, string owner);
        long Count(string name, string owner);
        long StartTest(SimulationModel simulation, string name, string owner);
        bool CompleteTest(long id, bool success, long iterations, string result);
        bool ErrorTest(long id, string error);
        int Delete(long id, string owner);
    }
}
