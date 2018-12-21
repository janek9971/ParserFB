using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
namespace ParserFB
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
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddArgument("--incognito");

            List<string> lista = new List<string>();
            List<Newtonsoft.Json.Linq.JToken> listJsons = new List<Newtonsoft.Json.Linq.JToken>();

            using (var _driver = new ChromeDriver(chromeOptions))
            {

                var time = 10;
                var navigation = _driver.Navigate();
                sw.Stop();
                Console.WriteLine("czas= " + sw.ElapsedMilliseconds);

                _driver.Navigate().GoToUrl("https://www.facebook.com/pg/klubhydrozagadka/events/");

                ParseWeb(_driver, "Hydro", ref listJsons);
                try
                {
                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(time);

                    _driver.Navigate().GoToUrl("https://www.facebook.com/pg/klubremont/events/");
                    ParseWeb(_driver, "Remont", ref listJsons);

                    _driver.Navigate().GoToUrl("  https://www.facebook.com/pg/klub.stodola/events/");
                    ParseWeb(_driver, "Stodoła", ref listJsons);



                    _driver.Close();

                }
                catch (Exception ex)
                {
                    time += 10;
                }


            }
            Console.WriteLine(listJsons);
          
            Console.ReadLine();
            Console.ReadLine();
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
        private static void ParseWeb(ChromeDriver _driver, string path, ref List<Newtonsoft.Json.Linq.JToken> lista)
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
                    Console.WriteLine(time);

                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(time);
                    var upcomingEventsIsLoaded = _driver.FindElement(By.XPath(" //*[@id='upcoming_events_card']/div/div[2]")).Displayed;

                    if (upcomingEventsIsLoaded)
                    {

                        var isDivExist = true;
                        //var result = default(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>);
                        var iterator = 2;

                        try
                        {
                                while (isDivExist)
                            {
                                var trySource = _driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{iterator}]"));
                                if (iterator % 2 == 0)
                                {
                                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(200);
                                }

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
                            if ((errorIterate >= 3 || list.Count > 3) && list[list.Count - 1] == iterator && list[list.Count - 2] == iterator)
                            {
                                divCount = iterator;
                                //result = _driver.FindElements(By.XPath($"//*[@id='upcoming_events_card']/div/div"));
                                isDivExist = false;
                                break;

                            }
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(ex);

                        }

                    }
                    
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);

                    time += 1;
                }

            }
            var sw = new Stopwatch();
            sw.Start();
            var objects = new List<ItemClub>();
            var dateEvent = default(string);
            var titleEvent = default(string);
            var timeEvent = default(string);
            var guestsEvent = default(string);
            var localizationEvent = default(string);

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
                JObject jsonClubs= new JObject();

                jsonClubs[$"Club{path}"] = JToken.FromObject(objects);
                File.WriteAllText($@"\Users\JANEK\Desktop\Strona\helloworld{path}Lista.txt", jsonClubs.ToString());

                lista.Add(jsonClubs);
                sw.Stop();
                Console.WriteLine("ParsowanieDanychCzas= " + sw.ElapsedMilliseconds);
            }
        }
    }


}