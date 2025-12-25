
namespace Sale_Saas.Application.Utilities
{
	public static class DictionaryExtensions
	{
		public static string ToPairString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, string pairSeparator = ", ", string keyValueSeparator = "=")
		{
			return string.Join(pairSeparator, dictionary.Select(pair => pair.Key + keyValueSeparator + pair.Value));
		}
	}
}
