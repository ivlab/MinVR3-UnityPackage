#include "config_val.h"

#include <assert.h>
#include <iostream>
#include <map>
#include <string>
#include <sstream>

#include "minvr3_utils.h"

// static member var
std::map<std::string, std::string> ConfigVal::map_;

bool ConfigVal::Contains(const std::string &key) {
    return map_.find(key) != map_.end();
}

void ConfigVal::AddOrReplace(const std::string &key, const std::string &value) {
    map_[key] = value;
}

void ConfigVal::AddOrAppend(const std::string &key, const std::string &value) {
    if (ConfigVal::Contains(key)) {
        map_[key] = map_[key] + " " + value;
    }
    else {
        map_[key] = value;
    }
}

void ConfigVal::Remove(const std::string &key) {
    map_.erase(key);
}

void ConfigVal::Clear() {
    map_.clear();
}

void ConfigVal::ParseConfigFile(std::string filename) {
    if (!MinVRUtils::FileExists(filename)) {
        return;
    }

    std::string text = MinVRUtils::ReadWholeFile(filename);

    // CLEANUP STEPS:
    // 1. add a \n so that every file ends in at least one \n
    text = text + std::string("\n");

    // 2. convert all endline characters to \n's
    MinVRUtils::ReplaceAllInPlace(text, "\r", "\n");

    // 3. remove any cases of two \n's next to each other
    MinVRUtils::ReplaceAllInPlace(text, "\n\n", "\n");

    // 4. remove any comments indicated by lines starting with #
    int startPos = 0;
    int commentPos = text.find("\n#", startPos);
    while (commentPos != std::string::npos) {
        int endOfCommentLine = text.find("\n", commentPos + 1);
        assert(endOfCommentLine != std::string::npos);
        text = text.substr(0, commentPos) + text.substr(endOfCommentLine);
        startPos = commentPos;
        commentPos = text.find("\n#", startPos);
    }

    // 5. lines ending in a single \ followed by a newline should be combined with the next line.
    startPos = 0;
    int slashNewlinePos = text.find("\\\n", startPos);
    while (slashNewlinePos != std::string::npos) {
        text = text.substr(0, slashNewlinePos) + text.substr(slashNewlinePos + 1);
        slashNewlinePos = text.find("\\\n", startPos);
    }

    // 6. if we have two backslashes \\ treat this as an escape sequence for
    // a single backslash, so replace the two with one
    MinVRUtils::ReplaceAllInPlace(text, "\\\\", "\\");


    // PARSE KEY = VALUE PAIRS ON THE REMAINING LINES
    while (text.size() > 0) {
        int eol = text.find("\n");
        std::string line = MinVRUtils::TrimWhitespace(text.substr(0, eol));
        if (line.length() > 1) {
        
            // split at the = sign
            int equalsPos = line.find("=");
            if (equalsPos == std::string::npos) {
                std::cout << "ConfigVal::ParseConfigFile() cannot parse the following line: '" << line << "'" << std::endl;
            }
            else {
                std::string value = MinVRUtils::TrimWhitespace(line.substr(equalsPos + 1));
                
                if (text[equalsPos-1] == '+') {
                    // += adds a space and concatenates the value with the current value for the key if the
                    // key is already contained in the map
                    std::string key = MinVRUtils::TrimWhitespace(line.substr(0, equalsPos - 1));
                    AddOrAppend(key, value);
                }
                else {
                    // normal = adds a new key or overwrites the value of an existing key in the map
                    std::string key = MinVRUtils::TrimWhitespace(line.substr(0, equalsPos));
                    AddOrReplace(key, value);
                }
            }
        }
        
        text = text.substr(eol+1);
    }
}

void ConfigVal::DebugPrintMap() {
    std::cout << "[start of ConfigVal map contents]" << std::endl;
    for (std::map<std::string,std::string>::iterator it = map_.begin(); it != map_.end(); it++) {
        std::cout << it->first << "  =  " << it->second << std::endl;
    }
    std::cout << "[end of ConfigVal map contents]" << std::endl;
}
