using EcommerceCore.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace EcommerceCore.Infrastructure.Services;

/// <summary>
/// Clase auxiliar para leer la configuración
/// </summary>
public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
}

public class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    // Configuración de Mailtrap
    private readonly SmtpSettings _settings = configuration.GetSection(key: "SmtpSettings").Get<SmtpSettings>() ?? throw new InvalidOperationException(message: "SmtpSettings no configurado");
    
    // Instancia del cliente SMTP
    public async Task SendVerificationEmailAsync(string toEmail, string userName, string verificationLink)
    {
        var emailSubject = "¡Bienvenido a EcommerceCore! Confirma tu email";

        // 1. Lee la plantilla
        var templatePath = Path.Combine(path1: AppContext.BaseDirectory,
            path2: "EmailTemplates",
            path3: "VerificationEmail.html");
        var htmlBody = await File.ReadAllTextAsync(path: templatePath);

        // 2. Reemplaza placeholders
        htmlBody = htmlBody.Replace(oldValue: "{{UserName}}",
            newValue: userName);
        htmlBody = htmlBody.Replace(oldValue: "{{Link}}",
            newValue: verificationLink);

        // 3. Envía el email (usando un nuevo método helper privado)
        await SendEmailInternalAsync(toEmail: toEmail,
            subject: emailSubject,
            htmlBody: htmlBody);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
    {
        var emailSubject = "Restablece tu contraseña de EcommerceCore";
        
        var templatePath = Path.Combine(path1: AppContext.BaseDirectory,
            path2: "EmailTemplates",
            path3: "PasswordResetEmail.html");
        var htmlBody = await File.ReadAllTextAsync(path: templatePath);
        
        htmlBody = htmlBody.Replace(oldValue: "{{UserName}}",
            newValue: userName);
        htmlBody = htmlBody.Replace(oldValue: "{{Link}}",
            newValue: resetLink);

        await SendEmailInternalAsync(toEmail: toEmail,
            subject: emailSubject,
            htmlBody: htmlBody);
    }

    private async Task SendEmailInternalAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        // Usamos la configuración genérica
        message.From.Add(address: new MailboxAddress(name: _settings.FromName ?? "EcommerceCore",
            address: _settings.FromEmail));
        message.To.Add(address: new MailboxAddress(name: toEmail,
            address: toEmail));
        message.Subject = subject;
        message.Body = new TextPart(subtype: "html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            // Conexión agnóstica (funciona para Mailtrap y AWS SES)
            await client.ConnectAsync(host: _settings.Host,
                port: _settings.Port,
                options: MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(userName: _settings.Username,
                password: _settings.Password);
            await client.SendAsync(message: message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(value: ex.Message);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(quit: true);
        }
    }
}