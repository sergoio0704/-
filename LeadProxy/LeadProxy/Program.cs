using System;
using System.Collections.Generic;
using LeadProxy.Clients;
using LeadProxy.Dto;

namespace LeadProxy
{
    class Program
    {
        private const string BX_ClientID = "local.5e85c27b09fac2.62891485";
        private const string BX_ClientSecret = "6lhHhp0nAFnVEcVL9B1822D6kKgbgpOKdhvRBg2Qf4NfYpQBB5";
        private const string BX_Portal = "https://b24-yqgspf.bitrix24.ru";
        private const string username = "serega1986-16@mail.ru";
        private const string password = "serg200189877123336";
        static void Main(string[] args)
        {
            Bitrix24Client portalClient1 = new Bitrix24Client(BX_Portal, BX_ClientID, BX_ClientSecret, username, password);
            
            //1
            List<LeadDto> leads = portalClient1.GetLeads();
            leads.ForEach(l => Console.WriteLine(l.TITLE));

            //2
            portalClient1.GetContactById(2);
            
            //3
            portalClient1.AddLead();
            Console.ReadKey();
        }
    }
}
