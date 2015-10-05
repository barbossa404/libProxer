/**
 *  Diese Datei ist ein Teil von libProxer. 
 *  libProxer ist ein .NET-Interface für http://www.proxer.me und steht in keiner Verbindung mit Proxer.
 * 
 *  Verwendung auf eigene Gefahr. Für die Verwendung in eigenen Anwendungen gelten die Nutzungsbedingungen von Proxer und der Proxer API
 *  (zu finden unter http://proxer.me/impressum?s=nutzungsbedingungen bzw. http://proxer.me/wiki/Proxer_API) und die Bedingungen der MIT-Lizenz. Siehe LICENSE.
 *
 * */

using System;
using System.Globalization;
using System.IO;
using System.Net;

namespace libProxer
{
    public class Anime
    {
        public enum AnimeStatus
        {
            INVALID,
            PreAiring,
            Airing,
            Abgeschlossen,
            Abgebrochen,
            NichtFertiggesubbt
        }

        public enum AnimeMedium
        {
            AnimeSerie,
            Film,
            OVA,
            Hentai
        }

        public struct Rating
        {
            public float score;
            public int votes;
        }

        public Anime(int _ID)
        {
            ID = _ID;
        }

        public void getInfo()
        {
            /*
                Diese Funktion ist eine behelfsmäßige Lösung, die die Infos direkt aus dem HTML ließt, also vermutlich bei einem 
                Update von Proxer gnadenlos kaputtgehen wird. Leider gibt es bisher keinen angenehmeren Weg an die Infos zu kommen.
                
                Scary Stringparsing ahead
             */

            // HTML der Detailsseite laden
            string pageHTML = Network.loadURL("https://proxer.me/info/" + ID);

            // Schritt für Schritt Informationen erfassen, dabei werfen wir alles weg, was nicht mehr gebraucht wird
            pageHTML = pageHTML.Substring(pageHTML.IndexOf("class=\"rating\">") + 15);

            // 1. Rating
            {
                string ratingString = pageHTML.Substring(0, pageHTML.IndexOf("Stimmen"));
                pageHTML = pageHTML.Substring(ratingString.Length + 10);

                rating = new Rating();
                ratingString = ratingString.Substring(23);

                int votesCountOffset = (ratingString.IndexOf("votes") + 7);
                if (!float.TryParse(ratingString.Substring(0, ratingString.IndexOf("<")), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out rating.score)
                    ||
                    !int.TryParse(
                        ratingString.Substring(votesCountOffset, ratingString.Length - 8 - votesCountOffset),
                        out rating.votes))
                    throw new InvalidDataException();
            }

            // 2. Titel
            {
                string titelAndSynonyms = pageHTML.Substring(pageHTML.IndexOf("Original") + 28,
                    pageHTML.IndexOf("Genre") - pageHTML.IndexOf("Original") + 28);
            }
        }


        /*
            Informationen zum Anime
         */
        public int ID;
        public string originalTitel;
        public string japTitel;
        public string[] titelSynonyme;
        
        public string[] genres;
        public int fsk;

        public AnimeStatus status;
        public AnimeMedium medium;
        public bool licensed;

        public string description;
        public string CoverURL;

        public Rating rating;
    }
}
