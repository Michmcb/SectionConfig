namespace SectionConfig
{
	using SectionConfig.IO;
	using System;
	/// <summary>
	/// A validated value which may be used as a Key.
	/// Allows caching a valid key, so it doesn't have to be validated over and over again.
	/// </summary>
	public readonly struct CfgKey : IEquatable<CfgKey>
	{
		private static readonly char[] forbiddenKeyChars = new char[] { CfgSyntax.Comment, CfgSyntax.KeyEnd, CfgSyntax.StartSectionOrList, CfgSyntax.EndSectionOrList, '\n', '\r' };
		internal const string InvalidCharDescription = "This key is not valid, because it's empty, entirely whitespace, or contains forbidden characters (One of #:{}\\n\\r). This is the key: ";
		internal CfgKey(string key)
		{
			KeyString = key;
		}
		/// <summary>
		/// The key.
		/// </summary>
		public string KeyString { get; }
		/// <summary>
		/// If <paramref name="key"/> is valid, returns a new <see cref="CfgKey"/>. If not, throws <see cref="InvalidCfgKeyException"/>.
		/// Keys cannot be empty, cannot be only whitespace, or contain syntax characters. Quotes are fine.
		/// </summary>
		/// <param name="key">The value to validate.</param>
		/// <returns>A key, if <paramref name="key"/> is valid.</returns>
		/// <exception cref="InvalidCfgKeyException">When <paramref name="key"/> is not valid.</exception>
		public static CfgKey Create(string key)
		{
			return TryCreate(key) ?? throw new InvalidCfgKeyException(InvalidCharDescription + key);
		}
		/// <summary>
		/// Checks <paramref name="key"/> for validity, and returns a new <see cref="CfgKey"/>.
		/// Keys cannot be empty, cannot be only whitespace, or contain syntax characters. Quotes are fine.
		/// </summary>
		/// <param name="key">The value to validate.</param>
		/// <returns>A key, if <paramref name="key"/> is valid. If <paramref name="key"/> is not valid, null.</returns>
		public static CfgKey? TryCreate(string key)
		{
			return (!string.IsNullOrWhiteSpace(key) && key.IndexOfAny(forbiddenKeyChars) == -1)
				? new(key)
				: null;
		}
		/// <summary>
		/// Compares <see cref="KeyString"/> for both this and the other instance for equality.
		/// </summary>
		/// <param name="obj">The other <see cref="KeyString"/> to compare.</param>
		/// <returns>True if both <see cref="KeyString"/> are equal, false otherwise.</returns>
		public override bool Equals(object? obj)
		{
			return obj is CfgKey key && Equals(key);
		}
		/// <summary>
		/// Compares <see cref="KeyString"/> for both this and the other instance for equality.
		/// </summary>
		/// <param name="other">The other <see cref="KeyString"/> to compare.</param>
		/// <returns>True if both <see cref="KeyString"/> are equal, false otherwise.</returns>
		public bool Equals(CfgKey other)
		{
			return KeyString.Equals(other.KeyString);
		}
		/// <summary>
		/// Returns the hashcode of <see cref="KeyString"/>.
		/// </summary>
		/// <returns>Hashcode of <see cref="KeyString"/>.</returns>
		public override int GetHashCode()
		{
			return KeyString.GetHashCode();
		}
		/// <summary>
		/// Returns <see cref="KeyString"/>.
		/// </summary>
		/// <returns><see cref="KeyString"/></returns>
		public override string? ToString()
		{
			return KeyString;
		}
		/// <summary>
		/// Identical to comparing the <see cref="KeyString"/> of <paramref name="left"/> and <paramref name="right"/> for equality, case sensitive.
		/// </summary>
		public static bool operator ==(CfgKey left, CfgKey right) => left.Equals(right);
		/// <summary>
		/// Identical to comparing the <see cref="KeyString"/> of <paramref name="left"/> and <paramref name="right"/> for inequality, case sensitive.
		/// </summary>
		public static bool operator !=(CfgKey left, CfgKey right) => !left.Equals(right);
	}
}
