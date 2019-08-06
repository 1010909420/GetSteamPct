using DAL.DAO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Model.Entity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetSteamPct.Middleware
{
    public class RequestLogMiddleware
    {
        private RequestDelegate _next;

        public RequestLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if(!context.Request.Path.ToString().ToUpper().Contains("GETNOWACTIVITYLIST"))
            {
                await _next(context);
            } else {
                DelImg();

                RequestLog requestLog = new RequestLog();
                RequestLogDAO requestLogDAO = new RequestLogDAO();

                var originalResponseStream = context.Response.Body;

                using (var ms = new MemoryStream())
                {
                    context.Response.Body = ms;
                    DateTime startDate = DateTime.Now;
                    await _next(context);
                    TimeSpan consume = DateTime.Now - startDate;

                    ms.Position = 0;
                    var responseReader = new StreamReader(ms);

                    var responseContent = responseReader.ReadToEnd();
                    String success = Regex.Match(responseContent, "\"success\":-?\\d*").Value;

                    Regex ipRegex = new Regex("((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}");

                    requestLog.createTime = DateTime.Now;
                    requestLog.path = context.Request.Path;
                    requestLog.clientIp = ipRegex.Match(context.Connection.RemoteIpAddress.ToString()).Value;
                    requestLog.serverIp = ipRegex.Match(context.Connection.LocalIpAddress.ToString()).Value;
                    requestLog.consume = consume.Milliseconds;

                    //{"code":0,"data":{"ip":"121.33.146.245","country":"中国","area":"","region":"广东","city":"广州","county":"XX","isp":"电信","country_id":"CN","area_id":"","region_id":"440000","city_id":"440100","county_id":"xx","isp_id":"100017"}}


                    String addr = "本地";
                    if(requestLog.clientIp != "127.0.0.1") {
                        addr = Tool.Tool.HttpGet("http://ip.taobao.com/service/getIpInfo.php", "ip=" + requestLog.clientIp);
                        JObject result = JObject.Parse(addr);
                        addr = result.Value<JObject>("data").Value<String>("region") + result.Value<JObject>("data").Value<String>("city");
                    }

                    _ = success.Contains("1") ? requestLog.isSuccess = 1 : requestLog.isSuccess = 0;
                    requestLog.addr = addr;
                    requestLogDAO.Add(requestLog);

                    ms.Position = 0;
                    await ms.CopyToAsync(originalResponseStream);
                    context.Response.Body = originalResponseStream;
                }

                //DateTime startDate = DateTime.Now;
                //await _next(context);
                //TimeSpan consume = DateTime.Now - startDate;
                //Console.WriteLine("请求耗时: " + consume.Milliseconds / 1000.0 + "秒");
            }
        }

        private void DelImg()
        {
            string appPath = AppContext.BaseDirectory;
            if (Directory.Exists(appPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(appPath);
                //directoryInfo = directoryInfo.Parent.Parent.Parent;
                appPath = directoryInfo.ToString() + "\\wwwroot\\images\\";
            }

            string[] files = Directory.GetFiles(appPath, "*.jpg", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string s = file;
                FileInfo f = new FileInfo(s);
                DateTime nowtime = DateTime.Now;
                TimeSpan t = nowtime - f.CreationTime;
                int hours = t.Hours;
                if (hours > 1)
                {
                    File.Delete(s);
                }
            }
        }
    }

    public static class RequestLogExtensions {
        public static IApplicationBuilder UseRequestLog(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLogMiddleware>();
        }
    }
}
