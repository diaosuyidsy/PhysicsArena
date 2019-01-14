/* tfxJSONObject.cs -- Simple C# JSON parser
  version 1.4 - March 17, 2014

  Copyright (C) 2012 Boomlagoon Ltd.
  contact@boomlagoon.com
  
  
  TextFx implementation changes:
  - Added an optional force_hide_errors boolean parameter to Parse() method, to keep the console clear when checking for JSON parsable text.

*/

//#define PARSE_ESCAPED_UNICODE

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WII || UNITY_PS3 || UNITY_XBOX360 || UNITY_FLASH
#define USE_UNITY_DEBUGGING
#endif

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

#if PARSE_ESCAPED_UNICODE
using System.Text.RegularExpressions;
#endif

#if USE_UNITY_DEBUGGING
using UnityEngine;
#else
using System.Diagnostics;
#endif

namespace Boomlagoon.TextFx.JSON {

	public static class BoomlagoonExtensions
	{
		public static T Pop<T>(this List<T> list) {
			var result = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return result;
		}
	}

	static class JSONLogger {
#if USE_UNITY_DEBUGGING
		public static void Log(string str) {
			Debug.Log(str);
		}
		public static void Error(string str) {
			Debug.LogError(str);
		}
#else
		public static void Log(string str) {
			Debug.WriteLine(str);
		}
		public static void Error(string str) {
			Debug.WriteLine(str);
		}
#endif
	}

	public enum tfxJSONValueType {
		String,
		Number,
		Object,
		Array,
		Boolean,
		Null
	}

	public class tfxJSONValue {

		public tfxJSONValue(tfxJSONValueType type) {
			Type = type;
		}

		public tfxJSONValue(string str) {
			Type = tfxJSONValueType.String;
			Str = str;
		}

		public tfxJSONValue(double number) {
			Type = tfxJSONValueType.Number;
			Number = number;
		}

		public tfxJSONValue(tfxJSONObject obj) {
			if (obj == null) {
				Type = tfxJSONValueType.Null;
			} else {
				Type = tfxJSONValueType.Object;
				Obj = obj;
			}
		}

		public tfxJSONValue(tfxJSONArray array) {
			Type = tfxJSONValueType.Array;
			Array = array;
		}

		public tfxJSONValue(bool boolean) {
			Type = tfxJSONValueType.Boolean;
			Boolean = boolean;
		}

		/// <summary>
		/// Construct a copy of the tfxJSONValue given as a parameter
		/// </summary>
		/// <param name="value"></param>
		public tfxJSONValue(tfxJSONValue value) {
			Type = value.Type;
			switch (Type) {
				case tfxJSONValueType.String:
					Str = value.Str;
					break;

				case tfxJSONValueType.Boolean:
					Boolean = value.Boolean;
					break;

				case tfxJSONValueType.Number:
					Number = value.Number;
					break;

				case tfxJSONValueType.Object:
					if (value.Obj != null) {
						Obj = new tfxJSONObject(value.Obj);
					}
					break;

				case tfxJSONValueType.Array:
					Array = new tfxJSONArray(value.Array);
					break;
			}
		}

		public tfxJSONValueType Type { get; private set; }
		public string Str { get; set; }
		public double Number { get; set; }
		public tfxJSONObject Obj { get; set; }
		public tfxJSONArray Array { get; set; }
		public bool Boolean { get; set; }
		public tfxJSONValue Parent { get; set; }

		public static implicit operator tfxJSONValue(string str) {
			return new tfxJSONValue(str);
		}

		public static implicit operator tfxJSONValue(double number) {
			return new tfxJSONValue(number);
		}

		public static implicit operator tfxJSONValue(tfxJSONObject obj) {
			return new tfxJSONValue(obj);
		}

		public static implicit operator tfxJSONValue(tfxJSONArray array) {
			return new tfxJSONValue(array);
		}

		public static implicit operator tfxJSONValue(bool boolean) {
			return new tfxJSONValue(boolean);
		}
		
		/// <returns>String representation of this tfxJSONValue</returns>
		public override string ToString() {
			switch (Type) {
				case tfxJSONValueType.Object:
					return Obj.ToString();

				case tfxJSONValueType.Array:
					return Array.ToString();

				case tfxJSONValueType.Boolean:
					return Boolean ? "true" : "false";

				case tfxJSONValueType.Number:
					return Number.ToString();

				case tfxJSONValueType.String:
					return "\"" + Str + "\"";

				case tfxJSONValueType.Null:
					return "null";
			}
			return "null";
		}

	}

	public class tfxJSONArray : IEnumerable<tfxJSONValue> {

		private readonly List<tfxJSONValue> values = new List<tfxJSONValue>();

		public tfxJSONArray() {
		}

		/// <summary>
		/// Construct a new array and copy each value from the given array into the new one
		/// </summary>
		/// <param name="array"></param>
		public tfxJSONArray(tfxJSONArray array) {
			values = new List<tfxJSONValue>();
			foreach (var v in array.values) {
				values.Add(new tfxJSONValue(v));
			}
		}

		/// <summary>
		/// Add a tfxJSONValue to this array
		/// </summary>
		/// <param name="value"></param>
		public void Add(tfxJSONValue value) {
			values.Add(value);
		}

		public tfxJSONValue this[int index] {
			get { return values[index]; }
			set { values[index] = value; }
		}

		/// <returns>
		/// Return the length of the array
		/// </returns>
		public int Length {
			get { return values.Count; }
		}

		/// <returns>String representation of this tfxJSONArray</returns>
		public override string ToString() {
			var stringBuilder = new StringBuilder();
			stringBuilder.Append('[');
			foreach (var value in values) {
				stringBuilder.Append(value.ToString());
				stringBuilder.Append(',');
			}
			if (values.Count > 0) {
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}

		public IEnumerator<tfxJSONValue> GetEnumerator() {
			return values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return values.GetEnumerator();
		}

		/// <summary>
		/// Attempt to parse a string as a JSON array.
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns>A new tfxJSONArray object if successful, null otherwise.</returns>
		public static tfxJSONArray Parse(string jsonString) {
			var tempObject = tfxJSONObject.Parse("{ \"array\" :" + jsonString + '}');
			return tempObject == null ? null : tempObject.GetValue("array").Array;
		}

		/// <summary>
		/// Empty the array of all values.
		/// </summary>
		public void Clear() {
			values.Clear();
		}

		/// <summary>
		/// Remove the value at the given index, if it exists.
		/// </summary>
		/// <param name="index"></param>
		public void Remove(int index) {
			if (index >= 0 && index < values.Count) {
				values.RemoveAt(index);
			} else {
				JSONLogger.Error("index out of range: " + index + " (Expected 0 <= index < " + values.Count + ")");
			}
		}

		/// <summary>
		/// Concatenate two tfxJSONArrays
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns>A new tfxJSONArray that is the result of adding all of the right-hand side array's values to the left-hand side array.</returns>
		public static tfxJSONArray operator +(tfxJSONArray lhs, tfxJSONArray rhs) {
			var result = new tfxJSONArray(lhs);
			foreach (var value in rhs.values) {
				result.Add(value);
			}
			return result;
		}

	}

	public class tfxJSONObject : IEnumerable<KeyValuePair<string, tfxJSONValue>> {

		private enum JSONParsingState {
			Object,
			Array,
			EndObject,
			EndArray,
			Key,
			Value,
			KeyValueSeparator,
			ValueSeparator,
			String,
			Number,
			Boolean,
			Null
		}

		private readonly IDictionary<string, tfxJSONValue> values = new Dictionary<string, tfxJSONValue>();

#if PARSE_ESCAPED_UNICODE
		private static readonly Regex unicodeRegex = new Regex(@"\\u([0-9a-fA-F]{4})");
		private static readonly byte[] unicodeBytes = new byte[2];
#endif

		public tfxJSONObject() {
		}

		/// <summary>
		/// Construct a copy of the given tfxJSONObject.
		/// </summary>
		/// <param name="other"></param>
		public tfxJSONObject(tfxJSONObject other) {
			values = new Dictionary<string, tfxJSONValue>();

			if (other != null) {
				foreach (var keyValuePair in other.values) {
					values[keyValuePair.Key] = new tfxJSONValue(keyValuePair.Value);
				}
			}
		}

		/// <param name="key"></param>
		/// <returns>Does 'key' exist in this object.</returns>
		public bool ContainsKey(string key) {
			return values.ContainsKey(key);
		}

		public tfxJSONValue GetValue(string key) {
			tfxJSONValue value;
			values.TryGetValue(key, out value);
			return value;
		}

		public string GetString(string key) {
			var value = GetValue(key);
			if (value == null) {
				JSONLogger.Error(key + "(string) == null");
				return string.Empty;
			}
			return value.Str;
		}

		public double GetNumber(string key) {
			var value = GetValue(key);
			if (value == null) {
				JSONLogger.Error(key + " == null");
				return double.NaN;
			}
			return value.Number;
		}

		public tfxJSONObject GetObject(string key) {
			var value = GetValue(key);
			if (value == null) {
				JSONLogger.Error(key + " == null");
				return null;
			}
			return value.Obj;
		}

		public bool GetBoolean(string key) {
			var value = GetValue(key);
			if (value == null) {
				JSONLogger.Error(key + " == null");
				return false;
			}
			return value.Boolean;
		}

		public tfxJSONArray GetArray(string key) {
			var value = GetValue(key);
			if (value == null) {
				JSONLogger.Error(key + " == null");
				return null;
			}
			return value.Array;
		}

		public tfxJSONValue this[string key] {
			get { return GetValue(key); }
			set { values[key] = value; }
		}

		public void Add(string key, tfxJSONValue value) {
			values[key] = value;
		}

		public void Add(KeyValuePair<string, tfxJSONValue> pair) {
			values[pair.Key] = pair.Value;
		}

		/// <summary>
		/// Attempt to parse a string into a tfxJSONObject.
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns>A new tfxJSONObject or null if parsing fails.</returns>
		public static tfxJSONObject Parse(string jsonString, bool force_hide_errors = false) {
			if (string.IsNullOrEmpty(jsonString)) {
				return null;
			}

			tfxJSONValue currentValue = null;

			var keyList = new List<string>();

			var state = JSONParsingState.Object;

			for (var startPosition = 0; startPosition < jsonString.Length; ++startPosition) {

				startPosition = SkipWhitespace(jsonString, startPosition);

				switch (state) {
					case JSONParsingState.Object:
						if (jsonString[startPosition] != '{') {
							return Fail('{', startPosition, force_hide_errors);
						}

						tfxJSONValue newObj = new tfxJSONObject();
						if (currentValue != null) {
							newObj.Parent = currentValue;
						}
						currentValue = newObj;

						state = JSONParsingState.Key;
						break;

					case JSONParsingState.EndObject:
						if (jsonString[startPosition] != '}') {
							return Fail('}', startPosition, force_hide_errors);
						}

						if (currentValue.Parent == null) {
							return currentValue.Obj;
						}

						switch (currentValue.Parent.Type) {

							case tfxJSONValueType.Object:
								currentValue.Parent.Obj.values[keyList.Pop()] = new tfxJSONValue(currentValue.Obj);
								break;

							case tfxJSONValueType.Array:
								currentValue.Parent.Array.Add(new tfxJSONValue(currentValue.Obj));
								break;

							default:
								return Fail("valid object", startPosition, force_hide_errors);

						}
						currentValue = currentValue.Parent;

						state = JSONParsingState.ValueSeparator;
						break;

					case JSONParsingState.Key:
						if (jsonString[startPosition] == '}') {
							--startPosition;
							state = JSONParsingState.EndObject;
							break;
						}

						var key = ParseString(jsonString, ref startPosition, force_hide_errors);
						if (key == null) {
							return Fail("key string", startPosition, force_hide_errors);
						}
						keyList.Add(key);
						state = JSONParsingState.KeyValueSeparator;
						break;

					case JSONParsingState.KeyValueSeparator:
						if (jsonString[startPosition] != ':') {
							return Fail(':', startPosition, force_hide_errors);
						}
						state = JSONParsingState.Value;
						break;

					case JSONParsingState.ValueSeparator:
						switch (jsonString[startPosition]) {

							case ',':
								state = currentValue.Type == tfxJSONValueType.Object ? JSONParsingState.Key : JSONParsingState.Value;
								break;

							case '}':
								state = JSONParsingState.EndObject;
								--startPosition;
								break;

							case ']':
								state = JSONParsingState.EndArray;
								--startPosition;
								break;

							default:
								return Fail(", } ]", startPosition, force_hide_errors);
						}
						break;

					case JSONParsingState.Value: {
						var c = jsonString[startPosition];
						if (c == '"') {
							state = JSONParsingState.String;
						} else if (char.IsDigit(c) || c == '-') {
							state = JSONParsingState.Number;
						} else
							switch (c) {

								case '{':
									state = JSONParsingState.Object;
									break;

								case '[':
									state = JSONParsingState.Array;
									break;

								case ']':
									if (currentValue.Type == tfxJSONValueType.Array) {
										state = JSONParsingState.EndArray;
									} else {
										return Fail("valid array", startPosition, force_hide_errors);
									}
									break;

								case 'f':
								case 't':
									state = JSONParsingState.Boolean;
									break;


								case 'n':
									state = JSONParsingState.Null;
									break;

								default:
									return Fail("beginning of value", startPosition, force_hide_errors);
							}

						--startPosition; //To re-evaluate this char in the newly selected state
						break;
					}

					case JSONParsingState.String:
						var str = ParseString(jsonString, ref startPosition, force_hide_errors);
						if (str == null) {
							return Fail("string value", startPosition, force_hide_errors);
						}

						switch (currentValue.Type) {

							case tfxJSONValueType.Object:
								currentValue.Obj.values[keyList.Pop()] = new tfxJSONValue(str);
								break;

							case tfxJSONValueType.Array:
								currentValue.Array.Add(str);
								break;

							default:
								if(!force_hide_errors)
									JSONLogger.Error("Fatal error, current JSON value not valid");
								return null;
						}

						state = JSONParsingState.ValueSeparator;
						break;

					case JSONParsingState.Number:
						var number = ParseNumber(jsonString, ref startPosition);
						if (double.IsNaN(number)) {
							return Fail("valid number", startPosition, force_hide_errors);
						}

						switch (currentValue.Type) {

							case tfxJSONValueType.Object:
								currentValue.Obj.values[keyList.Pop()] = new tfxJSONValue(number);
								break;

							case tfxJSONValueType.Array:
								currentValue.Array.Add(number);
								break;

							default:
								if(!force_hide_errors)
									JSONLogger.Error("Fatal error, current JSON value not valid");
								return null;
						}

						state = JSONParsingState.ValueSeparator;

						break;

					case JSONParsingState.Boolean:
						if (jsonString[startPosition] == 't') {
							if (jsonString.Length < startPosition + 4 ||
							    jsonString[startPosition + 1] != 'r' ||
							    jsonString[startPosition + 2] != 'u' ||
							    jsonString[startPosition + 3] != 'e') {
								return Fail("true", startPosition, force_hide_errors);
							}

							switch (currentValue.Type) {

								case tfxJSONValueType.Object:
									currentValue.Obj.values[keyList.Pop()] = new tfxJSONValue(true);
									break;

								case tfxJSONValueType.Array:
									currentValue.Array.Add(new tfxJSONValue(true));
									break;

								default:
									if(!force_hide_errors)
										JSONLogger.Error("Fatal error, current JSON value not valid");
									return null;
							}

							startPosition += 3;
						} else {
							if (jsonString.Length < startPosition + 5 ||
							    jsonString[startPosition + 1] != 'a' ||
							    jsonString[startPosition + 2] != 'l' ||
							    jsonString[startPosition + 3] != 's' ||
							    jsonString[startPosition + 4] != 'e') {
								return Fail("false", startPosition, force_hide_errors);
							}

							switch (currentValue.Type) {

								case tfxJSONValueType.Object:
									currentValue.Obj.values[keyList.Pop()] = new tfxJSONValue(false);
									break;

								case tfxJSONValueType.Array:
									currentValue.Array.Add(new tfxJSONValue(false));
									break;

								default:
									if(!force_hide_errors)
										JSONLogger.Error("Fatal error, current JSON value not valid");
									return null;
							}

							startPosition += 4;
						}

						state = JSONParsingState.ValueSeparator;
						break;

					case JSONParsingState.Array:
						if (jsonString[startPosition] != '[') {
							return Fail('[', startPosition, force_hide_errors);
						}

						tfxJSONValue newArray = new tfxJSONArray();
						if (currentValue != null) {
							newArray.Parent = currentValue;
						}
						currentValue = newArray;

						state = JSONParsingState.Value;
						break;

					case JSONParsingState.EndArray:
						if (jsonString[startPosition] != ']') {
							return Fail(']', startPosition, force_hide_errors);
						}

						if (currentValue.Parent == null) {
							return currentValue.Obj;
						}

						switch (currentValue.Parent.Type) {

							case tfxJSONValueType.Object:
								currentValue.Parent.Obj.values[keyList.Pop()] = new tfxJSONValue(currentValue.Array);
								break;

							case tfxJSONValueType.Array:
								currentValue.Parent.Array.Add(new tfxJSONValue(currentValue.Array));
								break;

							default:
								return Fail("valid object", startPosition, force_hide_errors);
						}
						currentValue = currentValue.Parent;

						state = JSONParsingState.ValueSeparator;
						break;

					case JSONParsingState.Null:
						if (jsonString[startPosition] == 'n') {
							if (jsonString.Length < startPosition + 4 ||
							    jsonString[startPosition + 1] != 'u' ||
							    jsonString[startPosition + 2] != 'l' ||
							    jsonString[startPosition + 3] != 'l') {
								return Fail("null", startPosition, force_hide_errors);
							}

							switch (currentValue.Type) {

								case tfxJSONValueType.Object:
									currentValue.Obj.values[keyList.Pop()] = new tfxJSONValue(tfxJSONValueType.Null);
									break;

								case tfxJSONValueType.Array:
									currentValue.Array.Add(new tfxJSONValue(tfxJSONValueType.Null));
									break;

								default:
									if(!force_hide_errors)
										JSONLogger.Error("Fatal error, current JSON value not valid");
									return null;
							}

							startPosition += 3;
						}
						state = JSONParsingState.ValueSeparator;
						break;

				}
			}
			if(!force_hide_errors)
				JSONLogger.Error("Unexpected end of string");
			return null;
		}

		private static int SkipWhitespace(string str, int pos) {
			for (; pos < str.Length && char.IsWhiteSpace(str[pos]); ++pos) ;
			return pos;
		}

		private static string ParseString(string str, ref int startPosition, bool force_hide_errors = false) {
			if (str[startPosition] != '"' || startPosition + 1 >= str.Length) {
				Fail('"', startPosition, force_hide_errors);
				return null;
			}

			var endPosition = str.IndexOf('"', startPosition + 1);
			if (endPosition <= startPosition) {
				Fail('"', startPosition + 1, force_hide_errors);
				return null;
			}

			while (str[endPosition - 1] == '\\') {
				endPosition = str.IndexOf('"', endPosition + 1);
				if (endPosition <= startPosition) {
					Fail('"', startPosition + 1, force_hide_errors);
					return null;
				}
			}

			var result = string.Empty;

			if (endPosition > startPosition + 1) {
				result = str.Substring(startPosition + 1, endPosition - startPosition - 1);
			}

			startPosition = endPosition;

#if PARSE_ESCAPED_UNICODE
			// Parse Unicode characters that are escaped as \uXXXX
			do {
				Match m = unicodeRegex.Match(result);
				if (!m.Success) {
					break;
				}

				string s = m.Groups[1].Captures[0].Value;
				unicodeBytes[1] = byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
				unicodeBytes[0] = byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
				s = Encoding.Unicode.GetString(unicodeBytes);

				result = result.Replace(m.Value, s);
			} while (true);
#endif

			return result;
		}

		private static double ParseNumber(string str, ref int startPosition) {
			if (startPosition >= str.Length || (!char.IsDigit(str[startPosition]) && str[startPosition] != '-')) {
				return double.NaN;
			}

			var endPosition = startPosition + 1;

			for (;
				endPosition < str.Length && str[endPosition] != ',' && str[endPosition] != ']' && str[endPosition] != '}';
				++endPosition) ;

			double result;
			if (
				!double.TryParse(str.Substring(startPosition, endPosition - startPosition), System.Globalization.NumberStyles.Float,
				                 System.Globalization.CultureInfo.InvariantCulture, out result)) {
				return double.NaN;
			}
			startPosition = endPosition - 1;
			return result;
		}

		private static tfxJSONObject Fail(char expected, int position, bool force_hide_errors = false) {
			return Fail(new string(expected, 1), position, force_hide_errors);
		}

		private static tfxJSONObject Fail(string expected, int position, bool force_hide_errors = false) {
			if(!force_hide_errors)
				JSONLogger.Error("Invalid json string, expecting " + expected + " at " + position);
			return null;
		}

		/// <returns>String representation of this tfxJSONObject</returns>
		public override string ToString() {
			var stringBuilder = new StringBuilder();
			stringBuilder.Append('{');

			foreach (var pair in values) {
				stringBuilder.Append("\"" + pair.Key + "\"");
				stringBuilder.Append(':');
				stringBuilder.Append(pair.Value.ToString());
				stringBuilder.Append(',');
			}
			if (values.Count > 0) {
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		public IEnumerator<KeyValuePair<string, tfxJSONValue>> GetEnumerator() {
			return values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return values.GetEnumerator();
		}

		/// <summary>
		/// Empty this tfxJSONObject of all values.
		/// </summary>
		public void Clear() {
			values.Clear();
		}

		/// <summary>
		/// Remove the tfxJSONValue attached to the given key.
		/// </summary>
		/// <param name="key"></param>
		public void Remove(string key) {
			if (values.ContainsKey(key)) {
				values.Remove(key);
			}
		}
	}
}