using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace System_Information
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var savepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.MachineName + ".json");

            try
            {
                var ftpserver = "";
                var ftpuser = "";
                var ftppass = "";

                foreach (var arg in args)
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

                var Information = GenerateInformation.GenerateInformation.Generation();

                if (ftpserver == "")
                    File.WriteAllText(savepath, JsonConvert.SerializeObject(Information));
                else
                    FTPUpload(JsonConvert.SerializeObject(Information), ftpserver, ftpuser, ftppass);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(savepath, Environment.MachineName + "_error.txt"), ex.ToString());
            }
        }

        private static void FTPUpload(string StringtoPut, string ftpserver, string ftpuser, string ftppass)
        {
            var request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpserver + "/" + Environment.MachineName + ".json");
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(ftpuser, ftppass);

            var bytes = Encoding.UTF8.GetBytes(StringtoPut);

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}