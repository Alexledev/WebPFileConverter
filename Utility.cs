using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFFIleConversion
{
    public class Utility
    {
        public static IEnumerable<string> GetQualityNum()
        {
            for (int i = 50; i < 101; i += 10) 
            {
                yield return i.ToString();                
            }
        }
    }
}
