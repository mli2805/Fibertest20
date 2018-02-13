using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public static class TreeOfAcceptableMeasParamsExt
    {
        public static List<string> Log(this TreeOfAcceptableMeasParams parameters)
        {
            var log = new List<string>();
            foreach (var pair in parameters.Units)
            {
                log.Add($@"Wave length {pair.Key}");
                log.AddRange(Log(pair.Value));
            }
            return log;
        }

        private static List<string> Log(BranchOfAcceptableMeasParams branch)
        {
            var log = new List<string>();
            log.Add($@"RI = {branch.RefractiveIndex}");
            log.Add($@"BC = {branch.BackscatteredCoefficient}");

            foreach (var pair in branch.Distances)
            {
                log.Add($@"Distance = {pair.Key}");
                log.AddRange(Log(pair.Value));
            }
            return log;
        }

        private static List<string> Log(LeafOfAcceptableMeasParams leaf)
        {
            var log = new List<string>();
            log.Add($@"resolutions:  {String.Join(@" ;   ", leaf.Resolutions)}");
            log.Add($@"pulse durations:  {String.Join(@" ;   ", leaf.PulseDurations)}");
            log.Add($@"time for meas:  {String.Join(@" ;   ", leaf.PeriodsToAverage)}");
            log.Add($@"count of meas:  {String.Join(@" ;   ", leaf.MeasCountsToAverage)}");
            return log;
        }
    }
}