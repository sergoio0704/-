using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Web;
using System;

namespace LeadProxy
{
    class Bitrix24
    {
        private const string BX_ClientID = "local.5e85c27b09fac2.62891485"; 
        private const string BX_ClientSecret = "6lhHhp0nAFnVEcVL9B1822D6kKgbgpOKdhvRBg2Qf4NfYpQBB5"; 
        private const string BX_Portal = "https://b24-yqgspf.bitrix24.ru"; 
        private const string BX_OAuthSite = "https://oauth.bitrix.info"; 

        //Служебные поля

        private string AccessToken;
        private string RefreshToken;
        private DateTime RefreshTime;
        private string Code;
        private string Cookie;

        public Bitrix24()

        {

            Connect();

        }

        public void Connect()

        {
            string BX_URI = BX_Portal + "/oauth/authorize/?client_id=" + BX_ClientID;
            HttpWebRequest requestLogonBitrix24 = (HttpWebRequest)WebRequest.Create(BX_URI);
            string username = "serega1986-16@mail.ru";
            string password = "serg200189877123336";
            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            requestLogonBitrix24.Headers.Add("Authorization", "Basic " + svcCredentials);
            requestLogonBitrix24.AllowAutoRedirect = false; 
            requestLogonBitrix24.Method = "POST";
            HttpWebResponse responseLogonBitrix24 = (HttpWebResponse)requestLogonBitrix24.GetResponse();
            if (responseLogonBitrix24.StatusCode == HttpStatusCode.Found)
            {
                Uri locationURI = new Uri(responseLogonBitrix24.Headers["Location"]);
                // Ловко парсим URL-адрес с помощью HttpUtility, подключите "System.Web" через пакеты NuGet
                var locationParams = System.Web.HttpUtility.ParseQueryString(locationURI.Query);
                Cookie = responseLogonBitrix24.Headers["Set-Cookie"];
                Code = locationParams["Code"];

                if (String.IsNullOrEmpty(Code))
                {
                    throw new FormatException("CodeNotFound");
                }
                //Закрываем подключение
                responseLogonBitrix24.Close();
                //Если код успешно получили, то формируем новый HTTP запрос для получения Токенов авторизации
                string BX_OAuth_URI = BX_OAuthSite + "/oauth/token" + "/?" + "grant_type=authorization_code" + "&" +
                "client_id=" + BX_ClientID + "&" +
                "client_secret=" + BX_ClientSecret + "&" +
                "code=" + Code;
                SetToken(BX_OAuth_URI);
            }
        }
        private void SetToken(string BX_OAuth_URI)
        {
            if (BX_OAuth_URI is null)
            {
                throw new ArgumentNullException(nameof(BX_OAuth_URI));
            }
          //Формируем новый HTTP запрос для получения Токенов авторизации
            HttpWebRequest requestLogonBitrixOAuth = (HttpWebRequest)WebRequest.Create(BX_OAuth_URI);
            requestLogonBitrixOAuth.Method = "POST";
            requestLogonBitrixOAuth.Headers["Cookie"] = Cookie; //Используем Куки полученный в предыдущем запросе авторизации
            //Подключаемся (отправляем запрос)
            HttpWebResponse responseLogonBitrixOAuth = (HttpWebResponse)requestLogonBitrixOAuth.GetResponse();
            //Если в ответ получаем статус-код отличный от 200, то это ошибка, вызываем исключение
            if (responseLogonBitrixOAuth.StatusCode != HttpStatusCode.OK)
            {
                throw new FormatException("ErrorLogonBitrixOAuth");
            }
            else
            {
                //Читаем тело ответа
                Stream dataStreamLogonBitrixOAuth = responseLogonBitrixOAuth.GetResponseStream();
                var readerLogonBitrixOAuth = new StreamReader(dataStreamLogonBitrixOAuth);
                string stringLogonBitrixOAuth = readerLogonBitrixOAuth.ReadToEnd();
                //Обязательно закрываем подключения и потоки
                readerLogonBitrixOAuth.Close();
                responseLogonBitrixOAuth.Close();
                //Ловко преобразуем тело ответа в формате JSON в .Net объект с помощью Newtonsoft.Json, не забудьте подключить Newtonsoft.Json через NuGet
                var converter = new ExpandoObjectConverter();
                dynamic objLogonBitrixOAuth = JsonConvert.DeserializeObject<ExpandoObject>(stringLogonBitrixOAuth, converter);
                //Записывем Токены авторизации в поля нашего класса из динамического объекта
                AccessToken = objLogonBitrixOAuth.access_token;
                RefreshToken = objLogonBitrixOAuth.refresh_token;
                RefreshTime = DateTime.Now.AddSeconds(objLogonBitrixOAuth.expires_in); //Добавляем к текущей дате количество секунд действия токена, обычно это плюс один час
                //Закрываем поток
                dataStreamLogonBitrixOAuth.Close();
            }
        }
        private void RefreshTokens()

        {
            if (RefreshTime == DateTime.MinValue) // Если RefreshTime пустая
            {
                //Тогда вызываем авторизацию по полной программе
                Connect();
                return; //Тогда дальше не идём
            }
            //Проверяем, если истекло время действия Токена авторизации, то обновляем его
            if (RefreshTime.AddSeconds(-5) < DateTime.Now)
            {
                //Формируем новый HTTP запрос для обновления Токена авторизации, здесь Code уже не нужен
                string BX_OAuth_URI = BX_OAuthSite + "/oauth/token" + "/?" + "grant_type=refresh_token" + "&" +
                "client_id=" + BX_ClientID + "&" +
                "client_secret=" + BX_ClientSecret + "&" +
                "refresh_token=" + RefreshToken;
                SetToken(BX_OAuth_URI);
            }
        }
        //Открытый метод для отправки REST-запросов в Битрикс24
        public string SendCommand(string Command, string Params = "", string Body = "")
        {
            //Проверяем и обновлем Токены авторизации
            RefreshTokens();
            //Проверяем возможное указание параметров
            string BX_REST_URI = "";
            if (String.IsNullOrEmpty(Params))
                BX_REST_URI = BX_Portal + "/rest/" + Command + "?auth=" + AccessToken;
            else
                BX_REST_URI = BX_Portal + "/rest/" + Command + "?auth=" + AccessToken + "&" + Params;
            //Создаем новое HTTP подключение для отправки REST-запроса в Битрикс24
            HttpWebRequest requestBitrixREST = (HttpWebRequest)WebRequest.Create(BX_REST_URI);
            requestBitrixREST.Method = "POST";
            requestBitrixREST.Headers["Cookie"] = Cookie; //Используем Куки полученный в запросе авторизации
            //Готовим тело запроса и вставляем его в тело POST-запроса
            byte[] byteArrayBody = Encoding.UTF8.GetBytes(Body);
            requestBitrixREST.ContentType = "application/x-www-form-urlencoded";
            requestBitrixREST.ContentLength = byteArrayBody.Length;
            Stream dataBodyStream = requestBitrixREST.GetRequestStream();
            dataBodyStream.Write(byteArrayBody, 0, byteArrayBody.Length);
            dataBodyStream.Close();
            //Отправляем данные в Битрикс24
            HttpWebResponse responseBitrixREST = (HttpWebResponse)requestBitrixREST.GetResponse();
            //Читаем тело ответа от Битрикс24
            Stream dataStreamBitrixREST = responseBitrixREST.GetResponseStream();
            var readerBitrixREST = new StreamReader(dataStreamBitrixREST);
            string stringBitrixREST = readerBitrixREST.ReadToEnd();
            //Закрываем все подключения и потоки
            readerBitrixREST.Close();
            dataStreamBitrixREST.Close();
            responseBitrixREST.Close();
            //Возвращаем строку ответа в формате JSON
            return stringBitrixREST;
        }

    }
}

