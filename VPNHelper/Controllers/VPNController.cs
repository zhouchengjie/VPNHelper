using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using VPNHelper.Utility;

namespace VPNHelper.Controllers
{
    public class VPNController : ApiController
    {
        public static string lastResult = null;
        public static Nullable<DateTime> lastGetTime = null;
        private static readonly object lockObj = new object();

        /// <summary>
        /// 有时间间隔
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Get(int refresh = 0)
        {
            //string REFRESHTIME = System.Configuration.ConfigurationManager.AppSettings["REFRESHTIME"];
            //int reFreshTime = int.Parse(REFRESHTIME);
            //LogHelper.Debug("Begin", refresh.ToString());
            string result;
            if(lastGetTime == null || lastResult == null)
            {
                result = GetIP();
			}
            else
            {           
                
                    if ((lastGetTime.Value < DateTime.Now) || refresh == 10086)
                    {
                        lock (lockObj)
                        {
                            if ((lastGetTime.Value < DateTime.Now) || refresh == 10086)
                            {
                                result = GetIP();
                            }
                            else
                            {
                                result = lastResult;
                            }
                        }
                    }
                    else
                    {
                        result = lastResult;
                    }
               
            }

            HttpResponseMessage msgResult = new HttpResponseMessage { Content = new StringContent(result, Encoding.GetEncoding("UTF-8"), "application/json") };
            return msgResult;
        }


        /// <summary>
        /// 芝麻代理：
        //  不会重复的
        //  ip是去重的
        //  每天只能使用1400个的
        /// </summary>
        /// <returns></returns>
        private string GetIP()
        {
            string ip = "";
            int code = -1;
            do
            {
				try
				{
                    string REFRESHTIME = System.Configuration.ConfigurationManager.AppSettings["REFRESHTIME"];
                    int reFreshTime = int.Parse(REFRESHTIME);
                    if (DateTime.Now.Hour >= 1 && DateTime.Now.Hour <= 6) 
                    {
                        reFreshTime = reFreshTime * 2;
                    }
                    string GETIPURL = System.Configuration.ConfigurationManager.AppSettings["GETIPURL"];
					string strResult = HttpHelper.Get(GETIPURL, "");
					var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(strResult);
					code = result.code;
					LogHelper.Debug("GetIP", strResult);
					if (result.data.Count > 0 && code == 0)
					{
                    
						string value = result.data[0].ip.ToString().Trim() + ":" + result.data[0].port.ToString().Trim();
                        //string expire_time = result.data[0].expire_time.ToString();
                        ip = value;
                        DateTime date = DateTime.Now.AddSeconds(reFreshTime);
                        //DateTime.TryParse(expire_time, out date);

						lastResult = ip;
                        lastGetTime = date;
                    }
				}
				catch (Exception ex)
				{
					LogHelper.Error("GetIP", ex.ToString());
                    System.Threading.Thread.Sleep(2000);
					break;
				}
            }
            while (code != 0);
            return ip;
        }

        public class VPNResult
        {
            /// <summary>
            /// 0为成功，1为失败
            /// </summary>
            public int code { set; get; }


            public VPNResultItem data { set; get; }

        }

        public class VPNResultItem
        {   
            public int count { set; get; }
            public List<string> proxy_list { set; get; }
        }

    }
}
