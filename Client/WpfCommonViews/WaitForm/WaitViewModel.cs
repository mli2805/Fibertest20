using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.WpfCommonViews
{
    public class WaitViewModel : Screen
    {
        public List<MyMessageBoxLineModel> Lines { get; set; }

        public void Initialize(bool isDrawingGraph)
        {
            var strs = isDrawingGraph
                ? new List<string>()
                  {
                      Resources.SID_Drawing_graph_of_traces_,
                      "",
                      Resources.SID_Depending_on_graph_size_and_performance_of_your_PC,
                      Resources.SID_it_could_take_a_few_minutes_,
                      "",
                      Resources.SID_Please__wait___
                  }
                : new List<string>()
                  {
                      Resources.SID_Database_optimization_,
                      "",
                      Resources.SID_Depending_on_database_size__choosen_parameters_and_performance_of_your_PC,
                      Resources.SID_it_could_take_a_few_minutes_,
                      "",
                      Resources.SID_Please__wait___
                  };

            Lines = strs.Select(s => new MyMessageBoxLineModel() { Line = s }).ToList();
            Lines[0].FontWeight = FontWeights.Bold;
        }


        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Long_operation__please_wait;
        }
    }
}
