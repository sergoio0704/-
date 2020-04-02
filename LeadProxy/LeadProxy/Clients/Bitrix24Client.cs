using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Net;
using System;
using System.Collections.Generic;
using LeadProxy.Dto;

namespace LeadProxy.Clients
{
    class Bitrix24Client
    {
        private const string BX_OAuthSite = "https://oauth.bitrix.info";

        private string _portalUri { get; set; }
        private string _clientId { get; set; }
        private string _clientSecret { get; set; }
        private string _username { get; set; }
        private string _password { get; set; }
        private HttpClient _client { get; set; }
        private string _accessToken { get; set; }
        private string _refreshToken { get; set; }
        private DateTime _refreshTime { get; set; }

        public Bitrix24Client(string portalUri, string clientId, string clientSecret, string username, string password)
        {
            _portalUri = portalUri;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _username = username;
            _password = password;
            Connect();
        }

        public void Connect()
        {
            HttpWebRequest request = HttpClient.CreateWebRequest($"{_portalUri}/oauth/authorize/?client_id={_clientId}");
            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(_username + ":" + _password));
            request.Headers.Add("Authorization", "Basic " + svcCredentials);
            string code = HttpClient.RequestCode(request);
            SetTokens($"{BX_OAuthSite}/oauth/token/?grant_type=authorization_code&client_id={_clientId}&client_secret={_clientSecret}&code={code}");
        }

        private void SetTokens(string uri)
        {
            string response = HttpClient.RequestGet(uri);
            var converter = new ExpandoObjectConverter();
            dynamic objLogonBitrixOAuth = JsonConvert.DeserializeObject<ExpandoObject>(response, converter);
            _accessToken = objLogonBitrixOAuth.access_token;
            _refreshToken = objLogonBitrixOAuth.refresh_token;
            _refreshTime = DateTime.Now.AddSeconds(objLogonBitrixOAuth.expires_in);
        }

        private void RefreshTokens()
        {
            if (_refreshTime == DateTime.MinValue)
            {
                Connect();
                return;
            }
            if (_refreshTime.AddSeconds(-5) < DateTime.Now)
            {
                SetTokens($"{BX_OAuthSite}/oauth/token/?grant_type=refresh_token&client_id={_clientId}&client_secret={_clientSecret}&refresh_token={_refreshToken}");
            }
        }

        public List<LeadDto> GetLeads()
        {
            RefreshTokens();
            string response =  HttpClient.RequestGet($"{ _portalUri}/rest/crm.lead.list?auth={_accessToken}");
            LeadsResult leadsResult = JsonConvert.DeserializeObject<LeadsResult>(response);
            return leadsResult.Result;
        }

        public ContactDto GetContactById(int contactId)
        {
            RefreshTokens();
            string response = HttpClient.RequestGet($"{ _portalUri}/rest/crm.contact.get/?auth={_accessToken}&id={contactId}");
            ContactResult contactResult = JsonConvert.DeserializeObject<ContactResult>(response);
            return contactResult.Result;
        }

        public string AddLead()
        {
            RefreshTokens();
            HttpWebRequest request = HttpClient.CreateWebRequest($"{ _portalUri}/rest/crm.lead.add?auth={_accessToken}");

            var data = new LeadCreateRequest
            {
                fields = new LeadDto { TITLE = "Hello", NAME = "Roma", OPPORTUNITY = 1233, LAST_NAME = "Павлович" }
            };
            var contentText = JsonConvert.SerializeObject(data);
            return HttpClient.RequestPost(request, contentText);
        }
    }

    public class LeadCreateRequest
    {
        public LeadDto fields { get; set; }
        public LeadCreateParameters @params { get; set; } = new LeadCreateParameters();
    }


    public class LeadCreateParameters
    {
        public string REGISTER_SONET_EVENT { get; set; } = "Y";
    }

    public class LeadsResult
    {
        public List<LeadDto> Result { get; set; }
    }

    public class ContactResult
    {
        public ContactDto Result { get; set; }
    }
}

