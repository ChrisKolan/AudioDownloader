using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    internal static class PingUtility
    {
        static long _roundtripTime;

        public static bool Pinger(out string pingException)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            var hostNameOrAddress = "youtube.com";

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 250;
            PingReply reply;

            try
            {
                reply = pingSender.Send(hostNameOrAddress, timeout, buffer, options);
            }
            catch (PingException exception)
            {
                pingException = exception.Message;
                pingSender.Dispose();
                _roundtripTime = -1;
                return false;
            }
            if (reply.Status != IPStatus.Success)
            {
                //Console.WriteLine("Address: {0}", reply.Address.ToString());
                //Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                //Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                //Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                //Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                pingException = "Sending of an Internet Control Message Protocol not successful";
                _roundtripTime = -1;
                pingSender.Dispose();
                return false;
            }
            pingException = "Sending of an Internet Control Message Protocol successful";
            _roundtripTime = reply.RoundtripTime;
            pingSender.Dispose();
            return true;
        }

        public static string AddPingToHelpButtonToolTip(string helpButtonToolTip)
        {
            var helpButtonToolTipList = helpButtonToolTip.Split( new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            var status = (_roundtripTime == -1) ? "Offline" : "Online";
            var pingData = new List<string>
            {
                "===========================",
                "Status \t\t   |\t" + status,
                "Roundtrip time \t   |\t" + _roundtripTime,
            };
            var combinedList = helpButtonToolTipList.Concat(pingData).ToList();

            if (combinedList.Count > 12)
            {
                combinedList.RemoveRange(8, 3);
            }

            return string.Join(Environment.NewLine, combinedList.ToArray());
        }
    }
}
