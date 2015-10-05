/**
 *  Diese Datei ist ein Teil von libProxer. 
 *  libProxer ist ein .NET-Interface für http://www.proxer.me und steht in keiner Verbindung mit Proxer.
 * 
 *  Verwendung auf eigene Gefahr. Für die Verwendung in eigenen Anwendungen gelten die Nutzungsbedingungen von Proxer und der Proxer API
 *  (zu finden unter http://proxer.me/impressum?s=nutzungsbedingungen bzw. http://proxer.me/wiki/Proxer_API) und die Bedingungen der MIT-Lizenz. Siehe LICENSE.
 *
 * */

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace libProxer
{
    public static class Network
    {
        public static string loadURL(string url, CookieContainer sessionCookie = null)
        {
            return loadURLPost(url, null, sessionCookie);
        }

        public static string loadURLPost(string url, Dictionary<string,string> postParams, CookieContainer sessionCookie = null)
        {
            // Request erzeugen
            HttpWebRequest http = (HttpWebRequest)HttpWebRequest.Create(url);
            
            // Wenn es einen Cookie gibt, setzen wir ihn
            if (sessionCookie != null)
                http.CookieContainer = sessionCookie;

            if (postParams != null)
            {
                // Zusammensetzen der Postparameter zu einem Datenblock
                StringBuilder postParamBuilder = new StringBuilder();
                foreach (string key in postParams.Keys)
                {
                    postParamBuilder.Append(
                        urlEncode(key)
                        + "=" +
                        urlEncode(postParams[key]) + "&");
                }

                byte[] dataBytes = StringToAscii(postParamBuilder.ToString());
                //http.Headers["Content-Length"] = dataBytes.Length.ToString();
                // todo: remove or fix


                // Signalisieren, dass wir Daten mit POST hochladen wollen
                http.Method = "POST";
                http.ContentType = "application/x-www-form-urlencoded";

                // Verbindung zum Server aufbauen und Requeststream erhalten
                var requestStreamAsync = http.GetRequestStreamAsync();
                if (requestStreamAsync.Wait(3000) == false)
                    throw new IOException("Failed to send request for url " + url);

                var requestStream = (Stream) requestStreamAsync.Result;

                // Daten senden
                requestStream.Write(dataBytes, 0, dataBytes.Length);
                requestStream.Flush();
                requestStream.Dispose();
            }
            else
                http.Method = "GET";

            // Antwort des Servers erfassen. Benutzung der Asynchronen-Methoden erforderlich um Kompabilität
            // zu allen Zielplattformen aufrechtzuerhalten
            var asyncResponse = http.GetResponseAsync();
            if (asyncResponse.Wait(3000) == false)
                throw new IOException("No response for url " + url);

            // String lesen und zurückliefern
            using (StreamReader sr = new StreamReader(asyncResponse.Result.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        private static string urlEncode(string data)
        {
            /*
                   Das Längenlimit für Strings liegt bei Uri.EscapeDataString bei 32766 (Dokumentation) bzw. 65519 (Windows 8, undokumentiert),
                   also wird der String bei Überlänge aufgeteilt.
             */
            if (data.Length > 32700)
            {
                StringBuilder t = new StringBuilder(data.Length);
                int blocks = data.Length / 32700;

                for (int i = 0; i <= blocks; i++)
                {
                    if (i < blocks)
                        t.Append(System.Uri.EscapeDataString(data.Substring(32700 * i, 32700)));
                    else
                    {
                        t.Append(System.Uri.EscapeDataString(data.Substring(32700 * i)));
                    }
                }

                return t.ToString();
            }
            else
            {
                return System.Uri.EscapeDataString(data);
            }
        }

        /*
            Von https://stackoverflow.com/questions/4022281/asciiencoding-in-windows-phone-7, als Ersatz für
            Encoding.ASCII.GetBytes(), was nicht auf auf allen Zielplatformen unterstützt wird.
         */
        private static byte[] StringToAscii(string s)
        {
            byte[] retval = new byte[s.Length];
            for (int ix = 0; ix < s.Length; ++ix)
            {
                char ch = s[ix];
                if (ch <= 0x7f) retval[ix] = (byte)ch;
                else retval[ix] = (byte)'?';
            }
            return retval;
        }
    }
}