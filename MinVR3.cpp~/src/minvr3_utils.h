#ifndef MINVR_UTILS_H
#define MINVR_UTILS_H

#include <string>
#include <vector>

/**
 * Collection of utility functions, mostly for working with files and strings.
*/
class MinVRUtils
{
public:

    static bool FileExists(const std::string &filename);

    static std::string ReadWholeFile(const std::string &filename);

    static bool BeginsWith(const std::string &test, const std::string &pattern);

    static std::string TrimWhitespace(const std::string &s);

    static std::string ReplaceAll(std::string str, const std::string &from, const std::string &to);

    static void ReplaceAllInPlace(std::string &str, const std::string &from, const std::string &to);

    static std::vector<std::string> Split(const std::string &s, const std::string &delim, const bool keep_empty = true);
    
    /// Finds the next " character in the string, ignoring any instances of \"
    static size_t FindNonEscapedQuote(const std::string &str, size_t pos);
};

#endif
