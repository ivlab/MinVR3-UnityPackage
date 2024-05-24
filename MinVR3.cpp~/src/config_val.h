
#ifndef CONFIG_VAL_H
#define CONFIG_VAL_H

#include <iostream>
#include <map>
#include <string>
#include <sstream>
#include <vector>

#include "minvr3_utils.h"

/// A static class that provides easy, read-only assess to configuration settings loaded from config files.
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
/// # the values can be enclosed in (), &lt;&gt;, or [] brackets.
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
/// accessed using the Get() member function.  
/// ```
/// bool b = ConfigVal::Get("MY_BOOLEAN", true);
/// int i = ConfigVal::Get("MY_INT", 2);
/// float f = ConfigVal::Get("MY_FLOAT", 1.0f);
/// std::string s1 = ConfigVal::Get("MY_STRING", "");
/// std::string s2 = ConfigVal::Get("MY_QUOTED_STRING", "");
/// std::vector<int> vi = ConfigVal::Get("MY_INT_ARRAY", std::vector<int>());
/// std::vector<float> vf = ConfigVal::Get("MY_FLOAT_ARRAY", std::vector<float>());
/// std::vector<bool> vb = ConfigVal::Get("MY_BOOL_ARRAY", std::vector<bool>());
/// std::vector<std::string> vs = ConfigVal::Get("MY_STRING_ARRAY", std::vector<std::string>());
/// ```
/// Note that the values are stored internally as strings.  They are not interpreted as a specific type until
/// they are accessed with a call to ConfigVal.Get() and the Get() method tries to convert the string to the
/// requested type.  To avoid re-converting each frame, consider accessing ConfigVals during startup and 
/// saving the result in a member variable for fast access while rendering. Any types that support the 
/// stream >> operator should work with Get(), and additional type-specific implementations of Get() can
/// be provided for any other types that do not support this operator.  
class ConfigVal
{
public:

    /** 
     * The main function for getting data out of the map.  This should work for any types that
     * define the >> stream operator.  To work with other types, this class can be extended with
     * additional, type-specific implementations of Get().
     */
    template <class T>
    static T Get(const std::string &key, const T &defaultValue, bool warnOnMissing = true) {
        if (ConfigVal::Contains(key)) {
            std::string valAsString = ConfigVal::map_[key];
            std::istringstream is(valAsString.c_str());
            T val;
            is >> val;
            if (is) {
                return val;
            } 
            else {
                std::cerr << "Problem retyping ConfigVal[" << key << "] = '" << valAsString << "'" << std::endl;
            }
        } 
        if (warnOnMissing) {
            std::cout << "No ConfigVal entry found for " << key << std::endl;
        }
        return defaultValue;
    }

    /** Overload for bool types to handle values that are spelled out (e.g., "false", "True", "FALSE")
     */
    static bool Get(const std::string &key, const bool &defaultValue, bool warnOnMissing = true) {
        if (ConfigVal::Contains(key)) {
            std::string valAsString = ConfigVal::map_[key];
            if ((valAsString == "TRUE") || (valAsString == "True") ||
                (valAsString == "true") || (valAsString == "1")) {
                return true;
            }
            else if ((valAsString == "FALSE") || (valAsString == "False") ||
                     (valAsString == "false") || (valAsString == "0")) {
                return false;
            }
            else {
                return Get(key, defaultValue, warnOnMissing);
            }
        }
        if (warnOnMissing) {
            std::cout << "No ConfigVal entry found for " << key << std::endl;
        }
        return defaultValue;
    }
    
    /** Overload for string types to handle strings in quotes
     */
    static std::string Get(const std::string& key, const char defaultValue[], bool warnOnMissing = true) {
        return Get(key, std::string(defaultValue), warnOnMissing);
    }

    /** Overload for string types to handle strings in quotes
     */
    static std::string Get(const std::string &key, const std::string &defaultValue, bool warnOnMissing = true) {
        if (ConfigVal::Contains(key)) {
            std::string value = ConfigVal::map_[key];
            
            // if first and last char are quotes and the last quote is not escaped, remove quotes.
            if ((value.length() >= 2) && (value[0] == '"') && (value[value.length()-1] == '"') &&
                (value[value.length()-2] != '\\')) {
                value = value.substr(1, value.length() - 2);
            }

            // convert any escaped quotes into regular quotes
            MinVRUtils::ReplaceAllInPlace(value, "\\\"", "\"");
            
            return value;
        }
        if (warnOnMissing) {
            std::cout << "No ConfigVal entry found for " << key << std::endl;
        }
        return defaultValue;
    }
    
    /** Overload for arrays of generic types stored in a std::vector<>
     */
    template <class T>
    static std::vector<T> Get(const std::string &key, const std::vector<T> &defaultValue, bool warnOnMissing = true) {
        if (ConfigVal::Contains(key)) {
            std::string valAsString = MinVRUtils::TrimWhitespace(ConfigVal::map_[key]);
            if (((valAsString[0] == '(') && (valAsString[valAsString.length()-1] == ')')) ||
                ((valAsString[0] == '[') && (valAsString[valAsString.length()-1] == ']'))) {
                valAsString = valAsString.substr(1, valAsString.length() - 2);
            }
            std::vector<std::string> valAsStringArray = MinVRUtils::Split(valAsString, ",");
            std::vector<T> valArray;
            
            for (auto it = valAsStringArray.begin(); it != valAsStringArray.end(); it++) {
                std::istringstream is(it->c_str());
                T val;
                is >> val;
                if (is) {
                    valArray.push_back(val);
                }
                else {
                    std::cerr << "Problem retyping ConfigVal[" << key << "] = '" << valAsString << "'" << std::endl;
                    return defaultValue;
                }
            }
            return valArray;
        }
        if (warnOnMissing) {
            std::cout << "No ConfigVal entry found for " << key << std::endl;
        }
        return defaultValue;
    }
    
    /** Override for arrays of bool stored in a std::vector<> to handle values that are spelled out (e.g., "false", "True", "FALSE")
     */
    static std::vector<bool> Get(const std::string &key, const std::vector<bool> &defaultValue, bool warnOnMissing = true) {
        if (ConfigVal::Contains(key)) {
            std::string valAsString = MinVRUtils::TrimWhitespace(ConfigVal::map_[key]);
            if (((valAsString[0] == '(') && (valAsString[valAsString.length()-1] == ')')) ||
                ((valAsString[0] == '[') && (valAsString[valAsString.length()-1] == ']'))) {
                valAsString = valAsString.substr(1, valAsString.length() - 2);
            }
            std::vector<std::string> valAsStringArray = MinVRUtils::Split(valAsString, ",");
            std::vector<bool> valArray;
            
            for (auto it = valAsStringArray.begin(); it != valAsStringArray.end(); it++) {
                std::string subValAsString = MinVRUtils::TrimWhitespace(*it);
                if ((subValAsString == "TRUE") || (subValAsString == "True") ||
                    (subValAsString == "true") || (subValAsString == "1")) {
                    valArray.push_back(true);
                }
                else if ((subValAsString == "FALSE") || (subValAsString == "False") ||
                         (subValAsString == "false") || (subValAsString == "0")) {
                    valArray.push_back(false);
                }
                else {
                    std::istringstream is(subValAsString);
                    bool val;
                    is >> val;
                    if (is) {
                        valArray.push_back(val);
                    }
                    else {
                        std::cerr << "Problem retyping ConfigVal[" << key << "] = '" << valAsString << "'" << std::endl;
                        return defaultValue;
                    }
                }
            }
            return valArray;
        }
        if (warnOnMissing) {
            std::cout << "No ConfigVal entry found for " << key << std::endl;
        }
        return defaultValue;
    }
    
    /** Override for arrays of std::string stored in a std::vector<>.  For string arrays, we require that each element of the array is enclosed in
     quotes, and they are all separated by commas, e.g.,:
          MY_STRING_ARRAY = "one", "two", "three"
     */
    static std::vector<std::string> Get(const std::string &key, const std::vector<std::string> &defaultValue, bool warnOnMissing = true) {
        if (ConfigVal::Contains(key)) {
            std::vector<std::string> rv;
            std::string valAsString = MinVRUtils::TrimWhitespace(ConfigVal::map_[key]);
            size_t pos = 0;
            int openQuote = MinVRUtils::FindNonEscapedQuote(valAsString, pos);
            while (openQuote != std::string::npos) {
                pos = openQuote + 1;
                int closeQuote = MinVRUtils::FindNonEscapedQuote(valAsString, pos);
                if (closeQuote != std::string::npos) {
                    pos = closeQuote + 1;
                    std::string subVal = valAsString.substr(openQuote + 1, closeQuote - openQuote - 1);
                    // convert any escaped quotes into regular quotes
                    MinVRUtils::ReplaceAllInPlace(subVal, "\\\"", "\"");
                    rv.push_back(subVal);
                }
                else {
                    std::cerr << "Problem with unmatched quotes when retyping ConfigVal[" << key << "] = '" << valAsString << "'" << std::endl;
                    return defaultValue;
                }
                openQuote = MinVRUtils::FindNonEscapedQuote(valAsString, pos);
            }
            return rv;
        }
        if (warnOnMissing) {
            std::cout << "No ConfigVal entry found for " << key << std::endl;
        }
        return defaultValue;
    }
    
    
    /**
     * True if a value for the specified ConfigVal is stored in the map.
    */
    static bool Contains(const std::string &key);

    /**
     * Adds a new key, value pair to the map, or if the key already exists in the map, replaces
     * its previous value with the new one provided.
    */
    static void AddOrReplace(const std::string &key, const std::string &value);

    /**
     * Adds a new key, value pair to the map, or if the key already exists in the map, appends a 
     * single space and the new value provided to the end of the existing value.
    */
    static void AddOrAppend(const std::string &key, const std::string &value);

    /**
     * Removes a single entry from the map.
    */
    static void Remove(const std::string &key);

    /**
     * Clears the entire map.
    */
    static void Clear();

    /**
     * Loads and parses a text file written in the format described in the comments at the top of the class.
     * This can be called multiple times to load multiple files into the same map.
    */
    static void ParseConfigFile(std::string filename);

    /**
     * Prints all key=value pairs stored in the map to stdout.
    */
    static void DebugPrintMap();

private:
    static std::map<std::string, std::string> map_;
};

#endif
