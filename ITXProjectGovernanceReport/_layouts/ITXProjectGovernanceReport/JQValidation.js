$(function() {
    $.fn.validateText = function(options) {
        var defaults = {
            empty: true,
            alert: false,
            errorclass: '',
            autofocus: true
        };
        var options = $.extend(defaults, options);
        var element = $(this);
        var IsErrorFound = false;
        element.each(function() {
            if (!IsErrorFound) {
                if (options.empty) {
                    var value = $.trim($(this).val());
                    if (value == "") {
                        IsErrorFound = true;
                        doDefaults(options, "Please input the selected field", $(this));
                    }
                }
            }
        });
        if (IsErrorFound)
            return false;
        return true;
    };
    $.fn.validateEmail = function(options) {
        var defaults = {
            empty: true,
            alert: true,
            errorclass: '',
            autofocus: true
        };
        var options = $.extend(defaults, options);
        var element = $(this);
        var IsErrorFound = false;
        element.each(function() {
            if (!IsErrorFound) {
                if (options.empty) {
                    var value = $.trim($(this).val());
                    if (value == "") {
                        IsErrorFound = true;
                        doDefaults(options, "Please input the selected field", $(this));
                    }
                }
                if (!IsErrorFound) {
                    var value = $.trim($(this).val());
                    var rege = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/;
                    if (!rege.test(value)) {
                        IsErrorFound = true;
                        doDefaults(options, "Please input valid email!", $(this));
                    }
                }
            }
        });
        if (IsErrorFound)
            return false;
        return true;
    };
    function doDefaults(options, msg, Object) {
        if (options.alert)
            alert(msg);
        if (options.errorclass != "")
            Object.addClass(options.errorclass);
        if (options.autofocus)
            Object.focus();
    }
});