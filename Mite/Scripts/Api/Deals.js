var DealsApi = {
    url: '/api/deals',
    formSelector: '#dealForm',

    errorFunc: function ($form, $msg, jqXhr) {
        var errors = [];
        switch (jqXhr.status) {
            case 400:
                var modelState = jqXhr.responseJSON.ModelState;
                for (var stateKey in modelState) {
                    modelState[stateKey].forEach(function (err) {
                        errors.push(err);
                    });
                }
                break;
            case 503:
                jqXhr.responseJSON.forEach(function (err) {
                    errors.push(err);
                });
                break;
            default:
                errors.push('Внутренняя ошибка');
                break;
        }
        $msg.removeClass('green').addClass('error');
        $form.form('add errors', errors);
    },
    _send: function (method, succCallback, $btn) {
        var self = DealsApi;
        var $form = $(self.formSelector);
        var $msg = $form.find('.message');
        if ($btn === undefined) {
            $btn = $form.find('.button');
        }
        if (!$form.form('validate form')) {
            return false;
        }
        $btn.addClass('loading disabled');
        return $.ajax({
            url: self.url,
            type: method,
            data: $form.serialize(),
            success: function (resp) {
                succCallback(resp);
            },
            error: function (jqXhr) {
                self.errorFunc($form, $msg, jqXhr);
            },
            complete: function () {
                $btn.removeClass('loading disabled');
            }
        });
    },
    update: function (btn) {
        var $form = $(this.formSelector);
        var $msg = $form.find('.message');

        return this._send('put', function (resp) {
            $msg.removeClass('error').addClass('green');
            $msg.html('Успешно');
        }, $(btn));
    },
    create: function () {
        var $form = $(this.formSelector);
        var $msg = $form.find('.message');

        return this._send('post', function () {
            $msg.removeClass('error').addClass('green');
            $msg.html('Успешно')
        });
    },
    remove: function (id) {
        var self = DealsApi;

        return $.ajax({
            type: 'delete',
            url: self.url,
            success: function () {

            },
            error: function () {
                swal('Упс!', 'Не удалось удалить сделку', 'error');
            }
        });
    }
}