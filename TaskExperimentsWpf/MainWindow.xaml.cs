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

    class FF
    {
        private Task Calc1()
        {
            return TaskEx.Delay(100);
        }

        Task Calc2()
        {
            return TaskEx.Delay(100);
        }

        async void Fff()
        {
            //            Func<int, int> f = x => x + 1;
            Func<Task> t1 = () => Calc1();
            Action last = () => Console.WriteLine("");
            //            Func<Task, Task> la2 = ll => Calc2().ContinueWith(ll);

            //            var ttotal = t1.ContinueWith(la2);
        }

        // Превратить в async метод
        Task Seq()
        {
            return Calc1().ContinueWith(t => Calc2());
        }


        async Task Wrap()
        {
            await Calc1();
            await Calc2();
            Console.WriteLine("");
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Func<int, int, int> op1 = (x, y) => x + y;

            Fffff(3, d => d * d);
            Fffff(5, d =>
            {
                var n = d - 5;
                return d - n;
            });

            Mm(op1, 3, 4);
        }

        private void Mm(Func<int, int, int> fu, int a, int b)
        {
            var result = fu(a, b);
            Console.WriteLine(result);
        }
        private void Fffff(int n, Func<double, double> ope)
        {
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(ope((double)i / 7));
            }
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