using Maggsoft.Core.IO;
using Maggsoft.Data.Sqlite.Messages;
using Maggsoft.Sqlite.Services.Messages;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Maggsoft.Sqlite.Messages;

/// <summary>
/// Email sender
/// </summary>
public partial class EmailSender(IMaggsoftFileProvider fileProvider, ISmtpBuilder smtpBuilder) : IEmailSender
{
    #region Fields

    private readonly IMaggsoftFileProvider _fileProvider = fileProvider;
    private readonly ISmtpBuilder _smtpBuilder = smtpBuilder;

    #endregion

    #region Utilities

    /// <summary>
    /// Create an file attachment for the specific file path
    /// </summary>
    /// <param name="filePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a leaf-node MIME part that contains an attachment.
    /// </returns>
    protected async Task<MimePart> CreateMimeAttachmentAsync(string filePath, string attachmentFileName = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        if (string.IsNullOrWhiteSpace(attachmentFileName))
            attachmentFileName = Path.GetFileName(filePath);

        return CreateMimeAttachment(
                attachmentFileName,
                await _fileProvider.ReadAllBytesAsync(filePath),
                _fileProvider.GetCreationTime(filePath),
                _fileProvider.GetLastWriteTime(filePath),
                _fileProvider.GetLastAccessTime(filePath));
    }

    /// <summary>
    /// Create an file attachment for the binary data
    /// </summary>
    /// <param name="attachmentFileName">Attachment file name</param>
    /// <param name="binaryContent">The array of unsigned bytes from which to create the attachment stream.</param>
    /// <param name="cDate">Creation date and time for the specified file or directory</param>
    /// <param name="mDate">Date and time that the specified file or directory was last written to</param>
    /// <param name="rDate">Date and time that the specified file or directory was last access to.</param>
    /// <returns>A leaf-node MIME part that contains an attachment.</returns>
    protected MimePart CreateMimeAttachment(string attachmentFileName, byte[] binaryContent, DateTime cDate, DateTime mDate, DateTime rDate)
    {
        if (!ContentType.TryParse(MimeKit.MimeTypes.GetMimeType(attachmentFileName), out var mimeContentType))
            mimeContentType = new ContentType("application", "octet-stream");

        return new MimePart(mimeContentType)
        {
            FileName = attachmentFileName,
            Content = new MimeContent(new MemoryStream(binaryContent)),
            ContentDisposition = new ContentDisposition
            {
                CreationDate = cDate,
                ModificationDate = mDate,
                ReadDate = rDate
            }
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sends an email
    /// </summary>
    /// <param name="emailAccount">Email account to use</param>
    /// <param name="subject">Subject</param>
    /// <param name="body">Body</param>
    /// <param name="fromAddress">From address</param>
    /// <param name="fromName">From display name</param>
    /// <param name="toAddress">To address</param>
    /// <param name="toName">To display name</param>
    /// <param name="replyTo">ReplyTo address</param>
    /// <param name="replyToName">ReplyTo display name</param>
    /// <param name="bcc">BCC addresses list</param>
    /// <param name="cc">CC addresses list</param>
    /// <param name="attachmentFilePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
    /// <param name="headers">Headers</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task SendEmailAsync(EmailAccount emailAccount, string subject, string body,
        string fromAddress, string fromName, string toAddress, string toName,
        string replyTo = null, string replyToName = null,
        IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
        string attachmentFilePath = null, string attachmentFileName = null,
        IDictionary<string, string> headers = null)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toName, toAddress));

        if (!string.IsNullOrEmpty(replyTo))
        {
            message.ReplyTo.Add(new MailboxAddress(replyToName, replyTo));
        }

        //BCC
        if (bcc != null)
        {
            foreach (var address in bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
            {
                message.Bcc.Add(new MailboxAddress("", address.Trim()));
            }
        }

        //CC
        if (cc != null)
        {
            foreach (var address in cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
            {
                message.Cc.Add(new MailboxAddress("", address.Trim()));
            }
        }

        //content
        message.Subject = subject;

        //headers
        if (headers != null)
            foreach (var header in headers)
            {
                message.Headers.Add(header.Key, header.Value);
            }

        var multipart = new Multipart("mixed")
        {
            new TextPart(TextFormat.Html) { Text = body }
        };

        //create the file attachment for this e-mail message
        if (!string.IsNullOrEmpty(attachmentFilePath) && _fileProvider.FileExists(attachmentFilePath))
        {
            multipart.Add(await CreateMimeAttachmentAsync(attachmentFilePath, attachmentFileName));
        }

       

        message.Body = multipart;

        //send email
        using var smtpClient = await _smtpBuilder.BuildAsync(emailAccount);
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }

    #endregion
}

/*
 public class MyEmailSender : IEmailSender
{
    private readonly EmailClient _client;

    public MyEmailSender(IConfiguration config)
    {
        var credential = new ChainedTokenCredential(
            new ClientSecretCredential( 
                _config["AZURE_TENANT_ID"],
                _config["AZURE_CLIENT_ID"],
                _config["AZURE_CLIENT_SECRET"]),
            new ManagedIdentityCredential()
        )
        _client = new EmailClient(new Uri("https://my-instance.communication.azure.com/")
    }

    public Task SendEmailAsync(string email, string subject, string message)
    {
        var recipients = new EmailRecipients(new [] { new EmailAddress(email) });
        var content = new EmailContent(subject)
        {
            PlainText = message
        };

        await _client.SendAsync(new EmailMessage("me@mywebsite.com", content, recipients);
    }

    public Task SendConfirmationLinkAsync<TUser>(TUser user, string email, string confirmationLink) where TUser : class
    {
        return SendEmailAsync(email, "Confirm your email for MyWebSite", $"Please confirm your MyWebSite account by <a href='{confirmationLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetLinkAsync<TUser>(TUser user, string email, string resetLink) where TUser : class
    {
        return SendEmailAsync(email, "Reset your password for MyWebSite", $"Please reset your MyWebSite password by <a href='{resetLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetCodeAsync<TUser>(TUser user, string email, string resetCode) where TUser : class
    {
        return SendEmailAsync(email, "Reset your password for MyWebSite", $"Reset your MyWebSite password using the following code: {resetCode}");
    }


    public async Task SendConfirmationEmailAsync<TUser>(TUser user, string email, string code, string confirmationLink, string? callbackUrl) where TUser : class
    {
        var frontendConfirmationLink = $"<a href='{callbackUrl}/{user.UserId}/{code}'>clicking here</a>";
        
        await SendEmailAsync(email, $"{user.Name}, confirm your email", $"Please confirm message (in my native language) {frontendConfirmationLink}.");
    }
    
    // Merged ResetCode & ResetLink into one.
    public async Task SendPasswordResetEmailAsync<TUser>(TUser user, string email, string resetCode, string resetLink, string? callbackUrl) where TUser : class
    {
        var frontendResetLink = $"<a href='{callbackUrl}/{email}/{resetCode}'>clicking here</a>";
    
        await SendEmailAsync(email, $"{user.Name}, reset your password", $"Reset password message (in my native language): {frontendResetLink}");
    }
}
 
 */