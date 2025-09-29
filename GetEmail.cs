using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace PostalService
{
    /// <summary>
    /// Cохранение потоков с файлами
    /// </summary>
    public class MemoryStreamAttachment
    {
        /// <summary>
        /// key
        /// </summary>
        public int Id { set; get; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName { set; get; }

        /// <summary>
        /// PostMessagesId
        /// </summary>
        public int PostMessagesId { set; get; }

        /// <summary>
        /// Поток
        /// </summary>
        [NotMapped]
        public MemoryStream MemoryStream { set; get; } = new MemoryStream();

        /// <summary>
        /// Полный путь к файлу вложения сохраненному отдельно
        /// </summary>
        public string FullFileNameOnDisk { set; get; }

        /// <summary>
        /// Частичный путь
        /// </summary>
        public string SubPath { set; get; }
    }

    /// <summary>
    /// Одно сообщение
    /// </summary>
    public class PostMessage : IPostMessage
    {
        /// <summary>
        /// Kye field
        /// </summary>
        public int Id { set; get; }

        /// <summary>
        /// Вложения
        /// </summary>
        public List<MemoryStreamAttachment> AttachmentFiles { set; get; } = new List<MemoryStreamAttachment>();

        private string FirstFrom { set; get; }

        private string senderString {set;get;}

        /// <summary>
        /// Отправитель адрес
        /// </summary>
        public string SenderString
        {
            get
            {
                if (Sender != null)
                {
                    senderString=Sender.Address;
                    return senderString;
                }
                else if (FirstFrom != null)
                {
                    senderString=FirstFrom;
                    return senderString;
                }
                else
                {
                    return senderString;
                }
            }
            set
            {
                senderString=value;
            }
        }

        /// <summary>
        /// Дата сообщения
        /// </summary>
        public DateTimeOffset Date { set; get; }

        /// <summary>
        /// Тело письма
        /// </summary>
        public string TextBody { set; get; }

        /// <summary>
        /// Тема
        /// </summary>
        public string Subject { set; get; }

        /// <summary>
        /// Отправитель из заголовка
        /// </summary>
        [NotMapped]
        public InternetAddressList From
        {
            set
            {
                _from = value;
                if (value.FirstOrDefault() is MailboxAddress mailboxAddress)
                {
                    FirstFrom = mailboxAddress.Address;
                }
            }
            get
            {
                return _from;
            }
        }

        [NotMapped]
        protected InternetAddressList _from;

        /// <summary>
        /// Отправитель
        /// </summary>
        [NotMapped]
        public MailboxAddress Sender { set; get; }
    }

    /// <summary>
    /// Получение почты
    /// </summary>
    public class GetEmail
    {
        private IGetSettings Settings { set; get; }

        private System.Threading.CancellationToken CancellationToken { set; get; } = default;

        /// <summary>
        /// Полученные сообщения
        /// </summary>
        public List<PostMessage> Messages { set; get; } = new List<PostMessage>();

        /// <summary>
        /// Путь к получаемым вложениям
        /// </summary>
        public static string PathToAttachment { set; get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");

        /// <summary>
        /// Список вложений
        /// </summary>
        public IList<FileInfo> SavedFilesAttachments { set; get; } = new List<FileInfo>();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="settings"></param>
        public GetEmail(IGetSettings settings)
        {
            Settings = settings;
        }

        /*
        public static void SaveFileMemoryStreamAttachment(MemoryStreamAttachment file, string startPath)
        {
            string pathToSave;
            int i = 0;
            var path = startPath;
            while (true)
            {
                if (!File.Exists(path))
                {
                    file.FullFileNameOnDisk = path;
                    pathToSave = path;
                    break;
                }
                else
                {
                    path = Path.Combine(Path.GetDirectoryName(startPath), Path.GetFileNameWithoutExtension(startPath) + i + Path.GetExtension(startPath));
                    i++;
                }
            }
            using FileStream fileStream = new(pathToSave, FileMode.Create, System.IO.FileAccess.Write);
            file.MemoryStream.WriteTo(fileStream);
            file.FullFileNameOnDisk = pathToSave;
        }
        */

        private PostMessage ReturnOneMessage(MimeMessage mimeMessage)
        {
            PostMessage oneMessage = new()
            {
                Date = mimeMessage.Date,
                Sender = mimeMessage.Sender,
                Subject = mimeMessage.Subject,
                TextBody = mimeMessage.TextBody,
                From = mimeMessage.From,
            };
            foreach (var x in mimeMessage.Attachments)
            {
                if (x.IsAttachment)
                {
                    MemoryStream memoryStream = new();
                    if (x is MessagePart part)
                    {
                        part.Message.WriteTo(memoryStream);
                    }
                    if (x is MimePart part1)
                    {
                        part1.Content.DecodeTo(memoryStream);
                    }

                    MemoryStreamAttachment streamAttachment = new()
                    {
                        MemoryStream = memoryStream,
                        FileName = x.ContentDisposition.FileName
                    };
                    oneMessage.AttachmentFiles.Add(streamAttachment);
                }
            }
            return oneMessage;
        }

        /// <summary>
        /// Для реализации в библиотеке Standart
        /// </summary>
        /// <param name="relativeTo"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetRelativePathStandart(string relativeTo, string path)
        {
            var uri = new Uri(relativeTo);
            var rel = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (!rel.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                rel = $".{Path.DirectorySeparatorChar}{rel}";
            }
            return rel;
        }

        public void SaveFileMemoryStreamAttachment(MemoryStreamAttachment file, ref string startPath)
        {
            string pathToSave;
            int i = 0;
            var path = startPath;
            while (true)
            {
                if (!File.Exists(path))
                {
                    file.FullFileNameOnDisk = path;
#if NET5_0_OR_GREATER
                    file.SubPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
#else
                    file.SubPath = GetRelativePathStandart(Directory.GetCurrentDirectory(), path);
#endif
                    pathToSave = path;
                    break;
                }
                else
                {
                    path = Path.Combine(Path.GetDirectoryName(startPath), Path.GetFileNameWithoutExtension(startPath) + i + Path.GetExtension(startPath));
                    i++;
                }
            }
            using (FileStream fileStream = new(pathToSave, FileMode.Create, System.IO.FileAccess.Write))
            {
                file.MemoryStream.WriteTo(fileStream);
                file.FullFileNameOnDisk = pathToSave;
            }
            SavedFilesAttachments.Add(new FileInfo(pathToSave));
        }

        /// <summary>
        /// Получение всех сообщений
        /// </summary>
        public IList<IPostMessage> SaveNewEmailToBaseAndAttachments()
        {
            if (!Directory.Exists(PathToAttachment))
            {
                Directory.CreateDirectory(PathToAttachment);
            }
            GetAll();
            PostBase postBase = new();
            foreach (var message in Messages)
            {
                if (!postBase.PostMessages.Where(x => x.Date == message.Date && x.SenderString == message.SenderString).Any())
                {
                    postBase.PostMessages.Add(message);
                    foreach (var file in message.AttachmentFiles)
                    {
                        if (file.FileName != null)
                        {
                            var path = Path.Combine(PathToAttachment, file.FileName);
                            SaveFileMemoryStreamAttachment(file, ref path);
                        }
                    }
                    postBase.SaveChanges();
                }
            }
            return Messages.ConvertAll(x => (IPostMessage)x);
        }

        public IList<IPostMessage> SaveNewEmailToBaseAndAttachments(DateTime datestart = default, DateTime dateEnd = default)
        {
            if (!Directory.Exists(PathToAttachment))
            {
                Directory.CreateDirectory(PathToAttachment);
            }
            if (datestart != default && dateEnd != default)
            {
                GetAll(datestart, dateEnd);
            }
            else
            {
                GetAll();
            }
            PostBase postBase = new();
            foreach (var message in Messages)
            {
                if (!postBase.PostMessages.Where(x => x.Date == message.Date && x.SenderString == message.SenderString).Any())
                {
                    postBase.PostMessages.Add(message);
                    foreach (var file in message.AttachmentFiles)
                    {
                        if (file.FileName != null)
                        {
                            var path = Path.Combine(PathToAttachment, file.FileName);
                            SaveFileMemoryStreamAttachment(file, ref path);
                        }
                    }
                    postBase.SaveChanges();
                }
            }
            return Messages.ConvertAll(x => (IPostMessage)x);
        }

        /// <summary>
        /// GetAttachmentsFullFileNameFromBase
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public string[] GetAttachmentsFullFileNameFromBase(DateTime dateStart, DateTime dateEnd)
        {
            PostBase postBase = new();
            var dates = postBase.PostMessages.Select(x => new { x.Date, x.Id }).AsEnumerable();
            if (dates.Any())
            {
                var idV = dates.Where(x => x.Date < dateEnd && x.Date >= dateStart).Select(y => y.Id).AsEnumerable();
                var loadMessages = postBase.PostMessages.Where(x => idV.Contains(x.Id))
                    .Select(y => y.AttachmentFiles)
                    .SelectMany(k => k).Where(d => d != null).Select(l => l.FullFileNameOnDisk);
                return loadMessages?.ToArray();
            }
            else
            {
                return null;
            }
            //return loadMessages?.ToArray();
        }

        /// <summary>
        /// Получает сообщения из базы данных в указанный диапазон дат.
        /// </summary>
        /// <param name="dateStart">Дата начала диапазона.</param>
        /// <param name="dateEnd">Дата окончания диапазона.</param>
        /// <returns>Коллекция сообщений.</returns>
        public IEnumerable<PostMessage> GetPostMessagesFromBase(DateTime dateStart, DateTime dateEnd)
        {
            PostBase postBase = new();
            var dates = postBase.PostMessages.Select(x => new { x.Date, x.Id }).AsEnumerable();
            if (dates.Any())
            {
                var idV = dates.Where(x => x.Date < dateEnd && x.Date >= dateStart).Select(y => y.Id);
    #if NET2_0_OR_GREATER
                idV = idV.ToHashSet();
    #endif
                var loadMessages = postBase.PostMessages.Where(x => idV.Contains(x.Id));
    #if NET2_0_OR_GREATER
                loadMessages = loadMessages.ToHashSet();
    #endif
                return loadMessages;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Получение всех сообщений
        /// </summary>
        public void GetAll()
        {
            using var client = new ImapClient();
            client.Timeout = 40000;
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.Connect(Settings.Host, Settings.Port, Settings.SecureSocket, CancellationToken);
            client.Authenticate(Settings.Login, Settings.Password, CancellationToken);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            //  Чтобы избавиться "System.NotSupportedException" в System.Private.CoreLib.dll: 'No data is available for encoding 50220. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.
            IMailFolder inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);
            var endRange = inbox.Count - Settings.InBoxCount;
            if (endRange < 0)
            {
                endRange = 0;
            }
            for (int i = inbox.Count - 1; i > endRange; i--)
            {
                var message = inbox.GetMessage(i);
                var addresses = inbox.GetMessage(i).From;

                if (addresses.FirstOrDefault() is MailboxAddress mailboxAddres)
                {
                    if (Settings.FromAdresses.Contains(mailboxAddres.Address))
                    {
                        Messages.Add(ReturnOneMessage(message));
                    }
                }
            }
            client.Disconnect(true);
        }

        /// <summary>
        /// Получает все сообщения электронной почты из входящих в указанный диапазон дат.
        /// </summary>
        /// <param name="dateTimeStart">Дата начала диапазона.</param>
        /// <param name="dateTimeEnd">Дата окончания диапазона.</param>
        public void GetAll(DateTime dateTimeStart, DateTime dateTimeEnd)
        {
            using var client = new ImapClient();
            client.Timeout = 40000;
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.Connect(Settings.Host, Settings.Port, Settings.SecureSocket, CancellationToken);
            client.Authenticate(Settings.Login, Settings.Password, CancellationToken);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            IMailFolder inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);
            var query = MailKit.Search.SearchQuery.DeliveredAfter(dateTimeStart).And(MailKit.Search.SearchQuery.DeliveredBefore(dateTimeEnd));
            var inboxIds = inbox.Search(query);
            foreach (var messageId in inboxIds)
            {
                var message = inbox.GetMessage(messageId);
                var addresses = message.From;
                if (addresses.FirstOrDefault() is MailboxAddress mailboxAddres)
                {
                    if (Settings.FromAdresses.Contains(mailboxAddres.Address))
                    {
                        Messages.Add(ReturnOneMessage(message));
                    }
                }
            }
            client.Disconnect(true);
        }
    }
}