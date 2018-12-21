using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
namespace ConsoleApp1
{
    static class Program
    {
        static void Main(string[] args)
        {

            var sw = new Stopwatch();
            sw.Start();
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            chromeOptions.AddArguments("--no-proxy-server");
            chromeOptions.AddArguments("--proxy-server='direct://'");
            chromeOptions.AddArguments("--proxy-bypass-list=*");
            chromeOptions.AddArgument("blink-settings=imagesEnabled=false");
            //var path = @"C:\Users\JANEK\AppData\Local\Google\Chrome\User Data\Default\Extensions\cjpalhdlnbpafiamejdnhcphjbkeiagm\1.17.4_0";
            //chromeOptions.AddArgument($"load-extension={path}");

            chromeOptions.AddArgument("--ignore-certificate-errors");
            //chromeOptions.AddArgument("--disable-popup-blocking");
            chromeOptions.AddArgument("--incognito");
            //chromeOptions.AddArgument("--disable-gl-drawing-for-tests");
            //chromeOptions.AddArgument("--disable-low-res-tiling");
            //chromeOptions.AddArgument("--enable-tcp-fastopen");
            List<string> lista = new List<string>();
            JObject jsonAll = new JObject();

            using (var _driver = new ChromeDriver(chromeOptions))
            {

                var time = 10;
                var x = _driver.Navigate();
                sw.Stop();
                Console.WriteLine("czas= " + sw.ElapsedMilliseconds);

                _driver.Navigate().GoToUrl("https://www.facebook.com/pg/klubhydrozagadka/events/");

                ParseWeb(_driver, "hydro", ref lista,ref jsonAll);
                try
                {
                    //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(time);

                    //_driver.Navigate().GoToUrl("https://www.facebook.com/pg/klubremont/events/");
                    //ParseWeb(_driver, "remont", ref lista);
                    //_driver.Navigate().GoToUrl("  https://www.facebook.com/pg/klub.stodola/events/");
                    //ParseWeb(_driver, "stodola", ref lista);


                    _driver.Close();

                }
                catch (Exception ex)
                {
                    time += 10;
                }


            }
            Console.WriteLine(jsonAll);
            var clubData = new StringBuilder();
            var clubDataList = new List<string>();
            var clubs = new List<string>();
            JObject rss= new JObject();
            Console.WriteLine(lista.Count);
            int i = 0;
            int z = 0;
            var objects = new List<Item>();
            var text=default(string);
            foreach (var inList in lista[0].Split('\n'))
            {
                
                if (i==0 || i==1)
                {
                    i++;
                    continue;
                }

               
                if (Int32.TryParse(inList, out z))
                {
                    if (clubData.Length > 0)
                    {
                        objects.Add(new Item { Header = clubDataList[0], Informations = clubDataList[1], ClubName = clubDataList[2], Localization = clubDataList[3] });
                        clubs.Add(clubData.ToString());
                        clubData.Clear();
                        clubDataList.Clear();
                    }
                    continue;
                }
                text = inList.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
         
                clubData.Append(text);
                clubDataList.Add(text);
                Console.WriteLine(text);
                i++;
            }
            foreach(var club in clubs)
            {
               Console.WriteLine(club);
            }
            foreach (var club in objects)
            {
                Console.WriteLine(club);
            }
            JObject json = new JObject();

            json["Clubs"] = JToken.FromObject(objects);
            Console.WriteLine(json.ToString());
            Console.ReadLine();
            Console.ReadLine();
        }
        class Item
        {
            public string Header { get; set; }
            public string Informations { get; set; }
            public string ClubName { get; set; }
            public string Localization { get; set; }

        }
        class ItemClub
        {
            public string Date { get; set; }
            public string Title { get; set; }
            public string Time { get; set; }
            public string Guests { get; set; }
            public string Localization { get; set; }

        }
        public static void Clear(this StringBuilder value)
        {
            value.Length = 0;
            value.Capacity = 0;
        }
        private static void ParseWeb(ChromeDriver _driver, string path, ref List<string> lista, ref JObject json)
        {
     
            Console.WriteLine(path);
            //var x = false;
            var errorIterate = 0;
            var time = 1;
            var divCount=default(int);
            List<int> list = new List<int>();
            while (true)
            {
                //Console.WriteLine(time);
                try
                {
                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(time);
                    var upcomingEventsIsLoaded = _driver.FindElement(By.XPath(" //*[@id='upcoming_events_card']/div/div[2]")).Displayed;

                    if (upcomingEventsIsLoaded)
                    {

                        var isDivExist = true;
                        var result = default(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>);
                        //StringBuilder strRes = new StringBuilder();
                        var iterator = 2;

                        try
                        {
                            while (isDivExist)
                            {
                                var trySource = _driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{iterator}]"));
                                //strRes.Append(trySource.Text);
                                if (iterator % 2 == 0)
                                {
                                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(50);
                                }

                                //Console.WriteLine(trySource.Text);
                                _driver.ExecuteScript("arguments[0].scrollIntoView(true);", trySource);

                                iterator++;
                            }
                        }
                        catch (OpenQA.Selenium.NoSuchElementException ex)
                        {
                            errorIterate++;
                            list.Add(iterator);
                            Console.WriteLine(list.Count);
                            foreach (var xex in list)
                            {
                                Console.WriteLine("lista=" + xex);
                            }
                            //Console.WriteLine("error= "+errorIterate + "iterator= " + iterator);
                            if ((errorIterate > 4 || list.Count > 4) && list[list.Count - 1] == iterator)
                            {
                                divCount = iterator;
                                Console.WriteLine(_driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[7]/table/tbody/tr/td[2]/div/div[1]")).Text);
                                Console.WriteLine(_driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[5]/table/tbody/tr/td[1]/span")).Text);
                                Console.WriteLine(_driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[42]/table/tbody/tr/td[3]/div/div[1]")).Text);
                               var info= _driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[7]/table/tbody/tr/td[2]/div/div[2]")).Text;

                                //Console.WriteLine(_driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[7]/table/tbody/tr/td[2]/div/div[2]/text()")));
                                var dateEvents =_driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[7]/table/tbody/tr/td[2]/div/div[2]/span[1]")).Text;
                                Console.WriteLine(dateEvents);
                                var subStrTrimmedDateEvent = info.Replace(dateEvents, "");
                                var guestss = subStrTrimmedDateEvent.Substring(subStrTrimmedDateEvent.IndexOfAny("0123456789".ToCharArray()));

                                Console.WriteLine(guestss);
                            

                                Console.WriteLine();


                                result = _driver.FindElements(By.XPath($"//*[@id='upcoming_events_card']/div/div"));
                                isDivExist = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);

                        }
                       

                        StringBuilder strRes = new StringBuilder();
                        foreach (var div in result)
                        {
                            strRes.Append(div.Text);
                        }

                        lista.Add(strRes.ToString());
                        //string output = JsonConvert.SerializeObject(strRes);
                        File.WriteAllText($@"\Users\JANEK\Desktop\Strona\helloworld{path}New.txt", strRes.ToString());
                        //File.WriteAllText($@"\Users\JANEK\Desktop\Strona\helloworld{path}Lista.txt", clubData.ToString());

                        //File.WriteAllText($@"\Users\JANEK\Desktop\Strona\helloworld{path}blabla.txt", xd.ToString());

                        break;
                    }
                    
                }
                catch (Exception ex)
                {
                    time += 1;
                }

            }
            var objects = new List<ItemClub>();
            var dateEvent = default(string);
            var titleEvent = default(string);
            var timeEvent = default(string);
            var guestsEvent = default(string);
            var localizationEvent = default(string);

            //objects.Add(new Item { Header = clubDataList[0], Informations = clubDataList[1], ClubName = clubDataList[2], Localization = clubDataList[3] });
            if (divCount > 0)
            {
                for (int i = 0; i < divCount; i++)
                {
                    if (i == 0 || i == 1)
                    {
                        continue;
                    }
                    dateEvent = _driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{i}]/table/tbody/tr/td[1]/span/span[2]")).Text 
                        + " " 
                        + _driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{i}]/table/tbody/tr/td[1]/span/span[1]")).Text;

                    titleEvent = _driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{i}]/table/tbody/tr/td[2]/div/div[1]")).Text;

                    var info = _driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{i}]/table/tbody/tr/td[2]/div/div[2]")).Text;
                    timeEvent = _driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{i}]/table/tbody/tr/td[2]/div/div[2]/span[1]")).Text;
                    var subStrTrimmedDateEvent = info.Replace(timeEvent, "");
                    guestsEvent = subStrTrimmedDateEvent.Substring(subStrTrimmedDateEvent.IndexOfAny("0123456789".ToCharArray()));
                    localizationEvent= _driver.FindElement(By.XPath($"  //*[@id='upcoming_events_card']/div/div[{i}]/table/tbody/tr/td[3]/div/div[1]")).Text;

                    objects.Add(new ItemClub {Date=dateEvent, Title=titleEvent,Time=timeEvent,Guests=guestsEvent,Localization=localizationEvent });
                    //Console.WriteLine(_driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{i}]/table/tbody/tr/td[2]/div/div[2]/span[1]")).Text);


                }
                //json = new JObject();

                json["Clubs"] = JToken.FromObject(objects);
            }
        }
    }
    //public static class WebDriverExtensions
    //{
    //    public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
    //    {
    //        if (timeoutInSeconds > 0)
    //        {
    //            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
    //            return wait.Until(drv => drv.FindElement(by));
    //        }
    //        return driver.FindElement(by);
    //    }
    //}


}