(function (global, factory) {
  typeof exports === 'object' && typeof module !== 'undefined' ? factory(exports) :
  typeof define === 'function' && define.amd ? define(['exports'], factory) :
  (global = typeof globalThis !== 'undefined' ? globalThis : global || self, factory(global.MinVR3 = {}));
})(this, (function (exports) { 'use strict';

  function _regeneratorRuntime() {
    _regeneratorRuntime = function () {
      return exports;
    };
    var exports = {},
      Op = Object.prototype,
      hasOwn = Op.hasOwnProperty,
      defineProperty = Object.defineProperty || function (obj, key, desc) {
        obj[key] = desc.value;
      },
      $Symbol = "function" == typeof Symbol ? Symbol : {},
      iteratorSymbol = $Symbol.iterator || "@@iterator",
      asyncIteratorSymbol = $Symbol.asyncIterator || "@@asyncIterator",
      toStringTagSymbol = $Symbol.toStringTag || "@@toStringTag";
    function define(obj, key, value) {
      return Object.defineProperty(obj, key, {
        value: value,
        enumerable: !0,
        configurable: !0,
        writable: !0
      }), obj[key];
    }
    try {
      define({}, "");
    } catch (err) {
      define = function (obj, key, value) {
        return obj[key] = value;
      };
    }
    function wrap(innerFn, outerFn, self, tryLocsList) {
      var protoGenerator = outerFn && outerFn.prototype instanceof Generator ? outerFn : Generator,
        generator = Object.create(protoGenerator.prototype),
        context = new Context(tryLocsList || []);
      return defineProperty(generator, "_invoke", {
        value: makeInvokeMethod(innerFn, self, context)
      }), generator;
    }
    function tryCatch(fn, obj, arg) {
      try {
        return {
          type: "normal",
          arg: fn.call(obj, arg)
        };
      } catch (err) {
        return {
          type: "throw",
          arg: err
        };
      }
    }
    exports.wrap = wrap;
    var ContinueSentinel = {};
    function Generator() {}
    function GeneratorFunction() {}
    function GeneratorFunctionPrototype() {}
    var IteratorPrototype = {};
    define(IteratorPrototype, iteratorSymbol, function () {
      return this;
    });
    var getProto = Object.getPrototypeOf,
      NativeIteratorPrototype = getProto && getProto(getProto(values([])));
    NativeIteratorPrototype && NativeIteratorPrototype !== Op && hasOwn.call(NativeIteratorPrototype, iteratorSymbol) && (IteratorPrototype = NativeIteratorPrototype);
    var Gp = GeneratorFunctionPrototype.prototype = Generator.prototype = Object.create(IteratorPrototype);
    function defineIteratorMethods(prototype) {
      ["next", "throw", "return"].forEach(function (method) {
        define(prototype, method, function (arg) {
          return this._invoke(method, arg);
        });
      });
    }
    function AsyncIterator(generator, PromiseImpl) {
      function invoke(method, arg, resolve, reject) {
        var record = tryCatch(generator[method], generator, arg);
        if ("throw" !== record.type) {
          var result = record.arg,
            value = result.value;
          return value && "object" == typeof value && hasOwn.call(value, "__await") ? PromiseImpl.resolve(value.__await).then(function (value) {
            invoke("next", value, resolve, reject);
          }, function (err) {
            invoke("throw", err, resolve, reject);
          }) : PromiseImpl.resolve(value).then(function (unwrapped) {
            result.value = unwrapped, resolve(result);
          }, function (error) {
            return invoke("throw", error, resolve, reject);
          });
        }
        reject(record.arg);
      }
      var previousPromise;
      defineProperty(this, "_invoke", {
        value: function (method, arg) {
          function callInvokeWithMethodAndArg() {
            return new PromiseImpl(function (resolve, reject) {
              invoke(method, arg, resolve, reject);
            });
          }
          return previousPromise = previousPromise ? previousPromise.then(callInvokeWithMethodAndArg, callInvokeWithMethodAndArg) : callInvokeWithMethodAndArg();
        }
      });
    }
    function makeInvokeMethod(innerFn, self, context) {
      var state = "suspendedStart";
      return function (method, arg) {
        if ("executing" === state) throw new Error("Generator is already running");
        if ("completed" === state) {
          if ("throw" === method) throw arg;
          return doneResult();
        }
        for (context.method = method, context.arg = arg;;) {
          var delegate = context.delegate;
          if (delegate) {
            var delegateResult = maybeInvokeDelegate(delegate, context);
            if (delegateResult) {
              if (delegateResult === ContinueSentinel) continue;
              return delegateResult;
            }
          }
          if ("next" === context.method) context.sent = context._sent = context.arg;else if ("throw" === context.method) {
            if ("suspendedStart" === state) throw state = "completed", context.arg;
            context.dispatchException(context.arg);
          } else "return" === context.method && context.abrupt("return", context.arg);
          state = "executing";
          var record = tryCatch(innerFn, self, context);
          if ("normal" === record.type) {
            if (state = context.done ? "completed" : "suspendedYield", record.arg === ContinueSentinel) continue;
            return {
              value: record.arg,
              done: context.done
            };
          }
          "throw" === record.type && (state = "completed", context.method = "throw", context.arg = record.arg);
        }
      };
    }
    function maybeInvokeDelegate(delegate, context) {
      var methodName = context.method,
        method = delegate.iterator[methodName];
      if (undefined === method) return context.delegate = null, "throw" === methodName && delegate.iterator.return && (context.method = "return", context.arg = undefined, maybeInvokeDelegate(delegate, context), "throw" === context.method) || "return" !== methodName && (context.method = "throw", context.arg = new TypeError("The iterator does not provide a '" + methodName + "' method")), ContinueSentinel;
      var record = tryCatch(method, delegate.iterator, context.arg);
      if ("throw" === record.type) return context.method = "throw", context.arg = record.arg, context.delegate = null, ContinueSentinel;
      var info = record.arg;
      return info ? info.done ? (context[delegate.resultName] = info.value, context.next = delegate.nextLoc, "return" !== context.method && (context.method = "next", context.arg = undefined), context.delegate = null, ContinueSentinel) : info : (context.method = "throw", context.arg = new TypeError("iterator result is not an object"), context.delegate = null, ContinueSentinel);
    }
    function pushTryEntry(locs) {
      var entry = {
        tryLoc: locs[0]
      };
      1 in locs && (entry.catchLoc = locs[1]), 2 in locs && (entry.finallyLoc = locs[2], entry.afterLoc = locs[3]), this.tryEntries.push(entry);
    }
    function resetTryEntry(entry) {
      var record = entry.completion || {};
      record.type = "normal", delete record.arg, entry.completion = record;
    }
    function Context(tryLocsList) {
      this.tryEntries = [{
        tryLoc: "root"
      }], tryLocsList.forEach(pushTryEntry, this), this.reset(!0);
    }
    function values(iterable) {
      if (iterable) {
        var iteratorMethod = iterable[iteratorSymbol];
        if (iteratorMethod) return iteratorMethod.call(iterable);
        if ("function" == typeof iterable.next) return iterable;
        if (!isNaN(iterable.length)) {
          var i = -1,
            next = function next() {
              for (; ++i < iterable.length;) if (hasOwn.call(iterable, i)) return next.value = iterable[i], next.done = !1, next;
              return next.value = undefined, next.done = !0, next;
            };
          return next.next = next;
        }
      }
      return {
        next: doneResult
      };
    }
    function doneResult() {
      return {
        value: undefined,
        done: !0
      };
    }
    return GeneratorFunction.prototype = GeneratorFunctionPrototype, defineProperty(Gp, "constructor", {
      value: GeneratorFunctionPrototype,
      configurable: !0
    }), defineProperty(GeneratorFunctionPrototype, "constructor", {
      value: GeneratorFunction,
      configurable: !0
    }), GeneratorFunction.displayName = define(GeneratorFunctionPrototype, toStringTagSymbol, "GeneratorFunction"), exports.isGeneratorFunction = function (genFun) {
      var ctor = "function" == typeof genFun && genFun.constructor;
      return !!ctor && (ctor === GeneratorFunction || "GeneratorFunction" === (ctor.displayName || ctor.name));
    }, exports.mark = function (genFun) {
      return Object.setPrototypeOf ? Object.setPrototypeOf(genFun, GeneratorFunctionPrototype) : (genFun.__proto__ = GeneratorFunctionPrototype, define(genFun, toStringTagSymbol, "GeneratorFunction")), genFun.prototype = Object.create(Gp), genFun;
    }, exports.awrap = function (arg) {
      return {
        __await: arg
      };
    }, defineIteratorMethods(AsyncIterator.prototype), define(AsyncIterator.prototype, asyncIteratorSymbol, function () {
      return this;
    }), exports.AsyncIterator = AsyncIterator, exports.async = function (innerFn, outerFn, self, tryLocsList, PromiseImpl) {
      void 0 === PromiseImpl && (PromiseImpl = Promise);
      var iter = new AsyncIterator(wrap(innerFn, outerFn, self, tryLocsList), PromiseImpl);
      return exports.isGeneratorFunction(outerFn) ? iter : iter.next().then(function (result) {
        return result.done ? result.value : iter.next();
      });
    }, defineIteratorMethods(Gp), define(Gp, toStringTagSymbol, "Generator"), define(Gp, iteratorSymbol, function () {
      return this;
    }), define(Gp, "toString", function () {
      return "[object Generator]";
    }), exports.keys = function (val) {
      var object = Object(val),
        keys = [];
      for (var key in object) keys.push(key);
      return keys.reverse(), function next() {
        for (; keys.length;) {
          var key = keys.pop();
          if (key in object) return next.value = key, next.done = !1, next;
        }
        return next.done = !0, next;
      };
    }, exports.values = values, Context.prototype = {
      constructor: Context,
      reset: function (skipTempReset) {
        if (this.prev = 0, this.next = 0, this.sent = this._sent = undefined, this.done = !1, this.delegate = null, this.method = "next", this.arg = undefined, this.tryEntries.forEach(resetTryEntry), !skipTempReset) for (var name in this) "t" === name.charAt(0) && hasOwn.call(this, name) && !isNaN(+name.slice(1)) && (this[name] = undefined);
      },
      stop: function () {
        this.done = !0;
        var rootRecord = this.tryEntries[0].completion;
        if ("throw" === rootRecord.type) throw rootRecord.arg;
        return this.rval;
      },
      dispatchException: function (exception) {
        if (this.done) throw exception;
        var context = this;
        function handle(loc, caught) {
          return record.type = "throw", record.arg = exception, context.next = loc, caught && (context.method = "next", context.arg = undefined), !!caught;
        }
        for (var i = this.tryEntries.length - 1; i >= 0; --i) {
          var entry = this.tryEntries[i],
            record = entry.completion;
          if ("root" === entry.tryLoc) return handle("end");
          if (entry.tryLoc <= this.prev) {
            var hasCatch = hasOwn.call(entry, "catchLoc"),
              hasFinally = hasOwn.call(entry, "finallyLoc");
            if (hasCatch && hasFinally) {
              if (this.prev < entry.catchLoc) return handle(entry.catchLoc, !0);
              if (this.prev < entry.finallyLoc) return handle(entry.finallyLoc);
            } else if (hasCatch) {
              if (this.prev < entry.catchLoc) return handle(entry.catchLoc, !0);
            } else {
              if (!hasFinally) throw new Error("try statement without catch or finally");
              if (this.prev < entry.finallyLoc) return handle(entry.finallyLoc);
            }
          }
        }
      },
      abrupt: function (type, arg) {
        for (var i = this.tryEntries.length - 1; i >= 0; --i) {
          var entry = this.tryEntries[i];
          if (entry.tryLoc <= this.prev && hasOwn.call(entry, "finallyLoc") && this.prev < entry.finallyLoc) {
            var finallyEntry = entry;
            break;
          }
        }
        finallyEntry && ("break" === type || "continue" === type) && finallyEntry.tryLoc <= arg && arg <= finallyEntry.finallyLoc && (finallyEntry = null);
        var record = finallyEntry ? finallyEntry.completion : {};
        return record.type = type, record.arg = arg, finallyEntry ? (this.method = "next", this.next = finallyEntry.finallyLoc, ContinueSentinel) : this.complete(record);
      },
      complete: function (record, afterLoc) {
        if ("throw" === record.type) throw record.arg;
        return "break" === record.type || "continue" === record.type ? this.next = record.arg : "return" === record.type ? (this.rval = this.arg = record.arg, this.method = "return", this.next = "end") : "normal" === record.type && afterLoc && (this.next = afterLoc), ContinueSentinel;
      },
      finish: function (finallyLoc) {
        for (var i = this.tryEntries.length - 1; i >= 0; --i) {
          var entry = this.tryEntries[i];
          if (entry.finallyLoc === finallyLoc) return this.complete(entry.completion, entry.afterLoc), resetTryEntry(entry), ContinueSentinel;
        }
      },
      catch: function (tryLoc) {
        for (var i = this.tryEntries.length - 1; i >= 0; --i) {
          var entry = this.tryEntries[i];
          if (entry.tryLoc === tryLoc) {
            var record = entry.completion;
            if ("throw" === record.type) {
              var thrown = record.arg;
              resetTryEntry(entry);
            }
            return thrown;
          }
        }
        throw new Error("illegal catch attempt");
      },
      delegateYield: function (iterable, resultName, nextLoc) {
        return this.delegate = {
          iterator: values(iterable),
          resultName: resultName,
          nextLoc: nextLoc
        }, "next" === this.method && (this.arg = undefined), ContinueSentinel;
      }
    }, exports;
  }
  function asyncGeneratorStep(gen, resolve, reject, _next, _throw, key, arg) {
    try {
      var info = gen[key](arg);
      var value = info.value;
    } catch (error) {
      reject(error);
      return;
    }
    if (info.done) {
      resolve(value);
    } else {
      Promise.resolve(value).then(_next, _throw);
    }
  }
  function _asyncToGenerator(fn) {
    return function () {
      var self = this,
        args = arguments;
      return new Promise(function (resolve, reject) {
        var gen = fn.apply(self, args);
        function _next(value) {
          asyncGeneratorStep(gen, resolve, reject, _next, _throw, "next", value);
        }
        function _throw(err) {
          asyncGeneratorStep(gen, resolve, reject, _next, _throw, "throw", err);
        }
        _next(undefined);
      });
    };
  }
  function _classCallCheck(instance, Constructor) {
    if (!(instance instanceof Constructor)) {
      throw new TypeError("Cannot call a class as a function");
    }
  }
  function _defineProperties(target, props) {
    for (var i = 0; i < props.length; i++) {
      var descriptor = props[i];
      descriptor.enumerable = descriptor.enumerable || false;
      descriptor.configurable = true;
      if ("value" in descriptor) descriptor.writable = true;
      Object.defineProperty(target, _toPropertyKey(descriptor.key), descriptor);
    }
  }
  function _createClass(Constructor, protoProps, staticProps) {
    if (protoProps) _defineProperties(Constructor.prototype, protoProps);
    if (staticProps) _defineProperties(Constructor, staticProps);
    Object.defineProperty(Constructor, "prototype", {
      writable: false
    });
    return Constructor;
  }
  function _inherits(subClass, superClass) {
    if (typeof superClass !== "function" && superClass !== null) {
      throw new TypeError("Super expression must either be null or a function");
    }
    subClass.prototype = Object.create(superClass && superClass.prototype, {
      constructor: {
        value: subClass,
        writable: true,
        configurable: true
      }
    });
    Object.defineProperty(subClass, "prototype", {
      writable: false
    });
    if (superClass) _setPrototypeOf(subClass, superClass);
  }
  function _getPrototypeOf(o) {
    _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf.bind() : function _getPrototypeOf(o) {
      return o.__proto__ || Object.getPrototypeOf(o);
    };
    return _getPrototypeOf(o);
  }
  function _setPrototypeOf(o, p) {
    _setPrototypeOf = Object.setPrototypeOf ? Object.setPrototypeOf.bind() : function _setPrototypeOf(o, p) {
      o.__proto__ = p;
      return o;
    };
    return _setPrototypeOf(o, p);
  }
  function _isNativeReflectConstruct() {
    if (typeof Reflect === "undefined" || !Reflect.construct) return false;
    if (Reflect.construct.sham) return false;
    if (typeof Proxy === "function") return true;
    try {
      Boolean.prototype.valueOf.call(Reflect.construct(Boolean, [], function () {}));
      return true;
    } catch (e) {
      return false;
    }
  }
  function _assertThisInitialized(self) {
    if (self === void 0) {
      throw new ReferenceError("this hasn't been initialised - super() hasn't been called");
    }
    return self;
  }
  function _possibleConstructorReturn(self, call) {
    if (call && (typeof call === "object" || typeof call === "function")) {
      return call;
    } else if (call !== void 0) {
      throw new TypeError("Derived constructors may only return object or undefined");
    }
    return _assertThisInitialized(self);
  }
  function _createSuper(Derived) {
    var hasNativeReflectConstruct = _isNativeReflectConstruct();
    return function _createSuperInternal() {
      var Super = _getPrototypeOf(Derived),
        result;
      if (hasNativeReflectConstruct) {
        var NewTarget = _getPrototypeOf(this).constructor;
        result = Reflect.construct(Super, arguments, NewTarget);
      } else {
        result = Super.apply(this, arguments);
      }
      return _possibleConstructorReturn(this, result);
    };
  }
  function _toPrimitive(input, hint) {
    if (typeof input !== "object" || input === null) return input;
    var prim = input[Symbol.toPrimitive];
    if (prim !== undefined) {
      var res = prim.call(input, hint || "default");
      if (typeof res !== "object") return res;
      throw new TypeError("@@toPrimitive must return a primitive value.");
    }
    return (hint === "string" ? String : Number)(input);
  }
  function _toPropertyKey(arg) {
    var key = _toPrimitive(arg, "string");
    return typeof key === "symbol" ? key : String(key);
  }
  function _classPrivateFieldGet(receiver, privateMap) {
    var descriptor = _classExtractFieldDescriptor(receiver, privateMap, "get");
    return _classApplyDescriptorGet(receiver, descriptor);
  }
  function _classPrivateFieldSet(receiver, privateMap, value) {
    var descriptor = _classExtractFieldDescriptor(receiver, privateMap, "set");
    _classApplyDescriptorSet(receiver, descriptor, value);
    return value;
  }
  function _classExtractFieldDescriptor(receiver, privateMap, action) {
    if (!privateMap.has(receiver)) {
      throw new TypeError("attempted to " + action + " private field on non-instance");
    }
    return privateMap.get(receiver);
  }
  function _classApplyDescriptorGet(receiver, descriptor) {
    if (descriptor.get) {
      return descriptor.get.call(receiver);
    }
    return descriptor.value;
  }
  function _classApplyDescriptorSet(receiver, descriptor, value) {
    if (descriptor.set) {
      descriptor.set.call(receiver, value);
    } else {
      if (!descriptor.writable) {
        throw new TypeError("attempted to set read only private field");
      }
      descriptor.value = value;
    }
  }
  function _checkPrivateRedeclaration(obj, privateCollection) {
    if (privateCollection.has(obj)) {
      throw new TypeError("Cannot initialize the same private elements twice on an object");
    }
  }
  function _classPrivateFieldInitSpec(obj, privateMap, value) {
    _checkPrivateRedeclaration(obj, privateMap);
    privateMap.set(obj, value);
  }

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
  var Vector2 = /*#__PURE__*/_createClass(function Vector2(x, y) {
    _classCallCheck(this, Vector2);
    this.x = x;
    this.y = y;
  });

  /**
   * Vector3: Mirrors Unity's Vector3
   */
  var Vector3 = /*#__PURE__*/_createClass(function Vector3(x, y, z) {
    _classCallCheck(this, Vector3);
    this.x = x;
    this.y = y;
    this.z = z;
  });

  /**
   * Vector4: Mirrors Unity's Vector4
   */
  var Vector4 = /*#__PURE__*/_createClass(function Vector4(x, y, z, w) {
    _classCallCheck(this, Vector4);
    this.x = x;
    this.y = y;
    this.z = z;
    this.w = w;
  });

  /**
   * Quaternion: Mirrors Unity's Quaternion
   */
  var Quaternion = /*#__PURE__*/_createClass(function Quaternion(x, y, z, w) {
    _classCallCheck(this, Quaternion);
    this.x = x;
    this.y = y;
    this.z = z;
    this.w = w;
  });

  /**
   * GameObject: Partial implementation to send/receive GameObjects from Unity
   */
  var GameObject = /*#__PURE__*/_createClass(function GameObject(instanceID) {
    _classCallCheck(this, GameObject);
    this.instanceID = instanceID;
  });

  /** Other types (int, string, float) are all just parsed as raw data (not objects) */

  /**
   * Represents a generic VREvent.
   */
  var _eventName = /*#__PURE__*/new WeakMap();
  var _dataTypeName = /*#__PURE__*/new WeakMap();
  var _data = /*#__PURE__*/new WeakMap();
  var VREvent = /*#__PURE__*/function () {
    function VREvent(eventName) {
      var dataTypeName = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : '';
      var data = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : null;
      _classCallCheck(this, VREvent);
      _classPrivateFieldInitSpec(this, _eventName, {
        writable: true,
        value: void 0
      });
      _classPrivateFieldInitSpec(this, _dataTypeName, {
        writable: true,
        value: void 0
      });
      _classPrivateFieldInitSpec(this, _data, {
        writable: true,
        value: void 0
      });
      _classPrivateFieldSet(this, _eventName, eventName);
      _classPrivateFieldSet(this, _dataTypeName, dataTypeName);
      _classPrivateFieldSet(this, _data, data);
    }
    _createClass(VREvent, [{
      key: "name",
      get: function get() {
        return _classPrivateFieldGet(this, _eventName);
      }
    }, {
      key: "dataTypeName",
      get: function get() {
        return _classPrivateFieldGet(this, _dataTypeName);
      }
    }, {
      key: "data",
      get: function get() {
        return _classPrivateFieldGet(this, _data);
      }
    }, {
      key: "toJson",
      value: function toJson() {
        var evt = {};
        evt.m_DataTypeName = _classPrivateFieldGet(this, _dataTypeName);
        evt.m_Name = _classPrivateFieldGet(this, _eventName);
        evt.m_Data = _classPrivateFieldGet(this, _data);
        return JSON.stringify(evt);
      }
    }], [{
      key: "fromJson",
      value: function fromJson(jsonString) {
        var json = JSON.parse(jsonString);

        // Mirrors the switch in VREvent.cs
        switch (json.m_DataTypeName) {
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
    }]);
    return VREvent;
  }();
  var VREventVector2 = /*#__PURE__*/function (_VREvent) {
    _inherits(VREventVector2, _VREvent);
    var _super = _createSuper(VREventVector2);
    function VREventVector2(eventName, data) {
      _classCallCheck(this, VREventVector2);
      return _super.call(this, eventName, 'Vector2', data);
    }
    return _createClass(VREventVector2);
  }(VREvent);
  var VREventVector3 = /*#__PURE__*/function (_VREvent2) {
    _inherits(VREventVector3, _VREvent2);
    var _super2 = _createSuper(VREventVector3);
    function VREventVector3(eventName, data) {
      _classCallCheck(this, VREventVector3);
      return _super2.call(this, eventName, 'Vector3', data);
    }
    return _createClass(VREventVector3);
  }(VREvent);
  var VREventVector4 = /*#__PURE__*/function (_VREvent3) {
    _inherits(VREventVector4, _VREvent3);
    var _super3 = _createSuper(VREventVector4);
    function VREventVector4(eventName, data) {
      _classCallCheck(this, VREventVector4);
      return _super3.call(this, eventName, 'Vector4', data);
    }
    return _createClass(VREventVector4);
  }(VREvent);
  var VREventQuaternion = /*#__PURE__*/function (_VREvent4) {
    _inherits(VREventQuaternion, _VREvent4);
    var _super4 = _createSuper(VREventQuaternion);
    function VREventQuaternion(eventName, data) {
      _classCallCheck(this, VREventQuaternion);
      return _super4.call(this, eventName, 'Quaternion', data);
    }
    return _createClass(VREventQuaternion);
  }(VREvent);
  var VREventGameObject = /*#__PURE__*/function (_VREvent5) {
    _inherits(VREventGameObject, _VREvent5);
    var _super5 = _createSuper(VREventGameObject);
    function VREventGameObject(eventName, data) {
      _classCallCheck(this, VREventGameObject);
      return _super5.call(this, eventName, 'GameObject', data);
    }
    return _createClass(VREventGameObject);
  }(VREvent);
  var VREventString = /*#__PURE__*/function (_VREvent6) {
    _inherits(VREventString, _VREvent6);
    var _super6 = _createSuper(VREventString);
    function VREventString(eventName, value) {
      _classCallCheck(this, VREventString);
      return _super6.call(this, eventName, 'String', value);
    }
    return _createClass(VREventString);
  }(VREvent);
  var VREventInt = /*#__PURE__*/function (_VREvent7) {
    _inherits(VREventInt, _VREvent7);
    var _super7 = _createSuper(VREventInt);
    function VREventInt(eventName, value) {
      _classCallCheck(this, VREventInt);
      return _super7.call(this, eventName, 'Int32', value);
    }
    return _createClass(VREventInt);
  }(VREvent);
  var VREventFloat = /*#__PURE__*/function (_VREvent8) {
    _inherits(VREventFloat, _VREvent8);
    var _super8 = _createSuper(VREventFloat);
    function VREventFloat(eventName, value) {
      _classCallCheck(this, VREventFloat);
      return _super8.call(this, eventName, 'Single', value);
    }
    return _createClass(VREventFloat);
  }(VREvent);

  // connect to a MinVR3 event host. returns the newly created websocket object.
  function connect(_x) {
    return _connect.apply(this, arguments);
  }
  function _connect() {
    _connect = _asyncToGenerator( /*#__PURE__*/_regeneratorRuntime().mark(function _callee(host) {
      var ws;
      return _regeneratorRuntime().wrap(function _callee$(_context) {
        while (1) switch (_context.prev = _context.next) {
          case 0:
            ws = new WebSocket("ws://".concat(host, "/vrevent"));
            return _context.abrupt("return", new Promise(function (resolve, reject) {
              ws.onopen = function () {
                console.log('MinVR3 connection opened');
                resolve();
              };
            }).then(function () {
              return ws;
            }));
          case 2:
          case "end":
            return _context.stop();
        }
      }, _callee);
    }));
    return _connect.apply(this, arguments);
  }

  exports.GameObject = GameObject;
  exports.Quaternion = Quaternion;
  exports.VREvent = VREvent;
  exports.VREventFloat = VREventFloat;
  exports.VREventGameObject = VREventGameObject;
  exports.VREventInt = VREventInt;
  exports.VREventQuaternion = VREventQuaternion;
  exports.VREventString = VREventString;
  exports.VREventVector2 = VREventVector2;
  exports.VREventVector3 = VREventVector3;
  exports.VREventVector4 = VREventVector4;
  exports.Vector2 = Vector2;
  exports.Vector3 = Vector3;
  exports.Vector4 = Vector4;
  exports.connect = connect;

  Object.defineProperty(exports, '__esModule', { value: true });

}));
