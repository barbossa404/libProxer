/**
 *  Diese Datei ist ein Teil von libProxer. 
 *  libProxer ist ein .NET-Interface für http://www.proxer.me und steht in keiner Verbindung mit Proxer.
 * 
 *  Verwendung auf eigene Gefahr. Für die Verwendung in eigenen Anwendungen gelten die Nutzungsbedingungen von Proxer und der Proxer API
 *  (zu finden unter http://proxer.me/impressum?s=nutzungsbedingungen bzw. http://proxer.me/wiki/Proxer_API) und die Bedingungen der MIT-Lizenz. Siehe LICENSE.
 *
 * */

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace libProxer
{
    public class News
    {
        private ArticleList articles;

        public News()
        {
            articles = new ArticleList();
        }

        public IEnumerable<Article> getNews()
        {
            return articles;
        }
    }

    public class Article
    {
        // NachrichtenID des Artikels
        public string articleID;

        // Der Titel
        public string title;
        
        // Eine kurze Beschreibung
        public string description;

        // Benutzername des Autors
        public string author;

        // Kategorie, in die der Artikel eingeordnet wird
        public string category;

        // Zeitpunkt der Veröffentlichung als Unix-Timestamp
        public string timestamp;                
        
        // Zahl der Aufrufe des Artikels
        public string views;

        // Link zum Forenthread
        public string threadURL;

        // Link zu Thumbnail des Titelbildes zum Artikel
        public string imageURL;
    }

#region Lazyload-Liste hier implementiert

    public class ArticleList : IEnumerable<Article>
    {
        public IEnumerator<Article> GetEnumerator()
        {
            // todo: ArticleEnumerator zwischenspeichern und nicht bei jedem Aufruf einen neuen anlegen!
            return new ArticleEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ArticleEnumerator : IEnumerator<Article>
    {
        // Warum zur Hölle heißen eigentlich Nachrichten "notifications"? Eine Trennung von Benachrichtigung wäre nett, irgendjemand ist garantiert eines Tages
        // sehr verwirrt darüber
        private struct NotificationsEntry
        {
            public string nid;
            public string time;
            public string description;
            public string image_id;
            public string subject;
            public string hits;
            public string thread;
            public string uname;
            public string catname;
            public string catid;
        }

        private struct ArticleResponse
        {
            public int error;
            public NotificationsEntry[] notifications;
        }


        private List<Article> currentArticles;
        private int currentPage;
        private int currentNewsPosition;

        public ArticleEnumerator()
        {
            Current = null;
            currentPage = 0;
            currentNewsPosition = -1;
            currentArticles = new List<Article>(15);
        }

        public Article Current { get; private set; }
        object IEnumerator.Current {
            get { return Current;  }
        }

        public void Dispose()
        {
            // Liste der aktuell geladenen Artikel verwerfen
            currentArticles.Clear();
            currentArticles = null;
            Reset();
        }

        public void Reset()
        {
            Current = null;
            currentPage = 0;
            currentNewsPosition = -1;
        }

        public bool MoveNext()
        {
            if (currentNewsPosition == 14 || currentNewsPosition < 0)
            {
                // An den Anfang der nächsten Seite springen
                currentPage++;
                currentNewsPosition = -1;

                // Geladene Artikel verwerfen
                currentArticles.Clear();

                // Nächste Seite laden und auswerten
                string newsJSON = Network.loadURL("https://proxer.me/notifications?format=json&s=news&p=" + currentPage);

                ArticleResponse t = JsonConvert.DeserializeObject<ArticleResponse>(newsJSON);

                if (t.error != 0)
                    return false;

                foreach (NotificationsEntry entry in t.notifications)
                {
                    Article article = new Article();
                    article.articleID = entry.nid;
                    article.author = entry.uname;
                    article.category = entry.catname;
                    article.description = entry.description;
                    article.imageURL = "http://cdn.proxer.me/news/" + entry.nid + "_" + entry.image_id + ".png";
                    article.threadURL = "https://proxer.me/forum/" + entry.catid + "/" + entry.thread;
                    article.timestamp = entry.time;
                    article.title = entry.subject;
                    article.views = entry.hits;

                    currentArticles.Add(article);
                }
            }

            currentNewsPosition++;
            Current = currentArticles[currentNewsPosition];

            return true;
        }
    }
#endregion
}
