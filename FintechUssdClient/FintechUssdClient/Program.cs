using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json.Linq;

namespace FintechUssdClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to myPhone");
            SimpleHTTPServer notificationHandler = new SimpleHTTPServer(5500);

           
        }
    }

    class SimpleHTTPServer
    {
        private System.Threading.Thread _serverThread;
        private System.Net.HttpListener _listener;
        private int _port;

        public SimpleHTTPServer(int port)
        {
           Initialize(port);
        }

        private void Listen()
        {
            _listener = new System.Net.HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    System.Net.HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void Initialize(int port)
        {
            this._port = port;
            _serverThread = new System.Threading.Thread(this.Listen);
            _serverThread.Start();
        }

        private void Process(HttpListenerContext context)
        {
            string request = context.Request.Url.AbsolutePath;
            query = context.Request.QueryString;
            var items = query.AllKeys.SelectMany(query.GetValues, (k, v) => new { key = k, value = v });
            Dictionary<String, String> queryItems = new Dictionary<string,string>();

            foreach (var item in items)
            {
                queryItems.Add(item.key.ToString(), item.value.ToString());
            }
                

            if (queryItems.ContainsKey("menuOption"))
            {
                String menuOption = queryItems["menuOption"];

                if (menuOption.Equals("2"))
                {
                    Console.WriteLine("______________Voucher received_____________");
                    if (queryItems.ContainsKey("data"))
                    {

                        String dataStr = queryItems["data"].Replace("\"\"", "'");
                        JObject data = (JObject)JObject.Parse(dataStr);
                        Console.WriteLine("voucher id: " + data.GetValue("VoucherId"));
                        Console.WriteLine("for customer phone number: " + data.GetValue("Owner"));
                        Console.WriteLine("from sender phone number: " + data.GetValue("sender"));
                        Console.WriteLine("Password: " + data.GetValue("PassKey"));
                        Console.WriteLine("amount: " + data.GetValue("Amount"));
                        IList<string> keys = data.Properties().Select(p => p.Name).ToList();
                        
                    }
                }

            }
            //String menuOption = request.Substring();

            //Adding permanent http response headers
                string mime;
                context.Response.ContentLength64 = 0;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));

                byte[] buffer = new byte[1024 * 16];
                int nbytes;

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();

            
            context.Response.OutputStream.Close();
        }

        public System.Collections.Specialized.NameValueCollection query { get; set; }
    }
}
