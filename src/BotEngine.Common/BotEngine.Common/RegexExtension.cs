using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bib3;

namespace BotEngine.Common;

public static class RegexExtension
{
	private const string XmlLabelRegexPattern = "[\\w\\d_]+";

	private const string XmlAttributWertGrenzeRegexPattern = "(\\\"|\\')";

	private const string XmlAttributPattern = "[\\w\\d_]+\\s*\\=\\s*((\\\"|\\')[^'\\\"]+(\\\"|\\')|([^\\s\\>]+))";

	private const string XmlTagBeginRegexPattern = "\\<([\\w\\d_]+|[\\w\\d_]+\\s*\\=\\s*((\\\"|\\')[^'\\\"]+(\\\"|\\')|([^\\s\\>]+)))(\\s+[\\w\\d_]+\\s*\\=\\s*((\\\"|\\')[^'\\\"]+(\\\"|\\')|([^\\s\\>]+)))*\\s*(|\\/)\\>";

	private const string XmlTagEndeRegexPattern = "\\<\\/[\\w\\d_]+\\>";

	private const string XmlTagRegexPattern = "(\\<([\\w\\d_]+|[\\w\\d_]+\\s*\\=\\s*((\\\"|\\')[^'\\\"]+(\\\"|\\')|([^\\s\\>]+)))(\\s+[\\w\\d_]+\\s*\\=\\s*((\\\"|\\')[^'\\\"]+(\\\"|\\')|([^\\s\\>]+)))*\\s*(|\\/)\\>|\\<\\/[\\w\\d_]+\\>)";

	private static Regex XmlTagRegex = new Regex("(\\<([\\w\\d_]+|[\\w\\d_]+\\s*\\=\\s*((\\\"|\\')[^'\\\"]+(\\\"|\\')|([^\\s\\>]+)))(\\s+[\\w\\d_]+\\s*\\=\\s*((\\\"|\\')[^'\\\"]+(\\\"|\\')|([^\\s\\>]+)))*\\s*(|\\/)\\>|\\<\\/[\\w\\d_]+\\>)", RegexOptions.Compiled);

	public static Match RegexMatchIfSuccess(this string input, Regex regex)
	{
		if (input == null || regex == null)
		{
			return null;
		}
		Match match = regex.Match(input);
		if (!match.Success)
		{
			return null;
		}
		return match;
	}

	public static Match RegexMatchIfSuccess(this string input, string regexPattern, RegexOptions regexOptions = RegexOptions.None)
	{
		return (input == null || regexPattern == null) ? null : input.RegexMatchIfSuccess(new Regex(regexPattern, regexOptions));
	}

	public static Regex AsRegex(this string pattern, RegexOptions regexOptions)
	{
		return (pattern == null) ? null : new Regex(pattern, regexOptions);
	}

	public static Regex AsRegexCompiled(this string pattern)
	{
		return pattern.AsRegex(RegexOptions.Compiled);
	}

	public static Regex AsRegexCompiledIgnoreCase(this string pattern)
	{
		return pattern.AsRegex(RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}

	public static bool RegexMatchSuccess(this string input, string regexPattern, RegexOptions regexOptions = RegexOptions.None)
	{
		return input != null && Regex.Match(input, regexPattern, regexOptions).Success;
	}

	public static bool RegexMatchSuccessIgnoreCase(this string input, string regexPattern, RegexOptions regexOptions = RegexOptions.None)
	{
		return input.RegexMatchSuccess(regexPattern, regexOptions | RegexOptions.IgnoreCase);
	}

	public static bool MatchSuccess(this Regex regex, string input)
	{
		return input != null && (regex?.Match(input)?.Success ?? false);
	}

	public static IEnumerable<string> ListStringBetweenRegexMatch(this Regex regex, string @string)
	{
		if (regex == null)
		{
			yield break;
		}
		string restString = @string;
		while (!restString.IsNullOrEmpty())
		{
			Match match = regex.Match(restString);
			if (match.Success)
			{
				yield return restString.Substring(0, match.Index);
				restString = restString.Substring(match.Index + match.Length);
				continue;
			}
			yield return restString;
			break;
		}
	}

	public static IEnumerable<string> ListStringBetweenXmlTag(this string @string)
	{
		return XmlTagRegex.ListStringBetweenRegexMatch(@string);
	}

	public static string ReplaceXmlTag(this string @string, string xmlTagReplacement)
	{
		if (@string == null)
		{
			return null;
		}
		return XmlTagRegex.Replace(@string, xmlTagReplacement ?? "");
	}

	public static string RemoveXmlTag(this string @string)
	{
		return @string.ReplaceXmlTag(null);
	}
}
