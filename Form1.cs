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


namespace UsbVal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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

                    MessageBox.Show(message, "Informações sobre dispositivos USB", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox7.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox7.Visible = true;
        }
    }
}
