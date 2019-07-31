!
function (entrance) {
    "use strict";
    if ("object" === typeof exports && "undefined" !== typeof module) {
        module.exports = entrance()
    }
    else if ("function" === typeof define && define.amd) {
        define([], entrance())
    }
    else {
        var f;
        if ("undefined" !== typeof window) {
            f = window
        }
        else {
            throw new Error('wrong execution environment')
        }
        f.OStream = entrance()
    }
}
(function () {
    var binaryPot = {
        init: function (array) {
            for (var i = 0; i < array.length; i++) {
                array[i] *= 0x1;
                if (array[i] < 0x0) {
                    array[i] += 0x100
                }
                else if (array[i] > 0xff) {
                    throw new Error('不合法字节流')
                }
            };
            return array
        },
        writeUTF: function (str, isGetBytes) {
            var back = [],
				byteSize = 0x0;
            for (var i = 0x0; i < str.length; i++) {
                var code = str.charCodeAt(i);
                if (code >= 0x0 && code <= 0x7f) {
                    byteSize += 0x1;
                    back.push(code)
                }
                else if (code >= 0x80 && code <= 0x7ff) {
                    byteSize += 0x2;
                    back.push((0xc0 | (0x1f & (code >> 0x6))));
                    back.push((0x80 | (0x3f & code)))
                }
                else if (code >= 0x800 && code <= 0xffff) {
                    byteSize += 0x3;
                    back.push((0xe0 | (0xf & (code >> 0xc))));
                    back.push((0x80 | (0x3f & (code >> 0x6))));
                    back.push((0x80 | (0x3f & code)))
                }
            };
            for (i = 0x0; i < back.length; i++) {
                if (back[i] > 0xff) {
                    back[i] &= 0xff
                }
            };
            if (isGetBytes) {
                return back
            };
            if (byteSize <= 0xff) {
                return [0x0, byteSize].concat(back)
            }
            else {
                return [byteSize >> 0x8, byteSize & 0xff].concat(back)
            }
        },
        readUTF: function (arr) {
            if (Object.prototype.toString.call(arr) == "[object String]") {
                return arr
            };
            var UTF = "",
				_4 = this.init(arr);
            for (var i = 0x0; i < _4.length; i++) {
                var one = _4[i].toString(0x2),
					v = one.match(/^1+?(?=0)/);
                if (v && one.length == 0x8) {
                    var bytesLength = v[0x0].length,
						store = _4[i].toString(0x2).slice(0x7 - bytesLength);
                    for (var st = 0x1; st < bytesLength; st++) {
                        store += _4[st + i].toString(0x2).slice(0x2)
                    }
                    UTF += String.fromCharCode(parseInt(store, 0x2));
                    i += bytesLength - 0x1
                }
                else {
                    UTF += String.fromCharCode(_4[i])
                }
            };
            return UTF
        },
        convertStream: function (x) {
            if (x instanceof Stream) {
                return x
            }
            else {
                return new Stream(x)
            }
        },
    };

    function baseRead(m, i, a) {
        var t = a ? a : [];
        for (var start = 0x0; start < i; start++) {
            t[start] = m.pool[m.position++]
        };
        return t
    };
    var supportArrayBuffer = (function () {
        return !!window.ArrayBuffer
    })();

    function Stream(array) {
        if (!(this instanceof Stream)) {
            return new Stream(array)
        };
        this.pool = [];
        if (Object.prototype.toString.call(array) === '[object Array]') {
            this.pool = binaryPot.init(array)
        }
        else if (Object.prototype.toString.call(array) == "[object ArrayBuffer]") {
            var arr = new Int8Array(array);
            this.pool = binaryPot.init([].slice.call(arr))
        }
        else {
            var arr = new Int8Array(new ArrayBuffer(0));
            this.pool = binaryPot.init([].slice.call(arr))
        }
        var self = this;
        this.position = 0x0;
        this.writen = 0x0;
        this.check = function () {
            return self.position >= self.pool.length
        }
    };
    Stream.parse = function (x) {
        return binaryPot.convertStream(x)
    };
    Stream.prototype = {
        readInt16: function () {
            if (this.check()) {
                return -0x1
            }
            return (this.pool[this.position++] & 0xff) + (this.pool[this.position++] & 0xff) << 0x8
        },
        readUInt16: function () {
            if (this.check()) {
                return -0x1
            };
            return (this.pool[this.position++]) | (this.pool[this.position++] << 0x8)
        },
        readInt32: function () {
            if (this.check()) {
                return -0x1
            };
            return ((this.pool[this.position++]) & 0xff) | ((this.pool[this.position++] & 0xff) << 0x8) | ((this.pool[this.position++] & 0xff) << 0x10) | ((this.pool[this.position++] & 0xff) << 0x18)
        },
        readLong: function () {
            if (this.check()) {
                return -0x1
            };
            var longVBinary = "";
            for (var i = 0; i < 8; i++) {
                longVBinary = this.pool[this.position++].toString(2) + longVBinary
            }
            return parseInt(longVBinary, 2)
        },
        readByte: function () {
            if (this.check()) {
                return -0x1
            };
            var val = this.pool[this.position++];
            if (val > 0xff) {
                val &= 0xff
            }
            return val
        },
        readBool: function () {
            this.readByte();
        },
        readBytes: function (bytesArray) {
            if (this.check()) {
                return -0x1
            }
            if (bytesArray) {
                return baseRead(this, bytesArray.length | 0x0, bytesArray)
            }
            else {
                return this.readByte()
            }
        },
        readLenAndUTF: function () {
            var big = arguments.length > 0x0 ? this.readByte() : this.readInt32();
          
            if (big == -1) {
                return null
            }
            var returnValue = binaryPot.readUTF(this.pool.slice(this.position, this.position += big));
            if (arguments.length > 0x0) {
                var skip = parseInt(arguments[0x0]);
                this.position += skip - big
            }
            return returnValue
        },
        readString0: function () {
            var p = this.position;
            while (this.pool[this.position++] != 0) { };
            var returnValue = binaryPot.readUTF(this.pool.slice(p, this.position - 1));
            return returnValue;
        },
        write: function (_11) {
            var b = _11;
            if (Object.prototype.toString.call(b).toLowerCase() == "[object array]") {
                //[].push.apply(this.pool, b);  //这个写法，超出长度会报错，改写成以下方法jxx    Uncaught RangeError: Maximum call stack size exceeded
                this.pool = this.pool.concat(b);
                this.writen += b.length
            }
            else {
                if (+b == b) {
                    if (b > 0xff) {
                        b &= 0xff
                    };
                    this.pool.push(b);
                    this.writen++
                }
            };
            return b
        },
        writeChar: function (v) {
            if (+v != v) {
                throw new Error("writeChar:arguments type is error")
            };
            this.write(v & 0xff);
            this.write((v >> 0x8) & 0xff)
        },
        writeBool: function (v) {
            this.write(v);
        },
        writeInt16: function (v) {
            if (+v != v) {
                throw new Error("writeInt16:arguments type is error")
            };
            this.write(v & 0xff);
            this.write((v >> 0x8) & 0xff)
        },
        writeInt32: function (v) {
            if (v !== Math.round(v)) {
                throw new Error("writeInt32:arguments type is error")
            };
            this.write(v & 0xff);
            this.write((v >> 0x8) & 0xff);
            this.write((v >> 0x10) & 0xff);
            this.write((v >> 0x18) & 0xff)
        },
        writeLong: function (v) {
            var binaryStr = util.strPadLeft(v.toString(2), 64);
            for (var i = 7; i >= 0; i--) {
                var tempb = binaryStr.substring(i * 8, (i + 1) * 8);
                this.write(parseInt(tempb, 2))
            }
        },
        writeUTF: function (str) {
            var val = binaryPot.writeUTF(str, true);
            [].push.apply(this.pool, val);
            this.writen += val.length;
        },
        writeString0: function (str) {
            var val = binaryPot.writeUTF(str, true);
            //[].push.apply(this.pool, val);
            this.pool = this.pool.concat(val);
            this.writen += val.length;
            this.write(0);
        },
        writeLenAndUTF: function (str) {
            var val = binaryPot.writeUTF(str, true);
            this.writeInt32(val.length);
            [].push.apply(this.pool, val);
            this.writen += val.length;
            return val.length
        },
        writeLenAndBytes: function (array) {
            if (Object.prototype.toString.call(array).toLowerCase() == "[object array]") {
                this.writeInt32(array.length);
                this.pool = this.pool.concat(array);
                this.writen += array.length
            }
            else {
                throw new Error('argment is  not  array')
            }
        },
        toComplements: function () {
            var _10 = this.pool;
            for (var i = 0x0; i < _10.length; i++) {
                if (_10[i] > 0x80) {
                    _10[i] -= 0x100
                }
            }
            return _10
        },
        getBytesArray: function (isCom) {
            if (isCom) {
                return this.toComplements()
            }
            return this.pool
        },
        toArrayBuffer: function () {
            if (supportArrayBuffer) {
                return new Uint8Array(this.getBytesArray()).buffer
            }
            else {
                throw new Error('not support arraybuffer')
            }
        },
        clear: function () {
            this.pool = [];
            this.writen = this.position = 0x0
        },
        append: function (stream) {
            if (!stream instanceof Stream) {
                throw new Error('argument type is not  Stream!')
            } [].push.apply(this.pool, stream.pool);
            this.writen += stream.pool.length;
            return this
        }
    };
    return Stream
});

var messageID = 0x1;
var util = {
    inherits: function (ctor, superCtor) {
        ctor.super_ = superCtor;
        ctor.prototype = Object.create(superCtor.prototype,
        {
            constructor:
            {
                value: ctor,
                enumerable: false,
                writable: true,
                configurable: true
            }
        })
    },
    extend: function (dest, source) {
        for (var key in source) {
            if (source.hasOwnProperty(key)) {
                dest[key] = source[key]
            }
        };
        return dest
    },
    setZeroTimeout: (function (global) {
        var timeouts = [];
        var messageName = 'zero-timeout-message';

        function setZeroTimeoutPostMessage(fn) {
            timeouts.push(fn);
            global.postMessage(messageName, '*')
        };

        function handleMessage(event) {
            if (event.source == global && event.data == messageName) {
                if (event.stopPropagation) {
                    event.stopPropagation()
                };
                if (timeouts.length) {
                    timeouts.shift()()
                }
            }
        };
        if (global.addEventListener) {
            global.addEventListener('message', handleMessage, true)
        }
        else if (global.attachEvent) {
            global.attachEvent('onmessage', handleMessage)
        };
        return setZeroTimeoutPostMessage
    }(this)),
    getMessageID: function () {
        return messageID++
    },
    SystemID: "_0",
    getbytes: function (str) {
        if (typeof str === "string") {
            var back = [],
                byteSize = 0x0;
            for (var i = 0x0; i < str.length; i++) {
                var code = str.charCodeAt(i);
                if (code >= 0x0 && code <= 0x7f) {
                    byteSize += 0x1;
                    back.push(code)
                }
                else if (code >= 0x80 && code <= 0x7ff) {
                    byteSize += 0x2;
                    back.push((0xc0 | (0x1f & (code >> 0x6))));
                    back.push((0x80 | (0x3f & code)))
                }
                else if (code >= 0x800 && code <= 0xffff) {
                    byteSize += 0x3;
                    back.push((0xe0 | (0xf & (code >> 0xc))));
                    back.push((0x80 | (0x3f & (code >> 0x6))));
                    back.push((0x80 | (0x3f & code)))
                }
            };
            for (i = 0x0; i < back.length; i++) {
                if (back[i] > 0xff) {
                    back[i] &= 0xff
                }
            };
            return back
        }
        else if (typeof str === "undefined") {
            return []
        }
        else {
            throw new Error('Invalid Str Input')
        }
    },
    getStr: function (_4) {
        var UTF = "";
        if (Object.prototype.toString.call(_4) !== '[object Array]') {
            return UTF
        };
        for (var i = 0x0; i < _4.length; i++) {
            var one = _4[i].toString(0x2),
                v = one.match(/^1+?(?=0)/);
            if (v && one.length == 0x8) {
                var bytesLength = v[0x0].length,
                    store = _4[i].toString(0x2).slice(0x7 - bytesLength);
                for (var st = 0x1; st < bytesLength; st++) {
                    store += _4[st + i].toString(0x2).slice(0x2)
                }
                UTF += String.fromCharCode(parseInt(store, 0x2));
                i += bytesLength - 0x1
            }
            else {
                UTF += String.fromCharCode(_4[i])
            }
        }
        return UTF
    },
    readyState: new Array("正在连接", "已建立连接", "正在关闭连接", "已关闭连接"),
    uuid: function () {
        var s = [];
        var hexDigits = "0123456789abcdef";
        for (var i = 0; i < 36; i++) {
            s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1)
        }
        s[14] = "4";
        s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);
        s[8] = s[13] = s[18] = s[23] = "-";
        var uuid = s.join("");
        return uuid
    },
    strPadLeft: function (str, nSize) {
        var len = 0;
        len = str.length;
        while (len < nSize) {
            str = '0' + str;
            len++
        }
        return str
    },
};