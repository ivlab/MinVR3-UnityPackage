
/** Lightweight C++ implementation of MinVR3 VREvents. Intended for communicating
 * between MinVR3 Unity programs and C++ programs.
 * 
 */

#ifndef MINVR3_VREVENT_H
#define MINVR3_VREVENT_H

#include <string>
#include <vector>

class VREvent {
public:
    VREvent(const std::string& event_name);
    VREvent(const std::string& event_name, const std::string& data_type_name);
    VREvent();
    virtual ~VREvent();

    std::string get_name() const;
    std::string get_data_type_name() const;

    virtual void SetFromJson(const std::string &eventJsonStr);
    virtual std::string ToJson() const;
    static VREvent* CreateFromJson(const std::string &eventJsonStr);
    virtual void Print(std::ostream& os) const;

protected:
    std::string name_;
    std::string data_type_name_;
};


class VREventInt : public VREvent {
public:
    VREventInt(const std::string& eventName, int data);
    VREventInt();
    virtual ~VREventInt();

    int get_data() const;

    virtual void SetFromJson(const std::string &eventJsonStr);
    virtual std::string ToJson() const;
    virtual void Print(std::ostream& os) const;

protected:
    int data_;
};


class VREventFloat : public VREvent {
public:
    VREventFloat(const std::string& eventName, float data);
    VREventFloat();
    virtual ~VREventFloat();

    float get_data() const;

    virtual void SetFromJson(const std::string &eventJsonStr);
    virtual std::string ToJson() const;
    virtual void Print(std::ostream& os) const;

protected:
    float data_;
};


class VREventVector2 : public VREvent {
public:
    VREventVector2(const std::string& eventName, float x, float y);
    VREventVector2();
    virtual ~VREventVector2();
    
    std::vector<float> get_data() const;
    float x() const;
    float y() const;

    virtual void SetFromJson(const std::string &eventJsonStr);
    virtual std::string ToJson() const;
    virtual void Print(std::ostream& os) const;

protected:
    float x_;
    float y_;
};


class VREventVector3 : public VREvent {
public:
    VREventVector3(const std::string& eventName, float x, float y, float z);
    VREventVector3();
    virtual ~VREventVector3();

    std::vector<float> get_data() const;
    float x() const;
    float y() const;
    float z() const;

    virtual void SetFromJson(const std::string &eventJsonStr);
    virtual std::string ToJson() const;
    virtual void Print(std::ostream& os) const;

protected:
    float x_;
    float y_;
    float z_;
};


class VREventVector4 : public VREvent {
public:
    VREventVector4(const std::string& eventName, float x, float y, float z, float w);
    VREventVector4();
    virtual ~VREventVector4();

    std::vector<float> get_data() const;
    float x() const;
    float y() const;
    float z() const;
    float w() const;

    virtual void SetFromJson(const std::string &eventJsonStr);
    virtual std::string ToJson() const;
    virtual void Print(std::ostream& os) const;

protected:
    float x_;
    float y_;
    float z_;
    float w_;
};


class VREventQuaternion : public VREvent {
public:
    VREventQuaternion(const std::string& eventName, float x, float y, float z, float w);
    VREventQuaternion();
    virtual ~VREventQuaternion();

    std::vector<float> get_data() const;
    float x() const;
    float y() const;
    float z() const;
    float w() const;

    virtual void SetFromJson(const std::string &eventJsonStr);
    virtual std::string ToJson() const;
    virtual void Print(std::ostream& os) const;

protected:
    float x_;
    float y_;
    float z_;
    float w_;
};


class VREventString : public VREvent {
public:
    VREventString(const std::string& eventName, const std::string& data);
    VREventString();
    virtual ~VREventString();

    std::string get_data() const;

    virtual void SetFromJson(const std::string &eventJsonStr);
    virtual std::string ToJson() const;
    virtual void Print(std::ostream& os) const;

protected:
    std::string str_;
};


std::ostream & operator<< ( std::ostream &os, const VREvent &e);
std::ostream & operator<< ( std::ostream &os, const VREventInt &e);
std::ostream & operator<< ( std::ostream &os, const VREventFloat &e);
std::ostream & operator<< ( std::ostream &os, const VREventVector2 &e);
std::ostream & operator<< ( std::ostream &os, const VREventVector3 &e);
std::ostream & operator<< ( std::ostream &os, const VREventVector4 &e);
std::ostream & operator<< ( std::ostream &os, const VREventQuaternion &e);
std::ostream & operator<< ( std::ostream &os, const VREventString &e);

#endif
