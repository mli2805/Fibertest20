using System.Collections.Generic;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.WpfCommonViews
{
    public static class MyMessageBoxExt
    {
        public static MyMessageBoxViewModel DrawingGraph()
        {
            List<string> strs = new List<string>{
                Resources.SID_Drawing_graph_of_traces_,
                "",
                Resources.SID_Depending_on_graph_size_and_performance_of_your_PC,
                Resources.SID_it_could_take_a_few_minutes_,
                "",
                Resources.SID_Please__wait___
            };
            return new MyMessageBoxViewModel(MessageType.LongOperation, strs, 0);
        }
    }
}