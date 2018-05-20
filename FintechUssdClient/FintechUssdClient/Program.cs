using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace FintechUssdClient
{
    class Program
    {
        static String customerNo = "0112211122";
        static String merchantNo = "3333333333";
        static void Main(string[] args)
        {
            int portNumber = Convert.ToInt16(args[0]);

            Console.WriteLine("____________________Welcome To myPhone________________________");
            SimpleHTTPServer notificationHandler = new SimpleHTTPServer(portNumber);

            if (portNumber != 5500)
            {
                while (true)
                {
                    Console.WriteLine("Please Enter USSD Code:");
                    String inputStr = Console.ReadLine();

                    if (inputStr.Equals("*555*5#"))
                    {
                        Console.WriteLine("______________Initiate Transaction Menu_______________");
                        Console.WriteLine("Please enter customer voucher number followed by transaction amount(space delimited):");
                        String tranReqInput = Console.ReadLine();

                        if (tranReqInput.Length != 0)
                        {
                            String[] pars = tranReqInput.Split(' ');
                            String voucherNo = pars[0];
                            String tranAmount = pars[1];
                            Console.WriteLine("_______________________Transaction request sent___________________");
                            Console.WriteLine("Merchant Cell no: " + merchantNo);
                            Console.WriteLine("Customer Cell no:" + customerNo);
                            Console.WriteLine("Voucher number: " + voucherNo);
                            Console.WriteLine("Transaction amount: " + tranAmount);
                            GetAsync(voucherNo, tranAmount).Wait();

                        }
                        else
                        {
                            Console.WriteLine("Invalid Response.");
                        }
                    }

                    else
                    {
                        Console.WriteLine("Unsupported USSD.");
                    }

                }
            }

            
           
        }

        static async Task GetAsync(String voucherNo, String tranAmount)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new
                   Uri("http://localhost:8000");

                var results = await
                   httpClient.GetStringAsync("/startTransaction/" + tranAmount + "/" + merchantNo + "/" + voucherNo);
                Console.WriteLine("Results " + results);
            }
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
            string rsp = String.Empty;
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
                    Console.WriteLine(DateTime.Now +  " ______________Voucher received_____________");
                    if (queryItems.ContainsKey("data"))
                    {

                        String dataStr = queryItems["data"].Replace("\"\"", "'");
                        JObject data = (JObject)JObject.Parse(dataStr);
                        Console.WriteLine("voucher id: " + data.GetValue("VoucherId"));
                        Console.WriteLine("for customer phone number: " + data.GetValue("Owner"));
                        Console.WriteLine("from sender phone number: " + data.GetValue("sender"));
                        Console.WriteLine("Password: " + data.GetValue("PassKey"));
                        Console.WriteLine("amount: " + data.GetValue("Amount"));
                      
                    }
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else if (menuOption.Equals("5"))
                {
                    String voucherNo = "";
                    String merchantNo = "";
                    bool respStatus = false;
                    context.Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                   
                    Console.WriteLine(DateTime.Now + " ______________Transaction Request Received_____________");
                    if (queryItems.ContainsKey("data"))
                    {

                        String dataStr = queryItems["data"].Replace("\"\"", "'");
                        JObject data = (JObject)JObject.Parse(dataStr);
                        merchantNo = (String) data.GetValue("mechCellNo");
                        Console.WriteLine("From merchant phone number: " + merchantNo);
                        voucherNo = (String) data.GetValue("voucherNumber");
                        Console.WriteLine("Voucher number: " + voucherNo);
                        Console.WriteLine("amount: " + data.GetValue("amount"));
                    }
                    Console.WriteLine("Please confirm the transaction request by selecting the correct menu option below:");
                    Console.WriteLine("1. Approve");
                    Console.WriteLine("9. Reject");
                    String selectStr = Console.ReadLine();
                   
                    if (selectStr.Equals("1"))
                    {
                        Console.WriteLine("Please enter the password for voucher number " + voucherNo);
                        String password =  Console.ReadLine();
                        if (password.Length != 0)
                        {
                            Console.WriteLine(DateTime.Now + " ______________Sending Transaction Approval To Merchant "+ merchantNo + "_____________");
                            respStatus = true;
                            Console.WriteLine(DateTime.Now + " ______________Transaction Approval Sent To Merchant " + merchantNo + "_____________");
                        }
                    }
                    else if (selectStr.Equals("9"))
                    {
                        Console.WriteLine("Decline Option Selected. Transaction Request Not Authorized.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid Option. Transaction Request Not Authorized.");
                    }

                    if (respStatus)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        rsp = "ok";
                    }
                }

            }
            context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("{data:" + rsp + "}");
            // Get a response stream and write the response to it.
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
        }

        public System.Collections.Specialized.NameValueCollection query { get; set; }
    }
}