# libProxer
Inoffizielle C#/.NET-Implementation der Proxer.Me API

Der im Wiki (https://proxer.me/wiki/Proxer_API) Teil der API wird voll unterstützt, weitere (undokumentierte) Teile füge ich
Laufe der Zeit hinzu.

*Benutzung:*
Verweis auf libProxer hinzufügen zum Projekt. Für ein Walkthrough der Verwendung siehe Demo-Anwendung im Unterverzeichnis Demo.

*Features*:
 - Auslesen der Benachrichtigungen
 - Nachrichten mit Links zu Threads und Titelbildern
 - Accountverwaltung mit Login/Logout
 
 Beispiel: 

```
void printNews()
{
  // Iterator (Liste) der Nachrichtenartikel erhalten
  News news = new News();
  var nachrichten = news.getNews();
  
  // Wir werden nur die ersten 10 Nachrichten ausgeben:
  nachrichten = nachrichten.Take(10);
  
  // Nachrichten ausgeben
  foreach (Article t in nachrichten)
  {
    Console.WriteLine(t.author + " schreibt " + t.title);
  }
}
```
