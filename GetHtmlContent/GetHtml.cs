using DAL.DAO;
using GetHtmlContent.Model;
using HtmlAgilityPack;
using Model.Entity;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;

namespace GetHtmlContent
{
    public class GetHtml
    {

        public void Start()
        {

            Get();
            //// 定时发送
            //DateTime nowDate = DateTime.Now;
            //DateTime Date = DateTime.Parse("11:05:00");
            //TimeSpan timeSpan = Date - nowDate;
            //int intTotalMilliseconds = timeSpan.TotalMilliseconds > 0 ? (int)timeSpan.TotalMilliseconds : (int)timeSpan.TotalMilliseconds + 24 * 60 * 60 * 1000;

            ////Console.Write(intTotalMilliseconds);
            //Timer timer = new Timer(e =>
            //{
            //    Get();
            //}, "", intTotalMilliseconds, 4 * 60 * 60 * 1000);


            
        }

        private void Get()
        {
            string appPath = AppContext.BaseDirectory;
            if(Directory.Exists(appPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(appPath);
                directoryInfo = directoryInfo.Parent.Parent.Parent;
                appPath = directoryInfo.ToString() + "\\wwwroot\\img\\";
            }

            List<GoodsInfo> list = new List<GoodsInfo>();

            for (int i = 0; i < 3; i++) {
                int start = 15 * i;
                string json = HttpGet("https://store.steampowered.com/contenthub/querypaginated/specials/TopSellers/render/?query=&start="+ start +"&count=15&cc=CN&l=schinese&v=4&tag=", "");
                json = Regex.Match(json, "\"results_html\":\"[\\S\\s]*\",").Value;
                int startIndex = "\"results_html\":\"".Count();
                json = json.Substring(startIndex, json.Count() - startIndex - 2);
                json = Regex.Unescape(json);
                string pageHtml = json;

                //初始化网络请求客户端
                HtmlWeb webClient = new HtmlWeb();
                //初始化文档
                //HtmlDocument doc = webClient.Load("https://store.steampowered.com/specials?p=4&tab=TopSellers");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(pageHtml);
                //查找节点
                //HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//div[@id='TopSellersRows']");
                HtmlNodeCollection itemNodes = doc.DocumentNode.SelectNodes(".//a[@class='tab_item  ']");
                //HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//a[@class='tab_item  ']");
                if (itemNodes != null)
                {
                    Regex priceRegex = new Regex("\\d+(\\.\\d+)?");
                    foreach (var item in itemNodes)
                    {
                        GoodsInfo g = new GoodsInfo();
                        g.name = item.SelectNodes(".//div[@class='tab_item_name']")[0].InnerText;
                        g.originalPrice = priceRegex.Match(item.SelectNodes(".//div[@class='discount_original_price']")[0].InnerText).Value;
                        g.finalPrice = priceRegex.Match(item.SelectNodes(".//div[@class='discount_final_price']")[0].InnerText).Value;
                        g.pct = priceRegex.Match(item.SelectNodes(".//div[@class='discount_pct']")[0].InnerText).Value;
                        g.img = item.SelectNodes(".//img[@class='tab_item_cap_img']")[0].Attributes["src"].Value;
                        g.img = FileDownSave(g.img, appPath);
                        foreach (var t in item.SelectNodes(".//span[@class='top_tag']"))
                        {
                            g.tag += t.InnerText;
                        }
                        list.Add(g);
                    }
                }
            }

            ActivityDAO activityDAO = new ActivityDAO();
            List<Activity> activityList = activityDAO.GetAll().Where(e => e.status == 1).ToList();

            int alc = activityList.Count();
            for (int i = 0; i < alc; i++)
            {
                activityList[i].endTime = DateTime.Now;
                activityList[i].status = 0;
            }
            int batch = 0;
            if(alc != 0) { 
                activityDAO.BatchUpdate(activityList);
                batch = activityList[0].batch;
            }

            GoodsDAO goodsDAO = new GoodsDAO();
            List<Activity> newActivityList = new List<Activity>();
            int rank = 1;
            foreach (var g in list)
            {
                Goods goods = goodsDAO.GetAll().Where(e => e.name == g.name).FirstOrDefault();
                if (goods == null)
                {
                    goods = new Goods() {
                        name = g.name,
                        price = decimal.Parse(g.originalPrice),
                        tag = g.tag,
                        imgURI = g.img,
                        createTime = DateTime.Now,
                        status = 1
                    };
                    goodsDAO.Add(goods);
                } else
                {
                    if(File.Exists(goods.imgURI)) { 
                        FileInfo fileInfo = new FileInfo(goods.imgURI);
                        fileInfo.Delete();
                    }

                    goods.imgURI = g.img;
                    goods.price = decimal.Parse(g.originalPrice);
                    goods.tag = g.tag;
                    goods.createTime = DateTime.Now;
                    goods.status = 1;
                    goodsDAO.Update(goods);
                }

                Activity activity = new Activity() {
                    goodsId = goods.id,
                    finalPrice = decimal.Parse(g.originalPrice),
                    pct = int.Parse(g.pct),
                    status = 1,
                    createTime = DateTime.Now,
                    rank = rank++,
                    batch = batch + 1
                };

                newActivityList.Add(activity);
            }
            activityDAO.SaveList(newActivityList);

        }


        public static string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }


        public static string FileDownSave(string url, string savePath)
        {
            var saveFileName = $"{Guid.NewGuid()}";
            if (!string.IsNullOrWhiteSpace(url))
            {
                savePath = savePath + saveFileName + ".jpg";
            }
            HttpClient httpClient = new HttpClient();
            //ftpClient.setFileType(FTP.BINARY_FILE_TYPE);
            var t = httpClient.GetByteArrayAsync(url);
            t.Wait();
            Stream responseStream = new MemoryStream(t.Result);
            Stream stream = new FileStream(savePath, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, bArr.Length);
            }
            stream.Close();
            responseStream.Close();
            return "img\\" + saveFileName + ".jpg";
        } 
    }
}
