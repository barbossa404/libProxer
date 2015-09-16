/**
 *  Diese Datei ist ein Teil von libProxer. 
 *  libProxer ist ein .NET-Interface für http://www.proxer.me und steht in keiner Verbindung mit Proxer.
 * 
 *  Verwendung auf eigene Gefahr. Für die Verwendung in eigenen Anwendungen gelten die Nutzungsbedingungen von Proxer und der Proxer API
 *  (zu finden unter http://proxer.me/impressum?s=nutzungsbedingungen bzw. http://proxer.me/wiki/Proxer_API) und die Bedingungen der MIT-Lizenz. Siehe LICENSE.
 *
 * */

using System;
using System.Linq;
using libProxer;

namespace Demo
{
    class Program
    {
        /*
         * Das hier ist nur ein Demoprogramm, 
         * NIEMALS JEMALS AUCH NUR DRAN DENKEN EIN PASSWORT IM KLARTEXT HIER ZU PLATZIEREN!
         */
        public const string userName = "Test";
        public const string password = "passwort für test";

        static void Main(string[] args)
        {
            /**
             * Alle Teile des APIs, die einen angemeldeten Benutzer erfordern, werden auf die Account-Klasse zurückgreifen.
             * Sitzungsverwaltung erfolgt hier.
             */
            Account nutzer = new Account(userName);

            // Der erstellte Benutzer wird eingeloggt:
            nutzer.login(password);

            // Als Beweis für einen erfolgreichen Login fragen wir unseren Sitzungsstatus bei Proxer an. Der Parameter true
            // erzwingt hier eine Anfrage an Proxer. Im Normalfall ist diese überflüssig, da sich der Sitzungsstatus nur selten
            // ändert.
            Console.WriteLine("Der Benutzer ist angemeldet? --> " + nutzer.isLoggedIn());

            // Am Ende können wir die Sitzung des Nutzers auch wieder beenden, dies ist normalerweise überflüssig, da Sitzungen nach
            // 60 Minuten ablaufen. Allerdings ist ein explizites Abmelden nie eine schlechte Idee, also bleibt hier die Möglichkeit.
            nutzer.logout();

            if (!nutzer.updateNotifications())
                Console.WriteLine("Ungelesene Nachrichten verfügbar!");
            if (nutzer.notifications.news > 0)
                Console.WriteLine("Neue News verfügbar: " + nutzer.notifications.news);

            /*
                libProxer unterstützt die Nachrichten von Proxer über die News-Klasse. Darin enthalten ist die Funktion getNews(), die eine Liste der Artikel
                zurückliefert. Diese ist implementiert als "Lazy-Load", lädt also die Artikel erst, wenn sie angefragt werden (in 15 Artikel Blöcken).
             */
            
            // Nachrichtenliste erhalten
            News proxerNews = new News();
            var nachrichten = proxerNews.getNews();
            
            // Wir werden nur die ersten zehn Nachrichten verwenden, ein Iterieren durch die Liste aller Nachrichten mit foreach-Schleife ist
            // möglich, aber nicht empfohlen. (Wie weit gehen die Proxer-Nachrichten zurück, die abrufbar sind? Keine Ahnung...)
            nachrichten = nachrichten.Take(10);

            foreach (Article article in nachrichten)
            {
                Console.WriteLine(article.author + " schreibt: " + article.title);
            }


            Console.ReadKey();
        }
    }
}
