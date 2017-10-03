var DealsMvc = {
    formSelector: '#dealForm',
    baseUrl: '/deals/',
    _send: function (action, method, succCallback, $btn, data) {
        var url = this.baseUrl + action,
            $form = $(this.formSelector),
            $msg = $form.find('.message');
        $btn.addClass('loading disabled');

        if (data === undefined || data === null) {
            data = $form.serialize();
        }
        if (!$form.form('validate form')) {
            return false;
        }
        return $.ajax({
            url: url,
            type: method,
            data: data,
            success: function (resp) {
                if (resp.status === undefined) {
                    resp = JSON.parse(resp);
                }
                switch (resp.status) {
                    case Settings.apiStatuses.validationError:
                        $msg.addClass('error').removeClass('green');
                        $form.form('add errors', resp.data);
                        break;
                    case Settings.apiStatuses.success:
                        succCallback(resp.data);
                        break;
                    default:
                        throw 'DealsMvc: not implemented handler for response.';
                }
            },
            error: function (jqXhr) {
                $msg.addClass('error').removeClass('green');
                $msg.html('Внутренняя ошибка');
            },
            complete: function () {
                $btn.removeClass('loading disabled');
            }
        });
    },
    pay: function (btn) {
        var $form = $(this.formSelector);
        var $msg = $form.find('.message');

        return this._send('pay', 'post', function (resp) {
            $msg.removeClass('error').addClass('green');
            $msg.html('Успешно.');
        }, $(btn));
    },
    close: function (btn, id) {
        var $form = $(this.formSelector);
        var $msg = $form.find('.message');

        return this._send('close', 'post', function (resp) {
            location.href = '/user/deals';
        }, $(btn));
    },
    toExpectPayment: function (btn) {
        var $form = $(this.formSelector);
        var $msg = $form.find('.message');

        return this._send('toexpectpayment', 'post', function (resp) {
            $msg.removeClass('error').addClass('green');
            $msg.html('Успешно. Обновите страницу для применения изменений.');
        }, $(btn));
    }
}