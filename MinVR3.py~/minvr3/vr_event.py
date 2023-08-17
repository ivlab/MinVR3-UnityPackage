import json

class UnityObjectEncoder(json.JSONEncoder):
    def default(self, o):
        if isinstance(o, UnityObject):
            return o.__dict__
        return json.JSONEncoder.default(self, o)

class UnityObject:
    pass

class Vector2(UnityObject):
    def __init__(self, x, y):
        self.x = x
        self.y = y
    def __str__(self):
        return f'Vector2({self.x}, {self.y})'

class Vector3(UnityObject):
    def __init__(self, x, y, z):
        self.x = x
        self.y = y
        self.z = z
    def __str__(self):
        return f'Vector3({self.x}, {self.y}, {self.z})'

class Vector4(UnityObject):
    def __init__(self, x, y, z, w):
        self.x = x
        self.y = y
        self.z = z
        self.w = w
    def __str__(self):
        return f'Vector4({self.x}, {self.y}, {self.z}, {self.w})'

class Quaternion(UnityObject):
    def __init__(self, x, y, z, w):
        self.x = x
        self.y = y
        self.z = z
        self.w = w
    def __str__(self):
        return f'Quaternion({self.x}, {self.y}, {self.z}, {self.w})'

class GameObject(UnityObject):
    ''' GameObject: Partial implementation to send/receive GameObjects from Unity '''
    def __init__(self, instanceID):
        self.instanceID = instanceID
    def __str__(self):
        return f'GameObject({self.instanceID})'

'''
Other types (int, string, float) are all just parsed as raw data (not objects) */
'''


class VREvent:
    def __init__(self, event_name, data_type_name='', data=None):
        self.event_name = event_name
        self.data_type_name = data_type_name
        self.data = data

    def to_json(self):
        j = {
            'm_Name': self.event_name,
            'm_DataTypeName': self.data_type_name,
        }
        if self.data is not None:
            j['m_Data'] = self.data
        js = json.dumps(j, cls=UnityObjectEncoder)
        return js

    @staticmethod
    def from_json(json_string):
        js = json.loads(json_string)

        # Mirrors the switch in VREvent.cs
        if js['m_DataTypeName'] == '':
            return VREvent(js['m_Name'])
        elif js['m_DataTypeName'] == 'Vector2':
            return VREventVector2(js['m_Name'], Vector2(js['m_Data']['x'], js['m_Data']['y']))
        elif js['m_DataTypeName'] == 'Vector3':
            return VREventVector3(js['m_Name'], Vector3(js['m_Data']['x'], js['m_Data']['y'], js['m_Data']['z']))
        elif js['m_DataTypeName'] == 'Vector4':
            return VREventVector4(js['m_Name'], Vector4(js['m_Data']['x'], js['m_Data']['y'], js['m_Data']['z'], js['m_Data']['w']))
        elif js['m_DataTypeName'] == 'Quaternion':
            return VREventQuaternion(js['m_Name'], Quaternion(js['m_Data']['x'], js['m_Data']['y'], js['m_Data']['z'], js['m_Data']['w']))
        elif js['m_DataTypeName'] == 'GameObject':
            return VREventGameObject(js['m_Name'], GameObject(js['m_Data']['instanceID']))
        elif js['m_DataTypeName'] == 'String':
            return VREventString(js['m_Name'], js['m_Data'])
        elif js['m_DataTypeName'] == 'Int32':
            return VREventInt(js['m_Name'], js['m_Data'])
        elif js['m_DataTypeName'] == 'Single':
            return VREventFloat(js['m_Name'], js['m_Data'])
        else:
            return None

    def __str__(self):
        return f'{str(type(self).__name__)}({self.event_name}, {self.data})'

class VREventVector2(VREvent):
    def __init__(self, event_name, data):
        super().__init__(event_name, 'Vector2', data)

class VREventVector3(VREvent):
    def __init__(self, event_name, data):
        super().__init__(event_name, 'Vector3', data)

class VREventVector4(VREvent):
    def __init__(self, event_name, data):
        super().__init__(event_name, 'Vector4', data)

class VREventQuaternion(VREvent):
    def __init__(self, event_name, data):
        super().__init__(event_name, 'Quaternion', data)

class VREventGameObject(VREvent):
    def __init__(self, event_name, data):
        super().__init__(event_name, 'GameObject', data)

class VREventString(VREvent):
    def __init__(self, event_name, data):
        super().__init__(event_name, 'String', data)

class VREventInt(VREvent):
    def __init__(self, event_name, data):
        super().__init__(event_name, 'Int32', data)

class VREventFloat(VREvent):
    def __init__(self, event_name, data):
        super().__init__(event_name, 'Single', data)