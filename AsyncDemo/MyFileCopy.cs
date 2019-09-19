using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncDemo
{
    class MyFileCopy
    {
        double TotalFiles = 0, FilesCompleted = 0;
        IProgress<int> progress;

        List<Task> TaskList = new List<Task>();
        SemaphoreSlim Semaphore = new SemaphoreSlim(5);

        object SemaphoreLock = new object();
        object ProgressLock = new object();

        public MyFileCopy(IProgress<int> p)
        {
            progress = p;
        }

        public async Task CopyAll(string InputFolder, string OutputFolder)
        {
            if(!Directory.Exists(InputFolder))
            {
                throw new DirectoryNotFoundException();
            }

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            IEnumerable<string> files = Directory.EnumerateFiles(InputFolder, "*.*", SearchOption.AllDirectories);

            TotalFiles = files.Count();

            foreach (string file in files)
            {
                await Semaphore.WaitAsync();

                TaskList.Add(Task.Run(()=>
                {
                    Copy(file, Path.Combine(OutputFolder, Path.GetFileName(file)));
                    lock (SemaphoreLock)
                    {
                        Semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(TaskList);
            MessageBox.Show("All Files Copied");
        }

        public void Copy(string InputPath, string OutputPath)
        {
            if (!File.Exists(InputPath))
            {
                throw new FileNotFoundException();
            }

            byte[] buffer = new byte[0x20000];

            using (var inStream = new FileStream(InputPath, FileMode.Open, FileAccess.Read))
            {
                using (var outStream = new FileStream(OutputPath, FileMode.Create))
                {
                    while ((inStream.Length - inStream.Position) >= buffer.Length)
                    {
                        inStream.Read(buffer, 0, buffer.Length);
                        outStream.Write(buffer, 0, buffer.Length);
                    }

                    buffer = new byte[inStream.Length - inStream.Position];
                    inStream.Read(buffer, 0, buffer.Length);
                    outStream.Write(buffer, 0, buffer.Length);
                    outStream.Flush();
                }
                buffer = null;
            }

            if (progress != null)
            {
                FilesCompleted++;
                int percent = (int)(FilesCompleted / TotalFiles * 100);
                progress.Report(percent);
            }		
        }
    }
}