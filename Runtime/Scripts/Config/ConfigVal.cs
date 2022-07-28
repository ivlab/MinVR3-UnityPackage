using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// A static class that provides easy, read-only assess to configuration settings loaded from config files.
    ///
    /// **File Naming and Attaching to Unity Game Objects**
    ///
    /// Config Files should be included in your Unity project as text file assets.  This requires them to
    /// have a .txt extension.  Our naming convention is configname.minvr.txt.  Where "configname"
    /// is replaced with "common" for files that are attached to VREngine, since these are common to
    /// all VRConfigs, and the name of the VRConfig for files that are attached to a specific VRConfig.
    ///
    /// **Config File Format**
    /// The basic form of a MinVR3 config file is a list of Key = Value pairs, with one entry per line.
    /// Beyond this, there are a few extra features:
    ///  - Lines that begin with the # symbol are ignored as comments.
    ///  - A \ symbol at the end of the line, means the value is continued on the next line.  If the
    ///    value should actually include a \, use \\ instead.
    ///  - Values for Vector, Matrix, and other array types should be separated by commas.
    ///
    /// An example config file looks like this:
    /// ```
    /// # MinVR3 Example Config File (common.minvr.txt)
    ///
    /// MY_INT = 2
    /// MY_FLOAT = 13.0
    ///
    /// # Booleans can use true/false or True/False
    /// MY_BOOLEAN = False
    ///
    /// # Strings that are not quoted will have any leading or trailing whitespace removed
    /// MY_STRING = Hello my MinVR friends
    ///
    /// # Strings that are quoted can include whitespace and escaped quotes
    /// MY_QUOTED_STRING = "  Hello my \"MinVR friends\"  "
    ///
    /// # Values for Vector, Quaternion, and Matrix types must be separated by commas.  Optionally,
    /// # the values can be enclosed in (), <>, or [] brackets.
    /// MY_VECTOR2 = (4.0, 4.0)
    /// MY_VECTOR3 = (5.0, 5.0, 5.0)
    /// MY_VECTOR4 = (6.0, 6.0, 6.0, 6.0)
    /// MY_QUATERNION = (0.0, 0.0, 0.0, 1.0)
    ///
    /// # Matrix types should list elements in ROW MAJOR order so that the matrix "looks" correct when
    /// # typed into the config file on four lines as shown below.  For a 4x4 homogeneous transformation
    /// # matrix, the last row should always be 0.0, 0.0, 0.0, 1.0, the right column contains the
    /// # translation, and the upper-left 3x3 contains the rotation and scale.  Note the use of \ at the
    /// # end of each line.  This is needed to tell the parser that the value continues on the next line.
    /// MY_MATRIX4X4 = \   
    /// (1.0, 0.0, 0.0, 2.0, \
    ///  0.0, 1.0, 0.0, 3.0, \
    ///  0.0, 0.0, 1.0, 4.0, \
    ///  0.0, 0.0, 0.0, 1.0)
    ///
    /// # A generic int array of any length
    /// MY_INT_ARRAY = 5, 4, 3, 2, 1
    ///
    /// # A generic float array of any length
    /// MY_FLOAT_ARRAY = 0.0, 1.0, 2.0, 3.0, 4.0, 5.0
    ///
    /// # An array of strings
    /// MY_STRING_ARRAY = "One", "Two Three", "Four Five Six", "a b c d \"e\" f", "done"
    /// ```
    /// 
    /// After the config file is parsed, the values specified in the example config file above can be
    /// accessed from within a script like this:
    /// ```
    /// bool b = ConfigVal.Get("MY_BOOLEAN", true);
    /// Debug.Log(b);
    /// 
    /// int i = ConfigVal.Get("MY_INT", 2);
    /// Debug.Log(i);
    /// 
    /// float f = ConfigVal.Get("MY_FLOAT", 1.0f);
    /// Debug.Log(f);
    ///
    /// string s1 = ConfigVal.Get("MY_STRING", "");
    /// Debug.Log(s1);
    /// 
    /// string s2 = ConfigVal.Get("MY_QUOTED_STRING", "");
    /// Debug.Log(s2);
    /// 
    /// Vector2 v2 = ConfigVal.Get("MY_VECTOR2", Vector2.zero);
    /// Debug.Log(v2);
    /// 
    /// Vector3 v3 = ConfigVal.Get("MY_VECTOR3", Vector3.zero);
    /// Debug.Log(v3);
    /// 
    /// Vector4 v4 = ConfigVal.Get("MY_VECTOR4", Vector4.zero);
    /// Debug.Log(v4);
    /// 
    /// Quaternion q = ConfigVal.Get("MY_QUATERNION", Quaternion.identity);
    /// Debug.Log(q);
    /// 
    /// Matrix4x4 m4 = ConfigVal.Get("MY_MATRIX4X4", Matrix4x4.identity);
    /// Debug.Log(m4);
    ///
    /// int[] intArray = new int[0];
    /// intArray = ConfigVal.Get("MY_INT_ARRAY", intArray);
    /// string iStr = "";
    /// foreach (int i in intArray) {
    ///     iStr += i.ToString() + ", ";
    /// }
    /// Debug.Log(iStr);
    /// 
    /// float[] floatArray = new float[0];
    /// floatArray = ConfigVal.Get("MY_FLOAT_ARRAY", floatArray);
    /// string fStr = "";
    /// foreach (float f in floatArray) {
    ///     fStr += f.ToString() + ", ";
    /// }
    /// Debug.Log(fStr);
    ///
    /// string[] strArray = new string[0];
    /// strArray = ConfigVal.Get("MY_STRING_ARRAY", strArray);
    /// foreach (string s in strArray) {
    ///     Debug.Log(s);
    /// }
    /// 
    /// ```
    /// Note that the values are stored internally as strings.  They are not interpreted as a specific type until
    /// they are accessed with a call to ConfigVal.Get() and the Get() method tries to convert the string to the
    /// requested type.  To avoid re-converting each frame, consider accessing ConfigVals from within Start() or
    /// Awake() and then saving the result in a member variable within your class.  
    /// </summary>
    public class ConfigVal
    {
        /// <summary>
        /// This template method works for accessing ConfigVals as any type that implements IConvertible.
        /// This includes bool, int, float, etc.  The type can typically be inferred from the type of the
        /// default value, so even though this is a template method, you can call it without using angle
        /// brackets, like so:
        /// ```
        /// bool b = ConfigVal.Get("MY_BOOLEAN", true);
        /// Debug.Log(b);
        /// 
        /// int i = ConfigVal.Get("MY_INT", 2);
        /// Debug.Log(i);
        /// 
        /// float f = ConfigVal.Get("MY_FLOAT", 1.0f);
        /// Debug.Log(f);
        /// ```
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The ConfigVal's key (lookup) name.</param>
        /// <param name="defaultValue">Value to return if the key is not found in the map or there is a problem
        /// converting the value found in the map to the type of defaultValue.</param>
        /// <param name="warnOnMissing">Logs a warning if the key is not found in the map.</param>
        /// <returns>The value found in the map under 'key' converted to be the same type as defaultValue.</returns>
        static public T Get<T>(string key, T defaultValue, bool warnOnMissing = true) where T : IConvertible
        {
            if (!m_ConfigMap.ContainsKey(key)) {
                if (warnOnMissing) {
                    Debug.LogWarning($"No ConfigVal entry found for {key}");
                }
                return defaultValue;
            }

            T converted = (T)Convert.ChangeType(m_ConfigMap[key], typeof(T));
            if (converted == null) {
                Debug.LogError($"ConfigVal[{key}] cannot convert value to type {typeof(T).Name} for value = '{m_ConfigMap[key]}'");
                return defaultValue;
            }

            return converted;
        }


        /// <summary>
        /// String values can be contained in double quotes or not.  If quotes are used, the value returned from Get
        /// will be the characters inside the quotes.  If you want this value to include an actual quote character,
        /// then escape it with a backslach (e.g., \").  If quotes are not used, then the value of the string will
        /// start with the first non-whitespace character after the equals sign and end with the last non-whitespace
        /// character on the line.
        /// ```
        /// ```
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="warnOnMissing"></param>
        /// <returns></returns>
        static public string Get(string key, string defaultValue, bool warnOnMissing = true)
        {
            if (!m_ConfigMap.ContainsKey(key)) {
                if (warnOnMissing) {
                    Debug.LogWarning($"No ConfigVal entry found for {key}");
                }
                return defaultValue;
            }

            string s = m_ConfigMap[key];
            Match match = Regex.Match(s, RegExPattern_QuotedString);
            if (match.Success) {
                s = match.Value;
                // remove beginning and ending quotes
                s = s.Substring(1);
                s = s.Substring(0, s.Length - 1);
                // replace escaped quotes within the string with regular quotes
                s = Regex.Replace(s, RegExPattern_EscapedQuote, "\"");
            } else {
                s = s.Trim();
            }
            return s;
        }


        // Additional implementations for useful types that do not implement IConvertible

        /// <summary>
        /// ```
        /// Vector2 v2 = ConfigVal.Get("MY_VECTOR2", Vector2.zero);
        /// Debug.Log(v2);
        /// ```
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="warnOnMissing"></param>
        /// <returns></returns>
        static public Vector2 Get(string key, Vector2 defaultValue, bool warnOnMissing = true)
        {
            float[] floats = null;
            if (TryGetFloatArray(key, defaultValue, warnOnMissing, 2, ref floats)) {
                return new Vector2(floats[0], floats[1]);
            } else {
                return defaultValue;
            }
        }

        /// <summary>
        /// /// ```
        /// Vector3 v3 = ConfigVal.Get("MY_VECTOR3", Vector3.zero);
        /// Debug.Log(v3);
        /// ```
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="warnOnMissing"></param>
        /// <returns></returns>
        static public Vector3 Get(string key, Vector3 defaultValue, bool warnOnMissing = true)
        {
            float[] floats = null;
            if (TryGetFloatArray(key, defaultValue, warnOnMissing, 3, ref floats)) {
                return new Vector3(floats[0], floats[1], floats[2]);
            } else {
                return defaultValue;
            }
        }

        /// <summary>
        /// ```
        /// Vector4 v4 = ConfigVal.Get("MY_VECTOR4", Vector4.zero);
        /// Debug.Log(v4);
        /// ```
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="warnOnMissing"></param>
        /// <returns></returns>
        static public Vector4 Get(string key, Vector4 defaultValue, bool warnOnMissing = true)
        {
            float[] floats = null;
            if (TryGetFloatArray(key, defaultValue, warnOnMissing, 4, ref floats)) {
                return new Vector4(floats[0], floats[1], floats[2], floats[3]);
            } else {
                return defaultValue;
            }
        }

        /// <summary>
        /// ```
        /// Quaternion q = ConfigVal.Get("MY_QUATERNION", Quaternion.identity);
        /// Debug.Log(q);
        /// ```
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="warnOnMissing"></param>
        /// <returns></returns>
        static public Quaternion Get(string key, Quaternion defaultValue, bool warnOnMissing = true)
        {
            float[] floats = null;
            if (TryGetFloatArray(key, defaultValue, warnOnMissing, 4, ref floats)) {
                return new Quaternion(floats[0], floats[1], floats[2], floats[3]);
            } else {
                return defaultValue;
            }
        }

        /// <summary>
        /// ```
        /// Matrix4x4 m4 = ConfigVal.Get("MY_MATRIX4X4", Matrix4x4.identity);
        /// Debug.Log(m4);
        /// ```
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="warnOnMissing"></param>
        /// <returns></returns>
        static public Matrix4x4 Get(string key, Matrix4x4 defaultValue, bool warnOnMissing = true)
        {
            float[] floats = null;
            if (TryGetFloatArray(key, defaultValue, warnOnMissing, 16, ref floats)) {
                Vector4 column0 = new Vector4(floats[0], floats[4], floats[8], floats[12]);
                Vector4 column1 = new Vector4(floats[1], floats[5], floats[9], floats[13]);
                Vector4 column2 = new Vector4(floats[2], floats[6], floats[10], floats[14]);
                Vector4 column3 = new Vector4(floats[3], floats[7], floats[11], floats[15]);
                return new Matrix4x4(column0, column1, column2, column3);
            } else {
                return defaultValue;
            }
        }

        /// <summary>
        /// ```
        /// float[] floatArray = new float[0];
        /// floatArray = ConfigVal.Get("MY_FLOAT_ARRAY", floatArray);
        /// string fStr = "";
        /// foreach (float f in floatArray) {
        ///     fStr += f.ToString() + ", ";
        /// }
        /// Debug.Log(fStr);
        /// ```
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="warnOnMissing"></param>
        /// <returns></returns>
        static public float[] Get(string key, float[] defaultValue, bool warnOnMissing = true)
        {
            float[] floats = null;
            if (TryGetFloatArray(key, defaultValue, warnOnMissing, 0, ref floats)) {
                return floats;
            } else {
                return defaultValue;
            }
        }


        /// <summary>
        /// ```
        /// int[] intArray = new int[0];
        /// intArray = ConfigVal.Get("MY_INT_ARRAY", intArray);
        /// string iStr = "";
        /// foreach (int i in intArray) {
        ///     iStr += i.ToString() + ", ";
        /// }
        /// Debug.Log(iStr);
        /// ```
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="warnOnMissing"></param>
        /// <returns></returns>
        static public int[] Get(string key, int[] defaultValue, bool warnOnMissing = true)
        {
            if (!m_ConfigMap.ContainsKey(key)) {
                if (warnOnMissing) {
                    Debug.LogWarning($"No ConfigVal entry found for {key}");
                }
                return defaultValue;
            }

            // get the value as a string, strip off any brackets and leading/trailing whitespace to end up with
            // a string of comma separated ints
            string valueStr = m_ConfigMap[key]
                .Replace("(", "").Replace(")", "")
                .Replace("<", "").Replace(">", "")
                .Replace("[", "").Replace("]", "")
                .Trim();

            int[] intValues = Array.ConvertAll(valueStr.Split(','), int.Parse);

            if (intValues.Length > 0) {
                return intValues;
            } else {
                return defaultValue;
            }
        }


        /// <summary>
        /// When defining a string array in a config file, the individual entries within the array must be contained
        /// in double quotes and be separated by commas.  For example:
        /// ```
        /// MY_STRING_ARRAY = "One", "Two Three", "Four Five Six", "a b c d \"e\" f", "done"
        /// ```
        /// This array could be read inside a script with code like this:
        /// ```
        /// string[] strArray = new string[0];
        /// strArray = ConfigVal.Get("MY_STRING_ARRAY", strArray);
        /// foreach (string s in strArray) {
        ///     Debug.Log(s);
        /// }
        /// ```
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="warnOnMissing"></param>
        /// <returns></returns>
        static public string[] Get(string key, string[] defaultValue, bool warnOnMissing = true)
        {
            if (!m_ConfigMap.ContainsKey(key)) {
                if (warnOnMissing) {
                    Debug.LogWarning($"No ConfigVal entry found for {key}");
                }
                return defaultValue;
            }
            
            string valueStr = m_ConfigMap[key];

            MatchCollection matches = Regex.Matches(valueStr, RegExPattern_QuotedString);
            string[] strings = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++) {
                strings[i] = matches[i].Value;
                // remove beginning and ending quotes
                strings[i] = strings[i].Substring(1);
                strings[i] = strings[i].Substring(0, strings[i].Length - 1);
                // replace escaped quotes within the string with regular quotes
                strings[i] = Regex.Replace(strings[i], RegExPattern_EscapedQuote, "\"");
            }

            if (strings.Length > 0) {
                return strings;
            } else {
                return defaultValue;
            }
        }


        static public void ParseConfigFile(TextAsset textAsset)
        {
            string text = textAsset.text;
            string[] lines = text.Split('\n');
            int i = 0;
            while (i < lines.Length) {
                int lineNo = i;
                string line = lines[i];

                // check if the line ends in a single backslash followed by optional whitespace
                Match lineEndsInSingleBackslash = Regex.Match(line, RegExPattern_LineEndsInSingleBackslash);
                while (lineEndsInSingleBackslash.Success) {
                    // remove the backslash and optional whitespace
                    line = Regex.Replace(line, RegExPattern_LineEndsInSingleBackslash, string.Empty);
                    // advance to the next line and append it to the current line
                    i++;
                    if (i < lines.Length) {
                        line = line + lines[i];
                    }
                    // check again, this next line may also end in a backslash
                    lineEndsInSingleBackslash = Regex.Match(line, RegExPattern_LineEndsInSingleBackslash);
                }

                // only proceed if the line is not whitespace only and is not a comment
                Match notAComment = Regex.Match(line, RegExPattern_NotAComment);
                Match notEmpty = Regex.Match(line, RegExPattern_NotEmpty);
                if ((notAComment.Success) && (notEmpty.Success)) {
                    // handle escaped backslashes by replacing any double-backslashes \\ with a single backslash \.
                    line = Regex.Replace(line, RegExPattern_DoubleBackslash, "\\");

                    // make sure the line is of the form: key = value
                    Match haveKeyEqualsValue = Regex.Match(line, RegExPattern_SomethingEqualsSomething);
                    if (!haveKeyEqualsValue.Success) {
                        Debug.LogError($"{textAsset.name}:{lineNo}: Invalid format for config file line: {line}");
                    } else {
                        int equalsIndex = line.IndexOf('=');
                        string key = line.Substring(0, equalsIndex).Trim();
                        string value = line.Substring(equalsIndex + 1).Trim();
                        //Debug.Log($"{key} == {value}");
                        AddOrReplace(key, value);
                    }
                }
                i++;
            }
        }


        // Helper for adding to the map
        static private void AddOrReplace(string key, string value)
        {
            if (m_ConfigMap == null) {
                m_ConfigMap = new Dictionary<string, string>();
            }
            if (m_ConfigMap.ContainsKey(key)) {
                m_ConfigMap[key] = value;
            } else {
                m_ConfigMap.Add(key, value);
            }
        }

        // Helper for types include a list of comma separate floats.  expectedLength==0 is a special flag for any length is ok
        static private bool TryGetFloatArray<T>(string key, T defaultValue, bool warnOnMissing, int expectedLength, ref float[] floatValues)
        {
            if (!m_ConfigMap.ContainsKey(key)) {
                if (warnOnMissing) {
                    Debug.LogWarning($"No ConfigVal entry found for {key}");
                }
                return false;
            }

            // get the value as a string, strip off any brackets and leading/trailing whitespace to end up with
            // a string of comma separated floats
            string valueStr = m_ConfigMap[key]
                .Replace("(", "").Replace(")", "")
                .Replace("<", "").Replace(">", "")
                .Replace("[", "").Replace("]", "")
                .Trim();

            floatValues = Array.ConvertAll(valueStr.Split(','), float.Parse);


            if (expectedLength == 0) {
                // special case where expected length is flexible
                if (floatValues.Length == 0) {
                    return false;
                } else {
                    return true;
                }
            } else {
                // length should match expected length
                if (floatValues.Length != expectedLength) {
                    Debug.LogError($"ConfigVal[{key}] cannot convert value to type {typeof(T).Name} for value = '{m_ConfigMap[key]}'");
                    return false;
                } else {
                    return true;
                }
            }
        }



        /// <summary>
        /// Regex pattern to detect a single backslash optionally followed by whitespace at the end of a line
        /// </summary>
        static string RegExPattern_LineEndsInSingleBackslash = @"[^\\]\\\s*$";

        /// <summary>
        /// Regex pattern to match double backslash \\
        /// </summary>
        static string RegExPattern_DoubleBackslash = @"\\\\";

        /// <summary>
        /// Regex pattern to match a line of the form:
        /// [optional-whitespace][non-whitespace][optional-whitespace]=[optional-whitespace][non-whitespace]
        /// </summary>
        static string RegExPattern_SomethingEqualsSomething = @"^[^\S\n\r]*\S*[^\S\n\r]*=[^\S\n\r]*\S*";


        /// <summary>
        /// Regex pattern to match a line that does not begin with a # symbol
        /// </summary>
        static string RegExPattern_NotAComment = @"^[^#]";

        /// <summary>
        /// Regex pattern to match a line that contains anything other than whitespace
        /// </summary>
        static string RegExPattern_NotEmpty = @"\S";

        /// <summary>
        /// Regex pattern to match quoted strings
        /// </summary>
        static string RegExPattern_QuotedString = @"""[^""\\]*(?:\\.[^""\\]*)*""";

        /// <summary>
        /// Regex pattern to match an escaped double quote
        /// </summary>
        static string RegExPattern_EscapedQuote = @"\\""";


        static Dictionary<string, string> m_ConfigMap;
    }

} // end namespace
