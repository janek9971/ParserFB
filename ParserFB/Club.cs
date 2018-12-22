using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ParserFB
{
   public class Club
    {
        [JsonConverter(typeof(ESDateTimeConverter))]
        public DateTime DateStart { get; set; }
        [JsonConverter(typeof(ESDateTimeConverter))]
        public DateTime DateEnd { get; set; }
        public string Title { get; set; }
        //public string Time { get; set; }
        public string Guests { get; set; }
        public string Localization { get; set; }

        //public DateTime DateT { get; set; }
    
    }
   // ReSharper disable once InconsistentNaming
   public class ESDateTimeConverter : IsoDateTimeConverter
   {
       public ESDateTimeConverter()
       {
           base.DateTimeFormat = "dd.MM.yyyy HH:mm:ss";

           // base.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
       }
   }
}
