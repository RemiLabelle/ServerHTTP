namespace Publication;

public class PublicationDatabase
{
    private Dictionary<string, List<Publication>> _publications;
    private int lastId = 0;

    public PublicationDatabase()
    {
        _publications = new();
    }

    public PublicationDatabase(Dictionary<string, List<Publication>> publications)
    {
        _publications = publications;
    }

    public Publication AddPublication(string username, string title, string content)
    {
        if (!_publications.ContainsKey(username))
        {
            _publications.Add(username, new List<Publication>());
        }

        Publication publication = new Publication(++lastId, title, content);
        _publications[username].Add(publication);
        return (Publication)publication.Clone();
    }

    public List<Publication>? GetPublicationsFromUsername(string username)
    {
        return _publications.TryGetValue(username, out var value) ? value : null;
    }

    public List<Publication> All()
    {
        return _publications.Values
            .SelectMany(p => p)
            .ToList();
    }
}
