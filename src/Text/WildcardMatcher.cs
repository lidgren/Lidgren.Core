using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Reusable wildcard matcher supporting * (matching anything) and ? (matching any one character)
	/// </summary>
	public readonly struct WildcardMatcher
	{
		private enum MatcherType
		{
			Nothing,           // ""
			Everything,        // "*"
			ExactPattern,      // "abc"
			LeadingStar,       // "*abc"
			TrailingStar,      // "abc*"
			SingleStar,        // "a*b"
			EnclosingStars,    // "*abc*"
			QuestionMarks,     // "?b?"
			Tokens,            // every other combination
		}

		private enum TokenType
		{
			Exact = 0,
			QuestionMark = 2,
			Star = 3
		}

		[DebuggerDisplay("{Type} {Start} to {Start + Length - 1}")]
		private struct Token
		{
			public int Start;
			public int Length;
			public TokenType Type;
		}

		private readonly MatcherType m_matchType;
		private readonly string m_pattern;
		private readonly FastList<Token> m_tokens;
		private readonly StringComparison m_stringComparison;

		public WildcardMatcher(string pattern, bool ignoreCase = false)
		{
			m_pattern = pattern;
			m_tokens = null;
			m_stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			if (pattern == null || pattern.Length == 0)
			{
				m_matchType = MatcherType.Nothing;
				return;
			}

			if (pattern.Equals("*", StringComparison.Ordinal))
			{
				m_matchType = MatcherType.Everything;
				return;
			}

			// find all tokens
			int numStars = 0;
			int curTokStart = 0;
			int curTokLen = 0;
			while(curTokStart + curTokLen < pattern.Length)
			{
				var examineIndex = curTokStart + curTokLen;
				var c = pattern[examineIndex];
				switch (c)
				{
					case '?':
					case '*':
						if (m_tokens == null)
							m_tokens = new FastList<Token>(8);

						// end current token
						if (curTokLen > 0)
						{
							ref var token = ref m_tokens.Add();
							token.Start = curTokStart;
							token.Length = curTokLen;
							token.Type = TokenType.Exact;
						}

						Token qtok;
						qtok.Start = examineIndex;
						qtok.Length = 1;

						if (c == '?')
						{
							qtok.Type = TokenType.QuestionMark;
							m_tokens.Add(qtok);
						}
						else
						{
							numStars++;
							qtok.Type = TokenType.Star;

							// remove consecutive stars
							var cnt = m_tokens.Count;
							if (cnt == 0 || m_tokens[cnt - 1].Type != TokenType.Star)
								m_tokens.Add(qtok); // else drop it
						}
						curTokStart = examineIndex + 1;
						curTokLen = 0;
						break;
					default:
						curTokLen++;
						break;
				}
			}

			if (curTokLen > 0)
			{
				// detect pattern without any wildcards
				if (m_tokens == null)
				{
					m_matchType = MatcherType.ExactPattern;
					return;
				}

				ref var token = ref m_tokens.Add();
				token.Start = curTokStart;
				token.Length = curTokLen;
				token.Type = TokenType.Exact;
			}

			// detect leading or trailing star
			if (m_tokens.Count == 2)
			{
				if (m_tokens[0].Type == TokenType.Star && m_tokens[1].Type == TokenType.Exact)
				{
					m_matchType = MatcherType.LeadingStar;
					return;
				}
				else if (m_tokens[0].Type == TokenType.Exact && m_tokens[1].Type == TokenType.Star)
				{
					m_matchType = MatcherType.TrailingStar;
					return;
				}
			}

			if (m_tokens.Count == 3)
			{
				// detect single sandwiched star
				if (m_tokens[0].Type == TokenType.Exact &&
				m_tokens[1].Type == TokenType.Star &&
				m_tokens[2].Type == TokenType.Exact)
				{
					m_matchType = MatcherType.SingleStar;
					return;
				}

				if (m_tokens[0].Type == TokenType.Star &&
				m_tokens[1].Type == TokenType.Exact &&
				m_tokens[2].Type == TokenType.Star)
				{
					// detect "contains"
					m_matchType = MatcherType.EnclosingStars;
					return;
				}
			}

			// complex token matching
			if (numStars == 0)
				m_matchType = MatcherType.QuestionMarks;
			else
				m_matchType = MatcherType.Tokens;
		}

		/// <summary>
		/// Returns true if input matches the pattern
		/// </summary>
		public readonly bool Matches(ReadOnlySpan<char> input)
		{
			if (m_matchType == MatcherType.Nothing)
				return input.Length == 0;
			if (m_matchType == MatcherType.Everything)
				return true;
			if (input.Length == 0)
				return false;

			var pattern = m_pattern.AsSpan();

			switch (m_matchType)
			{
				case MatcherType.ExactPattern:
					return input.Equals(pattern, m_stringComparison);
				case MatcherType.LeadingStar:
					return input.EndsWith(pattern.Slice(1), m_stringComparison);
				case MatcherType.TrailingStar:
					return input.StartsWith(pattern.Slice(0, pattern.Length - 1), m_stringComparison);
				case MatcherType.EnclosingStars:
					{
						ref readonly var secondToken = ref m_tokens[1];
						return input.Contains(pattern.Slice(secondToken.Start, secondToken.Length), m_stringComparison);
					}
				case MatcherType.QuestionMarks:
					{
						if (input.Length != pattern.Length)
							return false;
						if (m_stringComparison == StringComparison.Ordinal)
						{
							for (int i = 0; i < input.Length; i++)
							{
								var p = pattern[i];
								var c = input[i];
								if (p != c && p != '?')
									return false;
							}
						}
						else
						{
							for (int i = 0; i < input.Length; i++)
							{
								var p = char.ToLower(pattern[i]);
								var c = char.ToLower(input[i]);
								if (p != c && p != '?')
									return false;
							}
						}
						return true;
					}
				case MatcherType.SingleStar:
					{
						ref readonly var firstToken = ref m_tokens[0];
						CoreException.Assert(m_tokens[1].Type == TokenType.Star);
						ref readonly var lastToken = ref m_tokens[2];
						return input.StartsWith(pattern.Slice(0, firstToken.Length), m_stringComparison) &&
							input.EndsWith(pattern.Slice(lastToken.Start, lastToken.Length), m_stringComparison);
					}

				case MatcherType.Tokens:
					return MatchByTokens(input);
				default:
					CoreException.Throw("Unexpected MatcherType");
					return false;
			}
		}

		private readonly bool MatchByTokens(ReadOnlySpan<char> text)
		{
			var full = m_pattern.AsSpan();
			var tokens = m_tokens.ReadOnlySpan;

			while (tokens.Length > 0)
			{
				{
					ref readonly var firstToken = ref tokens[0];
					var firstTokenSlice = full.Slice(firstToken.Start, firstToken.Length);
					if (firstToken.Type == TokenType.Exact)
					{
						if (text.StartsWith(firstTokenSlice, m_stringComparison) == false)
							return false;
						text = text.Slice(firstToken.Length);
						tokens = tokens.Slice(1);
						continue;
					}
					if (firstToken.Type == TokenType.QuestionMark)
					{
						text = text.Slice(1);
						tokens = tokens.Slice(1);
						continue;
					}
				}

				{
					var lastTokenIndex = tokens.Length - 1;
					if (lastTokenIndex > 0)
					{
						ref readonly var lastToken = ref tokens[tokens.Length - 1];
						var lastTokenSlice = full.Slice(lastToken.Start, lastToken.Length);

						if (lastToken.Type == TokenType.Exact)
						{
							if (text.EndsWith(lastTokenSlice, m_stringComparison) == false)
								return false;
							text = text.Slice(0, text.Length - lastTokenSlice.Length);
							tokens = tokens.Slice(0, lastTokenIndex);
							continue;
						}
						if (lastToken.Type == TokenType.QuestionMark)
						{
							if (text.Length < 1)
								return false;
							text = text.Slice(0, text.Length - 1);
							tokens = tokens.Slice(0, lastTokenIndex);
							continue;
						}
					}
				}

				//
				// Ok, at this point FIRST and LAST tokens are STARS
				//

				CoreException.Assert(tokens[0].Type == TokenType.Star);
				CoreException.Assert(tokens.Length == 1 || tokens[tokens.Length - 1].Type == TokenType.Star);

				if (tokens.Length <= 2)
				{
					// only stars remaining!
					return true;
				}

				if (tokens.Length == 3)
				{
					// only one token in between; fast track
					ref readonly var midToken = ref tokens[1];
					switch (midToken.Type)
					{
						case TokenType.QuestionMark:
							return text.Length >= 1;
						case TokenType.Exact:
							return text.Contains(full.Slice(midToken.Start, midToken.Length), m_stringComparison);
						case TokenType.Star:
						default:
							CoreException.Throw("Unexpected token type");
							break;
					}
				}

				{
					ref readonly var firstToken = ref tokens[1];
					if (firstToken.Type == TokenType.Exact)
					{
						// narrow from start
						var idx = text.IndexOf(full.Slice(firstToken.Start, firstToken.Length), m_stringComparison);
						if (idx == -1)
							return false;

						tokens = tokens.Slice(2); // skip first star AND this token, since we've matched it
						text = text.Slice(idx + firstToken.Length);
						continue;
					}
				}

				{
					ref readonly var lastToken = ref tokens[tokens.Length - 2];
					if (lastToken.Type == TokenType.Exact)
					{
						// narrow from end
						var idx = text.LastIndexOf(full.Slice(lastToken.Start, lastToken.Length), m_stringComparison);
						if (idx == -1)
							return false;

						tokens = tokens.Slice(0, tokens.Length - 2); // skip last star AND this token, since we've matched it
						text = text.Slice(0, idx);
						continue;
					}
				}

				// ugh, we've got "*?...?*"
				CoreException.Assert(tokens[1].Type == TokenType.QuestionMark);
				CoreException.Assert(tokens[tokens.Length - 2].Type == TokenType.QuestionMark);

				// let's narrow from end
				if (text.Length < 2)
					return false; // can't possibly be two; gotta match at least two characters

				tokens = tokens.Slice(0, tokens.Length - 2);
				text = text.Slice(0, text.Length - 1);
				continue;
			}

			// out of tokens; success!
			return true;
		}
	}
}
