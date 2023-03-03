using Flee.PublicTypes;

using Newtonsoft.Json.Linq;

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace QueueConsumer;

public static class ConditionValidator
{
    public static (string expression, bool isValid) IsValid(this string json, string condition)
    {
        JObject obj;

        try
        {
            obj = JObject.Parse(json);
        }
        catch (Exception error)
        {
            Console.WriteLine("========================= Json With Error =========================");
            Console.WriteLine(json);
            Console.WriteLine("========================= Json With Error =========================");

            return (error.Message, false);
        }

        var expression = ReplaceParameters(obj, condition);

        ExpressionContext context = new ExpressionContext();

        IGenericExpression<bool> e = context.CompileGeneric<bool>(expression);

        return (expression, e.Evaluate());
    }

    private static string ReplaceParameters(JObject obj, string text)
    {
        var parameters = GetStringParameters(text);
        foreach (var parameter in parameters)
        {
            var newValue = GetValue(obj, parameter);
            text = text.Replace(parameter, newValue);
        }

        return text;
    }

    private static string[] GetStringParameters(string text)
    {
        var regex = new Regex(@"(?:(\{){1}[\w\.\-\[\]]+(\}){1})");
        var parameters = regex.Matches(text);

        return parameters.Select(r => r.Value).ToArray();
    }

    private static string GetValue(JObject obj, string path)
    {
        path = path.TrimStart('{').TrimEnd('}');
        var token = obj.SelectToken(path);
        return token?.ToString();
    }
}