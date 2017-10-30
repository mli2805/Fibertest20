using System;
using System.Threading.Tasks;
using System.Windows;
using Iit.Fibertest.Dto;

namespace TaskExperimentsWpf
{
    public class X
    {
        public Task<RtuInitializedDto> RtuInitializedEvent { get; set; }

        public X()
        {
        }

        public async void InitializeRtuAsync(TaskCompletionSource<RtuInitializedDto> param)
        {
            RtuInitializedEvent = param.Task;
            int i = 0;
            while (true)
            {
                i++;
                await TaskEx.Delay(10);
                Console.WriteLine(i);
                if (i == 200)
                {
                    var result = new RtuInitializedDto() { Version = "Rtu initialized" };
                    param.SetResult(result);
                }
            }
        }
    }


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            TaskCompletionSource<RtuInitializedDto> tcs = new TaskCompletionSource<RtuInitializedDto>();

            var myX = new X();
            myX.InitializeRtuAsync(tcs);
            var result = await myX.RtuInitializedEvent.TimeoutAfter(4000);

            Tb1.Text = result == null ? "timeout expired" : result.Version;
        }
    }

    public static class TaEx
    {
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout)
        {
            if (task.IsCompleted)
                return task.Result;
            return await TaskEx.WhenAny(task, TaskEx.Delay(millisecondsTimeout)) == task
                ? task.Result
                : default(TResult);
        }
    }
}