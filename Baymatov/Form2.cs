using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailKit.Net.Smtp;
using MimeKit;

namespace Baymatov
{
    public partial class Form2 : Form
    {
        private Dictionary<string, string> users = new Dictionary<string, string>
        {
            { "admin", "admin" },
            { "user", "user" }
        };

        private string tempPassword = string.Empty;
        public string CurrentUser { get; private set; }

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private bool IsValidLogin(string login, string password)
        {
            if (login == "user")
            {
                if (password == tempPassword)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Неверный временный пароль. Пожалуйста, проверьте свою электронную почту.");
                    return false;
                }
            }
            else
            {
                return users.TryGetValue(login, out string storedPassword) && storedPassword == password;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;
            string password = textBox2.Text;

            if (IsValidLogin(login, password))
            {
                CurrentUser = login;
                this.Hide();
                Form1 mainForm = new Form1(CurrentUser);
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.");
            }
        }

        private void SendPasswordToEmail(string login)
        {
            if (users.TryGetValue(login, out string password))
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Your Name", "your-email@mail.ru"));
                message.To.Add(new MailboxAddress("", login));
                message.Subject = "Ваш пароль";
                message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = $"Ваш пароль: {password}"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.mail.ru", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("your-email@mail.ru", "your-application-password");
                    client.Send(message);
                    client.Disconnect(true);
                }

                MessageBox.Show("Пароль отправлен на вашу почту.");
            }
            else
            {
                MessageBox.Show("Пользователь не найден.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string email = textBox3.Text;
            if (!string.IsNullOrEmpty(email))
            {
                tempPassword = GenerateTempPassword();
                SendTempPasswordToEmail(email, tempPassword);
                MessageBox.Show("Временный пароль отправлен на вашу электронную почту.");
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите адрес электронной почты.");
            }
        }

        private string GenerateTempPassword()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }

        private void SendTempPasswordToEmail(string email, string tempPassword)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Your App", "duck2000super@list.ru")); // 
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Временный пароль для входа";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = $"Ваш временный пароль: {tempPassword}"
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.mail.ru", 587, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate("duck2000super@list.ru", "13jbZt1nJk0t1TFxkKdd");
                client.Send(message);
                client.Disconnect(true);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
