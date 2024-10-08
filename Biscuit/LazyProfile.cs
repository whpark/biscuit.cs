﻿//=============================================================================
// 
// LazyProfile.cs
//      .ini file parser. lazy iteration.
// PWH. C++ ==> C#...
//
//

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Biscuit {
	using map_t = System.Collections.Generic.Dictionary<string, xLazyProfile>;

	public class xLazyProfile : IEquatable<xLazyProfile> {
		public struct sConfig {
			public bool bIGNORE_CASE = true;
			public bool bTREAT_BOOL_AS_INT = true;
			public bool bSTRING_BE_QUOTED = true;

			public sConfig() { }
			public sConfig(bool bIGNORE_CASE, bool bTREAT_BOOL_AS_INT, bool bSTRING_BE_QUOTED) {
				this.bIGNORE_CASE = bIGNORE_CASE;
				this.bTREAT_BOOL_AS_INT = bTREAT_BOOL_AS_INT;
				this.bSTRING_BE_QUOTED = bSTRING_BE_QUOTED;
			}
			public sConfig(sConfig B) {
				bIGNORE_CASE = B.bIGNORE_CASE;
				bTREAT_BOOL_AS_INT = B.bTREAT_BOOL_AS_INT;
				bSTRING_BE_QUOTED = B.bSTRING_BE_QUOTED;
			}

		}
		public sConfig m_config = new();

		//regex for section name
		// key : any trimmed(whitespace) chars quoted by bracket "[]", ex) [ HeadDriver1:1 ]
		// trailing : any string after section name
		protected static readonly Regex s_reSection = new Regex("\\s*\\[\\s*([^\\]]*[^\\]\\s]+)\\s*\\](.*)", RegexOptions.Compiled);

		// regex for item
		// key : any string except '=', including space
		// value : 1. any string except ';'.
		//         2. if value starts with '"', it can contain any character except '"'.
		// comment : any string after ';', including space.
		protected static readonly Regex s_reItem = new Regex("\\s*([\\w\\s]*\\w+)\\s*(=)\\s*(\"(?:[^\\\"]|\\.)*\"|[^;\\n]*)\\s*[^;]*(;.*)?", RegexOptions.Compiled);

		private map_t m_sections = new();			// child sections
		private List<string> m_items = new();
		private string m_line = "";					// anything after section name

		public xLazyProfile() { }
		public xLazyProfile(sConfig config) {
			m_config = new sConfig(config);
		}
		public xLazyProfile(xLazyProfile B) {
			m_sections = new map_t(B.m_sections);
			m_items = new List<string>(B.m_items);
			m_line = new string(B.m_line);

			m_config = B.m_config;
		}

		public bool Equals(xLazyProfile? B) {
			//if (this == B)
			//	return true;
			if (B is null)
				return this is null;
			return true
				&& m_sections.Count == B.m_sections.Count
				&& misc.EqualsMap(m_sections, B.m_sections)
				&& misc.EqualsLst(m_items, B.m_items)
				&& m_line == B.m_line
				&& m_config.Equals(B.m_config)
				;
		}

		// section
		public xLazyProfile this[string key] {
			get {
				if (m_sections.ContainsKey(key))
					return m_sections[key];
				else {
					return m_sections[key] = new xLazyProfile(m_config);
				}
			}
			set {
				m_sections[key] = value;
			}
		}
		//=> m_sections[key];

		public string? GetItemValueRaw(string key) {
			foreach (var item in m_items) {
				var match = s_reItem.Match(item);
				if (!match.Success || match.Index != 0)
					continue;
				// whole, key, '=', value, comments
				if (string.Compare(match.Groups[1].Value, key, m_config.bIGNORE_CASE) == 0)
					return match.Groups[3].Value;
			}
			return null;
		}

		// getter
		public T GetItemValue<T>(string key, T vDefault) {
			if (GetItemValueRaw(key) is string sv) {
				sv = sv.Trim();
				T t = vDefault;
				if (t is bool && m_config.bTREAT_BOOL_AS_INT) {
					if (int.TryParse(sv, out int v))
						return (T)(object)(v != 0);
					else if (bool.TryParse(sv, out bool b))
						return (T)(object)b;
					else
						return vDefault;
				}

				return t switch {
					int => (T)(object)int.Parse(sv),
					uint => (T)(object)uint.Parse(sv),
					float => (T)(object)float.Parse(sv),
					double => (T)(object)double.Parse(sv),
					string => (T)(object)(m_config.bSTRING_BE_QUOTED ? sv.Trim('"') : sv),
					_ => throw new NotImplementedException(),
				};
			}
			else {
				return vDefault;
			}
		}
		public string GetItemValue(string key, string vDefault) {
			if (GetItemValueRaw(key) is string sv) {
				if (m_config.bSTRING_BE_QUOTED)
					return sv.Trim('"');
				return sv;
			}
			else { return vDefault; }
		}

		public void SetItemValueRaw(string key, string value, string? comment = null/* comment starting with ';'*/) {
			// if comment does not start with ';', add one.
			if (comment != null && comment.StartsWith(';')) {
				comment = ";" + comment;
			}

			// preserve position of '=' and ';'
			int posEQ = -1;
			int posComment = -1;
			// find the item
			for (int i = 0; i < m_items.Count; i++) {
				var item = m_items[i];
				var match = s_reItem.Match(item);
				if (!match.Success || match.Index != 0)
					continue;
				// whole, key, '=', value, comment
				posEQ = Math.Max(posEQ, match.Groups[2].Index);
				posComment = Math.Max(posComment, match.Groups[4].Index);

				// if key matches
				if (string.Compare(match.Groups[1].Value, key, m_config.bIGNORE_CASE) != 0)
					continue;
				if (comment == null && match.Groups[4].Length > 0)
					comment = match.Groups[4].Value;
				//int starting = key1.begin() - whole.begin();
				var mWhole = match.Groups[0];
				var mValue = match.Groups[3];
				var mComment = match.Groups[4];
				if (mComment.Success)
					m_items[i] = $"{mWhole.Value[0..mValue.Index]}{value.PadRight(mComment.Index - mValue.Index)}{comment}";
				else
					m_items[i] = $"{mWhole.Value[0..mValue.Index]}{value}";
				return;
			}

			// if not found, add new item
			var str = $"{key.PadRight(Math.Max(posEQ, key.Length))}= {value}";

			if (comment is not null) {
				str += $"{comment.PadRight(Math.Max(0, posComment - str.Length))}";
			}
			if (m_items.Count == 0) {
				m_items.Add(str);
			}
			else {
				// let empty lines behind.
				for (int i = m_items.Count-1; i >= 0; i--) {
					var cur = m_items[i];
					if (cur.Trim() == "")
						continue;

					m_items.Insert(i+1, str);
					break;
				}
			}
		}

		public void SetItemValue(string key, string value, bool bDO_NOT_QUOTE_STRING = false) {
			if (m_config.bSTRING_BE_QUOTED && !bDO_NOT_QUOTE_STRING) {
				SetItemValueRaw(key, $"\"{value}\"");
			}
			else {
				SetItemValueRaw(key, value);
			}
		}

		public void SetItemValue<T>(string key, T value) {
			if (value is bool v && m_config.bTREAT_BOOL_AS_INT) {
				SetItemValueRaw(key, v ? "1" : "0");
			}
			else if (value is string str) {
				SetItemValue(key, str, false);
			}
			else {
				SetItemValueRaw(key, value.ToString());
			}
		}

		public void Clear() {
			m_sections.Clear();
			m_items.Clear();
			m_line = "";
		}
		public xLazyProfile GetSection(string key) => m_sections[key];

		public map_t GetSections() => m_sections;

		public List<string> GetRawItems() => m_items;

		public List<(string key, string value)> GetItemsView() {
			List<(string key, string value)> items = new();
			foreach (var item in m_items) {
				var match = s_reItem.Match(item);
				if (!match.Success || match.Index != 0)
					continue;
				// whole, key, '=', value, comment
				items.Add((match.Groups[1].Value, match.Groups[3].Value));
			}
			return items;
		}

		public void SetItemComment(string key, string comment) {
			string value = GetItemValueRaw(key) ?? "";
			SetItemValueRaw(key, value, comment);
		}

		public bool HasItem(string key) {
			var sv = GetItemValueRaw(key);
			if (sv is null)
				return false;
			sv = sv.Trim();
			return sv != "";
		}

		public bool HasSection(string key) {
			return m_sections.ContainsKey(key);
		}

		/// @brief Sync
		public void SyncItemValue<T>(bool bToProfile, string key, ref T value) {
			if (bToProfile) {
				SetItemValue(key, value);
			}
			else {
				value = GetItemValue(key, value);
			}
		}

		public delegate bool FuncIsItemDeprecated(string key, string value);
		public void DeleteItemsIf(FuncIsItemDeprecated funcIsItemDeprecated) {
			for (int i = 0; i < m_items.Count; i++) {
				var item = m_items[i];
				var match = s_reItem.Match(item);
				if (!match.Success || match.Index != 0)
					continue;
				// whole, key, '=', value, comment
				if (funcIsItemDeprecated(match.Groups[1].Value, match.Groups[3].Value))
					m_items.RemoveAt(i--);
			}
		}
		public delegate bool FuncIsSectionDeprecated(string key, xLazyProfile section);
		public void DeleteSectionsIf(FuncIsSectionDeprecated funcIsSectionDeprecated) {
			for (int i = 0; i < m_sections.Count; i++) {
				var item = m_sections.ElementAt(i);
				if (funcIsSectionDeprecated(item.Key, item.Value)) {
					m_sections.Remove(item.Key);
					i--;
				}
			}
		}

		public bool Load(string path) {
			Clear();
			m_sections.Add("", new xLazyProfile(m_config));
			if (!File.Exists(path))
				return false;

			string allText = File.ReadAllText(path);
			var section = m_sections.ElementAt(0).Value;
			foreach (var line in allText.Split('\n')) {
				//string str = line.Trim();
				//if (str.Length == 0)
				//	continue;
				var str = line.TrimEnd('\r');
				var m = s_reSection.Match(str);
				if (m.Success && m.Index == 0 && m.Groups[0].Index == 0) {
					string key = m.Groups[1].Value;
					if (key == "")
						continue;
					section = new xLazyProfile(m_config);
					m_sections.Add(key, section);
					m_sections[key].m_line = str;
				}
				else {
					section.m_items.Add(str);
				}
			}

			return true;
		}

		public bool Save(string path, bool bWriteBOM = true) {
			List<string> contents = new ();
			foreach (var sectionItem in m_sections) {
				var key = sectionItem.Key;
				var section = sectionItem.Value;

				if (key is not null && key != "") {
					if (section.m_line == null || section.m_line == "") {
						var str = $"[{key}]";
						contents.Add(str);
					}
					else {
						contents.Add(section.m_line);
					}
				}
				foreach (var item in section.m_items) {
					contents.Add(item);
				}
			}
			File.WriteAllLines(path, contents.ToArray());
			return true;
		}

	}
}
