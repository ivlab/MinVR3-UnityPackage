
#include "vr_event.h"

#include <string>
#include <iostream>

#include "json/json.h"


VREvent::VREvent(const std::string& event_name) {
	name_ = event_name;
	data_type_name_ = "";
}

VREvent::VREvent(const std::string& event_name, const std::string& data_type_name) {
	name_ = event_name;
	data_type_name_ = data_type_name;
}

VREvent::VREvent() {
	name_ = "";
	data_type_name_ = "";
}

VREvent::~VREvent() {}

std::string VREvent::get_name() const {
	return name_;
}

std::string VREvent::get_data_type_name() const {
	return data_type_name_;
}

void VREvent::SetFromJson(const std::string &eventJsonStr) {
    Json::Reader reader;
    Json::Value eventJson;
    if (!reader.parse(eventJsonStr, eventJson)) {
        std::cerr << reader.getFormattedErrorMessages() << std::endl;
        return;
    }
    name_ = eventJson["m_Name"].asString();
    data_type_name_ = eventJson["m_DataTypeName"].asString();
}

std::string VREvent::ToJson() const {
	Json::Value eventJson;
	eventJson["m_Name"] = name_;
	eventJson["m_DataTypeName"] = data_type_name_;
	Json::FastWriter fastWriter;
	std::string eventJsonStr = fastWriter.write(eventJson);
	return eventJsonStr;
}

VREvent* VREvent::CreateFromJson(const std::string& eventJsonStr) {
	Json::Reader reader;
	Json::Value eventJson;
	if (!reader.parse(eventJsonStr, eventJson)) {
		std::cerr << reader.getFormattedErrorMessages() << std::endl;
		return NULL;
	}

	std::string name = eventJson["m_Name"].asString();
	std::string data_type_name = eventJson["m_DataTypeName"].asString();

	// empty string means no data payload with the event
	if (data_type_name == "") {
		return new VREvent(name);
	}

	// else, different cases based on the data type:
	Json::Value data = eventJson["m_Data"];
	if (data_type_name == "Vector2") {
		return new VREventVector2(name, data["x"].asFloat(), data["y"].asFloat());
	}
	else if (data_type_name == "Vector3") {
		return new VREventVector3(name, data["x"].asFloat(), data["y"].asFloat(), data["z"].asFloat());
	}
	else if (data_type_name == "Vector4") {
		return new VREventVector4(name, data["x"].asFloat(), data["y"].asFloat(), data["z"].asFloat(), data["w"].asFloat());
	}
	else if (data_type_name == "Quaternion") {
		return new VREventQuaternion(name, data["x"].asFloat(), data["y"].asFloat(), data["z"].asFloat(), data["w"].asFloat());
	}
    else if (data_type_name == "String") {
		return new VREventString(name, data.asString());
	}
	else if (data_type_name == "Int32") {
		return new VREventInt(name, data.asInt());
	}
	else if (data_type_name == "Single") {
		return new VREventFloat(name, data.asFloat());
	}

	std::cerr << "VREvent::FromJson() unknown event data type: " << data_type_name << std::endl;
	return NULL;
}


VREventInt::VREventInt(const std::string& eventName, int data) : VREvent(eventName, "Int32") {
    data_ = data;
}

VREventInt::VREventInt() : VREvent("", "Int32") {
    data_ = 0;
}

VREventInt::~VREventInt() {}

int VREventInt::get_data() const { return data_; }

void VREventInt::SetFromJson(const std::string &eventJsonStr) {
    Json::Reader reader;
    Json::Value eventJson;
    if (!reader.parse(eventJsonStr, eventJson)) {
        std::cerr << reader.getFormattedErrorMessages() << std::endl;
        return;
    }
    name_ = eventJson["m_Name"].asString();
    data_type_name_ = eventJson["m_DataTypeName"].asString();
    data_ = eventJson["m_Data"].asInt();
}

std::string VREventInt::ToJson() const {
    Json::Value eventJson;
    eventJson["m_Name"] = name_;
    eventJson["m_DataTypeName"] = data_type_name_;
    eventJson["m_Data"] = data_;
    Json::FastWriter fastWriter;
    std::string eventJsonStr = fastWriter.write(eventJson);
    return eventJsonStr;
}


VREventFloat::VREventFloat(const std::string& eventName, float data) : VREvent(eventName, "Single") {
    data_ = data;
}

VREventFloat::VREventFloat() : VREvent("", "Single") {
    data_ = 0.0f;
}

VREventFloat::~VREventFloat() {}

float VREventFloat::get_data() const { return data_; }

void VREventFloat::SetFromJson(const std::string &eventJsonStr) {
    Json::Reader reader;
    Json::Value eventJson;
    if (!reader.parse(eventJsonStr, eventJson)) {
        std::cerr << reader.getFormattedErrorMessages() << std::endl;
        return;
    }
    name_ = eventJson["m_Name"].asString();
    data_type_name_ = eventJson["m_DataTypeName"].asString();
    data_ = eventJson["m_Data"].asFloat();
}

std::string VREventFloat::ToJson() const {
    Json::Value eventJson;
    eventJson["m_Name"] = name_;
    eventJson["m_DataTypeName"] = data_type_name_;
    eventJson["m_Data"] = data_;
    Json::FastWriter fastWriter;
    std::string eventJsonStr = fastWriter.write(eventJson);
    return eventJsonStr;
}


VREventVector2::VREventVector2(const std::string& eventName, float x, float y) : VREvent(eventName, "Vector2") {
	x_ = x;
	y_ = y;
}

VREventVector2::VREventVector2() : VREvent("", "Vector2") {
	x_ = 0;
	y_ = 0;
}

VREventVector2::~VREventVector2() {}

std::vector<float> VREventVector2::get_data() const {
    return std::vector<float>{x_, y_};
}

float VREventVector2::x() const { return x_; }
float VREventVector2::y() const { return y_; }

void VREventVector2::SetFromJson(const std::string &eventJsonStr) {
    Json::Reader reader;
    Json::Value eventJson;
    if (!reader.parse(eventJsonStr, eventJson)) {
        std::cerr << reader.getFormattedErrorMessages() << std::endl;
        return;
    }
    name_ = eventJson["m_Name"].asString();
    data_type_name_ = eventJson["m_DataTypeName"].asString();
    x_ = eventJson["m_Data"]["x"].asFloat();
    y_ = eventJson["m_Data"]["y"].asFloat();
}

std::string VREventVector2::ToJson() const {
	Json::Value eventJson;
	eventJson["m_Name"] = name_;
	eventJson["m_DataTypeName"] = data_type_name_;
	Json::Value dataJson;
	dataJson["x"] = x_;
	dataJson["y"] = y_;
	eventJson["m_Data"] = dataJson;
	Json::FastWriter fastWriter;
	std::string eventJsonStr = fastWriter.write(eventJson);
	return eventJsonStr;
}


VREventVector3::VREventVector3(const std::string& eventName, float x, float y, float z) : VREvent(eventName, "Vector3") {
	x_ = x;
	y_ = y;
	z_ = z;
}

VREventVector3::VREventVector3() : VREvent("", "Vector3") {
	x_ = 0;
	y_ = 0;
	z_ = 0;
}

VREventVector3::~VREventVector3() {}

std::vector<float> VREventVector3::get_data() const {
    return std::vector<float>{x_, y_, z_};
}

float VREventVector3::x() const { return x_; }
float VREventVector3::y() const { return y_; }
float VREventVector3::z() const { return z_; }

void VREventVector3::SetFromJson(const std::string &eventJsonStr) {
    Json::Reader reader;
    Json::Value eventJson;
    if (!reader.parse(eventJsonStr, eventJson)) {
        std::cerr << reader.getFormattedErrorMessages() << std::endl;
        return;
    }
    name_ = eventJson["m_Name"].asString();
    data_type_name_ = eventJson["m_DataTypeName"].asString();
    x_ = eventJson["m_Data"]["x"].asFloat();
    y_ = eventJson["m_Data"]["y"].asFloat();
    z_ = eventJson["m_Data"]["z"].asFloat();
}

std::string VREventVector3::ToJson() const {
	Json::Value eventJson;
	eventJson["m_Name"] = name_;
	eventJson["m_DataTypeName"] = data_type_name_;
	Json::Value dataJson;
	dataJson["x"] = x_;
	dataJson["y"] = y_;
	dataJson["z"] = z_;
	eventJson["m_Data"] = dataJson;
	Json::FastWriter fastWriter;
	std::string eventJsonStr = fastWriter.write(eventJson);
	return eventJsonStr;
}


VREventVector4::VREventVector4(const std::string& eventName, float x, float y, float z, float w) : VREvent(eventName, "Vector4") {
	x_ = x;
	y_ = y;
	z_ = z;
	w_ = w;
}

VREventVector4::VREventVector4() : VREvent("", "Vector4") {
	x_ = 0;
	y_ = 0;
	z_ = 0;
	w_ = 0;
}

VREventVector4::~VREventVector4() {}

std::vector<float> VREventVector4::get_data() const {
    return std::vector<float>{x_, y_, z_, w_};
}

float VREventVector4::x() const { return x_; }
float VREventVector4::y() const { return y_; }
float VREventVector4::z() const { return z_; }
float VREventVector4::w() const { return w_; }

void VREventVector4::SetFromJson(const std::string &eventJsonStr) {
    Json::Reader reader;
    Json::Value eventJson;
    if (!reader.parse(eventJsonStr, eventJson)) {
        std::cerr << reader.getFormattedErrorMessages() << std::endl;
        return;
    }
    name_ = eventJson["m_Name"].asString();
    data_type_name_ = eventJson["m_DataTypeName"].asString();
    x_ = eventJson["m_Data"]["x"].asFloat();
    y_ = eventJson["m_Data"]["y"].asFloat();
    z_ = eventJson["m_Data"]["z"].asFloat();
    w_ = eventJson["m_Data"]["w"].asFloat();
}

std::string VREventVector4::ToJson() const {
	Json::Value eventJson;
	eventJson["m_Name"] = name_;
	eventJson["m_DataTypeName"] = data_type_name_;
	Json::Value dataJson;
	dataJson["x"] = x_;
	dataJson["y"] = y_;
	dataJson["z"] = z_;
	dataJson["w"] = w_;
	eventJson["m_Data"] = dataJson;
	Json::FastWriter fastWriter;
	std::string eventJsonStr = fastWriter.write(eventJson);
	return eventJsonStr;
}


VREventQuaternion::VREventQuaternion(const std::string& eventName, float x, float y, float z, float w) : VREvent(eventName, "Quaternion") {
	x_ = x;
	y_ = y;
	z_ = z;
	w_ = w;
}

VREventQuaternion::VREventQuaternion() : VREvent("", "Quaternion") {
	x_ = 0;
	y_ = 0;
	z_ = 0;
	w_ = 0;
}

VREventQuaternion::~VREventQuaternion() {}

std::vector<float> VREventQuaternion::get_data() const {
    return std::vector<float>{x_, y_, z_, w_};
}

float VREventQuaternion::x() const { return x_; }
float VREventQuaternion::y() const { return y_; }
float VREventQuaternion::z() const { return z_; }
float VREventQuaternion::w() const { return w_; }

void VREventQuaternion::SetFromJson(const std::string &eventJsonStr) {
    Json::Reader reader;
    Json::Value eventJson;
    if (!reader.parse(eventJsonStr, eventJson)) {
        std::cerr << reader.getFormattedErrorMessages() << std::endl;
        return;
    }
    name_ = eventJson["m_Name"].asString();
    data_type_name_ = eventJson["m_DataTypeName"].asString();
    x_ = eventJson["m_Data"]["x"].asFloat();
    y_ = eventJson["m_Data"]["y"].asFloat();
    z_ = eventJson["m_Data"]["z"].asFloat();
    w_ = eventJson["m_Data"]["w"].asFloat();
}

std::string VREventQuaternion::ToJson() const {
	Json::Value eventJson;
	eventJson["m_Name"] = name_;
	eventJson["m_DataTypeName"] = data_type_name_;
	Json::Value dataJson;
	dataJson["x"] = x_;
	dataJson["y"] = y_;
	dataJson["z"] = z_;
	dataJson["w"] = w_;
	eventJson["m_Data"] = dataJson;
	Json::FastWriter fastWriter;
	std::string eventJsonStr = fastWriter.write(eventJson);
	return eventJsonStr;
}


VREventString::VREventString(const std::string& eventName, const std::string& data) : VREvent(eventName, "String") {
	str_ = data;
}

VREventString::VREventString() : VREvent("", "String") {
	str_ = "";
}

VREventString::~VREventString() {}

std::string VREventString::get_data() const { return str_; }

void VREventString::SetFromJson(const std::string &eventJsonStr) {
    Json::Reader reader;
    Json::Value eventJson;
    if (!reader.parse(eventJsonStr, eventJson)) {
        std::cerr << reader.getFormattedErrorMessages() << std::endl;
        return;
    }
    name_ = eventJson["m_Name"].asString();
    data_type_name_ = eventJson["m_DataTypeName"].asString();
    str_ = eventJson["m_Data"].asString();
}

std::string VREventString::ToJson() const {
	Json::Value eventJson;
	eventJson["m_Name"] = name_;
	eventJson["m_DataTypeName"] = data_type_name_;
	eventJson["m_Data"] = str_;
	Json::FastWriter fastWriter;
	std::string eventJsonStr = fastWriter.write(eventJson);
	return eventJsonStr;
}


void VREvent::Print(std::ostream& os) const {
    os << get_name() << " [" << get_data_type_name() << "]";
}

void VREventInt::Print(std::ostream& os) const {
    os << get_name() << " [" << get_data_type_name() << "] = " << get_data();
}

void VREventFloat::Print(std::ostream& os) const {
    os << get_name() << " [" << get_data_type_name() << "] = " << get_data();
}

void VREventVector2::Print(std::ostream& os) const {
    os << get_name() << " [" << get_data_type_name() << "] = (" << x() << ", " << y() << ")";
}

void VREventVector3::Print(std::ostream& os) const {
    os << get_name() << " [" << get_data_type_name() << "] = (" << x() << ", " << y() << ", " << z() << ")";
}

void VREventVector4::Print(std::ostream& os) const {
    os << get_name() << " [" << get_data_type_name() << "] = (" << x() << ", " << y() << ", " << z() << ", " << w() << ")";
}

void VREventQuaternion::Print(std::ostream& os) const {
    os << get_name() << " [" << get_data_type_name() << "] = (" << x() << ", " << y() << ", " << z() << ", " << w() << ")";
}

void VREventString::Print(std::ostream& os) const {
    os << get_name() << " [" << get_data_type_name() << "] = '" << get_data() + "'";
}


std::ostream & operator<< (std::ostream &os, const VREvent &e) {
    e.Print(os);
    return os;
}

std::ostream & operator<< ( std::ostream &os, const VREventInt &e) {
    e.Print(os);
    return os;
}

std::ostream & operator<< ( std::ostream &os, const VREventFloat &e) {
    e.Print(os);
    return os;
}

std::ostream & operator<< ( std::ostream &os, const VREventVector2 &e) {
    e.Print(os);
    return os;
}

std::ostream & operator<< ( std::ostream &os, const VREventVector3 &e) {
    e.Print(os);
    return os;
}

std::ostream & operator<< ( std::ostream &os, const VREventVector4 &e) {
    e.Print(os);
    return os;
}

std::ostream & operator<< ( std::ostream &os, const VREventQuaternion &e) {
    e.Print(os);
    return os;
}

std::ostream & operator<< ( std::ostream &os, const VREventString &e) {
    e.Print(os);
    return os;
}
