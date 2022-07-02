namespace Impact;

public class Response
{
    public string Body { get; set; }
    public int StatusCode { get; set; } = 200;

    internal Response(string msg, int statusCode = 200)
    {
        this.Body = msg;
        this.StatusCode = statusCode;
    }
    
    public static implicit operator Response(string msg) => new Response(msg);
}