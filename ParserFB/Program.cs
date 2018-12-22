using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using static System.Int32;

namespace ParserFB
{
    static class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("pl-PL");
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
            chromeOptions.AddArgument("--lang=pl-PL");
            List<JToken> listJsons = new List<JToken>();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
            using (var driver = new ChromeDriver(chromeOptions))
            {

                var time = 10;
                sw.Stop();
                Console.WriteLine("czas= " + sw.ElapsedMilliseconds);

                driver.Navigate().GoToUrl("https://www.facebook.com/pg/klubhydrozagadka/events/");

                ParseWeb(driver, "Hydro", ref listJsons);
                try
                {
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(time);

                    driver.Navigate().GoToUrl("https://www.facebook.com/pg/klubremont/events/");
                    ParseWeb(driver, "Remont", ref listJsons);

                    driver.Navigate().GoToUrl("  https://www.facebook.com/pg/klub.stodola/events/");
                    ParseWeb(driver, "Stodoła", ref listJsons);



                    driver.Close();

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

        private static void ParseWeb(ChromeDriver driver, string path, ref List<JToken> listJson)
        {
     
            Console.WriteLine(path);
            //var x = false;
            var errorIterate = 0;
            var time = 1;
            int divCount;
            var list = new List<int>();
            while (true)
            {
                //Console.WriteLine(time);
                try
                {
                    Console.WriteLine(time);

                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(time);
                    var upcomingEventsIsLoaded = driver.FindElement(By.XPath(" //*[@id='upcoming_events_card']/div/div[2]")).Displayed;

                    if (!upcomingEventsIsLoaded) continue;
                    //var result = default(System.Collections.ObjectModel.ReadOnlyCollection<IWebElement>);
                    var iterator = 2;

                    try
                    {
                        
                        while (true)
                        {
                            var trySource = driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{iterator}]"));
                            if (iterator % 2 == 0)
                            {
                                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(200);
                            }

                            driver.ExecuteScript("arguments[0].scrollIntoView(true);", trySource);

                            iterator++;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        errorIterate++;
                        list.Add(iterator);
                        Console.WriteLine(list.Count);
                        foreach (var xex in list)
                        {
                            Console.WriteLine("listJson=" + xex);
                        }
                        //Console.WriteLine("error= "+errorIterate + "iterator= " + iterator);
                        if ((errorIterate < 3 && list.Count <= 3) || list[list.Count - 1] != iterator ||
                            list[list.Count - 2] != iterator) continue;
                        divCount = iterator;
                        break;
                    }
                    catch (Exception)
                    {
                        //Console.WriteLine(ex);

                    }

                }
                catch (Exception)
                {
                    //Console.WriteLine(ex);

                    time += 1;
                }

            }
            var sw = new Stopwatch();

            if (divCount > 0)
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };
                JObject jsonClubs= new JObject();
                sw.Start();
                       JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
                jsonClubs[$"Club{path}"] = JToken.FromObject(ItemClubs(driver,divCount));
                sw.Stop();
                File.WriteAllText($@"\Users\JANEK\Desktop\Strona\helloworld{path}Lista.txt", jsonClubs.ToString());

                listJson.Add(jsonClubs);
         
                Console.WriteLine("ParsowanieDanychCzas= " + sw.ElapsedMilliseconds);
            }
        }

        private static List<Club> ItemClubs(ChromeDriver driver, int divCount)
        {
            var sw = new Stopwatch();
            var objects = new List<Club>();
            var xpathStringPart1 = "//*[@id='upcoming_events_card']/div/div[";
            var xPathStringPart2 = "/table/tbody/tr/td[";
            for (var i = 0; i < divCount; i++)
            {
                if (i == 0 || i == 1)
                {
                    continue;
                }
                sw.Start();

                var dateEvent = driver.FindElement(By.XPath($"{xpathStringPart1}{i}]{xPathStringPart2}1]/span/span[2]")).Text
                                   + " "
                                   + driver.FindElement(By.XPath($"{xpathStringPart1}{i}]{xPathStringPart2}1]/span/span[1]")).Text;

                var titleEvent = driver.FindElement(By.XPath($"{xpathStringPart1}{i}]{xPathStringPart2}2]/div/div[1]")).Text;

                var info = driver.FindElement(By.XPath($"{xpathStringPart1}{i}]{xPathStringPart2}2]/div/div[2]")).Text;
                var timeEvent = driver.FindElement(By.XPath($"{xpathStringPart1}{i}]{xPathStringPart2}2]/div/div[2]/span[1]")).Text;
                var subStrTrimmedDateEvent = info.Replace(timeEvent, "");
                var guestsEvent = subStrTrimmedDateEvent.Substring(subStrTrimmedDateEvent.IndexOfAny("0123456789".ToCharArray()));
           
                var localizationEvent = driver.FindElement(By.XPath($"  {xpathStringPart1}{i}]{xPathStringPart2}3]/div/div[1]")).Text;
                sw.Stop();

               ParseToDate(dateEvent, timeEvent, out DateTime dateStart, out DateTime dateEnd);
                objects.Add(new Club { DateStart = dateStart, DateEnd = dateEnd, Title = titleEvent, Guests = new string(guestsEvent.Where(char.IsDigit).ToArray()), Localization = localizationEvent});
                //Console.WriteLine(_driver.FindElement(By.XPath($"//*[@id='upcoming_events_card']/div/div[{i}]/table/tbody/tr/td[2]/div/div[2]/span[1]")).Text);

             //var x =   ParseToDate(dateEvent, timeEvent);
            }
            return objects;
        }
      

        private static void  ParseToDate(string dayAndMonth, string time, out DateTime dateStart, out DateTime dateEnd)
        {
            string day;
            string month;
            int monthInt;
            dateStart = default(DateTime);
            dateEnd = default(DateTime);
            var yearNow = DateTime.Now.Year;
            var monthNow = DateTime.Now.Month;
            if (time.Contains('–'))
            {
                
                 var splittedDate = (time.Split('–').Select(s => s.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)));
                 var enumerable = splittedDate as string[][] ?? splittedDate.ToArray();
               

                day = enumerable.ElementAt(0)[0];
                monthInt = enumerable[0][1].ToUpper().GetValueFromDateString();
                var year = enumerable.ElementAt(0).Length==3 ? enumerable.ElementAt(0)[2] : (monthInt<monthNow) ? (yearNow+1).ToString() : yearNow.ToString();
                dateStart = new DateTime(Parse(year), monthInt,Parse(day));
                day = enumerable.ElementAt(1)[0];
                monthInt = enumerable.ElementAt(1)[1].ToUpper().GetValueFromDateString();
                year = enumerable.ElementAt(1).Length == 3 ? enumerable.ElementAt(0)[2] : (monthInt < monthNow) ? (yearNow + 1).ToString() : yearNow.ToString();
                dateEnd = new DateTime(Parse(year), monthInt, Parse(day));
                return;
            }
          

            var splitted = dayAndMonth.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            day = splitted[0];
            month = splitted[1];
             monthInt = month.GetValueFromDateString();


            var hours=time.GetTime()["hour"];
          
           var minutes= time.GetTime()["minutes"];
           if (monthInt < monthNow)
           {
               yearNow = yearNow + 1;
           }
           CultureInfo culture = new CultureInfo("pl-PL");
    
           Thread.CurrentThread.CurrentCulture = culture;
           var resultDate = new DateTime(yearNow, monthInt, Parse(day),hours,minutes,0,DateTimeKind.Local);
           dateStart = resultDate;
           dateEnd = new DateTime(yearNow, monthInt, Parse(day));

        }
       
        private static Dictionary<string, int> GetTime(this string obj)
        {
          
                var trimmed = obj.Substring(obj.IndexOfAny("0123456789".ToCharArray()));
                var trimEnd = trimmed.Substring(trimmed.IndexOfAny("0123456789".ToCharArray()), 5);
                var hour = trimEnd.Substring(0, trimEnd.IndexOf(":"));
                var minutes = trimEnd.Substring(trimEnd.IndexOf(":") + 1);

                //int[] timeArray = {Parse(hour), Parse(minutes)};
                var timeDictionary = new Dictionary<string, int> {
                    { "hour", Parse(hour) },
                    { "minutes", Parse(minutes) }
                };
            return timeDictionary;

           
    
        }
        private static int GetValueFromDateTime(this object obj)
        {
            var result = obj.ToString();
            var resultInteger = Parse(result);
            return resultInteger;
        }

        private static int GetValueFromDateString(this string obj)
        {
            var thisMonth = 0;
            foreach (var monthDict in Months)
            {
                if (obj.Contains(monthDict.Key))
                {
                     thisMonth = monthDict.Value;
                }
            }

            return thisMonth;
        }
        public static IDictionary<string, int> Months = new Dictionary<string, int>
        {
            { "STY", 1},
            { "LUT", 2},
            { "MAR", 3},
            { "KWI", 4},
            { "MAJ", 5},
            { "CZE", 6},
            { "LIP", 7},
            { "SIE", 8},
            { "WRZ", 9},
            { "PAZ", 10},
            { "LIS", 11},
            { "GRU", 12},
        };
    }



}