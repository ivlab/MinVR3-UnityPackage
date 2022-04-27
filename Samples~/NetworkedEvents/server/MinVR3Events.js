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