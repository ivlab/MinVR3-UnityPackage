/* MinVR3Events.js
 *
 * Web version of MinVR3 event communication. Compatible with VREvent.cs, and
 * should implement all the types that are shown in the type mapping
 * `AvailableDataTypes`.
 *
 * Copyright (C) 2021, University of Minnesota
 * Authors: Bridger Herman <herma582@umn.edu>
 * 
 */

var MinVR3Event = {};

class VREvent {
    #eventName;
    #dataTypeName;

    get name() { return this.#eventName };
    get dataTypeName() { return this.#dataTypeName };

    constructor(eventName, dataTypeName) {
        this.#eventName = eventName;
        this.#dataTypeName = dataTypeName;
    }

    toJson() {
        let obj = this;
        let evt = {};
        evt.m_DataTypeName = this.#dataTypeName;
        evt.m_Name = this.#eventName;
        evt.m_Data = obj;
        return JSON.stringify(evt);
    }

    static fromJson(jsonString) {
        let json = JSON.parse(jsonString);

        // Mirrors the switch in VREvent.cs
        switch (json.m_DataTypeName)
        {
            case 'Vector2':
                return new MinVR3Event.Vector2(json.m_Name, json.m_Data.x, json.m_Data.y);
            case 'Vector3':
                return new MinVR3Event.Vector3(json.m_Name, json.m_Data.x, json.m_Data.y, json.m_Data.z);
            case 'Vector4':
                return new MinVR3Event.Vector4(json.m_Name, json.m_Data.x, json.m_Data.y, json.m_Data.z, json.m_Data.w);
            case 'Quaternion':
                return new MinVR3Event.Quaternion(json.m_Name, json.m_Data.x, json.m_Data.y, json.m_Data.z, json.m_Data.w);
            case 'string':
                return new MinVR3Event.String(json.m_Name, json.m_Data.value);
            case 'int':
                return new MinVR3Event.Int(json.m_Name, json.m_Data.value);
            case 'float':
                return new MinVR3Event.Float(json.m_Name, json.m_Data.value);
            default:
                return null;
        }
    }
}

MinVR3Event.Vector2 = class extends VREvent {
    constructor(eventName, x, y) {
        super(eventName, 'Vector2');
        this.x = x;
        this.y = y;
    }
}

MinVR3Event.Vector3 = class extends VREvent {
    constructor(eventName, x, y, z) {
        super(eventName, 'Vector3');
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

MinVR3Event.Vector4 = class extends VREvent {
    constructor(eventName, x, y, z, w) {
        super(eventName, 'Vector4');
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}

MinVR3Event.Quaternion = class extends VREvent {
    constructor(eventName, x, y, z, w) {
        super(eventName, 'Quaternion');
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}

MinVR3Event.String = class extends VREvent {
    constructor(eventName, value) {
        super(eventName, 'string');
        this.value = value;
    }
}

MinVR3Event.Int = class extends VREvent {
    constructor(eventName, value) {
        super(eventName, 'int');
        this.value = value;
    }
}

MinVR3Event.Float = class extends VREvent {
    constructor(eventName, value) {
        super(eventName, 'float');
        this.value = value;
    }
}