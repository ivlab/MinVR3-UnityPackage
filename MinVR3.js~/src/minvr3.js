/* MinVR3.js
 *
 * Web version of MinVR3 event communication. Compatible with VREvent.cs, and
 * should implement all the types that are shown in the type mapping
 * `AvailableDataTypes`.
 *
 * Copyright (C) 2023, University of Minnesota
 * Authors: Bridger Herman <herma582@umn.edu>
 * 
 */

/**
 * Vector2: Mirrors Unity's Vector2
 */
export class Vector2 {
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }
}

/**
 * Vector3: Mirrors Unity's Vector3
 */
export class Vector3 {
    constructor(x, y, z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

/**
 * Vector4: Mirrors Unity's Vector4
 */
export class Vector4 {
    constructor(x, y, z, w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}

/**
 * Quaternion: Mirrors Unity's Quaternion
 */
export class Quaternion {
    constructor(x, y, z, w) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}

/**
 * GameObject: Partial implementation to send/receive GameObjects from Unity
 */
export class GameObject {
    constructor(instanceID) {
        this.instanceID = instanceID;
    }
}

/** Other types (int, string, float) are all just parsed as raw data (not objects) */


/**
 * Represents a generic VREvent.
 */
export class VREvent {
    #eventName;
    #dataTypeName;
    #data;

    get name() { return this.#eventName };
    get dataTypeName() { return this.#dataTypeName };
    get data() { return this.#data };

    constructor(eventName, dataTypeName='', data=null) {
        this.#eventName = eventName;
        this.#dataTypeName = dataTypeName;
        this.#data = data;
    }

    toJson() {
        let evt = {};
        evt.m_DataTypeName = this.#dataTypeName;
        evt.m_Name = this.#eventName;
        evt.m_Data = this.#data;
        return JSON.stringify(evt);
    }

    static fromJson(jsonString) {
        let json = JSON.parse(jsonString);

        // Mirrors the switch in VREvent.cs
        switch (json.m_DataTypeName)
        {
            case '':
                return new VREvent(json.m_Name);
            case 'Vector2':
                return new VREventVector2(json.m_Name, new Vector2(json.m_Data.x, json.m_Data.y));
            case 'Vector3':
                return new VREventVector3(json.m_Name, new Vector3(json.m_Data.x, json.m_Data.y, json.m_Data.z));
            case 'Vector4':
                return new VREventVector4(json.m_Name, new Vector4(json.m_Data.x, json.m_Data.y, json.m_Data.z, json.m_Data.w));
            case 'Quaternion':
                return new VREventQuaternion(json.m_Name, new Vector4(json.m_Data.x, json.m_Data.y, json.m_Data.z, json.m_Data.w));
            case 'GameObject':
                return new VREventGameObject(json.m_Name, new GameObject(json.m_Data.instanceID));
            case 'String':
                return new VREventString(json.m_Name, json.m_Data);
            case 'Int32':
                return new VREventInt(json.m_Name, json.m_Data);
            case 'Single':
                return new VREventFloat(json.m_Name, json.m_Data);
            default:
                return null;
        }
    }
}

export class VREventVector2 extends VREvent {
    constructor(eventName, data) {
        super(eventName, 'Vector2', data);
    }
}

export class VREventVector3 extends VREvent {
    constructor(eventName, data) {
        super(eventName, 'Vector3', data);
    }
}

export class VREventVector4 extends VREvent {
    constructor(eventName, data) {
        super(eventName, 'Vector4', data);
    }
}

export class VREventQuaternion extends VREvent {
    constructor(eventName, data) {
        super(eventName, 'Quaternion', data);
    }
}

export class VREventGameObject extends VREvent {
    constructor(eventName, data) {
        super(eventName, 'GameObject', data);
    }
}

export class VREventString extends VREvent {
    constructor(eventName, value) {
        super(eventName, 'String', value);
    }
}

export class VREventInt extends VREvent {
    constructor(eventName, value) {
        super(eventName, 'Int32', value);
    }
}

export class VREventFloat extends VREvent {
    constructor(eventName, value) {
        super(eventName, 'Single', value);
    }
}

// connect to a MinVR3 event host. returns the newly created websocket object.
export async function connect(host) {
    const ws = new WebSocket(`ws://${host}/vrevent`);
    return new Promise((resolve, reject) => {
        ws.onopen = () => {
            console.log('MinVR3 connection opened');
            resolve();
        }
    }).then(() => ws);
}