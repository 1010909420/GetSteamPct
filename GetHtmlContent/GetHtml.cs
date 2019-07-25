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
            string appPath = AppContext.BaseDirectory + "img\\";

            List<GoodsInfo> list = new List<GoodsInfo>();


            WebClient MyWebClient = new WebClient();
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
            Byte[] pageData = MyWebClient.DownloadData("https://store.steampowered.com/specials?p=4&tab=TopSellers"); //从指定网站下载数据
            //string pageHtml = Encoding.Default.GetString(pageData);  //如果获取网站页面采用的是GB2312，则使用这句    
            string pageHtml = Encoding.UTF8.GetString(pageData); //如果获取网站页面采用的是UTF-8，则使用这句


            //初始化网络请求客户端
            HtmlWeb webClient = new HtmlWeb();
            //初始化文档
            //HtmlDocument doc = webClient.Load("https://store.steampowered.com/specials?p=4&tab=TopSellers");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageHtml);
            //查找节点
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//div[@id='TopSellersRows']");
            HtmlNodeCollection itemNodes = titleNodes[0].SelectNodes(".//a[@class='tab_item  ']");
            //HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//a[@class='tab_item  ']");
            if (itemNodes != null)
            {
                foreach (var item in itemNodes)
                {
                    GoodsInfo g = new GoodsInfo();
                    g.name = item.SelectNodes(".//div[@class='tab_item_name']")[0].InnerText;
                    g.originalPrice = item.SelectNodes(".//div[@class='discount_original_price']")[0].InnerText;
                    g.finalPrice = item.SelectNodes(".//div[@class='discount_final_price']")[0].InnerText;
                    g.pct = item.SelectNodes(".//div[@class='discount_pct']")[0].InnerText;
                    g.img = item.SelectNodes(".//img[@class='tab_item_cap_img']")[0].Attributes["src"].Value;
                    FileDownSave(g.img, appPath);
                    list.Add(g);
                }

                //GoodsDAO goodsDAO = new GoodsDAO();
                //List<Goods> goodsList = new List<Goods>();
                //foreach (var g in list)
                //{
                //    Goods goods = new Goods();
                //    Goods nowGoods = goodsDAO.GetAll().Where(e => e.name == g.name).FirstOrDefault();
                //    if (nowGoods == null)
                //    {
                //        goods.name = name;
                //    }
                //}
            }
        }


        public static String FileDownSave(string url, string savePath)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                var saveFileName = $"{Guid.NewGuid()}";
                savePath = savePath + saveFileName + ".jpg";
            }
            HttpClient httpClient = new HttpClient();
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
            return savePath;
        } 
    }
}
