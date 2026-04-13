using System.Text.Json;

namespace xFood.Web.Extensions
{
	public static class SessionExtensions
	{
		// Salva um objeto complexo (como nosso Carrinho) na sessão
		public static void Set<T>(this ISession session, string key, T value)
		{
			session.SetString(key, JsonSerializer.Serialize(value));
		}

		// Recupera o objeto da sessão
		public static T? Get<T>(this ISession session, string key)
		{
			var value = session.GetString(key);
			return value == null ? default : JsonSerializer.Deserialize<T>(value);
		}
	}
}