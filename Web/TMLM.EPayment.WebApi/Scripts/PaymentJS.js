//if (self === top) {
//    var antiClickjack = document.getElementById("antiClickjack");
//    if (antiClickjack) antiClickjack.parentNode.removeChild(antiClickjack);
//} else {
//    top.location = self.location;
//}

function PayJS(aid) {
    var isValid = true;
    if ($('#cardnumber').val().trim() == "") {
        $('#errorCardNumber').text("Card number is required");
        isValid = false;
    }
    else if ($('#cardnumber').val().trim().length != 16) {
        $('#errorCardNumber').text("Invalid Card number length");
        isValid = false;
    }
    else {
        $('#errorCardNumber').text("");
    }

    if ($('#securityCode').val().trim() == "") {
        $('#errorSecurityCode').text("Security code is required");

        isValid = false;
    }
    else {
        
        $('#errorSecurityCode').text("");
    }

    if ($('#bankname').val().trim() == "") {
        $('#errorBankName').text("Bank Name is required");

        isValid = false;
    }
    else {

        $('#errorBankName').text("");
    }

    var date = new Date();
    var month = date.getMonth();
    if (month.toString().length === 1) {
        month = "0" + month;
    }
    if ($("#ExpiryMonthDropDown").val() <= month && $("#ExpiryYearDropDown").val() == date.getFullYear().toString().substr(-2)) {
        $('#errorExpiryDate').text("Invalid expiry month");
        isValid = false;
    }
    else {
        $('#errorExpiryDate').text("");
    }
    


    if (aid == "ESUB") {
        if (!$('[name="accept"]').prop('checked')) {

            alert('Please accept declaration to proceed');
            isValid = false;
        }
    }
    if (!isValid) {
        return;
    }
    $('#loadingDiv').show();

    var path = window.location.protocol + "//" + window.location.host + $("#payButton").data("path");
    var formData = $("#MPGSPaymentform").serializeArray();
    formData.push({
        "name": "SecureIdResponseUrl",
        "value": path
    });
    formData.push({
        "name": "Aid",
        "value": aid
    });
    //var model = JSON.stringify($("#MPGSPaymentform").serialize());
    $.ajax({
        type: "POST",
        url: 'ProcessTransaction',
        data: formData,

        //data: JSON.stringify(requestData),
        //contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (res) {
            //$('#loadingDiv').hide();
            $('#responseDiv').append(res);
            $('form[name="echoForm"]').submit();
        },
        error: function (res) {
            $('#loadingDiv').hide()
        }
    });
}

var ccNum = "";
$('#cardnumber').focusout(function () {
    if (ccNum != $("#cardnumber").val()) {
        var previousCCNum = ccNum;
        ccNum = $("#cardnumber").val();
        if (ccNum != '' && ccNum.length >= 6) {
            $('#loadingDiv').show()
            $.ajax({
                type: "POST",
                url: 'getBankName',
                data: { "CCNumber": ccNum },
                dataType: "json",
                success: function (res) {
                    if (res.bin) {
                        $('#bankname').val(res.bankName);
                        $('#bankname').attr('disabled', 'disabled');

                    }
                    else {
                        if (previousCCNum != '' && previousCCNum.length >= 6) {
                            if (previousCCNum.substring(0, 6) != ccNum.substring(0, 6)) {
                                $('#bankname').removeAttr('disabled');
                                $('#bankname').val('');
                            }
                        }
                      
                    }
                    $('#loadingDiv').hide()

                },
                error: function (res) {

                    $('#loadingDiv').hide()
                }
            });
        }
        else if (ccNum == '' || (ccNum != '' && ccNum.length < 6)) {
            $('#bankname').removeAttr('disabled');
            $('#bankname').val('');
        };
    }
});


function OnlyNumber(i) {
    if (i.value.length >= 0 && i.value.length <= 16) {
        i.value = i.value.replace(/[^\d]+/, '');
    }
    else {
        i.value = i.value.substring(0, 16);
    }
}

function Pay() {
    $.ajax({
        type: "POST",
        url: 'MPGSPayment/PayTransaction',
        dataType: "json",
        //data: null,
        contentType: "application/json; charset=utf-8",
        success: function (res) {

            var result = res.d;
            document.getElementById("testing").innerHTML(result);
            //html content
            //window.location.href = res.resUrl;
            if (!alert(res)) {

            } else {

            }
        },
    });
}

function CancelJS() {
    var formData = $("#MPGSPaymentform").serializeArray();
    $('#loadingDiv').show();

    $.ajax({
        type: "POST",
        url: 'CancelTransaction',
        data: formData,
        dataType: "json",
        success: function (res) {

           // $('#loadingDiv').hide();
            $('#responseDiv').append(res);
            $('form[name="echoForm"]').submit();
        },
        error: function (res) {
            $('#loadingDiv').hide()
        }
    });
}

function RedirectJS() {
    var formData = $("#MPGSPaymentform").serializeArray();

    $.ajax({
        type: "POST",
        url: 'RedirectTransaction',
        data: formData,
        dataType: "json",
        success: function (res) {
            $('#responseDiv').append(res);
            $('form[name="echoForm"]').submit();
        },
    });
}


window.onload = function () {
    $('#loadingDiv').hide()
    var interval = 60 * 7,
        //var interval = 5,
        display = document.querySelector('#time');

    var start = Date.now(),
        diff,
        minutes,
        seconds,
        tempInterval = interval;

    timer();
    var timeout = setInterval(timer, 1000);

    function timer() {
        diff = interval - (((Date.now() - start) / 1000) | 0);

        minutes = (diff / 60) | 0;
        seconds = (diff % 60) | 0;

        minutes = minutes < 10 ? "0" + minutes : minutes;
        seconds = seconds < 10 ? "0" + seconds : seconds;

        display.textContent = minutes + ":" + seconds;

        if (diff <= 0) {
            start = Date.now() + 1000;
        }

        tempInterval--;
        if (tempInterval == 0) {
            display.textContent = "00 : 00";

            clearInterval(timeout);

            $("#dialog-ok").dialog({
                resizable: false,
                height: "auto",
                width: 400,
                modal: true,
                buttons: {
                    Ok: function () {
                        RedirectJS();
                    }
                }
            });


        }
    };
};




