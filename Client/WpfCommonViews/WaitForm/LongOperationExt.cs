﻿using System.Collections.Generic;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.WpfCommonViews
{
    public static class LongOperationExt
    {
        public static List<string> ToLines(this LongOperation longOperation)
        {
            switch (longOperation)
            {
                case LongOperation.DrawingGraph:
                    return new List<string>()
                    {
                        Resources.SID_Drawing_graph_of_traces_,
                        "",
                        Resources.SID_Depending_on_graph_size_and_performance_of_your_PC,
                        Resources.SID_it_could_take_a_few_minutes_,
                        "",
                        Resources.SID_Please__wait___
                    };
                case LongOperation.DbOptimization:
                    return new List<string>()
                    {
                        Resources.SID_Database_optimization_,
                        "",
                        Resources.SID_Depending_on_database_size__choosen_parameters_and_performance_of_your_PC,
                        Resources.SID_it_could_take_a_few_minutes_,
                        "",
                        Resources.SID_Please__wait___
                    };
                //case LongOperation.MakingSnapshot:
                default: return new List<string>() { "Making snapshot..." };
            }
        }

    }
}