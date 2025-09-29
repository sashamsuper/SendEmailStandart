using System;
using System.Collections.Generic;
using System.IO;
using MailKit.Security;

namespace PostalService
{
    // <summary>
    // Represents settings for sending emails.
    // </summary>
    public interface ISendSettings : IEmail
    {
        // <summary>
        // Gets or sets the sender's email address.
        // </summary>
        string FromAdress { set; get; }

        // <summary>
        // Gets or sets a value indicating whether to send an email if no attachments are present.
        // </summary>
        bool NoEmailIfNoAttachment { set; get; }

        // <summary>
        // Gets or sets a value indicating whether to delete the file after sending the email.
        // </summary>
        bool DeleteFileAfterSend { set; get; }

        // <summary>
        // Gets or sets the file paths to attach to the email.
        // </summary>
        string[] FilePath { set; get; }

        // <summary>
        // Gets or sets a value indicating whether to zip the folder before sending the email.
        // </summary>
        bool ZipDirectory { set; get; }

        // <summary>
        // Gets or sets a value indicating whether to delete the attachment files after sending the email.
        // // </summary>
        bool DeleteAttachmentFile { set; get; }
    }

    public interface IGetSettings : IEmail
    {
        // <summary>
        // С адресов
        // </summary>
        List<string> FromAdresses { get; set; }

        // <summary>
        // Количество просматриваемых сообщений
        // </summary>
        int InBoxCount { get; set; }
    }

    public interface IEmail : IEmailHostSettings, IPostMessage
    {
        string FromName { set; get; }

        // <summary>
        // В адреса
        // </summary>
        Dictionary<string, string> ToAdressName { set; get; }
    }

    // <summary>
    // Параметры подключения к почте
    // </summary>
    public interface IEmailHostSettings
    {
        // <summary>
        // URL хост
        // </summary>
        string Host { set; get; }

        // <summary>
        // Пароль сервера
        // </summary>
        string Password { set; get; }

        // <summary>
        // Логин сервера
        // </summary>
        string Login { set; get; }

        // <summary>
        // Порт сервера
        // </summary>
        int Port { set; get; }
        SecureSocketOptions SecureSocket { set; get; }
    }

    public interface IPostMessage
    {
        // <summary>
        // Текст сообщения
        // </summary>
        string TextBody { set; get; }

        // <summary>
        // Тема
        // </summary>
        string Subject { set; get; }

        //string TextBody { set; get; }
        //InternetAddressList From { set; get; }
        DateTimeOffset Date { set; get; }
        string SenderString { set; get; }
    }

    // <summary>
    // Represents settings for getting emails.
    // </summary>
    public class GetSettings : IGetSettings
    {
        public List<string> FromAdresses { get; set; }
        public string FromName { get; set; }
        public string TextBody { set; get; }
        public Dictionary<string, string> ToAdressName { get; set; }

        //public string TextMessage { get; set; }
        public string Subject { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public string Login { get; set; }
        public int Port { get; set; }
        public SecureSocketOptions SecureSocket { get; set; }
        public DateTimeOffset Date { set; get; }
        public int InBoxCount { get; set; } = 50;

        // <summary>
        // Represents settings for getting emails.
        // </summary>
        public string SenderString { get; set; }
    }

    public class EmailSettings : ISendSettings
    {
        public string FromAdress { set; get; }
        public string FromName { set; get; }
        public Dictionary<string, string> ToAdressName { set; get; } =
            new Dictionary<string, string>();
        public string TextBody { set; get; } =
            $"Сообщение от {DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}";
        public string Subject { set; get; } =
            $"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}";
        public string Host { set; get; }
        public DateTimeOffset Date { set; get; }
        public string ClientConnect // для совместимости
        {
            set { Host = value; }
            get { return Host; }
        }
        public string Password { set; get; }
        public string Login { set; get; }
        public int Port { set; get; } = 587;
        public SecureSocketOptions SecureSocket { set; get; } = SecureSocketOptions.None;
        public bool NoEmailIfNoAttachment { set; get; } = true;
        public bool DeleteFileAfterSend { set; get; } = false;
        public string[] FilePath
        {
            set
            {
                List<string> tmpListFile = new();
                foreach (var x in value)
                {
                    if (File.Exists(x))
                    {
                        tmpListFile.Add(x);
                    }
                    else if (Directory.Exists(x) && ZipDirectory)
                    {
                        ZipDir zipDir = new(x);
                        tmpListFile.Add(zipDir.ExtractFile);
                    }
                }
                filePath = tmpListFile.ToArray();
            }
            get { return filePath; }
        }
        private string[] filePath;
        public bool ZipDirectory { set; get; } = false;
        public bool DeleteAttachmentFile { set; get; } = false;
        public string SenderString { get; set; }
    }
}
