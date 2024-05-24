#include "minvr3_utils.h"

#include <algorithm>
#include <fstream>
#include <sstream>
#include <vector>

#ifdef WIN32
    #include <windows.h>
#else
    #include <sys/stat.h>
#endif


bool MinVRUtils::FileExists(const std::string &filename) {
#ifdef WIN32
	LPCTSTR  szPath = (LPCTSTR)filename.c_str();
	DWORD dwAttrib = GetFileAttributes(szPath);
	return (dwAttrib != INVALID_FILE_ATTRIBUTES &&
		!(dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
#else
    struct stat buf;
    return (stat(filename.c_str(), &buf) == 0);
#endif
}

std::string MinVRUtils::ReadWholeFile(const std::string &filename) {
    std::ifstream t(filename);
    std::stringstream buffer;
    buffer << t.rdbuf();
    std::string s = buffer.str();
    return s;
}

bool MinVRUtils::BeginsWith(const std::string &test, const std::string &pattern) {
    if (test.size() >= pattern.size()) {
        for (int i = 0; i < (int)pattern.size(); ++i) {
            if (pattern[i] != test[i]) {
                return false;
            }
        }
        return true;
    } else {
        return false;
    }
}

std::string MinVRUtils::TrimWhitespace(const std::string &s) {
    size_t left = 0;
    // Trim from left
    while ((left < s.length()) && iswspace(s[left])) {
        ++left;
    }
    int right = s.length() - 1;
    // Trim from right
    while ((right > (int)left) && iswspace(s[right])) {
        --right;
    }
    return s.substr(left, right - left + 1);
}

// https://stackoverflow.com/questions/2896600/how-to-replace-all-occurrences-of-a-character-in-string
std::string MinVRUtils::ReplaceAll(std::string str, const std::string &from, const std::string &to) {
    size_t start_pos = 0;
    while((start_pos = str.find(from, start_pos)) != std::string::npos) {
        str.replace(start_pos, from.length(), to);
        start_pos += to.length(); // Handles case where 'to' is a substring of 'from'
    }
    return str;
}
  
void MinVRUtils::ReplaceAllInPlace(std::string &str, const std::string &from, const std::string &to)
{
    size_t start_pos = 0;
    while((start_pos = str.find(from, start_pos)) != std::string::npos) {
        str.replace(start_pos, from.length(), to);
        start_pos += to.length(); // Handles case where 'to' is a substring of 'from'
    }
}
 
std::vector<std::string> MinVRUtils::Split(const std::string &s, const std::string &delim, const bool keep_empty) {
    std::vector<std::string> result;
    if (delim.empty()) {
        result.push_back(s);
        return result;
    }
    std::string::const_iterator substart = s.begin(), subend;
    while (true) {
        subend = std::search(substart, s.end(), delim.begin(), delim.end());
        std::string temp(substart, subend);
        if (keep_empty || !temp.empty()) {
            result.push_back(temp);
        }
        if (subend == s.end()) {
            break;
        }
        substart = subend + delim.size();
    }
    return result;
}

size_t MinVRUtils::FindNonEscapedQuote(const std::string &str, size_t pos) {
    size_t start = 0;
    size_t quotePos = str.find('"', pos);
    while ((quotePos != std::string::npos) && (quotePos != 0) && (str[quotePos-1] == '\\')) {
        pos = quotePos + 1;
        quotePos = str.find('"', pos);
    }
    return quotePos;
}
