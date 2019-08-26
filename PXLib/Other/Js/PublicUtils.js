
//2018-05-01 收集封装的一些常用js方法 基于jq
/*---------------------------------网络请求Ajax------------------------------------*/
function getTextAjax(url, parm, callBack) {
    $.ajax({
        type: 'post',
        dataType: "text",
        url: url,
        data: parm,
        cache: false,
        async: false,
        success: function (msg) {
            callBack(msg);
        }
    });
}
function getAjax(url, parm, isAsync, callBack) {
    $.ajax({
        type: 'post',
        dataType: "json",
        contentType: "application/json",
        url: url,
        //beforeSend: function (request) {
        //    request.setRequestHeader("AccessToken", "AAAAAAAAAAAAAAAAAAAAAAAAA");
        //},
        //data: parm,//二选一
        data: parm != null ? $.toJSON(parm) : {},//解决svc和aspx的空参数问题
        cache: false,
        async: isAsync,
        timeout: 3000,//超时时间设置，单位毫秒
        success: function (msg) {
            callBack(msg);
        },
        error: function (error) {
            console.log(JSON.stringify(error));
        }
    });
}
(function ($) {
    /*JSON转换*/
    $.toJSON = function (o) {
        if (typeof (JSON) == 'object' && JSON.stringify)
            return JSON.stringify(o); var type = typeof (o); if (o === null)
            return "null"; if (type == "undefined")
            return undefined; if (type == "number" || type == "boolean")
            return o + ""; if (type == "string")
            return $.quoteString(o); if (type == 'object') {
                if (typeof o.toJSON == "function")
                    return $.toJSON(o.toJSON()); if (o.constructor === Date) {
                        var month = o.getUTCMonth() + 1; if (month < 10) month = '0' + month; var day = o.getUTCDate(); if (day < 10) day = '0' + day; var year = o.getUTCFullYear(); var hours = o.getUTCHours(); if (hours < 10) hours = '0' + hours; var minutes = o.getUTCMinutes(); if (minutes < 10) minutes = '0' + minutes; var seconds = o.getUTCSeconds(); if (seconds < 10) seconds = '0' + seconds; var milli = o.getUTCMilliseconds(); if (milli < 100) milli = '0' + milli; if (milli < 10) milli = '0' + milli; return '"' + year + '-' + month + '-' + day + 'T' +
                            hours + ':' + minutes + ':' + seconds + '.' + milli + 'Z"';
                    }
                if (o.constructor === Array) {
                    var ret = []; for (var i = 0; i < o.length; i++)
                        ret.push($.toJSON(o[i]) || "null"); return "[" + ret.join(",") + "]";
                }
                var pairs = []; for (var k in o) {
                    var name; var type = typeof k; if (type == "number")
                        name = '"' + k + '"'; else if (type == "string")
                        name = $.quoteString(k); else
                        continue; if (typeof o[k] == "function")
                        continue; var val = $.toJSON(o[k]); pairs.push(name + ":" + val);
                }
                return "{" + pairs.join(", ") + "}";
            }
    };
    $.evalJSON = function (src) {
        if (typeof (JSON) == 'object' && JSON.parse)
            return JSON.parse(src); return eval("(" + src + ")");
    };
    $.secureEvalJSON = function (src) {
        if (typeof (JSON) == 'object' && JSON.parse)
            return JSON.parse(src); var filtered = src; filtered = filtered.replace(/\\["\\\/bfnrtu]/g, '@'); filtered = filtered.replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, ']'); filtered = filtered.replace(/(?:^|:|,)(?:\s*\[)+/g, ''); if (/^[\],:{}\s]*$/.test(filtered))
            return eval("(" + src + ")"); else
            throw new SyntaxError("Error parsing JSON, source is not valid.");
    };
    $.quoteString = function (string) {
        if (string.match(_escapeable)) {
            return '"' + string.replace(_escapeable, function (a) { var c = _meta[a]; if (typeof c === 'string') return c; c = a.charCodeAt(); return '\\u00' + Math.floor(c / 16).toString(16) + (c % 16).toString(16); }) + '"';
        }
        return '"' + string + '"';
    };
    var _escapeable = /["\\\x00-\x1f\x7f-\x9f]/g;
    var _meta = { '\b': '\\b', '\t': '\\t', '\n': '\\n', '\f': '\\f', '\r': '\\r', '"': '\\"', '\\': '\\\\' };
    $.windowWidth = function () {
        return $(window).width();
    };
    $.windowHeight = function () {
        return $(window).height();
    };
    $.arrayClone = function (data) {
        return $.map(data, function (obj) {
            return $.extend(true, {}, obj);
        });
    };

})(jQuery);

/*---------------------------------工具函数------------------------------------*/
function request(key) {
    //相当于GetUrlParame
    for (var u = location.search.slice(1), r = u.split("&"), i, t = 0; t < r.length; t++) if (i = r[t].split("="), i[0] == key) return unescape(i[1]) == "undefined" ? "" : unescape(i[1]);
    return ""
}
function changeURLArg(url, arg, arg_val) {
    var pattern = arg + '=([^&]*)';
    var replaceText = arg + '=' + arg_val;
    if (url.match(pattern)) {
        var tmp = '/(' + arg + '=)([^&]*)/gi';
        tmp = url.replace(eval(tmp), replaceText);
        return tmp;
    } else {
        if (url.match('[\?]')) {
            return url + '&' + replaceText;
        } else {
            return url + '?' + replaceText;
        }
    }
    return url + '\n' + arg + '\n' + arg_val;
}
function reload() {
    location.reload();
    return false;
}
function openWindowFrame(url, titel, witdth, height) {
    var winWidth = document.body.clientWidth;//那么中心点应该就是取1/2
    var winHeight = document.body.clientHeight;
    var center = { centerW: winWidth / 2, centerH: winHeight / 2 };
    window.open(url, titel, 'top=' + center.centerW - witdth / 2 + ',left=' + center.centerH - height / 2 + ',width=' + top + ',height=' + height + ', toolbar=no, menubar=no, scrollbars=no, resizable=no,location=no, status=no');
}
function IsNullOrEmpty(str) {
    var isOK = false;
    if (str == undefined || str == "" || str == null || str == "undefined" || str == "null") {
        isOK = true;
    }
    return isOK;
}
function deleteHtmlTag(str, lenth, endstr) {
    //删除HTML标签
    var title = str.replace(/<[^>]+>/g, "").replace(/&nbsp;/g, " ").replace(/[\r\n\s\f\t\v\o ]+/gm, "");//去掉所有的html标记和空格空行
    if (title.length >= lenth) {
        return title = title.substring(0, lenth) + endstr;
    }
    return title;
}
function isOverDate(endDate) {
    //判断最后时间是否过期
    var d = new Date(endDate.replace(/-/g, "/"));
    if (new Date() > Date.parse(d)) {
        return true;
    }
    return false;
}
function GetTime() {
    //获取当前时间 格式：2017 - 05 - 20 14: 24: 21
    function checkNum(num) {
        if (num < 10)
            return "0" + num;
        else
            return num;
    }
    var mydate = new Date();
    var str = "" + mydate.getFullYear() + "-";
    str += checkNum(mydate.getMonth() + 1) + "-";
    str += checkNum(mydate.getDate()) + " ";
    str += checkNum(mydate.getHours()) + ":";
    str += checkNum(mydate.getMinutes()) + ":";
    str += checkNum(mydate.getSeconds()) + "";
    return str;
}
function formatDate(v, format) {
    //时间格式转换formatDate("2017/6/28 19:20:20","yyyy-MM-dd HH:mm:ss")
    if (!v) return "";
    var d = v;
    if (typeof v === 'string') {
        if (v.indexOf("/Date(") > -1)
            d = new Date(parseInt(v.replace("/Date(", "").replace(")/", ""), 10));
        else
            d = new Date(Date.parse(v.replace(/-/g, "/").replace("T", " ").split(".")[0]));//.split(".")[0] 用来处理出现毫秒的情况，截取掉.xxx，否则会出错
    }
    var o = {
        "M+": d.getMonth() + 1,  //month
        "d+": d.getDate(),       //day
        "H+": d.getHours(),      //hour
        "m+": d.getMinutes(),    //minute
        "s+": d.getSeconds(),    //second
        "q+": Math.floor((d.getMonth() + 3) / 3),  //quarter
        "S": d.getMilliseconds() //millisecond
    };
    if (/(y+)/.test(format)) {
        format = format.replace(RegExp.$1, (d.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(format)) {
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
        }
    }
    return format;
}
function setTimeDesc(time, timeSelector) {
    //将长时间转换成微信时间描述
    var currentTime = Date.parse(new Date());
    var dateTime = time;//后台传递来的时间
    var ts = timeSelector;//选择器
    var d_day = Date.parse(new Date(dateTime));
    var day = Math.abs(parseInt((d_day - currentTime) / 1000 / 3600 / 24));//计算日期
    var hour = Math.abs(parseInt((d_day - currentTime) / 1000 / 3600));//计算小时
    var minutes = Math.abs(parseInt((d_day - currentTime) / 1000 / 60));//计算分钟
    var seconds = Math.abs(parseInt((d_day - currentTime) / 1000));//计算秒
    console.log(day);
    if (day >= 3 && day <= 6) {
        ts.text(parseInt(day) + "天前");
    }
    else if (day > 0 && day < 2) {
        ts.text("昨天");
    } else if (hour > 0 && hour < 24) {
        ts.text(parseInt(hour) + "小时前");
    } else if (minutes > 0 && minutes < 60) {
        ts.text(parseInt(minutes) + "分钟前");
    } else if (seconds > 0 && seconds < 60) {
        ts.text("刚刚");
    } else if (day >= 7) {
        ts.text(dateTime.toString());
    }
}
function formatSecondsToTime(seconds) {
    //将秒数转换成时间
    var min = Math.floor(seconds / 60),
        second = seconds % 60,
        hour, newMin, time;
    if (min > 60) {
        hour = Math.floor(min / 60);
        newMin = min % 60;
    }
    if (second < 10) { second = '0' + second; }
    if (min < 10) { min = '0' + min; }
    return time = hour ? (hour + ':' + newMin + ':' + second) : (min + ':' + second);
}
function toDecimal(num) {
    if (num == null || null == undefined || num == "") {
        num = "0";
    }
    num = num.toString().replace(/\$|\,/g, '');
    if (isNaN(num))
        num = "0";
    sign = (num == (num = Math.abs(num)));
    num = Math.floor(num * 100 + 0.50000000001);
    cents = num % 100;
    num = Math.floor(num / 100).toString();
    if (cents < 10)
        cents = "0" + cents;
    for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3); i++)
        num = num.substring(0, num.length - (4 * i + 3)) + '' +
            num.substring(num.length - (4 * i + 3));
    return (((sign) ? '' : '-') + num + '.' + cents);
}
function EmojiUtf16toStr(str) {
    /**
 * emoji转字符串
 * @param {Object} str
 */
    var patt = /[\ud800-\udbff][\udc00-\udfff]/g; // 检测utf16字符正则  
    str = str.replace(patt, function (char) {
        var H, L, code;
        if (char.length === 2) {
            H = char.charCodeAt(0); // 取出高位  
            L = char.charCodeAt(1); // 取出低位  
            code = (H - 0xD800) * 0x400 + 0x10000 + L - 0xDC00; // 转换算法  
            return "&#" + code + ";";
        }
        else {
            return char;
        }
    });
    return str;
}
function getRandomColor() {
    var color = ["FFCCFF", "FF99CC", "FF9999", "FF9966", "FF9933", "00CCFF", "0099FF", "33FF66", "FF9900", "FF3300", "FF33CC", "FF3399", "FF66FF", "FF0033", "6633CC", "666699", "660066", "33FF33", "339933"];
    var random = Math.round(Math.random() * (color.length - 2) + 1);
    return "#" + color[random];
}
function newGuid() {
    //生成GUID
    var s = [];
    var hexDigits = "0123456789abcdef";
    for (var i = 0; i < 36; i++) {
        s[i] = hexDigits.substr(Math.floor(Math.random() * 0x10), 1)
    }
    s[14] = "4";
    s[19] = hexDigits.substr((s[19] & 0x3) | 0x8, 1);
    s[8] = s[13] = s[18] = s[23] = "-";
    var uuid = s.join("");
    return uuid;
}
var isWap = function () {
    //是否是移动端
    if (navigator.userAgent.match(/(iPhone|iPod|Android|ios|MiuiBrowser)/i)) {
        return true;
    }
    return false;
};
var isInArray = function (arr, value) {
    //是否在数组中。
    for (var i = 0, l = arr.length; i < l; i++) {
        if (arr[i] === value) {
            return true;
        }
    }
    return false;
};
var roundString = function (len) {
    //获取随机字符串
    var result = "";
    var charArr = '01234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
    len = len || 5;
    for (var i = 0; i < len; i++) {
        var index = roundFromTo(0, charArr.length) - 1;
        result += charArr.charAt(index);
    }
    return result;
};
var roundFromTo = function (from, to) {
    //生成from到to的随机数；整数，包含to不包含from
    var react = to - from;
    return Math.ceil(Math.random() * react + from);
};

/*------------------------------------------------UI操作封装-------------------------------------------------*/
$.fn.GetWebControls = function (keyValue) {
    var reVal = "";
    $(this).find('input,select,textarea,.ui-select,img').each(function (r) {
        var id = $(this).attr('id');
        if (IsNullOrEmpty(id))//jxx改
        {
            if ($(this).is(":checked")) {
                id = $(this).attr('class');
            }
            else {
                return;
            }
        }
        var type = $(this).attr('type');
        if ($(this)[0].tagName.toLowerCase() == "img") //jxx改
        {
            type = "img";
        }
        switch (type) {
            case "checkbox":
                if ($("#" + id).is(":checked")) {
                    reVal += '"' + id + '"' + ':' + '"1",'
                } else {
                    reVal += '"' + id + '"' + ':' + '"0",'
                }
                break;
            case "select":
                var value = $("#" + id).attr('data-value');
                if (IsNullOrEmpty(value)) {
                    value = "";
                }
                reVal += '"' + id + '"' + ':' + '"' + $.trim(value) + '",'
                break;
            case "selectTree":
                var value = $("#" + id).attr('data-value');
                if (IsNullOrEmpty(value)) {
                    value = "";
                }
                reVal += '"' + id + '"' + ':' + '"' + $.trim(value) + '",'
                break;
            case "img":
                var value = $("#" + id).attr('src');
                if (IsNullOrEmpty(value)) {
                    value = "";
                }
                reVal += '"' + id + '"' + ':' + '"' + $.trim(value) + '",'
                break;
            case "radio":
                var value = $("." + id + ":checked").val();
                if (IsNullOrEmpty(value)) {
                    value = "";
                }
                reVal += '"' + id + '"' + ':' + '"' + $.trim(value) + '",'
                break;
            default:
                var value = $("#" + id).val();
                if (IsNullOrEmpty(value)) {
                    value = "";
                }
                reVal += '"' + id + '"' + ':' + '"' + $.trim(value) + '",'
                break;
        }
    });
    reVal = reVal.substr(0, reVal.length - 1);
    //if (!keyValue) {
    //    reVal = reVal.replace(/&nbsp;/g, '');
    //}
    reVal = reVal.replace(/\\/g, '\\\\');
    reVal = reVal.replace(/\n/g, '\\n');
    var postdata = jQuery.parseJSON('{' + reVal + '}');
    //阻止伪造请求
    //if ($('[name=__RequestVerificationToken]').length > 0) {
    //    postdata["__RequestVerificationToken"] = $('[name=__RequestVerificationToken]').val();
    //}
    return postdata;
};
$.fn.SetWebControls = function (data) {
    var $id = $(this)
    for (var key in data) {
        var id = $id.find('#' + key);
        if (id.length == 0)//jxx改，如果ID找不到则找CLASS
        {
            id = $id.find('.' + key).eq(0);
        }
        if (id.attr('id') || id.attr("class")) {
            var type = id.attr('type');
            if (id.hasClass("input-datepicker")) {
                type = "datepicker";
            }
            //jxx改
            if (id[0].tagName == "IMG") {
                type = "img";
            }
            var value = $.trim(data[key]).replace(/&nbsp;/g, '');
            switch (type) {
                case "checkbox":
                    if (value == 1) {
                        id.attr("checked", 'checked');
                    } else {
                        id.removeAttr("checked");
                    }
                    break;
                case "select":
                    id.ComboBoxSetValue(value);
                    break;
                case "selectTree":
                    id.ComboBoxTreeSetValue(value);
                    break;
                case "datepicker":
                    id.val(formatDate(value, 'yyyy-MM-dd'));
                    break;
                case "img": //jxx改
                    id.attr("src", value);
                    break;
                case "radio": //jxx改
                    $("." + key + "[value='" + value + "']").attr("checked", "checked");
                    break;
                default:
                    id.val(value);
                    break;
            }
        }
    }
}

/*------------------------------------------------字典对象-------------------------------------------------*/
function OrayDic() {
    //字典 var dic =new OrayDic();
    this.array = [];
    this.remove = function (key) {
        var index = this.array.indexOf(key);
        if (index != -0x1) {
            this.array.splice(index, 0x2)
        }
    }, this.getValue = function (key) {
        var index = this.array.indexOf(key);
        if (index != -0x1) {
            return this.array[index + 0x1]
        };
        return null
    }, this.setValue = function (key, value) {
        var index = this.array.indexOf(key);
        if (index != -0x1) {
            this.array[index + 0x1] = value
        }
        else {
            this.array.push(key, value)
        }
    }, this.getALL = function () {
        return this.array;
    }
}

/*------------------------------------------------cookie 缓存-------------------------------------------------*/
var Cookie = (function () {
    //cookie操作
    //titmeout 单位为天
    var data = {};
    var _init = function () {
        data = {};//初始化
        var cookieArray = document.cookie.split("; ");
        for (var i = 0; i < cookieArray.length; i++) {
            var arr = cookieArray[i].split("=");
            if (typeof (data[arr[0]]) == 'undefined') {
                data[arr[0]] = unescape(arr[1]);
            }
        }
        return data;
    }
    var get = function (key) {//没有key代表获取所有
        _init();
        if (key == undefined) return data;
        return data[key];
    };
    var set = function (key, value, timeout) {
        var str = escape(key) + "=" + escape(value);//不设置时间代表跟随页面生命周期
        if (timeout == undefined) {//时间以小时计
            timeout = 365;
        }
        var expDate = new Date();
        expDate.setTime(expDate.getTime() + timeout * 3600 * 24 * 1000);
        str += "; expires=" + expDate.toGMTString();
        document.cookie = str;
    };
    var del = function (key) {
        document.cookie = key + "=;expires=" + (new Date(0)).toGMTString();
    };
    var clear = function () {
        _init();
        for (var key in data) {
            del(key);
        }
    }
    return {
        get: get,
        set: set,
        del: del,
        clear: clear
    }
})();
var LocalData = (function () {
    //LocalData操作 数据存储
    //var LocalData = function () { };
    var nameSpace = 'kodcloud-';
    var makeKey = function (key) {
        if (key != '') {
            return nameSpace + key;
        } else {
            return key;
        }
    }
    var support = function () {
        try {
            var supported = !!window.localStorage;
            if (supported) {
                window.localStorage.setItem("storage", "");
                window.localStorage.removeItem("storage");
            }
            return supported;
        } catch (err) {
            return false;
        }
    }
    var get = function (key) {//没有key代表获取所有
        key = makeKey(key);
        if (support()) {
            if (key != undefined) {
                return localStorage.getItem(key);
            } else {
                var result = {};
                for (var i = 0; i < localStorage.length; i++) {
                    result[localStorage.key(i)] = localStorage.getItem(localStorage.key(i));
                }
                return result;
            }
        } else {
            return Cookie.get(key);
        }
    };
    var set = function (key, value, timeout) {
        key = makeKey(key);
        if (support()) {
            localStorage.setItem(key, value);
        } else {
            Cookie.set(key, value, timeout);
        }
    };
    var del = function (key) {
        key = makeKey(key);
        if (support()) {
            localStorage.removeItem(key);
        } else {
            Cookie.del(key);
        }
    };

    //复杂数据读写 只存储json数据
    var setConfig = function (key, value) {
        key = makeKey(key);
        value = base64Encode(jsonEncode(value));
        if (support()) {
            localStorage.setItem(key, value);
        }
    }
    //复杂数据读写
    var getConfig = function (key) {
        var result = this.get(key);
        if (result === null || result == undefined || result == '') {
            return false;
        } else {
            return jsonDecode(base64Decode(result));
        }
    }
    var clear = function () {
        if (support()) {
            for (var i = 0; i < storage.length; i++) {
                localStorage.removeItem(storage.key(i));
            }
        } else {
            Cookie.clear();
        }
    }
    return {
        setSpace: function (space) {
            nameSpace = space ? space : '';
        },
        support: support,
        get: get,
        set: set,
        setConfig: setConfig,
        getConfig: getConfig,
        del: del,
        clear: clear
    }
})();

/*------------------------------------------------编码处理-------------------------------------------------*/
var Base64 = (function () {
    var _keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    var encode = function (input) {
        var output = "";
        var chr1, chr2, chr3, enc1, enc2, enc3, enc4;
        var i = 0;
        input = utf8Encode(input);
        while (i < input.length) {
            chr1 = input.charCodeAt(i++);
            chr2 = input.charCodeAt(i++);
            chr3 = input.charCodeAt(i++);
            enc1 = chr1 >> 2;
            enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
            enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
            enc4 = chr3 & 63;
            if (isNaN(chr2)) {
                enc3 = enc4 = 64;
            } else if (isNaN(chr3)) {
                enc4 = 64;
            }
            output = output +
                _keyStr.charAt(enc1) + _keyStr.charAt(enc2) +
                _keyStr.charAt(enc3) + _keyStr.charAt(enc4);
        }
        return output;
    }
    // public method for decoding  
    var decode = function (input) {
        var output = "";
        var chr1, chr2, chr3;
        var enc1, enc2, enc3, enc4;
        var i = 0;
        input = input.replace(/[^A-Za-z0-9\+\/\=]/g, "");
        while (i < input.length) {
            enc1 = _keyStr.indexOf(input.charAt(i++));
            enc2 = _keyStr.indexOf(input.charAt(i++));
            enc3 = _keyStr.indexOf(input.charAt(i++));
            enc4 = _keyStr.indexOf(input.charAt(i++));
            chr1 = (enc1 << 2) | (enc2 >> 4);
            chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
            chr3 = ((enc3 & 3) << 6) | enc4;
            output = output + String.fromCharCode(chr1);
            if (enc3 != 64) {
                output = output + String.fromCharCode(chr2);
            }
            if (enc4 != 64) {
                output = output + String.fromCharCode(chr3);
            }
        }
        output = utf8Decode(output);
        return output;
    }
    // private method for UTF-8 encoding  
    utf8Encode = function (string) {
        var utftext = "";
        for (var n = 0; n < string.length; n++) {
            var c = string.charCodeAt(n);
            if (c < 128) {
                utftext += String.fromCharCode(c);
            } else if ((c > 127) && (c < 2048)) {
                utftext += String.fromCharCode((c >> 6) | 192);
                utftext += String.fromCharCode((c & 63) | 128);
            } else {
                utftext += String.fromCharCode((c >> 12) | 224);
                utftext += String.fromCharCode(((c >> 6) & 63) | 128);
                utftext += String.fromCharCode((c & 63) | 128);
            }

        }
        return utftext;
    }

    // private method for UTF-8 decoding  
    utf8Decode = function (utftext) {
        var string = "";
        var i = 0;
        var c = c1 = c2 = 0;
        while (i < utftext.length) {
            c = utftext.charCodeAt(i);
            if (c < 128) {
                string += String.fromCharCode(c);
                i++;
            } else if ((c > 191) && (c < 224)) {
                c2 = utftext.charCodeAt(i + 1);
                string += String.fromCharCode(((c & 31) << 6) | (c2 & 63));
                i += 2;
            } else {
                c2 = utftext.charCodeAt(i + 1);
                c3 = utftext.charCodeAt(i + 2);
                string += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
                i += 3;
            }
        }
        return string;
    };
    return {
        encode: encode,
        decode: decode
    }
})();

var Base64Server = (function () {
    var encode = function (stringToEncode) {
        var encodeUTF8string = function (str) {
            return encodeURIComponent(str).replace(/%([0-9A-F]{2})/g,
                function toSolidBytes(match, p1) {
                    return String.fromCharCode('0x' + p1)
                });
        }
        if (typeof window !== 'undefined') {
            if (typeof window.btoa !== 'undefined') {
                return window.btoa(encodeUTF8string(stringToEncode));
            }
        } else {
            return new Buffer(stringToEncode).toString('base64');
        }
        var b64 = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/='
        var o1, o2, o3, h1, h2, h3, h4, bits;
        var i = 0;
        var ac = 0;
        var enc = '';
        var tmpArr = [];
        if (!stringToEncode) {
            return stringToEncode
        }
        stringToEncode = encodeUTF8string(stringToEncode)
        do {
            o1 = stringToEncode.charCodeAt(i++);
            o2 = stringToEncode.charCodeAt(i++);
            o3 = stringToEncode.charCodeAt(i++);
            bits = o1 << 16 | o2 << 8 | o3;
            h1 = bits >> 18 & 0x3f;
            h2 = bits >> 12 & 0x3f;
            h3 = bits >> 6 & 0x3f;
            h4 = bits & 0x3f;
            tmpArr[ac++] = b64.charAt(h1) + b64.charAt(h2) + b64.charAt(h3) + b64.charAt(h4);
        } while (i < stringToEncode.length)

        enc = tmpArr.join('');
        var r = stringToEncode.length % 3;
        return (r ? enc.slice(0, r - 3) : enc) + '==='.slice(r || 3);
    }
    // public method for decoding  
    var decode = function (encodedData) {
        var decodeUTF8string = function (str) {
            try {
                return decodeURIComponent(str.split('').map(function (c) {
                    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
                }).join(''));
            } catch (e) {
                return str;
            }
        }
        if (typeof window !== 'undefined') {
            if (typeof window.atob !== 'undefined') {
                return decodeUTF8string(window.atob(encodedData))
            }
        } else {
            return new Buffer(encodedData, 'base64').toString('utf-8')
        }
        var b64 = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/='
        var o1, o2, o3, h1, h2, h3, h4, bits;
        var i = 0
        var ac = 0
        var dec = ''
        var tmpArr = []

        if (!encodedData) {
            return encodedData
        }
        encodedData += ''
        do {
            h1 = b64.indexOf(encodedData.charAt(i++))
            h2 = b64.indexOf(encodedData.charAt(i++))
            h3 = b64.indexOf(encodedData.charAt(i++))
            h4 = b64.indexOf(encodedData.charAt(i++))

            bits = h1 << 18 | h2 << 12 | h3 << 6 | h4
            o1 = bits >> 16 & 0xff
            o2 = bits >> 8 & 0xff
            o3 = bits & 0xff;

            if (h3 === 64) {
                tmpArr[ac++] = String.fromCharCode(o1)
            } else if (h4 === 64) {
                tmpArr[ac++] = String.fromCharCode(o1, o2)
            } else {
                tmpArr[ac++] = String.fromCharCode(o1, o2, o3)
            }
        } while (i < encodedData.length);
        dec = tmpArr.join('')
        return decodeUTF8string(dec.replace(/\0+$/, ''))
    }
    return {
        encode: encode,
        decode: decode
    }
})();
// 处理 emoji时有差异;
var base64Encode = Base64Server.encode;
var base64Decode = Base64Server.decode;

/*------------------------------------------------加密、解密-------------------------------------------------*/
