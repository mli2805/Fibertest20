using System;
using System.IO;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class VeexRtuCompletedTestsSceneSetter
    {
        public static void ClearTests(this FakeVeexRtuModel fakeVeexRtuModel)
        {
            fakeVeexRtuModel.CompletedTests.Clear();
        }

        public static void AddOkTest(this FakeVeexRtuModel fakeVeexRtuModel, Model writeModel, Guid traceId, BaseRefType baseRefType)
        {
            var test = writeModel.VeexTests.First(t => t.TraceId == traceId && t.BasRefType == baseRefType);

            var completedTest = new CompletedTest()
            {
                result = "Ok",
                testId = test.TestId.ToString(),
                started = DateTime.Now,
                indicesOfReferenceTraces = new[] {0},
                traceChange = new TraceChange()
                {

                }
            };
            fakeVeexRtuModel.CompletedTests.Add(completedTest);
        }

        public static void AddFailedBopTest(this FakeVeexRtuModel fakeVeexRtuModel, Model writeModel, Guid traceId, Guid otauId,
            BaseRefType baseRefType)
        {
            var test = writeModel.VeexTests.First(t => t.TraceId == traceId && t.BasRefType == baseRefType);

            var completedTest = new CompletedTest()
            {
                result = "failed",
                extendedResult = "otau_failed",
                failure = new Failure()
                {
                    otauId = "S2_" + otauId
                },
                testId = test.TestId.ToString(),
                started = DateTime.Now,
                indicesOfReferenceTraces = new[] {0},
                traceChange = new TraceChange()
                {

                }
            };
            fakeVeexRtuModel.CompletedTests.Add(completedTest);
        }

        public static void SetSorBytesToReturn(this FakeVeexRtuModel fakeVeexRtuModel, string filename)
        {
            fakeVeexRtuModel.SorBytesToReturn = File.ReadAllBytes(filename);
        }

    }
}