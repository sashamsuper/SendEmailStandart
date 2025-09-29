//#define NOTEST
using System;
using System.IO;
using MimeKit;
#if TEST

using System.Reflection;

#endif

namespace PostalService
{
    public class SendEmail
    {
        private EmailSettings Settings { set; get; }

        public SendEmail(EmailSettings settings)
        {
            Settings = settings;
        }

        public bool Send()
        {
            try
            {
                
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(Settings.FromName, Settings.FromAdress));
                    foreach (var x in Settings.ToAdressName)
                    {
                        message.To.Add(new MailboxAddress(x.Value, x.Key));
                    }
                    // создаем объект сообщения));
                    DateTime datetimeNow = DateTime.Now;
                    message.Subject = Settings.Subject;
                    var builder = new BodyBuilder { TextBody = Settings.TextBody };
                    foreach (var x in Settings.FilePath)
                        builder.Attachments.Add(x);
                    message.Body = builder.ToMessageBody();
                    using (var client = new MailKit.Net.Smtp.SmtpClient())
                    {
                        client.Connect(Settings.Host, Settings.Port, Settings.SecureSocket);
                        client.Authenticate(Settings.Login, Settings.Password);
                        client.Send(message);
                        client.Disconnect(true);
                    }
                    if (Settings.DeleteFileAfterSend)
                    {
                        foreach (var x in Settings.FilePath)
                            if (File.Exists(x))
                                File.Delete(x);
                    }
                    return true;
                
            }
            catch (Exception e)
            {
#if TEST
                //string basepath = System.IO.Directory.GetCurrentDirectory();

                string basepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                using StreamWriter sw = File.AppendText(Path.Combine(basepath, "logemail.txt"));
                Proverka.WriteTextAsync(
                    Path.Combine(basepath, "logemail.txt"),
                    $"Send error.Message:{e.Message};{e.StackTrace};DateTime:{DateTime.Now}"
                );
                return false;
#endif
            }
            return false;
        }
    }
}
