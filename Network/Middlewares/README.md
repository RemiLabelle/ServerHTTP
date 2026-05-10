# Middleware de compression
> Appliquable à une instance de la classe **Router** ou **Route**.
> Gère la décompression d'une requête si nécessaire et compresse la réponse selon les besoins du client.

## Usage

Le middleware de compression, étant une implémentation de l'interface IMiddleware, peut être appliqué à un router ou à une route spécifique. Lorsqu'appliqué, celui-ci vient s'immiscer dans le traitement d'une requête pour effectuer sa décompression : Il vérifie d'abord l'en-tête "Transfer-Encoding" pour rapiécer<sup>1</sup> le corps de la requête s'il y a lieu, puis vérifie ensuite l'en-tête "Content-Encoding" pour reconnaitre l'algorithme de compression employé. La reconnaissance se fait à l'aide d'un dictionnaire associant le code textuel de la compression à un générateur de stream<sup>2</sup>. La décompression est enfin effectuée<sup>3</sup> avec le stream généré et la requête est reconstruite avec son corps en texte clair. Finalement, il poursuit la chaine d'exécution et retourne la réponse générée. D'autant plus, il gère son encodage si le client avait spécifé l'en-tête "Accept-Encoding" dans sa requête et gère son morcelage<sup>4</sup> pour optimiser le temps de réponse.

## Limites
- Certains algorithmes de compression comme zstd ne sont pas pris en compte ce qui peut corrompre le corps d'une réponse.
- Gère un seul algorithme de compression. Si Content-Encoding est multiple, cela peut corrompre le corps d'une requête.
- Compresse toujours les réponses, ce qui n'est pas toujours optimal quand le corps est plus court.

## Utils

<sup>1</sup> `Task<byte[]> DechunkBodyAsync(byte[] body)`\
Méthode utilisée en classe pour organiser le corps d'une requête morcelée. Détecte la taille de chaque morceau du corps pour les recevoir progressivement.

<sup>2</sup> `delegate Stream GenerateCompressionStream(Stream stream, CompressionMode direction)`\
Délègue l'instanciation du stream de compression. Évite d'avoir une méthode pour chaque type de stream.

<sup>3</sup> `Task<byte[]> HandleCompressionAsync(byte[] body, GenerateCompressionStream streamBuilder, CompressionMode direction)`\
Méthode utilisée pour décompresser les requêtes et compresser les réponses selon le paramètre de mode de compression. Utilise le delegate de génération de stream de compression pour instancier le stream approprié.

<sup>4</sup> `Task<byte[]> ChunkBodyAsync(byte[] body, int chunkSize)`\
Divise le corps d'une réponse selon le paramètre de taille de morceau pour optimiser l'envoi de ressources au client.