using GenerateInformation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace System_Information
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            string ftpserver = "";
            string ftpuser = "";
            string ftppass = "";

            string savepath = "";

            foreach (string arg in args)
            {
                if (arg.ToLower().StartsWith("ftp://"))
                {
                    ftpserver = arg.Split('@')[1];
                    ftpuser = arg.Replace("ftp://", "").Split('@')[0].Split(':')[0];
                    ftppass = arg.Replace("ftp://", "").Split('@')[0].Split(':')[1];
                }
                else
                {
                    savepath = Path.Combine(args[0], Environment.MachineName + ".json");
                }
            }

            if (savepath == "") { savepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.MachineName + ".json"); }

            try
            {
                General Information = GenerateInformation.GenerateInformation.Generation();

                if (ftpserver == "") { File.WriteAllText(savepath, JsonConvert.SerializeObject(Information)); }
                else { FTPUpload(JsonConvert.SerializeObject(Information), ftpserver, ftpuser, ftppass); }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(savepath, Environment.MachineName + "_error.txt"), ex.ToString());
            }
        }

        static void FTPUpload(string StringtoPut, string ftpserver, string ftpuser, string ftppass)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpserver + "/" + Environment.MachineName + ".json");
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(ftpuser, ftppass);

            byte[] bytes = Encoding.UTF8.GetBytes(StringtoPut);

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}