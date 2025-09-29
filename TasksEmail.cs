//#define NOTEST
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if TEST

using System.Reflection;
#endif

namespace PostalService
{
    public class Proverka
    {
        public static void NotMain()
        {
#if (TEST)
            string basepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (StreamWriter sw = File.AppendText(Path.Combine(basepath, "logemail.txt")))
            {
                sw.WriteLine($"Main proverka start.DateTime{DateTime.Now}");
            }
#endif
            EmailSettings settings = new() { FilePath = new string[] { @"B:\YANDEX\1.txt" } };
            SendEmailAndFile sendEmail = new() { settings = settings };
            sendEmail.OneEmailTaskStart();
        }

        public static async Task WriteTextAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (
                FileStream sourceStream = new(
                    filePath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true
                )
            )
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            }
            ;
        }
    }

    public class SendEmailAndFile
    {
        public EmailSettings settings;
        public int ThreadSleep = 86400000 / 3;
        public CancellationTokenSource tokenSource = new();
        private CancellationToken token;

        public void ManyEmailTaskStart()
        {
            Task task = new(() => ManyEmailTask(token));
            task.Start();
        }

        public void OneEmailTaskStart()
        {
            //OneEmailTask(token);
            Task task = new(() => OneEmailTask(token));
            //Task task = new Task(() => Console.WriteLine("##"));
            task.Start();
            Console.ReadLine();
        }

        public SendEmailAndFile()
        {
            token = tokenSource.Token;
        }

        public void OneEmailTask(CancellationToken ct)
        {
#if (TEST)
            string basepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Proverka.WriteTextAsync(
                Path.Combine(basepath, "logemail.txt"),
                "--------------------------------------"
            );
            Proverka.WriteTextAsync(
                Path.Combine(basepath, "logemail.txt"),
                $"OneEmail task start:{DateTime.Now}"
            );
#endif
            if (ct.IsCancellationRequested)
                return;
            SendEmail sendEmail = new(settings);
            sendEmail.Send();
        }

        public void ManyEmailTask(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;
#if (TEST)
            Int64 TaskNumber = 0;
#endif
            while (true)
            {
#if (TEST)
                string basepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                using (StreamWriter sw = File.AppendText(Path.Combine(basepath, "logemail.txt")))
                {
                    sw.WriteLine($"TaskNumber:{TaskNumber};DateTime:{DateTime.Now}");
                }
#endif
                if (settings.NoEmailIfNoAttachment && settings.FilePath.Length == 0)
                    break;
                else
                {
                    OneEmailTask(ct);
                    Thread.Sleep(ThreadSleep);
                    if (ct.IsCancellationRequested)
                        break;
#if (TEST)
                    if (TaskNumber < Int64.MaxValue)
                        TaskNumber++;
                    else
                        TaskNumber = 0;
#endif
                }
            }
        }
    }
}
