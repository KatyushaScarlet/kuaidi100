using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("快递100 API调用例子 v1.0");
                    Console.WriteLine("");
                    Console.Write("请输入你要查询的快递单号：");
                    string postID = Console.ReadLine();
                    string[] company = autoNumber(postID);
                    if (company.Count() == 0)
                    {
                        Console.WriteLine("你输入的快递单号格式不正确！");
                        Console.ReadKey();
                        continue;
                    }

                    for (int i = 0; i < company.Count(); i++)
                    {
                        Console.WriteLine((i + 1).ToString() + "." + getName(company[i]));
                    }
                    Console.Write("\n以上是查询到的快递公司，请根据序号选择：");
                    int chose = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("\n正在查询中，请稍候");
                    int status = -1;
                    string[,] result = getMessage(company[chose - 1], postID, out status);
                    Console.WriteLine("————————————————————");
                    Console.WriteLine("包裹状态：" + getStatus(status));
                    for (int i = 0; i < result.GetLength(0); i++)
                    {
                        Console.WriteLine("[" + result[i, 0] + "][" + result[i, 1] + "]" + result[i, 2]);
                    }
                    Console.WriteLine("————————————————————");
                    Console.WriteLine("按任意键重新开始......");
                    Console.ReadKey();
                }
                catch (Exception)
                {
                    Console.WriteLine("\n输入错误,请重试！");
                    Console.ReadKey();
                }
                
            }
        }

        public static string[] autoNumber(string postID)//根据单号查快递公司（可能出现多种情况）
        {
            string json = HttpGet("https://www.kuaidi100.com/autonumber/autoComNum?text=" + postID);//GET地址
            JObject jo = JsonConvert.DeserializeObject(json) as JObject;//反序列化为对象
            int count = jo["auto"].Count();//读快递公司数量
            string[] output = new string[count];
            for (int i = 0; i < count; i++)
            {
                output[i] = jo["auto"][i]["comCode"].ToString();//赋值快递公司代码
            }
            return output;
        }

        public static string[,] getMessage(string comCode, string postID, out int status)//根据单号与公司查物流信息，返回状态与物流信息
        {
            /*
            status：
            0：在途，即货物处于运输过程中；
            1：揽件，货物已由快递公司揽收并且产生了第一条跟踪信息；
            2：疑难，货物寄送过程出了问题；
            3：签收，收件人已签收；
            4：退签，即货物由于用户拒签、超区等原因退回，而且发件人已经签收；
            5：派件，即快递正在进行同城派件；
            6：退回，货物正处于退回发件人的途中；  
            */
            Random rdm = new Random();
            string json = HttpGet("https://www.kuaidi100.com/query?type=" + comCode + "&postid=" + postID + "&id=1&valicode=&temp="+ rdm.Next().ToString());
            JObject jo = JsonConvert.DeserializeObject(json) as JObject;
            status = Convert.ToInt32(jo["state"]);//读status
            int count = jo["data"].Count();//读物流信息条数
            string[,] output = new string[count,3];//每条信息分别包含时间，地点，物流信息
            for (int i = 0; i < count; i++)
            {
                output[i, 0] = jo["data"][i]["time"].ToString();//读时间
                output[i, 1] = jo["data"][i]["location"].ToString();//读地点（部分物流无此项）
                output[i, 2] = jo["data"][i]["context"].ToString();//读信息
            }

            return output;
        }

        public static string getName(string shortName)
        {
            JObject jo = JsonConvert.DeserializeObject(Resource1.CompanyInfo) as JObject;

            foreach (var item in jo["company"])
            {
                if (item["number"].ToString() == shortName)
                {
                    return item["name"].ToString();
                }
            }

            return "";
        }

        public static string getStatus(int status)
        {
            switch (status)
            {
               case 0:
                    return "在途";
                case 1:
                    return "揽件";
                case 2:
                    return "疑难";
                case 3:
                    return "签收";
                case 4:
                    return "退签";
                case 5:
                    return "派件";
                case 6:
                    return "退回";
                default:
                    return "错误";
            }
        }

        public static string HttpGet(string Url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
    }
}
