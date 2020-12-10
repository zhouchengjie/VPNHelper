using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace VPNHelper.Utility
{
    public class HttpHelper
    {

        static HttpHelper()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                            new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);
        }
        public static string DELETE(string url, string data, string charsSet = "utf-8")
        {
            return Response(url, data, null, "DELETE", charsSet);
        }

        public static string Get(string url, string data, HttpCookieCollection cookies, string charsSet = "utf-8")
        {
            return Response(url, data, cookies, "GET", charsSet);
        }
        public static string Get(string url, string data, string charsSet = "utf-8")
        {
            return Get(url, data, null, charsSet);
        }
        public static string Post(string url, string data, string charsSet = "utf-8", string conentType = "", NameValueCollection header = null)
        {
            return Post(url, data, null, charsSet, conentType, header);
        }
        public static string Post(string url, string data, HttpCookieCollection cookies, string charsSet = "utf-8", string contentType = "", NameValueCollection header = null)
        {
            return Response(url, data, cookies, "POST", charsSet, contentType, header);
        }
        /// <summary>
        /// 向指定地址发送POST请求
        /// </summary>
        /// <param name="getUrl">指定的网页地址</param>
        /// <param name="postData">POST的数据（格式为：p1=v1&p1=v2）</param>
        /// <param name="chars_set">可采用如UTF-8,GB2312,GBK等</param>
        /// <returns>页面返回内容</returns>
        public static string Response(string url, string postData, HttpCookieCollection cookies, string method = "POST", string charsSet = "utf-8", string contentType = "application/x-www-form-urlencoded", NameValueCollection header = null)
        {

            Encoding encoding = Encoding.GetEncoding(charsSet);
            HttpWebRequest Request;
            if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
            {
                //是https请求的时候
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);
                Request = WebRequest.Create(url) as HttpWebRequest;
                Request.ProtocolVersion = HttpVersion.Version11;
                //LogHelper.LogInfo("HttpHelper Response url=" + url);
            }
            else
                Request = (HttpWebRequest)WebRequest.Create(url);
            //设置CookieContainer，商城页面使用了cookie，此处不设置CookieContainer会请求失败
            Request.CookieContainer = new CookieContainer();
            Request.Method = method;
            if (!string.IsNullOrEmpty(contentType))
            {
                Request.ContentType = contentType;//"application/x-www-form-urlencoded";
            }
            else Request.ContentType = "application/x-www-form-urlencoded";
            Request.AllowAutoRedirect = true;
            if (header != null)
            {//增加请求头
                Request.Headers.Add(header);
            }
            byte[] postdata = encoding.GetBytes(postData);
            if (!method.Equals("get", StringComparison.CurrentCultureIgnoreCase) || !string.IsNullOrEmpty(postData))
            {
                using (Stream newStream = Request.GetRequestStream())
                {
                    newStream.Write(postdata, 0, postdata.Length);
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)Request.GetResponse())
            {

                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, encoding, true))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

        }

        public static bool RemoteCertificateValidationCallback(
            Object sender,
            X509Certificate certificate,
            X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static byte[] Post4ReturnByteArray(string url, string para)
        {
            using (WebClient wc = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                string[] paraArray = para.Split('&');
                foreach (string paraTemp in paraArray)
                {
                    string[] paraTempArray = paraTemp.Split('=');
                    if (paraTempArray.Length >= 2)
                    {
                        string name = paraTempArray[0];
                        string value = paraTemp.Substring(name.Length + 1);
                        data[name] = value;
                    }
                }
                byte[] bs = wc.UploadValues(url, "POST", data);
                return bs;
            }
        }


        /// <summary>
        ///获取返回到浏览器的URL地址
        /// </summary>
        /// <param name="Url">地址</param>
        /// <param name="postDataStr">参数</param>
        /// <returns></returns>
        public static string GetBrowserUrl(string Url, string postDataStr)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
                Stream myRequestStream = request.GetRequestStream();
                StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
                myStreamWriter.Write(postDataStr);
                myStreamWriter.Close();
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream myResponseStream = response.GetResponseStream())
                    {
                        using (StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8")))
                        {
                            string retString = myStreamReader.ReadToEnd();
                            string ret = response.ResponseUri.ToString();
                            myResponseStream.Close();
                            myStreamReader.Close();
                            return ret + "|" + retString;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }

        }


        //public static bool IsWeChat(HttpContextBase context)
        //{
        //    return context != null && context.Request.UserAgent.Contains("MicroMessenger");
        //    //HttpContext.Current.Request.Browser.IsMobileDevice && HttpContext.Current.Request.UserAgent.Contains("MicroMessenger");
        //}

        //public static bool isMobile(HttpContextBase context)
        //{
        //    //return true;
        //    //LogHelper.LogError(HttpContext.Current.Request.UserAgent.ToString() + HttpContext.Current.Request.Browser.IsMobileDevice+HttpContext.Current.Request.Browser.Platform +HttpContext.Current.Request.Browser.MobileDeviceModel+ "");
        //    return context != null && (context.Request.UserAgent.ToUpper().Contains("ANDROID") || context.Request.UserAgent.ToUpper().Contains("IPHONE"));
        //}

        public static bool DownloadFile(string URL, string filename)
        {
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);

                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
                myrp.Close();
                Myrq.Abort();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Debug("DownloadFile",ex.ToString());
                return false;
            }
        }

        public static Stream DownloadFileGetStream(string URL, string filename)
        {

            System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
            System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
            System.IO.Stream st = myrp.GetResponseStream();
            //System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
            //OSSService
            BufferedStream bufferstream = new BufferedStream(st);
            return bufferstream;
        }

        /// <summary>
        /// PostStream
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="fileName"></param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string PostFile(string url, string fileName, string filePath)
        {
            // 边界符  
            var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
            var beginBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            //将项目内路径拼接上项目根目录
            string filepath = HttpRuntime.AppDomainAppPath + filePath;
            var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);

            // 最后的结束符  
            var endBoundary = Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");

            // 文件参数头  
            const string filePartHeader =
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\";filelength=\"{2}\"\r\n" +
                 "Content-Type: application/octet-stream\r\n\r\n";
            var fileHeader = string.Format(filePartHeader, "media", fileName, fileStream.Length);
            var fileHeaderBytes = Encoding.UTF8.GetBytes(fileHeader);

            // 开始拼数据  
            var memStream = new MemoryStream();
            memStream.Write(beginBoundary, 0, beginBoundary.Length);

            // 文件数据  
            memStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);
            var buffer = new byte[1024];
            int bytesRead; // =0  
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                memStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            // Key-Value数据  
            var stringKeyHeader = "\r\n--" + boundary +
                                   "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                                   "\r\n\r\n{1}\r\n";

            Dictionary<string, string> stringDict = new Dictionary<string, string>();
            stringDict.Add("len", "500");
            stringDict.Add("wid", "300");
            foreach (byte[] formitembytes in from string key in stringDict.Keys
                                             select string.Format(stringKeyHeader, key, stringDict[key])
                                                 into formitem
                                             select Encoding.UTF8.GetBytes(formitem))
            {
                memStream.Write(formitembytes, 0, formitembytes.Length);
            }

            // 写入最后的结束边界符  
            memStream.Write(endBoundary, 0, endBoundary.Length);

            //倒腾到tempBuffer?  
            memStream.Position = 0;
            var tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();

            // 创建webRequest并设置属性  
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.Timeout = 100000;
            webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webRequest.ContentLength = tempBuffer.Length;

            var requestStream = webRequest.GetRequestStream();
            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();

            var httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
            string responseContent;
            using (var httpStreamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("utf-8")))
            {
                responseContent = httpStreamReader.ReadToEnd();
            }

            httpWebResponse.Close();
            webRequest.Abort();
            return responseContent;
        }

        /// <summary>
        /// 上传永久视频素材（不行的）
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="fileName"></param>
        /// <param name="filePath">文件路径</param>
        /// <param name="title">标题</param>
        /// <param name="introduction">描述</param>
        /// <returns></returns>
        public static string PostPerpetualViodeMaterial(string url, string fileName, string filePath, string title, string introduction)
        {
            // 边界符  
            var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
            var beginBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            //将项目内路径拼接上项目根目录
            string filepath = HttpRuntime.AppDomainAppPath + filePath;
            var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);

            // 最后的结束符  
            var endBoundary = Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");

            // 文件参数头  
            const string filePartHeader =
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\";filelength=\"{2}\"\r\n" +
                 "Content-Type: application/octet-stream\r\n\r\n";
            var fileHeader = string.Format(filePartHeader, "media", fileName, fileStream.Length);
            var fileHeaderBytes = Encoding.UTF8.GetBytes(fileHeader);

            // 文件参数头  
            const string videoPartHeader =
                "Content-Disposition: form-data; name=\"{0}\"; title=\"{1}\";introduction=\"{2}\"\r\n" +
                 "Content-Type: application/octet-stream\r\n\r\n";
            var videoHeader = string.Format(videoPartHeader, "description", title, introduction);
            var videoHeaderBytes = Encoding.UTF8.GetBytes(fileHeader);

            // 开始拼数据  
            var memStream = new MemoryStream();
            memStream.Write(beginBoundary, 0, beginBoundary.Length);

            // 文件数据  
            memStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);
            var buffer = new byte[1024];
            int bytesRead; // =0  
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                memStream.Write(buffer, 0, bytesRead);
            }

            fileStream.Close();

            // Key-Value数据  
            var stringKeyHeader = "\r\n--" + boundary +
                                   "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                                   "\r\n\r\n{1}\r\n";

            Dictionary<string, string> stringDict = new Dictionary<string, string>();
            stringDict.Add("len", "500");
            stringDict.Add("wid", "300");
            foreach (byte[] formitembytes in from string key in stringDict.Keys
                                             select string.Format(stringKeyHeader, key, stringDict[key])
                                                 into formitem
                                             select Encoding.UTF8.GetBytes(formitem))
            {
                memStream.Write(formitembytes, 0, formitembytes.Length);
            }
            // 写入最后的结束边界符  
            memStream.Write(endBoundary, 0, endBoundary.Length);

            //倒腾到tempBuffer?  
            memStream.Position = 0;
            var tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Write(videoHeaderBytes, 0, videoHeaderBytes.Length);
            memStream.Close();

            // 创建webRequest并设置属性  
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.Timeout = 100000;
            webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webRequest.ContentLength = tempBuffer.Length;

            var requestStream = webRequest.GetRequestStream();
            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();

            var httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
            string responseContent;
            using (var httpStreamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("utf-8")))
            {
                responseContent = httpStreamReader.ReadToEnd();
            }

            httpWebResponse.Close();
            webRequest.Abort();
            return responseContent;
        }
    }
}
