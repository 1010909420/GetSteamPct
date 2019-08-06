using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DAL.DAO;
using GetHtmlContent.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;

namespace GetSteamPct.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ActivityController : Controller
    {
        [HttpGet("GetActivityList")]
        public IActionResult GetActivityList()
        {
            ActivityDAO activityDAO = new ActivityDAO();
            List<Activity> list = new List<Activity>();
            list = activityDAO.GetAll().Where(e => e.status == 1).Include(e => e.goods).ToList();
            return Json(list);
        }

        [HttpGet("GetNowActivityList")]
        public IActionResult GetNowActivityList(int page)
        {
            try
            {
                string appPath = AppContext.BaseDirectory;
                if (Directory.Exists(appPath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(appPath);
                    //directoryInfo = directoryInfo.Parent.Parent.Parent;
                    appPath = directoryInfo.ToString() + "\\wwwroot\\images\\";
                }

                List<GoodsInfo> list = new List<GoodsInfo>();

                int start = 15 * page;
                string json = Tool.Tool.HttpGet("https://store.steampowered.com/contenthub/querypaginated/specials/TopSellers/render/?query=&start=" + start + "&count=15&cc=CN&l=schinese&v=4&tag=", "");
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
                HtmlNodeCollection itemNodes = doc.DocumentNode.SelectNodes(".//a[@class='tab_item  ']");
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
                        g.img = Tool.Tool.FileDownSave(g.img, appPath);
                        foreach (var t in item.SelectNodes(".//span[@class='top_tag']"))
                        {
                            g.tag += t.InnerText;
                        }
                        list.Add(g);
                    }
                }

                int @int = 123;

                return Json(new { success = 1, list });
            }
            catch (Exception ex)
            {
                return Json(new { success = 0, ex });
            }
        }
    }
}