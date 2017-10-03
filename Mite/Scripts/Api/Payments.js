var PaymentsApi = {
    //Url обязательно должен быть задан в форме, собственно как и метод(post, get)
    _send: function ($form, message, onSuccess) {
        var $msg = message.container;

        if ($form.form('validate form')) {
            $form.addClass('loading');

            return $.ajax({
                url: $form.attr('action'),
                type: $form.attr('method'),
                data: $form.serialize(),
                success: function (resp) {
                    if (resp.status == undefined) {
                        resp = JSON.parse(resp);
                    }
                    if (+resp.status == Settings.apiStatuses.validationError) {
                        $msg.removeClass('green');
                        $form.form('add errors', resp.data);
                    } else {
                        $msg.html(message.successContent);
                        $msg.removeClass('error');
                        $msg.addClass('green');

                        if (onSuccess !== null)
                            onSuccess(resp);
                    }
                },
                error: function () {
                    $msg.removeClass('green');
                    $form.form('add errors', ['Внутренняя ошибка.']);
                },
                complete: function () {
                    $form.removeClass('loading');
                }
            });
        }
        return false;
    },
    yandex: {
        payIn: function() {
            var $form = $('#yaPayInForm');
            var $msg = $('#yaPayInMsg');

            return PaymentsApi._send($form, {
                container: $msg,
                successContent: 'Успешно.'
            });
        },
        payOut: function () {
            var self = this;
            var $form = $('#yaPayOutForm');
            var $msg = $('#yaPayOutMsg');

            return PaymentsApi._send($form, {
                container: $msg,
                successContent: 'Успешно.'
            });
        }
    },
    bank: {
        payIn: function () {
            var $form = $('#bankPayInForm');
            var $msg = $('#bankPayInMsg');

            return PaymentsApi._send($form, {
                container: $msg,
                successContent: 'Запрос успешно отправлен.',
            }, function (resp) {
                if (resp.status === Settings.apiStatuses.success) {
                    location.href = resp.message;
                }
            });
        }
    },
    webmoney: {
        payIn: function () {
            var $form = $('#wmPayInForm');
            var $msg = $('#wmPayInMsg');

            return PaymentsApi._send($form, {
                container: $msg,
                successContent: 'Запрос отправлен. Ожидайте СМС с кодом.'
            }, function (resp) {
                $('.tab[data-tab="payIn/wmconfirm"]').siblings('.tab').hide();
                $('.tab[data-tab="payIn/wmconfirm"]').show();
            });
        },
        confirmPayIn: function () {
            var $form = $('#wmConfirmForm');
            var $msg = $('#wmConfirmMsg');

            return PaymentsApi._send($form, {
                container: $msg,
                successContent: 'Счёт успешно пополнен'
            });
        }
    }
}