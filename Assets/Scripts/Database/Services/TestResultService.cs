/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using PetaPoco;
using System;
using System.Collections.Generic;

namespace Simulator.Database.Services
{
    public class TestResultService : ITestResultService
    {
        public IEnumerable<TestResultModel> List(string filter, int offset, int count, string owner)
        {
            using (var db = DatabaseManager.Open())
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    var cleanFilter = $"%{filter.Replace("%", "").Replace("_", "")}%";
                    var filterSql = Sql.Builder
                        .Where("(name LIKE @0)", cleanFilter)
                        .Where("owner = @0 OR owner IS NULL", owner)
                        .OrderBy("id")
                        .Append("LIMIT @0, @1", offset, count);

                    return db.Fetch<TestResultModel>(filterSql);
                }

                var sql = Sql.Builder
                    .Where("owner = @0 OR owner IS NULL", owner)
                    .OrderBy("id")
                    .Append("LIMIT @0, @1", offset, count);

                return db.Fetch<TestResultModel>(sql);
            }
        }

        public IEnumerable<TestResultModel> List(long simId, int offset, int count, string owner)
        {
            using (var db = DatabaseManager.Open())
            {
                var filterSql = Sql.Builder
                    .Where(@"(simulation = @0)", simId)
                    .Where("owner = @0 OR owner IS NULL", owner)
                    .OrderBy("id")
                    .Append("LIMIT @0, @1", offset, count);

                return db.Fetch<TestResultModel>(filterSql);
            }
        }

        public TestResultModel Get(long id, string owner)
        {
            using (var db = DatabaseManager.Open())
            {
                var sql = Sql.Builder.Where("id = @0", id).Where("owner = @0 OR owner IS NULL", owner);
                return db.Single<TestResultModel>(sql);
            }
        }

        public long Count(long simId, string owner)
        {
            using (var db = DatabaseManager.Open())
            {
                return db.ExecuteScalar<long>("SELECT COUNT(*) FROM testresults WHERE simulation = @0 AND (owner = @1 or owner IS NULL)", simId, owner);
            }
        }

        public long Count(string name, string owner)
        {
            using (var db = DatabaseManager.Open())
            {
                return db.ExecuteScalar<long>($"SELECT COUNT(*) FROM testresults WHERE (name = '{name}' OR name LIKE '{name} (%)') AND (owner = @0 or owner IS NULL)", owner);
            }
        }

        public long StartTest(SimulationModel simulation, string name, string owner)
        {
            TestResultModel testResult = new TestResultModel ()
            {
                Name = name,
                Owner = owner,
                Simulation = simulation.Id,
                RuntimeTemplateType = simulation.RuntimeTemplateType,
                Status = "inprogress",
                Created = DateTime.UtcNow,
            };
            return Add(testResult);
        }

        public bool CompleteTest(long id, bool success, long iterations, string result)
        {
            using (var db = DatabaseManager.Open())
            {
                string status = "completed";
                int affected = db.Update<TestResultModel>("set status = @0, success = @1, iterations = @2, result = @3 where id = @4",
                                                          status, success, iterations, result, id);
                return affected > 0;
            }
        }

        public bool ErrorTest(long id, string error)
        {
            using (var db = DatabaseManager.Open())
            {
                string status = "error";
                int affected = db.Update<TestResultModel>("set status = @0, result = @1 where id = @2 and owner = @3",
                                                          status, error, id);
                return affected > 0;
            }
        }

        public int Delete(long id, string owner)
        {
            using (var db = DatabaseManager.Open())
            {
                var sql = Sql.Builder.Where("id = @0", id).Where("owner = @0 OR owner IS NULL", owner);
                return db.Delete<TestResultModel>(sql);
            }
        }

        protected long Add(TestResultModel testResult)
        {
            using (var db = DatabaseManager.Open())
            {
                return (long)db.Insert(testResult);
            }
        }

        protected int Update(TestResultModel testResult)
        {
            using (var db = DatabaseManager.Open())
            {
                return db.Update(testResult);
            }
        }
    }
}
