
#include <string>
#include <iostream>

#include <minvr3.h>

void helpAndExit(int exitcode) {
    std::cout << "Help: This program prints the content of the ConfigVal map after parsing" << std::endl;
    std::cout << "the config files and/or key=value pairs specified on the command line." << std::endl;
    std::cout << "The key=value are processed in the order specified on the command line," << std::endl;
    std::cout << "with new values overriding previous ones." << std::endl << std::endl;
    std::cout << "Command line format: [-f filename [-f filename...]] [-c key=value [-c key=value...]]" << std::endl;
    std::cout << std::endl;
    std::cout << "All parameters are optional and can be specified multiple times:" << std::endl;
    std::cout << "   -f filename      Specifies a key=value config file to process." << std::endl;
    std::cout << "   -c key=value     Specifies a config key=value pair to process." << std::endl;
    std::cout << "   -h               Show this help message." << std::endl;
    exit(exitcode);
}

int main(int argc, char* argv[]) {
    for (int i=1; i < argc; i++) {
        std::string arg(argv[i]);
        if ((arg == "-f") || (arg == "--configfile")) {
            if (argc > i+1) {
                std::string filename(argv[i+1]);
                ConfigVal::ParseConfigFile(filename);
                i++;
            }
            else {
                std::cerr << "Command line error: missing config file name." << std::endl;
                helpAndExit(1);
            }
        }
        else if ((arg == "-c") || (arg == "--configval")) {
            if (argc > i+1) {
                std::string keyValPair(argv[i+1]);
                int equalsPos = keyValPair.find("=");
                if (equalsPos != std::string::npos) {
                    std::string key = keyValPair.substr(0, equalsPos);
                    std::string val = keyValPair.substr(equalsPos + 1);
                    ConfigVal::AddOrReplace(key, val);
                    i++;
                }
                else {
                    std::cerr << "Command line error: badly formed key=value pair." << std::endl;
                    helpAndExit(1);
                }
            }
            else {
                std::cerr << "Command line error: missing key=value pair." << std::endl;
                helpAndExit(1);
            }
        }
        else if ((arg == "-h") || (arg == "--help")) {
            helpAndExit(0);
        }
        else {
            std::cout << "Warning: Did not recognize command line argument: '" << arg << "'" << std::endl;
        }
    }

    ConfigVal::DebugPrintMap();
    
    
    std::cout << "[test getting and retyping a few values]" << std::endl;
    
    bool b = ConfigVal::Get("MY_BOOLEAN", true);
    std::cout << "MY_BOOLEAN is " << b << std::endl;
    
    int i = ConfigVal::Get("MY_INT", -1);
    std::cout << "MY_INT is " << i << std::endl;

    float f = ConfigVal::Get("MY_FLOAT", -1.1f);
    std::cout << "MY_FLOAT is " << f << std::endl;

    double d = ConfigVal::Get("MY_DOUBLE", -1.1);
    std::cout << "MY_DOUBLE is " << d << std::endl;

    std::string s = ConfigVal::Get("MY_STRING", std::string());
    std::cout << "MY_STRING is " << s << std::endl;

    std::string qs = ConfigVal::Get("MY_QUOTED_STRING", std::string());
    std::cout << "MY_QUOTED_STRING is " << qs << std::endl;

    std::vector<int> vi = ConfigVal::Get("MY_INT_ARRAY", std::vector<int>());
    std::cout << "MY_INT_ARRAY is ";
    for (auto it = vi.begin(); it != vi.end(); it++) {
        std::cout << *it << " ";
    }
    std::cout << std::endl;
    
    std::vector<float> vf = ConfigVal::Get("MY_FLOAT_ARRAY", std::vector<float>());
    std::cout << "MY_FLOAT_ARRAY is ";
    for (auto it = vf.begin(); it != vf.end(); it++) {
        std::cout << *it << " ";
    }
    std::cout << std::endl;
    
    std::vector<bool> vb = ConfigVal::Get("MY_BOOL_ARRAY", std::vector<bool>());
    std::cout << "MY_BOOL_ARRAY is ";
    for (auto it = vb.begin(); it != vb.end(); it++) {
        std::cout << *it << " ";
    }
    std::cout << std::endl;
    
    std::vector<std::string> vs = ConfigVal::Get("MY_STRING_ARRAY", std::vector<std::string>());
    std::cout << "MY_STRING_ARRAY is ";
    for (auto it = vs.begin(); it != vs.end(); it++) {
        std::cout << *it << " ";
    }
    std::cout << std::endl;

    return 0;
}
