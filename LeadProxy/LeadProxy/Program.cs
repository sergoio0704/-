using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Web;


namespace LeadProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitrix24 bx_logon = new Bitrix24();
            string TaskListByJSON = bx_logon.SendCommand("crm.lead.list","{}", "{ filter: { \"PHONE\": \"895\" }, select: [\"ID\", \"TITLE\"]}");
            Console.WriteLine(TaskListByJSON);
            Console.ReadKey();
        }
    }
}
