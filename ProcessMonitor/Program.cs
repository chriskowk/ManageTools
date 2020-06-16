using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace ProcessMonitor
{
    class Program
    {
        static readonly List<int> _dumpPIDs = new List<int>();
        private static readonly string _processName = System.Configuration.ConfigurationManager.AppSettings["ProcessName"];
        private static readonly string _adplusPath = System.Configuration.ConfigurationManager.AppSettings["adplusPath"];
        private static int _ellipsisPrintTimes;
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("... ...");
                _ellipsisPrintTimes++;

                if (_ellipsisPrintTimes % 10 == 0) { Console.WriteLine(); }

                ThreadProc();
                Thread.Sleep(10000);
            }
        }

        private static void ThreadProc()
        {
            foreach (Process vProcess in Process.GetProcesses())
            {
                try
                {
                    string processName = vProcess.ProcessName;
                    if (processName.IndexOf(_processName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Console.WriteLine("{0}-{1}", vProcess.ProcessName, vProcess.Id);
                        _ellipsisPrintTimes = 0;

                        if (_dumpPIDs.Contains(vProcess.Id)) { continue; }

                        _dumpPIDs.Add(vProcess.Id);

                        DumpProcessDeg d = DumpProcess;
                        d.BeginInvoke(vProcess.Id, null, null);
                        DumpProcess(vProcess.Id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private delegate void DumpProcessDeg(int pid);
        static void DumpProcess(int pid)
        {
            ProcessStartInfo psf = new ProcessStartInfo
            {
                FileName = Path.Combine(_adplusPath, "adplus.exe"),
                Arguments = $"-crash -FullOnFirst -p {pid} -o c:\\dumps",
                WorkingDirectory = _adplusPath,
                UseShellExecute = true
            };
            Process proc;
            try
            {
                proc = Process.Start(psf);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                Console.WriteLine("系统找不到指定的程序文件。/r{0}", e);
                return;
            }
            proc.EnableRaisingEvents = true;
            Console.WriteLine("外部程序的开始执行时间：{0}", proc.StartTime);
            proc.WaitForExit(60000);
            if (proc.HasExited == false)
            {
                Console.WriteLine("由主程序强行终止外部程序的运行！");
                proc.Kill();
            }
            else
            {
                Console.WriteLine("由外部程序正常退出！");
            }
            Console.WriteLine("外部程序的结束运行时间：{0}", proc.ExitTime);
            Console.WriteLine("外部程序在结束运行时的返回值：{0}", proc.ExitCode);
        }
    }
}

