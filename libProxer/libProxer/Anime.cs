/**
 *  Diese Datei ist ein Teil von libProxer. 
 *  libProxer ist ein .NET-Interface für http://www.proxer.me und steht in keiner Verbindung mit Proxer.
 * 
 *  Verwendung auf eigene Gefahr. Für die Verwendung in eigenen Anwendungen gelten die Nutzungsbedingungen von Proxer und der Proxer API
 *  (zu finden unter http://proxer.me/impressum?s=nutzungsbedingungen bzw. http://proxer.me/wiki/Proxer_API) und die Bedingungen der MIT-Lizenz. Siehe LICENSE.
 *
 * */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

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

        public enum Genre
        {
            Unbekannt,
            Abenteuer,
            Drama,
            Horror,
            Military,
            School,
            Shounen,
            Superpower,
            Action,
            Ecchi,
            Josei,
            Musik,
            SciFi,
            Shounen_Ai,
            Vampire,
            Adult,
            Fantasy,
            Magic,
            Mystery,
            Seinen,
            Slice_of_Life,
            Violence,
            Comedy,
            Harem,
            MartialArt,
            Psychological,
            Shoujou,
            Splatter,
            Yaoi,
            Cyberpunk,
            Historical,
            Mecha,
            Romance,
            Shoujou_Ai,
            Sport,
            Yuri
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

            #region 1. Rating
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
            #endregion

            #region 2. Titel
            {
                int titleStartOffset = pageHTML.IndexOf("Original") + 28;

                // HTML bis zum Anfang der Genreliste ausschneiden
                string titelAndSynonyms = pageHTML.Substring(titleStartOffset,
                    pageHTML.IndexOf("Genre") - titleStartOffset);

                // Extrahierten Teil aus der Gesamtmenge entfernen
                pageHTML = pageHTML.Substring(titleStartOffset + titelAndSynonyms.Length);

                String[] t = titelAndSynonyms.Split(new string[]{"<tr>\n<td><"}, StringSplitOptions.RemoveEmptyEntries);

                if (t.Length < 2)
                    throw new InvalidDataException();

                // Titel wird zuerst extrahiert
                originalTitel = t[0].Substring(0, t[0].IndexOf("<"));
                japTitel = t[1].Substring(26, t[1].Length - 38);

                // Eventuell folgen nun noch mehrere Synonyme für den Anime
                if (t.Length > 2)
                {
                    titelSynonyme = new string[t.Length - 2];

                    for (int i = 2; i < t.Length - 1; i++)
                    {
                        titelSynonyme[i - 2] = t[i].Substring(23, t[i].Length -35);
                    }

                    int index = titelSynonyme.Length - 1;
                    titelSynonyme[index] = t[index].Substring(23, t[index].IndexOf("</td>", 23) - 23);
                }
            }
            #endregion
            
            #region 3. Genre
            {
                // Liste der Genres extrahieren
                int bereichsEnde = pageHTML.IndexOf("FSK");
                string genreString = pageHTML.Substring(pageHTML.IndexOf("</a>"), bereichsEnde);
                var t = genreString.Split(new string[] {"</a>"}, StringSplitOptions.RemoveEmptyEntries);

                // Set der Genres anlegen
                genres = new HashSet<Genre>();

                for (int i = 0; i < t.Length - 1; i++)
                {
                    
                    Genre currentGenre;
                    if (!Enum.TryParse(t[i].Substring(t[i].IndexOf("\">") + 2).Replace('-', '_'), out currentGenre))
                        currentGenre = Genre.Unbekannt;

                    // Kleine Debughilfe, wird bei Release-Builds nie ausgeführt
                    Debug.WriteLine("Parse Genre: " + t[i].Substring(t[i].IndexOf("\">") + 2).Replace('-', '_') + " ist " + currentGenre);

                    genres.Add(currentGenre);
                }
            }
            #endregion

        }


        /*
            Informationen zum Anime
         */
        public int ID;
        public string originalTitel;
        public string japTitel;
        public string[] titelSynonyme;
        
        public HashSet<Genre> genres;
        public int fsk;

        public AnimeStatus status;
        public AnimeMedium medium;
        public bool licensed;

        public string description;
        public string CoverURL;

        public Rating rating;
    }
}
