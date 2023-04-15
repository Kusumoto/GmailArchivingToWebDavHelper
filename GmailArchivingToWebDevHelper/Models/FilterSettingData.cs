using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMailArchivingToWebDavHelper.Models
{
    public class FilterSettingData
    {
        public string HeaderRegEx { get; set; } = "";
        public string FilePath { get; set; } = "";
    }
}
