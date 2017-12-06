using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsoleApplication
{
    class Program
    {

        static void Main(string[] args)
        {
            /*
             * CancellationTokenSource - 取消任务的操作需要用到的一个类
             *     Token - 一个 CancellationToken 类型的对象，用于通知取消指定的操作
             *     IsCancellationRequested - 是否收到了取消操作的请求
             *     Cancel() - 结束任务的执行
             * ParallelOptions - 并行运算选项
             *     CancellationToken - 设置一个 Token，用于取消任务时的相关操作
             *     MaxDegreeOfParallelism - 指定一个并行循环最多可以使用多少个线程
             */
            //CancellationTokenSource cts = new CancellationTokenSource();
            //ParallelOptions pOption = new ParallelOptions() { CancellationToken = cts.Token };
            //pOption.MaxDegreeOfParallelism = 10;

            string s = "超时";
            Console.WriteLine(s.IndexOf("超时"));

            //int ss = 1;
            //var tasks = new Action[] { () => Task1(ss), () => Task2(), () => Task3() };
            //// System.Threading.Tasks.Parallel.Invoke - 并行调用多个任务
            //System.Threading.Tasks.Parallel.Invoke(tasks);
            //Console.ReadLine();

        }

        static void Task1(int i)
        {
            Thread.Sleep(3000);
            Console.WriteLine("Task1 - " + "ThreadId:" + Thread.CurrentThread.ManagedThreadId.ToString() + " - " + DateTime.Now.ToString("HH:mm:ss"));
            Console.WriteLine("<br />");
        }

        static void Task2()
        {
            System.Threading.Thread.Sleep(3000);
            Console.WriteLine("Task2 - " + "ThreadId:" + Thread.CurrentThread.ManagedThreadId.ToString() + " - " + DateTime.Now.ToString("HH:mm:ss"));
            Console.WriteLine("<br />");
        }

        static void Task3()
        {
            System.Threading.Thread.Sleep(3000);
            Console.WriteLine("Task3 - " + "ThreadId:" + Thread.CurrentThread.ManagedThreadId.ToString() + " - " + DateTime.Now.ToString("HH:mm:ss"));
            Console.WriteLine("<br />");
        }
        
        //static void Main(string[] args)
        //{
        //      Thread[]  threadHZ = new Thread[3];
        //        for (int i = 0; i < threadHZ.Length; i++)
        //        {

        //            threadHZ[i] = new System.Threading.Thread(new System.Threading.ThreadStart(f1));
        //            threadHZ[i].Start();
        //            Console.WriteLine("This is Main.{0}", 1);
        //            threadHZ[i].Join();
        //            Console.WriteLine("This is Main.{0}", 2);
                   
        //        }
        //        Console.ReadLine();

        //}

        //static void f1()
        //{
        //    System.Threading.Thread y = new System.Threading.Thread(new System.Threading.ThreadStart(f2));
        //    y.Start();
        //    y.Join();
        //    Console.WriteLine("This is F1.{0}", 1);
        //}

        //static void f2()
        //{
        //    Console.WriteLine("This is F2.{0}", 1);
        //}

    }
}
