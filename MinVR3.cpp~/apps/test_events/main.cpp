
#include <string>
#include <iostream>

#include <minvr3.h>

int main(int argc, char* argv[])
{
    VREventFloat e1("example_float", 5.0f);
    std::cout << "Orig Event: " << e1 << std::endl;
    std::string json1 = e1.ToJson();
    std::cout << "To Json: " << json1;
    VREvent *e2 = VREvent::CreateFromJson(json1);
    std::cout << "From Json: " << *e2 << std::endl << std::endl;
    delete e2;
    
    VREventVector3 e3("example_vec3", 5.0f, 6.0f, 7.0f);
    std::cout << "Orig Event: " << e3 << std::endl;
    std::string json2 = e3.ToJson();
    std::cout << "To Json: " << json2;
    VREvent *e4 = VREvent::CreateFromJson(json2);
    std::cout << "From Json: " << *e4 << std::endl << std::endl;
    delete e4;
    
    return 0;
}
