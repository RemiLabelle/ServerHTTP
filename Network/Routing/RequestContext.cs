using Network.HttpMessages;

namespace Network.Routing;

public class RequestContext
{
    public RequestContext(Request request, string prefix)
    {
        Request = request;
        Prefix = prefix;
    }

    public Request Request { get; private set; }
    public string Prefix { get; private set; }

    public class Builder
    {
        private Request? _request;
        private string? _prefix;

        public Builder() { }

        public Builder(RequestContext requestCtx)
        {
            _request = requestCtx.Request;
            _prefix = requestCtx.Prefix;
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

        public Builder Reset()
        {
            _request = null;
            _prefix = null;
            return this;
        }

        public RequestContext Build()
        {
            return new RequestContext(_request ?? new Request.Builder().Build(), _prefix ?? "/");
        }
    }
}
