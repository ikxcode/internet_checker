using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace InternetChecker.Core
{
    public class ConnectionChecker
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly HttpClient client = new HttpClient();

        private bool _isCurrentConnectionStatusOK = true;
        private readonly IStatusChangeNotifier _notifier;


        public ConnectionChecker(IStatusChangeNotifier notifier)
        {
            _notifier = notifier;
        }


        public void SetStatusAndNotifyIfRequired(bool isNewConnectionStatusOK)
        {
            if (isNewConnectionStatusOK == _isCurrentConnectionStatusOK) return;

            _isCurrentConnectionStatusOK = isNewConnectionStatusOK;
            _notifier.Notify(isNewConnectionStatusOK);
        }


        private async Task<bool> MakeHttpCall(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    log.Info("Endpoint:  " + endpoint + ",  Status: " + response.StatusCode);
                    return true;
                }
                else
                {
                    log.Warn("Time -  local: " + DateTime.Now + ",  UTC: " + DateTime.UtcNow + ",  Unix Epoch: " + GetTimeUnixEpochOffset());
                    log.Warn("Endpoint:  " + endpoint + ",  Status: " + response.StatusCode);
                    return false;
                }
            }
            catch (HttpRequestException x)
            {
                log.Debug("HTTP Call Issue dtails ...", x);
                return false;
            }
        }


        private async Task<bool> SendPing(string ipAdress)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ipAdress);
                Ping pingSender = new Ping();
                PingReply r = await pingSender.SendPingAsync(ip);
                if (r.Status == IPStatus.Success)
                {
                    log.Info("Endpoint:  " + ipAdress + ",  Status: " + r.Status + ",  RoundTripTime: " + r.RoundtripTime);
                    return true;
                }
                else
                {
                    log.Warn("Time -  local: " + DateTime.Now + ",  UTC: " + DateTime.UtcNow + ",  Unix Epoch: " + GetTimeUnixEpochOffset());
                    log.Warn("Endpoint:  " + ipAdress + ",  Status: " + r.Status + ",  RoundTripTime: " + r.RoundtripTime);
                    return false;
                }
            }
            catch (PingException x)
            {
                log.Debug("HTTP Call Issue dtails ...", x);
                return false;
            }
        }


        private long GetTimeUnixEpochOffset()
        {
            TimeSpan timestamp = DateTime.UtcNow - DateTime.UnixEpoch;
            return Convert.ToInt64(timestamp.TotalSeconds);
        }


        private void ProcessResponses(bool[] responses)
        {
            bool ok = true;
            foreach (var r in responses)
            {
                if (!r)
                {
                    ok = false;
                    break;
                }
            }

            SetStatusAndNotifyIfRequired(ok);
        }


        public async Task Run()
        {
            while (true)
            {
                try
                {
                    log.Info("---");
                    log.Info("Time -  local: " + DateTime.Now + ",  UTC: " + DateTime.UtcNow + ",  Unix Epoch: " + GetTimeUnixEpochOffset());
                    var responseTasks = new List<Task<bool>>();
                    responseTasks.Add(MakeHttpCall("https://www.google.com/"));
                    responseTasks.Add(MakeHttpCall("http://216.58.211.164/"));
                    responseTasks.Add(SendPing("216.58.211.164"));
                    responseTasks.Add(SendPing("192.168.77.10"));
                    responseTasks.Add(SendPing("192.168.77.1"));
                    var responses = await Task.WhenAll(responseTasks);
                    ProcessResponses(responses);
                }
                catch (Exception x)
                {
                    log.Error("Could not make contact ...", x);
                    SetStatusAndNotifyIfRequired(false);
                }
                finally
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}
