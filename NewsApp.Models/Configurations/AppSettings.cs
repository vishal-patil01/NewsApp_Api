using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.Models.Configurations
{
    public class AppSettings
    {
        public static ConfigurationSettings ConfigurationSettings{get;set;}
    }
    public class ConfigurationSettings
    {
        public string NewsServiceBaseUrl { get; set; }
    }
}
