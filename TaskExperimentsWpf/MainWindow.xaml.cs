using System;
using System.Threading.Tasks;
using System.Windows;

namespace TaskExperimentsWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = "Experiment";
            Tb1.Text = "Let's go!";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Experiment();
//            Experiment().Wait();
        }

        private async Task Experiment()
        {
            await ThirdTask();
        }

        async Task ThirdTask()
        {
            for (int i = 0; i < 1000; i++)
            {
                await Task.Delay(10);
                Console.WriteLine($"Third {(i*3333)}");
            }
        }

        async Task FirstTask()
        {
            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine($"First {Math.Sqrt(i*1111)}");
            }
        }

        void SecondTask()
        {
            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine($"Second {Math.Sqrt(i*2222)}");
            }
        }


        private void Pospone()
        {
            Task task1 = new Task(() => {
                Console.WriteLine("Id задачи: {0}", Task.CurrentId);
            });

            Task task2 = new Task(SecondTask);
            Task<int> taskI = new Task<int>(Age);
            var r = Sum(2, 3);
            Console.WriteLine(r.Result);


            // задача продолжения
            task1.ContinueWith(Display);

            task1.Start();

            task1.Wait();
            Task.Delay(1000).ContinueWith(_ => task2);


            Task.WaitAll();
            Task.WhenAll();

            // ждем окончания второй задачи
            //            task2.Wait();
            //            Console.WriteLine("Выполняется работа метода Main");
            //            for (int i = 0; i < 200; i++)
            //            {
            //                Console.WriteLine(i);  
        }

        int Age()
        {
            return 4;
        }

         Task<int> Ffff()
        {
            var sum = Sum(4, 3);
            Console.WriteLine(sum);
            return sum;
        }

         async Task<int> Sum(int param1, int param2)
        {
            Task task1 = new Task(() => {
                Console.WriteLine("Id задачи: {0}", Task.CurrentId);
            });
            task1.Start();
            await task1;
            //            while (!task1.GetAwaiter().IsCompleted)
            //            {

            //            }

            await Ffff();
            await Task.Delay(10000);
            return (param1 + param2);
        }



         void Display(Task t)
        {
            Console.WriteLine("Id задачи: {0}", Task.CurrentId);
            Console.WriteLine("Id предыдущей задачи: {0}", t.Id);
        }
    }
}
