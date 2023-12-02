using Microsoft.AspNetCore.Mvc;

public class BaseController : Controller
{
    public string ToUpperFirst(string inputString)
    {
        return char.ToUpper(inputString[0]) + inputString[1..];
    }
}