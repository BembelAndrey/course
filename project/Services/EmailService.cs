using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace project.Services
{
    public class SmtpConfig
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = "";
        public string SenderPassword { get; set; } = "";
    }

    public class EmailService
    {
        public static void SendOrderNotification(string email, int orderId, string status)
        {
            if (string.IsNullOrWhiteSpace(email)) return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string logPath = Path.Combine(baseDir, "email_error.log");

            try
            {
                string configPath = Path.Combine(baseDir, "smtpsettings.json");
                
                // Если файла настроек нет, создаем шаблонный
                if (!File.Exists(configPath))
                {
                    var defaultConfig = new SmtpConfig();
                    File.WriteAllText(configPath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Файл smtpsettings.json не найден. Создан шаблон по пути {configPath}\n");
                    return;
                }

                // Читаем настройки
                var config = JsonSerializer.Deserialize<SmtpConfig>(File.ReadAllText(configPath));
                
                if (config == null || string.IsNullOrWhiteSpace(config.SenderEmail) || string.IsNullOrWhiteSpace(config.SenderPassword))
                {
                    File.AppendAllText(logPath, $"[{DateTime.Now}] В файле {configPath} не заполнены SenderEmail или SenderPassword.\n");
                    return;
                }

                var fromAddress = new MailAddress(config.SenderEmail, "SoundProject");
                var toAddress = new MailAddress(email);
                string subject = $"Обновление статуса заказа #{orderId}";
                string body = $"Здравствуйте!\n\nСтатус вашего заказа #{orderId} был изменен на: {status}.\n\nС уважением,\nКоманда SoundProject.";

                var smtp = new SmtpClient
                {
                    Host = config.SmtpServer,
                    Port = config.SmtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, config.SenderPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
                
                File.AppendAllText(Path.Combine(baseDir, "email_success.log"), $"[{DateTime.Now}] Успешно отправлено на {email}\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] ОШИБКА ОТПРАВКИ на {email}:\n{ex.Message}\nВнутренняя ошибка: {ex.InnerException?.Message}\n\n");
            }
        }
    }
}
