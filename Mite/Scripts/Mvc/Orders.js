var OrdersMvc = {
    /**
     * Шаблон отправки заказа
     * @param {string} url адрес запроса
     * @param {JQuery<HTMLElement>} $form форма
    */
    _send: function (url, $form) {
        $form.addClass('loading');
        return $.ajax({
            data: new FormData($form[0]),
            url: url,
            type: 'post',
            processData: false,
            contentType: false,
            success: function (resp) {
                if (resp.status === Settings.apiStatuses.validationError) {
                    $form.removeClass('m-success');
                    $form.form('add errors', resp.data);
                }
            },
            error: function () {
                $form.removeClass('m-success');
                $form.form('add errors', ['Внутренняя ошибка']);
            },
            complete: function () {
                $form.removeClass('loading');
            }
        });
    },
    /**
     * Добавить заказ
     * @param {string|HTMLElement} formSelector форма
    */
    add: function (formSelector) {
        var $form = $(formSelector);
        return this._send('/orders/add', $form).done(function (resp) {
            if (resp.status === Settings.apiStatuses.success)
                location.reload();
        });
    },
    /**
     * Обновить заказ(шаблон)
     * @param {string|HTMLElement} formSelector форма
    */
    update: function (formSelector) {
        var $form = $(formSelector);
        return this._send('/orders/update', $form).done(function (resp) {
            if (resp.status === Settings.apiStatuses.success)
                location.reload();
        });
    },
    /**
     * Удалить заказ
     * @param {string} id id удаляемого шаблона заказа
     * @param {HTMLElement} removeElement форма
    */
    remove: function (id, removeElement) {
        return $.ajax({
            type: 'post',
            url: '/orders/remove/' + id,
            success: function (resp) {
                if (resp.status === Settings.apiStatuses.success)
                    removeElement.remove();
                else
                    iziToast.error({
                        title: 'Упс!',
                        message: resp.data[0]
                    });
            },
            error: function () {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Внутренняя ошибка сервера'
                });
            }
        });
    },
    addRequest: function (btn) {
        var $btn = $(btn);
        return this._send('/orders/addrequest', $btn.parents('.form')).done(function () {
            $btn.hide().siblings('.button').show();
        });
    },
    removeRequest: function (btn) {
        var $btn = $(btn);
        return this._send('/orders/removerequest', $btn.parents('.form')).done(function () {
            $btn.hide().siblings('.button').show();
        });
    },
    choseExecuter: function (executerId, orderId, btn) {
        $(btn).addClass('loading disabled');
        return $.post('/orders/choseexecuter', {
            executerId: executerId,
            orderId: orderId
        }, function (resp) {
            if (resp.status !== Settings.apiStatuses.validationError) {
                iziToast.success({
                    title: 'Успех!',
                    message: 'Исполнитель успешно выбран'
                });
            } else {
                iziToast.error({
                    title: 'Упс!',
                    message: resp.data[0]
                });
            }
            }).fail(function () {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Внутренняя ошибка'
                });
            }).always(function () {
            $(btn).removeClass('loading disabled');
        });
    }
}