using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using GenerateInformation.GenInfo;
using Newtonsoft.Json;

namespace ConverterHTML
{
    public partial class Converter : Form
    {
        private static bool colch;
        private string path = "";

        public Converter()
        {
            InitializeComponent();
        }

        public static void ExToFile(Exception ex)
        {
            var file = AppDomain.CurrentDomain.BaseDirectory + "\\errors.log";

            var log = new List<string>();

            log.Add(DateTime.Now.ToString());
            log.Add(ex.ToString());
            log.Add("");

            File.AppendAllLines(file, log);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            path = Program.path != "" ? Program.path : "";

            if (path == "")
            {
                var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK) path = dialog.SelectedPath;
            }

            var files = Directory.GetFiles(Path.Combine(path, "InputFiles"), "*.json");

            progressBar1.Maximum = files.Length;

            var thread = new Thread(Cvrt);
            thread.Start(files);
        }

        private void Cvrt(object files)
        {
            var GenList = new List<General>();

            Directory.CreateDirectory(Path.Combine(path, "html"));

            foreach (var file in (string[])files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var comp = JsonConvert.DeserializeObject<General>(json);

                    if (!comp.IsCorrect()) throw new Exception("Недостаточно данных по компьютеру " + (comp.MachineName ?? "не определено"));

                    BeginInvoke((Action)(() => { labelStatus.Text = "Конвертация: " + comp.MachineName; }));

                    //Convert values
                    comp.RAMArray.MaxCapacity = comp.RAMArray.MaxCapacity / 1024 / 1024;
                    foreach (var item in comp.RAMArray.RAM) item.Capacity = item.Capacity / 1024 / 1024;
                    foreach (var item in comp.Drives) item.Size = item.Size / 1024 / 1024 / 1024;
                    //Convert values

                    #region GENERAL

                    var listhtml = new List<string>();
                    listhtml.Add(File.ReadAllText(Path.Combine(path, @"source\head.htm")));

                    listhtml.Add("<title>Компьютер " + comp.MachineName + "</title>");
                    listhtml.Add("</head>");
                    listhtml.Add("<center>");

                    var colorU = "e6e6e6";
                    var LAA = "";
                    if (comp.LoginAsAdministrator)
                    {
                        colorU = "ffbec7";
                        LAA = " (Администратор локальной машины)";
                    }

                    listhtml.Add("<table width=90% cellspacing=0 cellpadding=0 border=0 style=\"padding-left: 30px\">");
                    listhtml.Add("<tr><td valign=middle align=center width=180><img src=" + "\"../source/comp.png" + "\" width=150px height=150px></td>");
                    listhtml.Add("<td><table width=100% border=0>");
                    listhtml.Add("<tr height=25><td width=120 align=right></td><td bgcolor=919191 align=center colspan=2><font color=white><b>Компьютер " + comp.MachineName + "</b></font></td></tr>");
                    listhtml.Add("<tr><td width=120 align=right>Описание</td><td bgcolor=e6e6e6 width=65%>" + comp.Description + "</td><td bgcolor=e6e6e6 align=center> " +
                                 comp.GenDate.ToString("dd.MM.yyyy HH:mm:ss") + " </td></tr>");
                    listhtml.Add("<tr><td width=120 align=right>ОС</td><td bgcolor=e6e6e6 colspan=2>" + comp.WinVersion + "</td></tr>");
                    listhtml.Add("<tr><td width=120 align=right>Установлен</td><td bgcolor=e6e6e6 colspan=2>" + comp.InstallDate.ToString("dd.MM.yyyy HH:mm:ss") + "</td></tr>");
                    listhtml.Add("<tr><td width=120 align=right>Запущен</td><td bgcolor=e6e6e6 colspan=2>" + comp.LastBootUpTime.ToString("dd.MM.yyyy HH:mm:ss") + "</td></tr>");
                    listhtml.Add("<tr><td width=120 align=right>Архитектура</td><td bgcolor=e6e6e6 colspan=2>" + comp.OSArchitecture + "</td></tr>");
                    listhtml.Add("<tr><td width=120 align=right>С/Н</td><td bgcolor=e6e6e6 colspan=2>" + comp.SerialNumber + "</td></tr>");
                    listhtml.Add("<tr><td width=120 align=right>Системный диск</td><td bgcolor=e6e6e6 colspan=2>" + comp.SystemDrive + "</td></tr>");
                    listhtml.Add("<tr><td width=120 align=right>Пользователь</td><td bgcolor=" + colorU + " colspan=2>" + comp.UserName + LAA + "</td></tr>");
                    listhtml.Add("</table>");
                    listhtml.Add("</td></tr>");

                    #endregion

                    #region MONITOR

                    if (comp.Monitors != null && comp.Monitors.Count > 0)
                    {
                        listhtml.Add("<tr><td colspan=2 align=left><hr width=100%></td></tr>");
                        listhtml.Add("<tr><td valign=middle align=center width=180><img src=" + "\"../source/monitor.png" + "\" width=150px height=150px></td>");
                        listhtml.Add("<td><table width=100% border=0>");
                        listhtml.Add("<tr><td width=120 align=right valign=center>Мониторы</td><td bgcolor=e6e6e6>");

                        foreach (var item in comp.Monitors)
                        {
                            listhtml.Add(Helpers.GetMonitorManufacturer(item.ManufacturerName) + " | ");
                            listhtml.Add(item.UserFriendlyName + " | ");
                            listhtml.Add("Активен: " + (item.Active ? "Да" : "Нет") + "<br>");
                            listhtml.Add("S/N: " + item.SerialNumberID + "<br>");
                            listhtml.Add("Год производства: " + item.YearOfManufacture + "г.");
                            listhtml.Add("<hr>");
                        }

                        listhtml.RemoveAt(listhtml.Count - 1);
                        listhtml.Add("</table>");
                        listhtml.Add("</td></tr>");
                    }

                    #endregion

                    #region NETWORK

                    listhtml.Add("<tr><td colspan=2 align=left><hr width=100%></td></tr>");
                    listhtml.Add("<tr><td valign=middle align=center width=180><img src=" + "\"../source/net.png" + "\" width=150px height=150px></td>");
                    listhtml.Add("<td><table width=100% border=0>");
                    listhtml.Add("<tr><td width=120 align=right valign=center>Сетевые карты</td><td bgcolor=e6e6e6>");

                    foreach (var item in comp.NetworkAdapters)
                    {
                        listhtml.Add(item.Name + " | ");
                        listhtml.Add(item.Desription + " | ");
                        listhtml.Add(item.IPAddress + "<br>");
                        listhtml.Add("DNS: " + item.DnsSuffix + " | ");
                        listhtml.Add("Speed: " + item.Speed + " Мбит | ");
                        listhtml.Add("MAC: " + item.MAC);
                        listhtml.Add("<hr>");
                    }

                    listhtml.RemoveAt(listhtml.Count - 1);
                    listhtml.Add("</td>");

                    #endregion

                    #region MARKS

                    if (!comp.IsVirtualMachine)
                    {
                        try
                        {
                            double avgmem = comp.RAMArray.RAM.Sum(t => t.Speed) / comp.RAMArray.RAM.Count;

                            var totalMemMark = -3 + avgmem / 177;
                            totalMemMark = Math.Round(totalMemMark, 0);
                            if (totalMemMark > 10) totalMemMark = 10;

                            totalMemMark += comp.RAMArray.RAMCapacity / 1638.4;
                            totalMemMark = Math.Round(totalMemMark, 0);
                            if (totalMemMark > 20) totalMemMark = 20;

                            var totalCPUmark = comp.CPUs.Sum(t => t.MaxClockSpeed) / 1000 * comp.CPUs.Sum(t => t.NumberOfCores) * 1.666666666666667;
                            double totalDriveMark = 25;

                            totalCPUmark = Math.Round(totalCPUmark, 2);

                            var drive = comp.Drives.Find(t => t.IsPrimary);

                            try
                            {
                                totalDriveMark += Helpers.GetDiskType(drive.MediaType) == "SSD" ? 15 : -15;

                                if (comp.Marks.DiskScore >= 3 && comp.Marks.DiskScore < 8) totalDriveMark -= 10;
                                if (comp.Marks.DiskScore >= 8) totalDriveMark += 10;
                            }
                            catch { }

                            var tempvlv2 = levelP2(totalCPUmark, totalDriveMark, comp.Marks.GraphicsScore, totalMemMark, comp.RAMArray.RAMCapacity, comp.Drives.Sum(t => (long)t.Size));

                            var LVLtoStr = GeTotaltMarkFromLVL2(tempvlv2[0]);

                            comp.Marks.TotalMark = tempvlv2[0];

                            listhtml.Add("<th rowspan=3 width=300 valign=top  bgcolor=e6e6e6>");
                            listhtml.Add("<table width=" + "\"100%" + "\" cellspacing=" + "\"1" + "\" cellpadding=" + "\"0" + "\" bgcolor=" + "\"0" + "\"><tr><td>");
                            listhtml.Add("<table width=100% border=0  bgcolor=#c8c8c8>");
                            listhtml.Add("<tr><th rowspan=6><img src=" + "\"../source/" + Math.Round(tempvlv2[0] - 1) + ".png" + "\" width=\"50px\" align=" + "\"bottom" + "\" border=" + "\"0" +
                                         "\"><br>" + LVLtoStr + "</th>");
                            listhtml.Add("<td align=right>Оценка CPU</td><td width=40% align=center bgcolor=" + getcolorOfLVL2(tempvlv2[1], 1) + ">" + Math.Round(ReturnListMarks(tempvlv2)[1], 2) +
                                         "</td></tr>");
                            listhtml.Add("<tr><td align=right>Оценка диска</td><td align=center bgcolor=" + getcolorOfLVL2(tempvlv2[2], 2) + ">" + Math.Round(ReturnListMarks(tempvlv2)[2], 2) +
                                         "</td></tr>");
                            listhtml.Add("<tr><td align=right>Оценка GPU</td><td align=center bgcolor=" + getcolorOfLVL2(tempvlv2[3], 3) + ">" + Math.Round(ReturnListMarks(tempvlv2)[3], 2) +
                                         "</td></tr>");
                            listhtml.Add("<tr><td align=right>Оценка RAM</td><td align=center bgcolor=" + getcolorOfLVL2(tempvlv2[4], 4) + ">" + Math.Round(ReturnListMarks(tempvlv2)[4], 2) +
                                         "</td></tr>");
                            listhtml.Add("<tr><td align=right>Размер RAM</td><td align=center bgcolor=" + getcolorOfLVL2(tempvlv2[5], 5) + ">" + comp.RAMArray.RAMCapacity + " МБ</td></tr>");
                            listhtml.Add("<tr><td align=right>Размер диска</td><td align=center bgcolor=" + getcolorOfLVL2(tempvlv2[6], 6) + ">" + comp.Drives.Sum(t => (long)t.Size) +
                                         " ГБ</td></tr>");
                            listhtml.Add("</table>");
                        }
                        catch
                        {
                            listhtml.Add("<th rowspan=3 width=250 valign=top  bgcolor=e6e6e6>");
                            listhtml.Add("<table width=" + "\"100%" + "\" cellspacing=" + "\"1" + "\" cellpadding=" + "\"0" + "\" bgcolor=" + "\"0" + "\"><tr><td>");
                            listhtml.Add("<table width=100% border=0  bgcolor=#c8c8c8>");
                            listhtml.Add("<tr><th rowspan=3><img src=" + "\"\" alt=" + "\"\" align=" + "\"bottom" + "\" border=" + "\"0" + "\"><br>UNKNOWN</th>");

                            listhtml.Add("</table>");
                        }
                    }
                    else
                    {
                        listhtml.Add("<th rowspan=3 width=250 valign=top  bgcolor=e6e6e6>");
                        listhtml.Add("<table width=" + "\"100%" + "\" cellspacing=" + "\"1" + "\" cellpadding=" + "\"0" + "\" bgcolor=" + "\"0" + "\"><tr><td>");
                        listhtml.Add("<table width=100% border=0  bgcolor=#c8c8c8>");
                        listhtml.Add("<tr><th rowspan=3><img src=" + "\"\" alt=" + "\"\" align=" + "\"bottom" + "\" border=" + "\"0" + "\"><br>UNKNOWN</th>");

                        listhtml.Add("</table>");
                    }


                    listhtml.Add("</td></tr></table>");
                    listhtml.Add("</th></tr>");
                    listhtml.Add("</table>");
                    listhtml.Add("</td></tr>");

                    #endregion

                    #region MOTHERBOARD

                    listhtml.Add("<tr><td colspan=2 align=left height=4><hr width=100%></td></tr>");

                    listhtml.Add("<tr><td valign=top align=center width=180><p><img src=\"../source/mother.png\" width=150px height=150px></td>");
                    listhtml.Add("<td><table width=100% border=0>");
                    listhtml.Add("<tr><td width=120 valign=top align=right>Материнская плата</td><td bgcolor=e6e6e6>" + comp.Motherboard.Manufacturer + "<br>" + comp.Motherboard.Product + "<br>" +
                                 "S/N: " + comp.Motherboard.SerialNumber + "</td></tr>");


                    listhtml.Add("<tr><td width=120 valign=top align=right>Процессор</td><td bgcolor=e6e6e6>");

                    foreach (var cpu in comp.CPUs)
                    {
                        listhtml.Add(cpu.Name + "<br>" + "Сокет: " + Helpers.GetSocket(cpu.UpgradeMethod) + "<br>" + "Максимальная частота: " + cpu.MaxClockSpeed + " ГЦ<br>" + "Число ядер: " +
                                     cpu.NumberOfCores + "<br>" + "S/N: " + cpu.ProcessorId);
                        listhtml.Add("<hr>");
                    }

                    listhtml.RemoveAt(listhtml.Count - 1);
                    listhtml.Add("</td></tr>");


                    listhtml.Add("<tr><td width=120 valign=top align=right>Видеоадаптер</td><td bgcolor=e6e6e6>" + comp.VideoAdapter.AdapterCompatibility + " " + comp.VideoAdapter.Name + "<br>" +
                                 comp.VideoAdapter.AdapterRAM + " МБ.<br>" + comp.VideoAdapter.CurrentHorizontalResolution + "x" + comp.VideoAdapter.CurrentVerticalResolution + "</td></tr>");


                    listhtml.Add("<tr><td width=120 align=right valign=top>Оперативная память</td><td bgcolor=e6e6e6>");

                    foreach (var ram in comp.RAMArray.RAM)
                    {
                        listhtml.Add(ram.BankLabel + ": ");
                        listhtml.Add(ram.DDRType + " " + ram.Manufacturer + " " + ram.PartNumber + " | ");
                        listhtml.Add(ram.Capacity + " МБ | ");
                        listhtml.Add(ram.Speed + " ГЦ");
                        listhtml.Add("<br>");
                    }

                    listhtml.Add("Всего памяти: " + comp.RAMArray.RAMCapacity + " МБ.<br>Всего слотов: " + comp.RAMArray.MemoryDevices + "</td></tr>");

                    listhtml.Add("</table>");
                    listhtml.Add("</td></tr>");

                    #endregion

                    #region DISKS

                    if (comp.Monitors != null && comp.Monitors.Count > 0)
                    {
                        listhtml.Add("<tr><td colspan=2 align=left><hr width=100%></td></tr>");
                        listhtml.Add("<tr><td valign=middle align=center width=180><img src=" + "\"../source/drive.png" + "\" width=150px height=150px></td>");
                        listhtml.Add("<td><table width=100% border=0>");
                        listhtml.Add("<tr><td width=120 align=right valign=center>Накопители</td><td bgcolor=e6e6e6>");

                        foreach (var drive in comp.Drives)
                        {
                            var driveInterface = "Unknown";

                            if (drive.BusType != 0)
                                driveInterface = Helpers.GetDiskInterface(drive.BusType);
                            else if (drive.InterfaceType != "") driveInterface = drive.InterfaceType;
                            if (driveInterface == "USB") drive.MediaType = 9999;

                            listhtml.Add(Helpers.GetDiskType(drive.MediaType) + " | " + "");
                            listhtml.Add(drive.Model + "<br>");
                            listhtml.Add("С/Н: " + drive.SerialNumber + "<br>");
                            listhtml.Add("Объём: " + drive.Size + "<br>");
                            listhtml.Add("Основной: " + (drive.IsPrimary ? "Да" : "Нет") + "<br>");
                            listhtml.Add("Интерфейс: " + driveInterface);
                            listhtml.Add("<hr>");
                        }

                        listhtml.RemoveAt(listhtml.Count - 1);
                        listhtml.Add("</table>");
                        listhtml.Add("</td></tr>");
                    }

                    #endregion

                    #region PRINTER

                    listhtml.Add("<tr><td colspan=2 align=left><hr width=100%></td></tr>");

                    listhtml.Add("<tr><td valign=middle align=center width=180><img src=" + "\"../source/printer.png" + "\" width=150px height=150px></td>");
                    listhtml.Add("<td><table width=100% border=0>");
                    listhtml.Add("<tr><td width=120 align=right valign=center>Принтеры</td><td bgcolor=e6e6e6>");

                    foreach (var item in comp.Printers)
                    {
                        listhtml.Add(item.Name + "<br>");
                        listhtml.Add("ИД: " + item.DeviceID + "<br>");
                        listhtml.Add("Драйвер: " + item.DriverName + "<br>");
                        listhtml.Add("Общий: " + (item.Shared ? "Да" : "Нет") + " | ");
                        listhtml.Add("Сетевой: " + (item.Network ? "Да" : "Нет") + " | ");
                        listhtml.Add("Порт: " + item.PortName);
                        if (item.ErrorInformation != null && item.ErrorInformation != "") listhtml.Add("<br><div style=\"color: red\">Ошибка: " + item.ErrorInformation + "</div>");
                        listhtml.Add("<hr>");
                    }

                    listhtml.RemoveAt(listhtml.Count - 1);
                    listhtml.Add("</table>");
                    listhtml.Add("</td></tr>");


                    listhtml.Add("<tr><td align=left colspan=2>");
                    listhtml.Add(
                        "<table width=100%> <tr><td width=20 align=left><a href=\"javascript:CloseSubDiv(300)\"><img src=\"../source/close_all.png\" title=\"Свернуть все\" width=\"16px\"></a></td>");
                    listhtml.Add(
                        "<td><hr  width=100%></td><td width=20 align=right><a href=\"javascript:CloseSubDiv(300)\"><img src=\"../source/close_all.png\" title=\"Свернуть все\" width=\"16px\"></a></td>");
                    listhtml.Add("</tr></table>");
                    listhtml.Add("</td></tr>");
                    listhtml.Add("");
                    listhtml.Add("");

                    listhtml.Add("<tr><td colspan=2>");

                    #endregion

                    #region DEVICES

                    listhtml.Add("<a href=\"javascript:shiftSubDiv(1)\" title=\"Развернуть/Свернуть Устройства\">");
                    listhtml.Add("<img id=image1 src=\"../source/plus.png\" title=\"\"> Устройства</a>");
                    listhtml.Add("<div id=\"subDiv1\" style=\"display:none\"><table width=100%>");

                    foreach (var device in comp.Devices)
                        try
                        {
                            listhtml.Add("<tr><td width=20></td><td  bgcolor=" + getcolor() + ">  <img src=\"../source/hard.png\" border=\"0\" width=15px height=15px>" + device.Caption +
                                         " | Описание: ");
                            listhtml.Add(device.Description + " | Статус: ");
                            listhtml.Add(device.Status);
                            listhtml.Add("</td></tr>");
                        }
                        catch
                        {
                            listhtml.Add("<tr><td width=20></td><td  bgcolor=" + getcolor() + ">  <img src=\"../source/hard.png\" border=\"0\" width=15px height=15px>Нет сведений");
                            listhtml.Add("</td></tr>");
                        }

                    listhtml.Add("</table></div><br>");

                    #endregion

                    #region SOFT

                    listhtml.Add("<a href=\"javascript:shiftSubDiv(2)\" title=\"Развернуть/Свернуть Софт\">");
                    listhtml.Add("<img id=image2 src=\"../source/plus.png\" title=\"\"> Софт</a>");
                    listhtml.Add("<div id=\"subDiv2\" style=\"display:none\"><table width=100%>");

                    foreach (var app in comp.Applications)
                        listhtml.Add("<tr><td width=20></td><td  bgcolor=" + getcolor() + "><img src=\"../source/soft.png\" border=\"0\" width=15px height=15px>" + app.DisplayName + " | " +
                                     app.DisplayVersion + " | " + app.Publisher + "</td></tr>");
                    listhtml.Add("</table></div><br>");

                    #endregion

                    #region REG

                    listhtml.Add("<a href=\"javascript:shiftSubDiv(3)\" title=\"Развернуть/Свернуть Автозапуск\">");
                    listhtml.Add("<img id=image3 src=\"../source/plus.png\" title=\"\"> Автозапуск</a>");
                    listhtml.Add("<div id=\"subDiv3\" style=\"display:none\"><table width=100%>");

                    foreach (var StUp in comp.startUpKeys)
                    {
                        listhtml.Add("<tr><td width=20></td><td  bgcolor=" + getcolor() + ">  <img src=\"../source/reg.png\" border=\"0\" width=15px height=15px>" + StUp.Key + " | Путь: ");
                        listhtml.Add(StUp.Value);
                        listhtml.Add("</td></tr>");
                    }

                    listhtml.Add("</table></div><br>");

                    #endregion

                    #region SMART

                    listhtml.Add("<a href=\"javascript:shiftSubDiv(4)\" title=\"Развернуть/Свернуть Автозапуск\">");
                    listhtml.Add("<img id=image4 src=\"../source/plus.png\" title=\"\"> SMART</a>");
                    listhtml.Add("<div id=\"subDiv4\" style=\"display:none\">");


                    foreach (var drive in comp.Drives)
                        if (drive.SmartAttributes.Any(t => t.Value.HasData))
                        {
                            drive.SmartAttributes = CheckSmart(drive.SmartAttributes);

                            listhtml.Add("<p>" + drive.Model + "</p>");

                            listhtml.Add("<table>");
                            listhtml.Add("<tr>");
                            listhtml.Add("<th>Hex</th>");
                            listhtml.Add("<th>Имя атрибута</th>");
                            listhtml.Add("<th>Текущее (у.е)</th>");
                            listhtml.Add("<th>Худшее (у.е)</th>");
                            listhtml.Add("<th>Плохо если (у.е)</th>");
                            listhtml.Add("<th>RAW Значение</th>");
                            listhtml.Add("</tr>");

                            foreach (var SMART in drive.SmartAttributes)
                                if (SMART.Value.HasData)
                                {
                                    var color = getcolor();
                                    if (SMART.Value.Status == 2) color = "#FF2F2F";
                                    if (SMART.Value.Status == 1) color = "#FFFF00";

                                    listhtml.Add("<tr bgcolor=" + color + "><td>" + SMART.Key + "</td>");
                                    listhtml.Add("<td>" + SMART.Value.Attribute + "</td>");
                                    listhtml.Add("<td>" + SMART.Value.Current + "</td>");
                                    listhtml.Add("<td>" + SMART.Value.Worst + "</td>");
                                    listhtml.Add("<td>" + SMART.Value.Threshold + "</td>");
                                    listhtml.Add("<td>" + SMART.Value.Data + "</td>");
                                    listhtml.Add("</tr>");
                                }

                            listhtml.Add("</table>");
                        }

                    listhtml.Add("</div><br>");

                    #endregion

                    #region SOFT

                    listhtml.Add("<a href=\"javascript:shiftSubDiv(5)\" title=\"Развернуть/Свернуть Службы\">");
                    listhtml.Add("<img id=image5 src=\"../source/plus.png\" title=\"\"> Службы</a>");
                    listhtml.Add("<div id=\"subDiv5\" style=\"display:none\"><table width=100%>");

                    foreach (var service in comp.Services)
                    {
                        listhtml.Add("<tr><td width=20></td><td  bgcolor=" + getcolor() + ">  <img src=\"../source/service.png\" border=\"0\" width=15px height=15px>" + service.DisplayName);
                        listhtml.Add(" | " + service.ServiceName);
                        listhtml.Add(" | Статус: " + service.Status);
                        listhtml.Add("</td></tr>");
                    }

                    listhtml.Add("");
                    listhtml.Add("");
                    listhtml.Add("</table>");
                    listhtml.Add("</td></tr>");
                    listhtml.Add("<tr><td align=left colspan=2>");
                    listhtml.Add(
                        "<table width=100%> <tr><td width=20 align=left><a href=\"javascript:CloseSubDiv(300)\"><img src=\"../source/close_all.png\" title=\"Свернуть все\" width=\"16px\"></a></td>");
                    listhtml.Add(
                        "<td><hr  width=100%></td><td width=20 align=right><a href=\"javascript:CloseSubDiv(300)\"><img src=\"../source/close_all.png\" title=\"Свернуть все\" width=\"16px\"></a></td>");
                    listhtml.Add("</tr></table>");
                    listhtml.Add("</td></tr>");
                    listhtml.Add("");
                    listhtml.Add("");

                    listhtml.Add("</table></div><br>");

                    #endregion

                    File.WriteAllLines(Path.Combine(path, @"html\" + comp.MachineName + ".htm"), listhtml);

                    GenList.Add(comp);
                }
                catch (Exception ex)
                {
                    ExToFile(ex);
                }

                BeginInvoke((Action)(() => { progressBar1.Value++; }));
            }

            var Svod = new List<string>();


            Svod.Add(File.ReadAllText(Path.Combine(path, @"source\SvodAllHead.htm"), Encoding.UTF8));

            Svod.Add("<tr><td colspan=5 align=right><b><i>Всего : " + GenList.Count + "</i></b></td></tr>");
            Svod.Add("<tr><td align=right colspan=2><div id=\"subDiv1\" style=\"display:block\"><table width=98%>");
            Svod.Add("<tr align=center bgcolor=#c8c8c8><td width=20%><b>Имя компьютера</b></td><td width=12%><b>Пользователь</b></td><td width=20%><b>Версия Windows</b></td><td width=15%><b>Описание</b></td><td width=6%><b>RAM</b></td><td width=7%><b>SMART</b></td><td width=10%><b>Оценка</b></td><td width=10%><b>Время отчета</b></td></tr>");
            Svod.Add("");

            foreach (var comp in GenList)
            {
                var scolor = getcolor();
                var smartcolor = "";

                var SMARTStatus = "UNKN";

                SMARTStatus = SMARTVerdict(comp.GetArrtibPrimaryDriveOrEmpty());

                if (SMARTStatus == "BAD") smartcolor = "#FF2F2F";
                if (SMARTStatus == "WARN") smartcolor = "#FFFF00";

                var LVLtoStr = "UNKNOWN";

                if (comp.Marks != null && !comp.IsVirtualMachine)
                    LVLtoStr = GeTotaltMarkFromLVL2(comp.Marks.TotalMark);
                else if (comp.IsVirtualMachine) LVLtoStr = "VIRTUAL";

                Svod.Add("<tr bgcolor=" + scolor + ">" + "<td><a href=\"" + comp.MachineName + ".htm\" id=\"" + comp.MachineName + "\" class=\"R_menu\">" + comp.MachineName + "</a></td>" + "<td>" +
                         comp.UserName + "</td>" + "<td>" + comp.WinVersion + "</td>" + "<td>" + comp.Description + "</td>" + "<td><center>" + comp.RAMArray.RAMCapacity + "</center></td>" +
                         "<td bgcolor=\"" + smartcolor + "\"><center>" + SMARTStatus + "</center></td>" + "<td bgcolor=\"" + getcolorOfMark(LVLtoStr) + "\"><center>" + LVLtoStr + "</center></td>" +
                         "<td><center>" + comp.GenDate.ToString("dd.MM.yyyy HH:mm:ss") + "</center></td></tr>");
            }

            Svod.Add("");
            Svod.Add("</table></div></td></tr>");
            Svod.Add("</table>");
            Svod.Add("<a name=\"down\">&nbsp;</a>");
            Svod.Add("</body></html>");

            File.WriteAllLines(Path.Combine(path, @"html\SvodAll.htm"), Svod, Encoding.UTF8);


            var OSSvod = GenList.GroupBy(t => t.WinVersion);
            GroupGen(Path.Combine(path, @"html\SvodOS.htm"), "Группировка по операционным системам", OSSvod, "../source/win.png");

            var CPUSvod = GenList.GroupBy(t => t.CPUs[0].Name);
            GroupGen(Path.Combine(path, @"html\SvodCPU.htm"), "Группировка по процессорам", CPUSvod, "../source/cpu.png");

            var MarkMotherboardSvod = GenList.GroupBy(t => t.Motherboard.Manufacturer + " " + t.Motherboard.Product);
            GroupGen(Path.Combine(path, @"html\SvodMotherboard.htm"), "Группировка по материнским платам", MarkMotherboardSvod, "../source/mother.png");

            var RamModelSvod = GenList.SelectMany(t => t.RAMArray.RAM, (comp, t) => new KeyValuePair<string, General>(t.Manufacturer + " " + t.PartNumber, comp));
            GroupGen(Path.Combine(path, @"html\SvodRAMmodel.htm"), "Группировка по модели ОЗУ", RamModelSvod, "../source/ram.png");

            var RAMvolSvod = GenList.GroupBy(t => t.RAMArray.RAMCapacity.ToString());
            GroupGen(Path.Combine(path, @"html\SvodRAMvol.htm"), "Группировка по объёму ОЗУ", RAMvolSvod, "../source/ram.png");

            var RAMtypeSvod = GenList.GroupBy(t => t.RAMArray.RAM[0].DDRType);
            GroupGen(Path.Combine(path, @"html\SvodRAMtype.htm"), "Группировка по типу ОЗУ", RAMtypeSvod, "../source/ram.png");

            var RAMhzSvod = GenList.GroupBy(t => (t.RAMArray.RAM.Sum(s => s.Speed) / t.RAMArray.RAM.Count).ToString());
            GroupGen(Path.Combine(path, @"html\SvodRAMhz.htm"), "Группировка по средней частоте ОЗУ", RAMhzSvod, "../source/ram.png");

            var AppSvod = GenList.SelectMany(t => t.Applications, (comp, t) => new KeyValuePair<string, General>(t.DisplayName, comp));
            GroupGen(Path.Combine(path, @"html\SvodAPPS.htm"), "Группировка по приложениям", AppSvod, "../source/soft.png");

            var MarkSvod = GenList.GroupBy(t => GeTotaltMarkFromLVL2(t.Marks.TotalMark));
            GroupGen(Path.Combine(path, @"html\SvodMark.htm"), "Группировка по оценочным данным", MarkSvod, "../source/mark.png");

            var DriveModelSvod = GenList.SelectMany(t => t.Drives, (comp, t) => new KeyValuePair<string, General>(Helpers.GetDiskType(t.MediaType) + " | " + t.Model, comp));
            GroupGen(Path.Combine(path, @"html\SvodModelDrive.htm"), "Группировка по медели накопителя", DriveModelSvod, "../source/drive.png");

            var DriveTypeSvod = GenList.SelectMany(t => t.Drives, (comp, t) => new KeyValuePair<string, General>(Helpers.GetDiskType(t.MediaType), comp));
            GroupGen(Path.Combine(path, @"html\SvodTypeDrive.htm"), "Группировка по типу накопителя", DriveTypeSvod, "../source/drive.png");

            var DriveSizeSvod = GenList.SelectMany(t => t.Drives, (comp, t) => new KeyValuePair<string, General>(t.Size.ToString(), comp));
            GroupGen(Path.Combine(path, @"html\SvodSizeDrive.htm"), "Группировка по объему накопителя", DriveSizeSvod, "../source/drive.png");

            var PrinterSvod = GenList.SelectMany(t => t.Printers, (comp, t) => new KeyValuePair<string, General>(t.Name, comp));
            GroupGen(Path.Combine(path, @"html\SvodPrinter.htm"), "Группировка по принтера", PrinterSvod, "../source/printer.png");

            var SMARTSvod = GenList.GroupBy(t => SMARTVerdict(t.GetArrtibPrimaryDriveOrEmpty()));
            GroupGen(Path.Combine(path, @"html\SvodSMART.htm"), "Группировка по SMART", SMARTSvod, "../source/SMART.png");

            Thread.Sleep(1000);

            BeginInvoke((Action)(() =>
            {
                if (Program.path != "")
                    Close();
                else
                    labelStatus.Text = "Готово!";
            }));
        }

        private void GroupGen(string NameGroup, string TextGroup, IEnumerable<IGrouping<string, General>> GenList, string iconpath)
        {
            BeginInvoke((Action)(() => { labelStatus.Text = "Группировка: " + TextGroup; }));

            var GenList_sub = GenList.ToDictionary(t => t.Key, g => g.ToList());
            GroupGen_sub(NameGroup, GenList_sub, iconpath);
        }

        private void GroupGen(string NameGroup, string TextGroup, IEnumerable<KeyValuePair<string, General>> GenList, string iconpath)
        {
            BeginInvoke((Action)(() => { labelStatus.Text = "Группировка: " + TextGroup; }));

            var GenList_sub = GenList.GroupBy(t => t.Key).ToDictionary(g => g.Key, g => g.Select(v => v.Value).ToList());
            GroupGen_sub(NameGroup, GenList_sub, iconpath);
        }

        private void GroupGen_sub(string NameGroup, Dictionary<string, List<General>> GenList, string iconpath)
        {
            var Svod = new List<string>();

            if (GenList.All(t => Regex.IsMatch(t.Key, @"^\d*$")))
                GenList = GenList.OrderBy(t => long.Parse(t.Key)).ToDictionary(k => k.Key, v => v.Value);
            else
                GenList = GenList.OrderBy(t => t.Key).ToDictionary(k => k.Key, v => v.Value);

            //HEAD
            Svod.Add("<html>");
            Svod.Add("<head>");
            Svod.Add("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=Utf-8\">");
            Svod.Add("<title>Учет компьютеров</title>");
            Svod.Add("<link rel=\"stylesheet\" href=\"../source/svod.css\">");
            Svod.Add("<script src=\"../source/svod.js\"></script>");
            Svod.Add("</head>");
            Svod.Add("<body LEFTMARGIN=\"2\" RIGHTMARGIN=\"0\" MARGINWIDTH=\"0\" MARGINHEIGHT=\"0\" topMargin=0 bgColor=\"#ffffff\">");
            Svod.Add("<p>");
            Svod.Add("<table class=\"L_menu\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\"  width=100%>");
            //HEAD

            Svod.Add("<p>\r\n<table class=\"L_menu\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" width=\"100%\">\r\n<tbody>");

            var subdiv = 1;

            foreach (var item in GenList)
            {
                Svod.Add("<tr bgcolor=\"#e6e6e6\" width=\"100%\">\r\n<td width=\"90%\">\r\n<a href=\"javascript:shiftSubDiv(" + subdiv + ")\" title=\"Развернуть/Свернуть\">\r\n" + "<img id=\"image" +
                         subdiv + "\" src=\"../source/plus.png\" title=\"\">\r\n<img src=\"" + iconpath + "\" title=\"\" width=\"32px\" height=\"32px\">\r\n<b>" + item.Key + "</b>\r\n</a>\r\n<i>" +
                         item.Value.Count() + "</i>\r\n</td>\r\n</tr>");

                Svod.Add("<tr>\r\n<td align=\"right\" colspan=\"2\"><div id=\"subDiv" + subdiv + "\" style=\"display:none\"><table width=\"98%\">\r\n<tbody>");
                Svod.Add(
                    "<tr align=center bgcolor=#c8c8c8><td width=20%><b>Имя компьютера</b></td><td width=12%><b>Пользователь</b></td><td width=20%><b>Версия Windows</b></td><td width=15%><b>Описание</b></td><td width=6%><b>RAM</b></td><td width=7%><b>SMART</b></td><td width=10%><b>Оценка</b></td><td width=10%><b>Время отчета</b></td></tr>");

                foreach (var comp in item.Value)
                {
                    var scolor = getcolor();
                    var smartcolor = "";

                    var SMARTStatus = "UNKN";

                    SMARTStatus = SMARTVerdict(comp.GetArrtibPrimaryDriveOrEmpty());

                    if (SMARTStatus == "BAD") smartcolor = "#FF2F2F";
                    if (SMARTStatus == "WARN") smartcolor = "#FFFF00";

                    var LVLtoStr = "UNKNOWN";

                    if (comp.Marks != null && !comp.IsVirtualMachine)
                        LVLtoStr = GeTotaltMarkFromLVL2(comp.Marks.TotalMark);
                    else if (comp.IsVirtualMachine) LVLtoStr = "VIRTUAL";

                    Svod.Add("<tr bgcolor=" + scolor + ">" + "<td><a href=\"" + comp.MachineName + ".htm\" id=\"" + comp.MachineName + "\" class=\"R_menu\">" + comp.MachineName + "</a></td>" +
                             "<td>" + comp.UserName + "</td>" + "<td>" + comp.WinVersion + "</td>" + "<td>" + comp.Description + "</td>" + "<td><center>" + comp.RAMArray.RAMCapacity +
                             "</center></td>" + "<td bgcolor=\"" + smartcolor + "\"><center>" + SMARTStatus + "</center></td>" + "<td bgcolor=\"" + getcolorOfMark(LVLtoStr) + "\"><center>" +
                             LVLtoStr + "</center></td>" + "<td><center>" + comp.GenDate.ToString("dd.MM.yyyy HH:mm:ss") + "</center></td></tr>");
                }

                Svod.Add("<tr><td colspan=\"9\" align=\"right\"><b><i>Итого: " + item.Value.Count() + "</i></b></td></tr>\r\n\r\n");
                Svod.Add("</tbody></table></div></td></tr>");

                subdiv++;
            }

            Svod.Add("</tbody></table>");

            File.WriteAllLines(NameGroup, Svod, Encoding.UTF8);
        }

        private Dictionary<int, Smart> CheckSmart(Dictionary<int, Smart> SMART)
        {
            SMART = SMART.Where(t => t.Value.HasData).ToDictionary(k => k.Key, v => v.Value);

            foreach (var smart in SMART)
                if (!smart.Value.IsOK)
                    smart.Value.Status = 2;
                else
                    smart.Value.Status = 0;

            if (SMART.ContainsKey(5) && SMART[5].Data > 1) SMART[5].Status = 2;
            if (SMART.ContainsKey(10) && SMART[10].Data > 5) SMART[10].Status = 1;
            if (SMART.ContainsKey(11) && SMART[11].Data > 10) SMART[11].Status = 1;
            if (SMART.ContainsKey(184) && SMART[184].Data > 0) SMART[184].Status = 1;
            if (SMART.ContainsKey(187) && SMART[187].Data > 0) SMART[187].Status = 1;
            if (SMART.ContainsKey(188) && SMART[188].Data > 10) SMART[188].Status = 1;
            if (SMART.ContainsKey(196) && SMART[196].Data > 10) SMART[196].Status = 1;
            if (SMART.ContainsKey(198) && SMART[198].Data > 0) SMART[198].Status = 2;
            if (SMART.ContainsKey(199) && SMART[199].Data > 10) SMART[199].Status = 1;
            if (SMART.ContainsKey(200) && SMART[200].Data > 0) SMART[200].Status = 1;
            if (SMART.ContainsKey(202) && SMART[202].Data > 5) SMART[202].Status = 1;

            return SMART;
        }

        private string SMARTVerdict(Dictionary<int, Smart> SMART)
        {
            if (SMART.All(t => !t.Value.HasData)) return "UNKN";
            if (SMART.Any(t => t.Value.Status == 2)) return "BAD";
            if (SMART.Any(t => t.Value.Status == 1)) return "WARN";

            return "OK";
        }

        private static double[] levelP2(double CPUScoreF, double DiskScoreF, float GraphicsScoreF, double MemoryScoreF, float TotalmemF, float TotalDrivesSizeF)
        {
            CPUScoreF = CPUScoreF * 12 / 30;
            DiskScoreF = DiskScoreF * 12 / 50;
            MemoryScoreF = MemoryScoreF * 12 / 20;

            var memoryStand = 8192f;
            var disksizeStand = 220f;

            var GraphicsScore = GraphicsScoreF + 0.1f;

            var TotalmemMark = 100f / (memoryStand / TotalmemF) / 10f;
            float TotalDrivesMark = 0;
            var TotalDrivesSize = TotalDrivesSizeF;

            TotalDrivesMark = 100f / (disksizeStand / TotalDrivesSize) / 10f;

            if (TotalDrivesMark > 10) TotalDrivesMark = 10;
            if (TotalmemMark > 10) TotalmemMark = 10;

            var obshLVLMass = new double[7];
            obshLVLMass[0] = (CPUScoreF + DiskScoreF + MemoryScoreF) * 12 / 36;
            if (obshLVLMass[0] > 12) obshLVLMass[0] = 12;
            obshLVLMass[1] = CPUScoreF;
            obshLVLMass[2] = DiskScoreF;
            obshLVLMass[3] = GraphicsScore;
            obshLVLMass[4] = MemoryScoreF;
            obshLVLMass[5] = TotalmemMark;
            obshLVLMass[6] = TotalDrivesMark;

            return obshLVLMass;
        }

        private static string GeTotaltMarkFromLVL2(double mark)
        {
            if (mark < 3) return "1";
            if (mark >= 3 && mark < 4) return "2";
            if (mark >= 4 && mark < 5) return "3";
            if (mark >= 5 && mark < 6) return "4";
            if (mark >= 6 && mark < 7) return "5";
            if (mark >= 7 && mark < 8) return "6";
            if (mark >= 8 && mark < 9) return "7";
            if (mark >= 9 && mark < 10) return "8";
            if (mark >= 10 && mark < 11) return "9";
            if (mark >= 11) return "10";

            return "1";
        }

        private static string getcolorOfLVL2(double LVL, int ind)
        {
            if (ind == 3)
                LVL = LVL * 12 / 10;
            else if (ind == 5)
                LVL = LVL * 12 / 10;
            else if (ind == 6) LVL = LVL * 12 / 10;

            LVL = (float)Math.Round(LVL);

            LVL--;
            if (LVL == 0) return "#ff0000";
            if (LVL == 1) return "#ff2600";
            if (LVL == 2) return "#ff5500";
            if (LVL == 3) return "#ff7300";
            if (LVL == 4) return "#ff9100";
            if (LVL == 5) return "#ffae00";
            if (LVL == 6) return "#ffcc00";
            if (LVL == 7) return "#ffe100";
            if (LVL == 8) return "#f7ff00";
            if (LVL == 9) return "#bbff00";
            if (LVL == 10) return "#6fff00";
            if (LVL >= 11) return "#00ff09";
            return "#ff0000";
        }

        private static string getcolorOfMark(string LVL)
        {
            if (LVL == "1") return "#ff5500";
            if (LVL == "2") return "#ff7300";
            if (LVL == "3") return "#ff9100";
            if (LVL == "4") return "#ffae00";
            if (LVL == "5") return "#ffcc00";
            if (LVL == "6") return "#ffe100";
            if (LVL == "7") return "#f7ff00";
            if (LVL == "8") return "#bbff00";
            if (LVL == "9") return "#6fff00";
            if (LVL == "10") return "#00ff09";
            return "#ffffff";
        }

        private static double[] ReturnListMarks(double[] LVL)
        {
            return new[]
            {
                LVL[0], LVL[1], LVL[2],
                LVL[3] * 10 / 10, LVL[4], LVL[5] * 10 / 10, LVL[5] * 10 / 10
            };
        }

        private static string getcolor()
        {
            if (colch)
            {
                colch = false;
                return "E6E6FA";
            }

            colch = true;
            return "F0FFFF";
        }
    }
}