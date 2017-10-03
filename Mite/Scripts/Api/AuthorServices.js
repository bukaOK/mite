var AuthorServiceApi = {
    formSelector: '#authorServiceForm',
    msgSelector: '#serviceEditMsg',
    _send: function (url, type) {
        var $form = $(this.formSelector);
        var $msg = $(this.msgSelector);
        if (!$form.form('validate form')) {
            return false;
        }
        $form.addClass('loading');
        return $.ajax({
            url: url,
            type: type,
            data: $form.serialize(),
            success: function (resp) {
                $msg.removeClass('error').addClass('green');
                $msg.html('Успешно.');
            },
            error: function (jqXhr) {
                console.log(jqXhr);
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
            complete: function () {
                $form.removeClass('loading');
            }
        });
    },
    add: function () {
        return this._send('/api/authorservices/', 'post');
    },
    update: function () {
        return this._send('/api/authorservices/', 'put');
    },
    remove: function (id) {
        return $.ajax({
            url: '/api/authorservices/',
            type: 'delete',
            data: 'id=' + id,
            success: function () {
                swal('Успешно!', 'Услуга успешно удалена.', 'success');
            },
            error: function (jqXhr) {
                swal('Упс!', 'Не удалось удалить услугу.', 'error');
            }
        })
    }
}