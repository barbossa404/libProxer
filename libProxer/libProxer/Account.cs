/**
 *  Diese Datei ist ein Teil von libProxer. 
 *  libProxer ist ein .NET-Interface für http://www.proxer.me und steht in keiner Verbindung mit Proxer.
 * 
 *  Verwendung auf eigene Gefahr. Für die Verwendung in eigenen Anwendungen gelten die Nutzungsbedingungen von Proxer und der Proxer API
 *  (zu finden unter http://proxer.me/impressum?s=nutzungsbedingungen bzw. http://proxer.me/wiki/Proxer_API) und die Bedingungen der MIT-Lizenz. Siehe LICENSE.
 *
 * */


using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace libProxer
{

    public static class Account
    {
        struct LoginResponse
        {
            public int error;
            public string uid;
            public string avatar;
        }

        public struct Notifications
        {
            public int privateNachrichten;
            public int freundschaftsAnfragen;
            public int news;
            public int allgemeineBenachrichtigungen;
        }

        // Benutzername auf Proxer
        public static string userName;

        // Benachrichtigungen für den aktuellen Benutzer.
        // Vorsicht: Wird nicht automatisch aktualisiert, dafür ist ein Aufruf von updateNotifications() notwendig.
        public static Notifications notifications;


        private static bool _loggedIn = false;
        private static string _uid;
        private static CookieContainer _sessionCookie;

        static Account()
        {
            userName = null;
            _sessionCookie = new CookieContainer();
        }

        /**
         * Meldet den Benutzer bei Proxer an. Eine Sitzung ist 60 Minuten gültig.
         * @param password: Passwort. Wird nicht innerhalb eines Accounts gespeichert.
         * */
        public static bool login(string _userName, string _password)
        {
            // Benutzername übernehmen
            userName = _userName;

            // Anfrage zusammenstellen
            var postParameter = new Dictionary<string, string>();
            postParameter.Add("username", userName);
            postParameter.Add("password", _password);

            string jsonResponse = Network.loadURLPost("https://proxer.me/login?format=json&action=login", postParameter,
                _sessionCookie);

            // Antwort auswerten
            LoginResponse antwort = JsonConvert.DeserializeObject<LoginResponse>(jsonResponse);

            if (antwort.error != 0)
            {
                // todo: Fehler mit mehr Details melden
                return false;
            }

            _uid = antwort.uid;

            return true;
        }

        /**
         * Liefert, ob der Benutzer angemeldet ist.
         * @param forceOnline: Prüft den Zustand der Sitzung bei Proxer,
         *                     sonst wird nur der interne Status berücksichtigt.
         *                     
         * Der interne Status ist vorzuziehen, wenn diese Funktion häufiger aufgerufen wird.
         * (Onlineprüfung erfolgt synchron und kann Verzögerungen nach sich ziehen.)
         * 
         */
        public static bool isLoggedIn(bool forceOnline = true)
        {
            if (forceOnline)
            {
                string jsonResponse = Network.loadURL("https://proxer.me/login?format=json&action=login", _sessionCookie);

                // Antwort aus JSON zurückwandeln
                LoginResponse antwort = JsonConvert.DeserializeObject<LoginResponse>(jsonResponse);

                // Wenn kein Fehler gemeldet wurde, ist die Sitzung noch gültig
                // Der interne Status wird mit aktualisiert
                return (_loggedIn = (antwort.error == 0));
            }

            return _loggedIn;
        }

        /**
         * Beendet die Sitzung eines Benutzers. True wird zurückgeliefert, wenn die Sitzung beendet werden
         * konnte oder bereits abgelaufen war.
         * 
         */
        public static bool logout()
        {
            // todo: Etwas machen hier^^
            _sessionCookie = null;
            return true;
        }

        /**
         * Prüft auf neue Benachrichtigungen und liefert true zurück, wenn bisher ungelesene Benachrichtigungen
         * vorliegen. Ein Zugriff auf die geladenen Benachrichtigungen ist möglich über Account.Notifications.
         * Vorsicht: Diese werden nicht automatisch aktualisiert! 
         */
        public static bool updateNotifications()
        {
            string response = Network.loadURL("https://proxer.me/notifications?format=raw&s=count", _sessionCookie);

            // Ein Fehler ist aufgetreten, die Sitzung ist nicht mehr gültig!
            if (response == "1")
            {
                // todo: Mehr Fehlerinformationen zur Verfügung stellen, nicht einfach still fehlschlagen
                _loggedIn = false;
                return false;
            }

            // Benachrichtigungen übernehmen
            Notifications t = new Notifications();
            var parts = response.Split('#');

            // todo: API-Fehler zurückmelden
            // Eine Exception hier würde nie durch eine Client-Anwendung abgefangen werden und im Zweifelsfall einfach zu
            // einem für den User unersichtlichen Crash der Anwendung führen...
            if (parts.Length != 6)
                return false;

             if (!(int.TryParse(parts[2], out t.privateNachrichten) 
                 && int.TryParse(parts[3], out t.freundschaftsAnfragen) 
                 && int.TryParse(parts[4], out t.news) 
                 && int.TryParse(parts[5], out t.allgemeineBenachrichtigungen)))
                    return false;

            notifications = t;

            return (t.allgemeineBenachrichtigungen + t.freundschaftsAnfragen + t.news + t.privateNachrichten) > 0;
        }
    }
}
