using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO;


namespace UsbVal
{
    public partial class Form1 : Form
    {
        private Timer _usbCheckTimer;
        private bool port1 = false;
        private bool port2 = false;
        private bool port3 = false;
        private bool port4 = false;
        private bool sdcard = false;
        private bool usbtest = false;
        private string sdresult;
        private string generatedFilePath = @"C:\Temp\GeneratedFile_100MB.txt"; // Caminho temporário para salvar o arquivo gerado
        public Form1()
        {
            InitializeComponent();
            InitializeUsbCheckTimer();
        }

        public class PortInfo
        {
            public string PortName { get; }
            public string Caption { get; }
            public string Manufacturer { get; }
            public string DeviceID { get; }
            public string LocationInformation { get; }



            public PortInfo(string portName, string caption, string manufacturer, string deviceID, string locationInformation)
            {
                PortName = portName;
                Caption = caption;
                Manufacturer = manufacturer;
                DeviceID = deviceID;
                LocationInformation = locationInformation;
            }
        }

        public static List<PortInfo> GetUSBPortInfos()
        {
            List<PortInfo> ports = new List<PortInfo>();

            using (ManagementClass managementClass = new ManagementClass("Win32_PnPEntity"))
            {
                foreach (ManagementObject instance in managementClass.GetInstances())
                {
                    var classGuid = instance.GetPropertyValue("ClassGuid");
                    if (classGuid == null || classGuid.ToString().ToUpper() != "{36FC9E60-C465-11CF-8056-444553540000}")
                        continue; // USB devices are under this GUID.

                    try
                    {
                        string caption = instance.GetPropertyValue("Caption")?.ToString();
                        string manufacturer = instance.GetPropertyValue("Manufacturer")?.ToString();
                        string deviceID = instance.GetPropertyValue("PnpDeviceID")?.ToString();
                        string regPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + deviceID;
                        string regPathDevParams = regPath + "\\Device Parameters";
                        string locationInfo = Registry.GetValue(regPath, "LocationInformation", "")?.ToString();
                        string portName = Registry.GetValue(regPathDevParams, "PortName", "")?.ToString();

                        if (caption != null)
                        {
                            int pos = caption.IndexOf(" (COM");
                            if (pos > 0) // Remove COM port from description
                                caption = caption.Substring(0, pos);

                            ports.Add(new PortInfo(portName, caption, manufacturer, deviceID, locationInfo));
                        }
                    }
                    catch (NullReferenceException)
                    {
                        // Skip devices that cause null reference exceptions.
                    }
                }
            }

            return ports;
        }

        private void InitializeUsbCheckTimer()
        {
            _usbCheckTimer = new Timer();
            _usbCheckTimer.Interval = 2000; // Intervalo de 5 segundos (pode ajustar conforme necessário)
            _usbCheckTimer.Tick += UsbCheckTimer_Tick;
            _usbCheckTimer.Start();
        }

        private void UsbCheckTimer_Tick(object sender, EventArgs e)
        {
            List<PortInfo> usbPorts = GetUSBPortInfos();

            bool foundSpecificPort = false;

            foreach (var port in usbPorts)
            {
                // Verifica se o nome da porta é "Port_#0012.Hub_#0001"
                if (port.LocationInformation == "Port_#0012.Hub_#0001" && port1==false)
                {
                    pictureBox7.Visible = true;
                    port1 = true;
                    GenerateAndCopyFileToUsb(progressBar1);  // Copiar o arquivo para a unidade USB
                    foundSpecificPort = true;                  
                    break;
                }

                if (port.LocationInformation == "Port_#0009.Hub_#0001" && port2 == false)
                {
                    pictureBox8.Visible = true;
                    port2 = true;
                    GenerateAndCopyFileToUsb(progressBar4);  // Copiar o arquivo para a unidade USB Port_#0004.Hub_#0001
                    foundSpecificPort = true;
                    break;
                }


                if (port.LocationInformation == "Port_#0004.Hub_#0001" && port3 == false)
                {
                    pictureBox3.Visible = true;
                    port3 = true;
                    GenerateAndCopyFileToUsb(progressBar3);  // Copiar o arquivo para a unidade USB Port_#0004.Hub_#0001 Port_#0017.Hub_#0001
                    foundSpecificPort = true;
                    break;
                }

                if (port.LocationInformation == "Port_#0017.Hub_#0001" && port4 == false)
                {
                    pictureBox11.Visible = true;
                    port4 = true;
                    GenerateAndCopyFileToUsb(progressBar6);  // Copiar o arquivo para a unidade USB Port_#0004.Hub_#0001 
                    foundSpecificPort = true;
                    break;
                }
                sdresult = GetSdDriveLetter();
                if (port.LocationInformation != "Port_#0012.Hub_#0001" && port.LocationInformation != "Port_#0009.Hub_#0001" && port.LocationInformation != "Port_#0004.Hub_#0001" && port.LocationInformation != "Port_#0017.Hub_#0001" && sdcard ==false && sdresult != null)
                {
                    pictureBox10.Visible = true;
                    sdcard = true;
                    GenerateAndCopyFileToUsb(progressBar5);  // Copiar o arquivo para a unidade USB Port_#0004.Hub_#0001 
                    foundSpecificPort = true;
                    break;

                }
                if(port1==true && port2 == true && port3 == true && port4 == true && sdcard == true && usbtest==false)
                {
                    pictureBox1.BackColor = Color.Green;
                    usbtest = true;
                    MessageBox.Show("Teste Usb Finalizado Com Sucesso!!!!!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }



            // Caso queira também exibir informações dos dispositivos, pode usar o código abaixo.
            // Isso pode ser removido se não for necessário.
            //if (!foundSpecificPort && usbPorts.Count > 0)
            //{
            //    string message = "Dispositivos USB conectados:\n\n";
            //    foreach (var port in usbPorts)
            //    {
            //        message += $"Dispositivo: {port.Caption}\n";
            //        message += $"Fabricante: {port.Manufacturer}\n";
            //        message += $"ID do Dispositivo: {port.DeviceID}\n";
            //        message += $"Localização: {port.LocationInformation}\n";
            //        message += $"Porta: {port.PortName}\n";
            //        message += "---------------------------------------------------\n";
            //    }

            //    MessageBox.Show(message, "Informações sobre dispositivos USB", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
        }


        private void GenerateAndCopyFileToUsb(ProgressBar x)
        {
            // Gerar um arquivo de 100 MB
            GenerateLargeFile(generatedFilePath);

            string usbDriveLetter = GetUsbDriveLetter();

            if (!string.IsNullOrEmpty(usbDriveLetter))
            {
                string destinationFilePath = Path.Combine(usbDriveLetter, Path.GetFileName(generatedFilePath));

                // Exibindo a ProgressBar
                x.Visible = true;
                x.Value = 0;
                x.Maximum = (int)new FileInfo(generatedFilePath).Length;

                using (FileStream sourceStream = new FileStream(generatedFilePath, FileMode.Open, FileAccess.Read))
                using (FileStream destStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[8192]; // Buffer de 8KB
                    long totalBytesRead = 0;
                    int bytesRead;

                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destStream.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        x.Value = (int)totalBytesRead; // Atualiza o progresso
                    }
                }
                
                MessageBox.Show("Tranferencia Validada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
              //  MessageBox.Show("Não foi possível identificar a unidade USB.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateLargeFile(string path)
        {
            const int fileSizeInMB = 100;
            byte[] buffer = new byte[1024 * 1024]; // 1MB por buffer
            Random rand = new Random();

            // Criação do diretório se não existir
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory); // Cria o diretório
            }

            // Criação do arquivo de 100 MB
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                for (int i = 0; i < fileSizeInMB; i++)
                {
                    rand.NextBytes(buffer); // Preencher com bytes aleatórios
                    fs.Write(buffer, 0, buffer.Length); // Escrever no arquivo
                }
            }
        }


        private string GetUsbDriveLetter()
        {
            // Obtém as unidades montadas no sistema
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    // Retorna a letra da unidade USB
                    return drive.Name; // Exemplo: "E:\", "F:\"
                }
            }
            return null; // Se não encontrar uma unidade USB
        }

        private string GetSdDriveLetter()
        {
            // Obtém as unidades montadas no sistema
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable && drive.VolumeLabel == "SDHC")
                {
                    // Retorna a letra da unidade USB
                    return drive.Name; // Exemplo: "E:\", "F:\"
                }
            }
            return null; // Se não encontrar uma unidade USB
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            {
                List<PortInfo> usbPorts = GetUSBPortInfos();

                if (usbPorts.Count == 0)
                {
                    MessageBox.Show("Nenhum dispositivo USB encontrado.");
                }
                else
                {
                    string message = "Dispositivos USB conectados:\n\n";
                    foreach (var port in usbPorts)
                    {
                        message += $"Dispositivo: {port.Caption}\n";
                        message += $"Fabricante: {port.Manufacturer}\n";
                        message += $"ID do Dispositivo: {port.DeviceID}\n";
                        message += $"Localização: {port.LocationInformation}\n";
                        message += $"Porta: {port.PortName}\n";
                        message += "---------------------------------------------------\n";
                    }

         //           MessageBox.Show(message, "Informações sobre dispositivos USB", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox7.Visible = false;
            pictureBox8.Visible = false;
            pictureBox3.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox7.Visible = true;
            pictureBox8.Visible = true;
            pictureBox3.Visible = true;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {

        }
    }
}
