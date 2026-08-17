// JSB runtime conventions:
// - object id 0 is null/undefined.
// - object id 1 is the global object.
// - retained object/function refs returned to C# must be released by JSB_ReleaseRef.
// - return values and callback arguments share the same stack value queue and are consumed by JSB_PopXXX.
// - JS exceptions are stored on the current call frame and popped by JSB_PopExceptionJson.
var LibraryJSB = {
    JSB_SetCallbackFunc: function(funcPtr) {
        Module.JSBState = Module.JSBState || {
            // Retained JavaScript object references. C# stores ids, JS owns the values.
            objects: { 1: typeof window !== 'undefined' ? window : (typeof global !== 'undefined' ? global : this) },
            nextId: 2,

            // Nested C# -> JS and JS -> C# calls share a frame stack. Each frame has outgoing args,
            // readable values, a read cursor, and an optional normalized JS exception.
            callStacks: [],
            currStack: null,

            // Last JSB operation status. StatusCode mirrors JSB.StatusCode in JSB.cs.
            statusCode: 0,
            errorMessage: "",

            callbackFunc: null,

            // ---------------------------------------------------------------------
            // Object refs
            // ---------------------------------------------------------------------
            retainObj: function(obj) {
                if (obj === null || obj === undefined) return 0;
                var id = Module.JSBState.nextId++;
                Module.JSBState.objects[id] = obj;
                return id;
            },
            releaseObj: function(id) {
                if (id > 1) {
                    delete Module.JSBState.objects[id];
                }
            },
            getObj: function(id) {
                return Module.JSBState.objects[id];
            },

            // ---------------------------------------------------------------------
            // Value and memory helpers
            // ---------------------------------------------------------------------
            getTypeHint: function(val) {
                if (val === null || val === undefined) return 0; // Null
                if (typeof val === 'boolean') return 1; // Boolean
                if (typeof val === 'number') return 2; // Number
                if (typeof val === 'string') return 3; // String
                return 4; // Object
            },
            toInt: function(val) {
                var num = Number(val);
                return num < 0 ? Math.ceil(num) : Math.floor(num);
            },
            allocateString: function(str) {
                if (str === null || str === undefined) return 0;
                var strStr = String(str);
                var len = lengthBytesUTF8(strStr) + 1;
                var ptr = _malloc(len);
                stringToUTF8(strStr, ptr, len);
                return ptr;
            },

            // ---------------------------------------------------------------------
            // Status and exceptions
            // ---------------------------------------------------------------------
            setInternalError: function(statusCode, errMsg) {
                Module.JSBState.statusCode = statusCode;
                Module.JSBState.errorMessage = errMsg || "Unknown error";
            },
            clearError: function() {
                Module.JSBState.statusCode = 0;
                Module.JSBState.errorMessage = "";
            },
            normalizeException: function(e) {
                var isError = e instanceof Error;
                var message = isError && e.message ? String(e.message) : String(e);
                return {
                    name: isError && e.name ? String(e.name) : "",
                    message: message,
                    stack: isError && e.stack ? String(e.stack) : ""
                };
            },
            setException: function(e) {
                if (!Module.JSBState.currStack) {
                    Module.JSBState.setInternalError(4, "stack is empty");
                    return;
                }
                Module.JSBState.currStack.exception = Module.JSBState.normalizeException(e);
                Module.JSBState.statusCode = 1;
                Module.JSBState.errorMessage = "";
            },

            // ---------------------------------------------------------------------
            // Stack frames
            // ---------------------------------------------------------------------
            createFrame: function(values) {
                return { args: [], values: values || [], valueIndex: 0, exception: null };
            },
            pushFrame: function(values) {
                Module.JSBState.currStack = Module.JSBState.createFrame(values);
                Module.JSBState.callStacks.push(Module.JSBState.currStack);
                return Module.JSBState.currStack;
            },
            popFrame: function() {
                Module.JSBState.callStacks.pop();
                Module.JSBState.currStack = Module.JSBState.callStacks.length > 0
                    ? Module.JSBState.callStacks[Module.JSBState.callStacks.length - 1]
                    : null;
            },
            setReturnValue: function(value) {
                if (Module.JSBState.currStack) {
                    Module.JSBState.currStack.values[0] = value;
                }
            },
            setNullReturn: function() {
                Module.JSBState.setReturnValue(null);
                return 0; // Null
            },
            popValue: function() {
                if (!Module.JSBState.currStack) {
                    Module.JSBState.setInternalError(4, "stack is empty");
                    return undefined;
                }
                var stack = Module.JSBState.currStack;
                if (stack.valueIndex >= stack.values.length) {
                    Module.JSBState.setInternalError(5, "stack value is missing");
                    return undefined;
                }
                return stack.values[stack.valueIndex++];
            },
            peekValue: function() {
                if (!Module.JSBState.currStack) {
                    Module.JSBState.setInternalError(4, "stack is empty");
                    return undefined;
                }
                var stack = Module.JSBState.currStack;
                if (stack.valueIndex >= stack.values.length) {
                    Module.JSBState.setInternalError(5, "stack value is missing");
                    return undefined;
                }
                return stack.values[stack.valueIndex];
            },
            checkValueType: function(value, expectedType) {
                if (value === undefined) {
                    Module.JSBState.setInternalError(2, `type check error: expect ${expectedType}, get undefined`);
                    return false;
                } else {
                    var valueType = typeof(value);
                    if (expectedType != valueType){
                        Module.JSBState.setInternalError(2, `type check error: expect ${expectedType}, get ${valueType}`);
                        return false;
                    }
                    return true;
                }

            },
            // ---------------------------------------------------------------------
            // Callback and argument object helpers
            // ---------------------------------------------------------------------
            createCallback: function(handlerId, callbackId) {
                return function() {
                    if (!Module.JSBState.callbackFunc) return;

                    var values = [];
                    for (var i = 0; i < arguments.length; i++) {
                        values.push(arguments[i]);
                    }
                    Module.JSBState.clearError();
                    Module.JSBState.pushFrame(values);

                    var func = Module.JSBState.callbackFunc;
                    try {
                        if (typeof wasmTable !== 'undefined') {
                            wasmTable.get(func)(handlerId, callbackId);
                        } else if (typeof dynCall === 'function') {
                            dynCall('vii', func, [handlerId, callbackId]);
                        } else if (typeof Module !== 'undefined' && Module['dynCall_vii']) {
                            Module['dynCall_vii'](func, handlerId, callbackId);
                        } else {
                            {{{ makeDynCall('vii', 'func') }}}(handlerId, callbackId);
                        }
                    } catch(e) {
                        console.error("JSB callback error", e);
                    } finally {
                        Module.JSBState.popFrame();
                    }
                };
            },
            processObjectRefKeys: function(obj) {
                if (obj && typeof obj === 'object' && Array.isArray(obj.__obj_ref_keys)) {
                    var objRefKeys = obj.__obj_ref_keys;
                    for (var k = 0; k < objRefKeys.length; k++) {
                        var objRefKey = objRefKeys[k];
                        var objRefId = obj[objRefKey];
                        if (typeof objRefId === 'number') {
                            obj[objRefKey] = Module.JSBState.getObj(objRefId);
                        }
                    }
                }
                return obj;
            },
            // ---------------------------------------------------------------------
            // Binary helpers
            // ---------------------------------------------------------------------
            toByteArray: function(value) {
                if (value === null || value === undefined) {
                    return new Uint8Array(0);
                }
                if (value instanceof ArrayBuffer) {
                    return new Uint8Array(value);
                }
                if (ArrayBuffer.isView(value)) {
                    return new Uint8Array(value.buffer, value.byteOffset, value.byteLength);
                }
                if (Array.isArray(value)) {
                    return new Uint8Array(value);
                }
                return new Uint8Array(0);
            }
        };
        Module.JSBState.callbackFunc = funcPtr;
    },

    JSB_ReleaseRef: function(id) {
        Module.JSBState.releaseObj(id);
    },

    JSB_CreateObject: function() {
        Module.JSBState.clearError();
        try {
            return Module.JSBState.retainObj({});
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            return 0;
        }
    },

    JSB_SetDouble: function(id, propPtr, val) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            if (obj === undefined) {
                Module.JSBState.setInternalError(7, "Object not found: " + id);
                return;
            }
            obj[UTF8ToString(propPtr)] = Number(val);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
        }
    },

    JSB_SetBool: function(id, propPtr, val) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            if (obj === undefined) {
                Module.JSBState.setInternalError(7, "Object not found: " + id);
                return;
            }
            obj[UTF8ToString(propPtr)] = Boolean(val);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
        }
    },

    JSB_SetInt: function(id, propPtr, val) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            if (obj === undefined) {
                Module.JSBState.setInternalError(7, "Object not found: " + id);
                return;
            }
            obj[UTF8ToString(propPtr)] = Module.JSBState.toInt(val);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
        }
    },

    JSB_SetObjectRef: function(id, propPtr, valueRefId) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            if (obj === undefined) {
                Module.JSBState.setInternalError(7, "Object not found: " + id);
                return;
            }
            obj[UTF8ToString(propPtr)] = Module.JSBState.getObj(valueRefId);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
        }
    },

    JSB_SetFunc: function(id, propPtr, handlerId, callbackId) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            if (obj === undefined) {
                Module.JSBState.setInternalError(7, "Object not found: " + id);
                return;
            }
            obj[UTF8ToString(propPtr)] = Module.JSBState.createCallback(handlerId, callbackId);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
        }
    },

    JSB_SetString: function(id, propPtr, valPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            if (obj === undefined) {
                Module.JSBState.setInternalError(7, "Object not found: " + id);
                return;
            }
            obj[UTF8ToString(propPtr)] = UTF8ToString(valPtr);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
        }
    },

    JSB_SetNull: function(id, propPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            if (obj === undefined) {
                Module.JSBState.setInternalError(7, "Object not found: " + id);
                return;
            }
            obj[UTF8ToString(propPtr)] = null;
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
        }
    },

    JSB_SetBytes: function(id, propPtr, dataPtr, length) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            if (obj === undefined) {
                Module.JSBState.setInternalError(7, "Object not found: " + id);
                return;
            }
            var len = Number(length || 0);
            var bytes = new Uint8Array(len > 0 && dataPtr ? len : 0);
            if (bytes.byteLength > 0) {
                bytes.set(HEAPU8.subarray(dataPtr, dataPtr + len));
            }
            obj[UTF8ToString(propPtr)] = bytes.buffer;
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
        }
    },

    JSB_GetDouble: function(id, propPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            var prop = UTF8ToString(propPtr);
            return Number(obj[prop]);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            return 0;
        }
    },

    JSB_GetBool: function(id, propPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            var prop = UTF8ToString(propPtr);
            return Boolean(obj[prop]);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            return false;
        }
    },

    JSB_GetInt: function(id, propPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            var prop = UTF8ToString(propPtr);
            return Module.JSBState.toInt(obj[prop]);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            return 0;
        }
    },

    JSB_GetObjectRef: function(id, propPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            var prop = UTF8ToString(propPtr);
            return Module.JSBState.retainObj(obj[prop]);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            return 0;
        }
    },

    JSB_GetString: function(id, propPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            var prop = UTF8ToString(propPtr);
            var val = obj[prop];
            return Module.JSBState.allocateString(val == null ? "" : String(val));
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            return Module.JSBState.allocateString("");
        }
    },

    JSB_GetJson: function(id, propPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            var prop = UTF8ToString(propPtr);
            var val = obj[prop];
            var jsonStr = JSON.stringify(val);
            if (jsonStr === undefined) jsonStr = "null";
            return Module.JSBState.allocateString(jsonStr);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            return Module.JSBState.allocateString("null");
        }
    },

    JSB_GetTypeHint: function(id, propPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            var prop = UTF8ToString(propPtr);
            return Module.JSBState.getTypeHint(obj[prop]);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            return 0;
        }
    },

    JSB_GetBytesLength: function(id, propPtr) {
        Module.JSBState.clearError();
        try {
            var obj = Module.JSBState.getObj(id);
            var prop = UTF8ToString(propPtr);
            return Module.JSBState.toByteArray(obj[prop]).byteLength;
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            return 0;
        }
    },

    JSB_GetBytes: function(id, propPtr, bufferPtr, length) {
        Module.JSBState.clearError();
        try {
            if (!bufferPtr) return;
            var obj = Module.JSBState.getObj(id);
            var prop = UTF8ToString(propPtr);
            var bytes = Module.JSBState.toByteArray(obj[prop]);
            var len = Math.min(Number(length || 0), bytes.byteLength);
            HEAPU8.set(bytes.subarray(0, len), bufferPtr);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
        }
    },

    JSB_GetStatusCode: function() {
        return Module.JSBState.statusCode;
    },

    JSB_GetErrorMessage: function() {
        return Module.JSBState.allocateString(Module.JSBState.errorMessage);
    },

    JSB_PopExceptionJson: function() {
        if (!Module.JSBState.currStack || !Module.JSBState.currStack.exception) {
            return Module.JSBState.allocateString("null");
        }

        var exception = Module.JSBState.currStack.exception;
        Module.JSBState.currStack.exception = null;
        return Module.JSBState.allocateString(JSON.stringify(exception));
    },

    JSB_BeginCall: function() {
        Module.JSBState.clearError();
        Module.JSBState.pushFrame();
    },

    JSB_PushBool: function(val) {
        if (Module.JSBState.currStack) Module.JSBState.currStack.args.push(Boolean(val));
    },

    JSB_PushInt: function(val) {
        if (Module.JSBState.currStack) Module.JSBState.currStack.args.push(Number(val));
    },

    JSB_PushString: function(valPtr) {
        if (Module.JSBState.currStack) Module.JSBState.currStack.args.push(UTF8ToString(valPtr));
    },

    JSB_PushDouble: function(val) {
        if (Module.JSBState.currStack) Module.JSBState.currStack.args.push(Number(val));
    },

    JSB_PushBytes: function(dataPtr, length) {
        if (!Module.JSBState.currStack) return;
        var len = Number(length || 0);
        if (len <= 0 || !dataPtr) {
            Module.JSBState.currStack.args.push(new Uint8Array(0).buffer);
            return;
        }

        var bytes = new Uint8Array(len);
        bytes.set(HEAPU8.subarray(dataPtr, dataPtr + len));
        Module.JSBState.currStack.args.push(bytes.buffer);
    },

    JSB_PushNull: function() {
        if (Module.JSBState.currStack) Module.JSBState.currStack.args.push(null);
    },

    JSB_PushJson: function(jsonPtr) {
        if (!Module.JSBState.currStack) return;
        var jsonStr = UTF8ToString(jsonPtr);
        try {
            var obj = JSON.parse(jsonStr);
            obj = Module.JSBState.processObjectRefKeys(obj);
            Module.JSBState.currStack.args.push(obj);
        } catch(e) {
            Module.JSBState.setInternalError(6, e.toString());
            Module.JSBState.currStack.args.push(null);
        }
    },

    JSB_CreateFunc: function(handlerId, callbackId) {
        return Module.JSBState.retainObj(Module.JSBState.createCallback(handlerId, callbackId));
    },

    JSB_PushObjectRef: function(objectRefId) {
        if (Module.JSBState.currStack) Module.JSBState.currStack.args.push(Module.JSBState.getObj(objectRefId));
    },

    JSB_EndCall: function(id, fnPtr) {
        if (Module.JSBState.statusCode !== 0) {
            return Module.JSBState.setNullReturn();
        }

        var obj = Module.JSBState.getObj(id);
        if (obj === undefined) {
            Module.JSBState.setInternalError(7, "Object not found: " + id);
            return Module.JSBState.setNullReturn();
        }

        var fn = UTF8ToString(fnPtr);
        var args = Module.JSBState.currStack ? Module.JSBState.currStack.args : [];

        var ret;
        if (fn) {
            var func = obj[fn];
            if (typeof func !== 'function') {
                Module.JSBState.setInternalError(3, "Function not found: " + fn);
                return Module.JSBState.setNullReturn();
            }

            try {
                ret = func.apply(obj, args);
            } catch(e) {
                Module.JSBState.setException(e);
                return Module.JSBState.setNullReturn();
            }
        } else {
            if (typeof obj === 'function') {
                try {
                    ret = obj.apply(null, args);
                } catch(e) {
                    Module.JSBState.setException(e);
                    return Module.JSBState.setNullReturn();
                }
            } else {
                ret = obj;
            }
        }

        Module.JSBState.setReturnValue(ret);
        return Module.JSBState.getTypeHint(ret);
    },

    JSB_ClearCall: function() {
        Module.JSBState.popFrame();
    },

    JSB_JsonCall: function(id, fnPtr, jsonPtr) {
        Module.JSBState.clearError();
        Module.JSBState.pushFrame();

        var obj = Module.JSBState.getObj(id);
        if (obj === undefined) {
            Module.JSBState.setInternalError(7, "Object not found: " + id);
            return Module.JSBState.setNullReturn();
        }

        var fn = UTF8ToString(fnPtr);
        var jsonStr = UTF8ToString(jsonPtr);

        var args = [];
        if (jsonStr) {
            try {
                var parsed = JSON.parse(jsonStr);
                parsed = Module.JSBState.processObjectRefKeys(parsed);
                args = [parsed];
            } catch(e) {
                Module.JSBState.setInternalError(6, e.toString());
                return Module.JSBState.setNullReturn();
            }
        }

        var ret;
        if (fn) {
            var func = obj[fn];
            if (typeof func !== 'function') {
                Module.JSBState.setInternalError(3, "Function not found: " + fn);
                return Module.JSBState.setNullReturn();
            }

            try {
                ret = func.apply(obj, args);
            } catch(e) {
                Module.JSBState.setException(e);
                return Module.JSBState.setNullReturn();
            }
        } else {
            if (typeof obj === 'function') {
                try {
                    ret = obj.apply(null, args);
                } catch(e) {
                    Module.JSBState.setException(e);
                    return Module.JSBState.setNullReturn();
                }
            } else {
                ret = obj;
            }
        }

        Module.JSBState.setReturnValue(ret);
        return Module.JSBState.getTypeHint(ret);
    },

    JSB_PopInt: function() {
        var val = Module.JSBState.popValue();
        return Module.JSBState.toInt(val);
    },

    JSB_PopBool: function() {
        var val = Module.JSBState.popValue();
        return Boolean(val);
    },

    JSB_PopObjectRef: function() {
        var val = Module.JSBState.popValue();
        return Module.JSBState.retainObj(val);
    },

    JSB_PopString: function() {
        var val = Module.JSBState.popValue();
        return Module.JSBState.allocateString(val == null ? "" : String(val));
    },

    JSB_PopJson: function() {
        var val = Module.JSBState.popValue();
        var jsonStr = JSON.stringify(val);
        if (jsonStr === undefined) jsonStr = "null";
        return Module.JSBState.allocateString(jsonStr);
    },

    JSB_PeekBytesLength: function() {
        return Module.JSBState.toByteArray(Module.JSBState.peekValue()).byteLength;
    },

    JSB_PopBytes: function(bufferPtr, length) {
        var bytes = Module.JSBState.toByteArray(Module.JSBState.popValue());
        if (!bufferPtr) return;
        var len = Math.min(Number(length || 0), bytes.byteLength);
        HEAPU8.set(bytes.subarray(0, len), bufferPtr);
    },

    JSB_PopDouble: function() {
        if (!Module.JSBState.currStack) {
            Module.JSBState.setInternalError(4, "stack is empty");
            return 0;
        }
        var val = Module.JSBState.popValue();
        return Number(val);
    }
};

mergeInto(LibraryManager.library, LibraryJSB);
