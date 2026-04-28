using Network.HttpMessages;

namespace Network.Routing;

public class RequestContext
{
    public RequestContext(Request request, string prefix)
    {
        Request = request;
        Prefix = prefix;
    }

    public RequestContext(Request request, string prefix, IDictionary<string, object> metaDatas)
        : this(request, prefix)
    {
        MetaDatas = metaDatas;
    }

    public Request Request { get; private set; }
    public string Prefix { get; private set; }
    public IDictionary<string, object> MetaDatas { get; private set; } = new Dictionary<string, object>();

    public class Builder
    {
        private Request? _request;
        private string? _prefix;
        private IDictionary<string, object> _metaDatas = new Dictionary<string, object>();

        public Builder() { }

        public Builder(RequestContext requestCtx)
        {
            _request = requestCtx.Request;
            _prefix = requestCtx.Prefix;
            _metaDatas = requestCtx.MetaDatas;
        }

        public Builder SetRequest(Request request)
        {
            _request = request;
            return this;
        }

        public Builder SetPrefix(string prefix)
        {
            _prefix = prefix;
            return this;
        }

        public Builder AppendToPrefix(string addition)
        {
            _prefix += addition;
            return this;
        }

        public Builder AddToMetadatas(string key, object value)
        {
            _metaDatas.Add(key, value);
            return this;
        }

        public Builder RemoveToMetadatas(string key)
        {
            _metaDatas.Remove(key);
            return this;
        }

        public Builder Reset()
        {
            _request = null;
            _prefix = null;
            _metaDatas = new Dictionary<string, object>();
            return this;
        }

        public RequestContext Build()
        {
            return new RequestContext(_request ?? new Request.Builder().Build(), _prefix ?? "/", _metaDatas);
        }
    }
}
