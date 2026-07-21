using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProntoAutoLogin
{
    static class Program
    {
        private const string AppName = "ProntoAutoLogin";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. Try to load existing credentials
            var (regNum, password) = LoadCredentials();

            // 2. If no credentials exist, show setup GUI
            if (string.IsNullOrEmpty(regNum) || string.IsNullOrEmpty(password))
            {
                if (!PromptSetup(out regNum, out password))
                {
                    return; // User closed setup without saving
                }
                SaveCredentials(regNum, password);
            }

            // 3. Run network monitor invisibly in background
            MonitorNetwork(regNum, password);
        }

        // Encrypt credentials using Windows DPAPI (tied to current Windows User Account)
        private static void SaveCredentials(string regNum, string password)
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
            Directory.CreateDirectory(appData);

            byte[] regBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(regNum), null, DataProtectionScope.CurrentUser);
            byte[] passBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), null, DataProtectionScope.CurrentUser);

            File.WriteAllBytes(Path.Combine(appData, "reg.dat"), regBytes);
            File.WriteAllBytes(Path.Combine(appData, "pass.dat"), passBytes);
        }

        private static (string, string) LoadCredentials()
        {
            try
            {
                string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
                string regPath = Path.Combine(appData, "reg.dat");
                string passPath = Path.Combine(appData, "pass.dat");

                if (!File.Exists(regPath) || !File.Exists(passPath)) return (null, null);

                byte[] regBytes = ProtectedData.Unprotect(File.ReadAllBytes(regPath), null, DataProtectionScope.CurrentUser);
                byte[] passBytes = ProtectedData.Unprotect(File.ReadAllBytes(passPath), null, DataProtectionScope.CurrentUser);

                return (Encoding.UTF8.GetString(regBytes), Encoding.UTF8.GetString(passBytes));
            }
            catch
            {
                return (null, null);
            }
        }

        private static bool PromptSetup(out string regNum, out string password)
        {
            regNum = "";
            password = "";

            Form form = new Form
            {
                Width = 340,
                Height = 220,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Pronto AutoLogin Setup",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label lblReg = new Label { Left = 20, Top = 15, Text = "Registration Number:", AutoSize = true };
            TextBox txtReg = new TextBox { Left = 20, Top = 35, Width = 285 };

            Label lblPass = new Label { Left = 20, Top = 70, Text = "Password:", AutoSize = true };
            TextBox txtPass = new TextBox { Left = 20, Top = 90, Width = 285, PasswordChar = '*' };

            Button btnSave = new Button { Text = "Save & Start", Left = 100, Top = 130, Width = 120, DialogResult = DialogResult.OK };

            form.Controls.AddRange(new Control[] { lblReg, txtReg, lblPass, txtPass, btnSave });
            form.AcceptButton = btnSave;

            if (form.ShowDialog() == DialogResult.OK)
            {
                regNum = txtReg.Text.Trim();
                password = txtPass.Text.Trim();
                return !string.IsNullOrEmpty(regNum) && !string.IsNullOrEmpty(password);
            }

            return false;
        }

        private static void MonitorNetwork(string regNum, string password)
        {
            string currentSsid = GetCurrentSsid();

            // Ignore SSL errors if local gateway uses self-signed cert
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using (var client = new HttpClient(handler))
            {
                while (true)
                {
                    Thread.Sleep(3000); // Check SSID every 3 seconds
                    string newSsid = GetCurrentSsid();

                    if (newSsid != currentSsid)
                    {
                        currentSsid = newSsid;
                        if (!string.IsNullOrEmpty(currentSsid))
                        {
                            Thread.Sleep(2000); // Wait 2 seconds post-connection
                            _ = SendAuthRequest(client, regNum, password);
                        }
                    }
                }
            }
        }

        private static string GetCurrentSsid()
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "wlan show interfaces",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                foreach (var line in output.Split('\n'))
                {
                    if (line.Contains(" SSID ") && !line.Contains("BSSID"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1) return parts[1].Trim();
                    }
                }
            }
            catch { }
            return null;
        }

        private static async Task SendAuthRequest(HttpClient client, string regNum, string password)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "http://phc.prontonetworks.com/cgi-bin/authlogin?URI=http://example.com");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.Headers.Add("Origin", "http://phc.prontonetworks.com");
                request.Headers.Add("Referer", "http://phc.prontonetworks.com/cgi-bin/authlogin?URI=http://www.msftconnecttest.com/redirect");

                var content = new FormUrlEncodedContent(new[]
                {
                    new System.Collections.Generic.KeyValuePair<string, string>("userId", regNum),
                    new System.Collections.Generic.KeyValuePair<string, string>("password", password),
                    new System.Collections.Generic.KeyValuePair<string, string>("serviceName", "ProntoAuthentication")
                });

                request.Content = content;
                await client.SendAsync(request);
            }
            catch { }
        }
    }
}