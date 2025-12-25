using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Web;

namespace Sale_Saas.Application.Utilities
{
    public class WorkingOnFile
    {
        public static string TransferFileToFTPUrl(string FtpUrl, string fileName, byte[] data, string userName, string password, string UploadDirectory)
        {            
            String uploadUrl = "ftp://" + String.Format("{0}{1}/{2}", FtpUrl, UploadDirectory, fileName);
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(uploadUrl);
            req.Proxy = null;
            req.Method = WebRequestMethods.Ftp.UploadFile;
            req.Credentials = new NetworkCredential(userName, password);
            req.UseBinary = true;
            req.UsePassive = true;
            req.ContentLength = data.Length;
            Stream stream = req.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            FtpWebResponse res = (FtpWebResponse)req.GetResponse();
            return res.StatusDescription;
        }
        public static string TransferFileToFTPUrl(string FtpUrl, string fileName, string userName, string password, string UploadDirectory)
        {
            string PureFileName = new FileInfo(fileName).Name;
            String uploadUrl = "ftp://" + String.Format("{0}{1}/{2}", FtpUrl, UploadDirectory, PureFileName);
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(uploadUrl);
            req.Proxy = null;
            req.Method = WebRequestMethods.Ftp.UploadFile;
            req.Credentials = new NetworkCredential(userName, password);
            req.UseBinary = true;
            req.UsePassive = true;
            byte[] data = File.ReadAllBytes(fileName);
            req.ContentLength = data.Length;
            Stream stream = req.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            FtpWebResponse res = (FtpWebResponse)req.GetResponse();
            return res.StatusDescription;
        }

        public static string DeleteFileByFTPUrl(string FtpUrl, string fileName, string userName, string password, string UploadDirectory)
        {
            string PureFileName = new FileInfo(fileName).Name;
            String fileUrl = "ftp://" + String.Format("{0}{1}/{2}", FtpUrl, UploadDirectory, PureFileName);
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(fileUrl);
            req.Proxy = null;
            req.Method = WebRequestMethods.Ftp.DeleteFile;
            req.Credentials = new NetworkCredential(userName, password);

            FtpWebResponse res = (FtpWebResponse)req.GetResponse();
            string result = res.StatusDescription;
            res.Close();

            return result;
        }

        public static string DownloadFileFTP(string FtpUrl, string fileName, string userName, string password, string UploadDirectory, string savedFile)
        {
            string result = String.Empty;
            string PureFileName = new FileInfo(fileName).Name;
            String uploadUrl = "ftp://" + String.Format("{0}{1}/{2}", FtpUrl, UploadDirectory, PureFileName);

            try
            {
                using (WebClient request = new WebClient())
                {
                    request.Credentials = new NetworkCredential(userName, password);
                    byte[] fileData = request.DownloadData(uploadUrl);

                    using (FileStream file = File.Create(savedFile))
                    {
                        file.Write(fileData, 0, fileData.Length);
                        file.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }            
            
            return result;
        }

        public static byte[] DownloadFileFTPToArray(string FtpUrl, string fileName, string userName, string password, string UploadDirectory)
        {
            byte[] result;
            string PureFileName = new FileInfo(fileName).Name;
            String uploadUrl = "ftp://" + String.Format("{0}{1}/{2}", FtpUrl, UploadDirectory, PureFileName);

            using (WebClient request = new WebClient())
            {
                request.Credentials = new NetworkCredential(userName, password);
                result = request.DownloadData(uploadUrl);
            }

            return result;
        }

        public static List<string> ListFtpDirectory(string FtpUrl, string userName, string password, string UploadDirectory)
        {
            String fileUrl = "ftp://" + String.Format("{0}{1}", FtpUrl, UploadDirectory);
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(fileUrl);
            req.Proxy = null;
            req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            req.Credentials = new NetworkCredential(userName, password);
            

            try
            {
                FtpWebResponse res = (FtpWebResponse)req.GetResponse();
                // StreamReader srd = new StreamReader(FtpResponse.GetResponseStream(), Encoding.GetEncoding("GB2312"));
                StreamReader srd = new StreamReader(res.GetResponseStream(), Encoding.Default);
                string ResponseBackStr = srd.ReadToEnd();
                srd.Close();
                res.Close();


                string[] ListDetails = ResponseBackStr.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                List<string> RtnList = new List<string>();
                foreach (string ListDetail in ListDetails)
                {
                    if (ListDetail.StartsWith("d") && (!ListDetail.EndsWith(".")))
                    {
                        string FtpDirName = ListDetail.Substring(ListDetail.IndexOf(':') + 3).TrimStart();
                        RtnList.Add(FtpDirName + "|D");
                    }
                    else if (ListDetail.StartsWith("-"))
                    {
                        string FtpDirName = ListDetail.Substring(ListDetail.IndexOf(':') + 3).TrimStart();
                        RtnList.Add(FtpDirName + "|F");
                    }
                }


                return RtnList;
            }
            catch (WebException e)
            {
                FtpWebResponse FtpResponse = (FtpWebResponse)e.Response;
                FtpResponse.Close();


                return new List<string>();
            }
        }

        public static byte[] Download(string filename)
        {
            return System.IO.File.ReadAllBytes(filename);
        }
    }
}
