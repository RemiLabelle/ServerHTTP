namespace Publication;

public class Publication(int id, string title, string content)
{
    public int Id { get; set; } = id;
    public string Title { get; set; } = title;
    public string Content { get; set; } = content;
}
