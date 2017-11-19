var DealsMvc = {
    formSelector: '#dealForm',
    baseUrl: '/deals/',
    /**
     * Отправка запроса
     * @param {string} action action url
     * @param {string} method метод запроса
     * @param {function} succCallback callback при успехе
     * @param {JQuery<HTMLElement>} $btn кнопка сохранения
     * @param {Object} data данные для запроса
     * @param {boolean} validate нужна ли валидация полей
    */
    _send: function (action, method, succCallback, $btn, data, validate) {
        var url = this.baseUrl + action,
            $form = $(this.formSelector),
            $msg = $form.find('.message');
        $btn.addClass('loading disabled');

        if (data === undefined) {
            data = $form.serialize();
        }
        if (validate !== false && !$form.form('validate form')) {
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
                $form.form('add errors', ['Внутренняя ошибка']);
            },
            complete: function () {
                $btn.removeClass('loading disabled');
            }
        });
    },
    rate: function (id, value) {
        var url = 'rate/' + id + '?value=' + value;
        return this._send(url, 'post', function (resp) {
            $('#rating-res').text(value);
        }, $(null), null);
    },
    giveFeedback: function (btn) {
        return this._send('givefeedback', 'post', function (resp) {
            $(DealsMvc.formSelector + ' .message').removeClass('error').addClass('green')
                .html('Отзыв успешно оставлен');
        }, $(btn));
    },
    pay: function (btn) {
        var $btn = $(btn);

        return this._send('pay', 'post', function (resp) {
            $btn.attr('disabled', '');
            $btn.addClass('disabled');
            $btn.text('Денежная оплата совершена');

            if (resp.payed) {
                location.reload();
            }
        }, $btn);
    },
    confirm: function (btn, id) {
        return this._send('confirm/' + id, 'post', function () {
            location.reload();
        }, $(btn), null);
    },
    moderConfirm: function (btn, id, confirm) {
        var url = 'moderconfirm/' + id + '?confirm=' + confirm.toString();
        return this._send(url, 'post', function () {
            location.reload();
        }, $(btn), null);
    },
    openDispute: function (btn, id) {
        return this._send('opendispute/' + id, 'post', function () {
            location.reload();
        }, $(btn), null);
    },
    checkVkRepost: function (btn, id) {
        var $btn = $(btn);
        return this._send('checkvkrepost/' + id, 'post', function (resp) {
            $btn.attr('disabled', '');
            $btn.addClass('disabled');
            $btn.text('Репост подтвержден');

            if (resp.payed) {
                $('.step.active').removeClass('active');
                $('.step[data-step="' + resp.next + '"]').addClass('active');
            }
        }, $btn, null);
    },
    confirmVkRepost: function (btn, id) {
        var $btn = $(btn);

        return this._send('confirmvkrepost/' + id, 'post', function (resp) {
            $btn.attr('disabled', '');
            $btn.addClass('disabled');
            $btn.text('Репост подтвержден');
            if (resp.payed) {
                $('.step.active').removeClass('active');
                $('.step[data-step="' + resp.next + '"]').addClass('active');
            }
        }, $btn, null)
    },
    close: function (btn, id) {
        return this._send('reject/' + id, 'post', function (resp) {
            location.reload();
        }, $(btn), null, false);
    },
    toExpectPayment: function (btn) {
        var $form = $(this.formSelector);
        var $msg = $form.find('.message');

        return this._send('toexpectpayment', 'post', function (resp) {
            $msg.removeClass('error').addClass('green');
            $msg.html('Успешно. Обновите страницу для применения изменений.');
        }, $(btn));
    },
    /**
     * Загрузить результат работы(изображение)
     * @param {HTMLElement} btn кнопка сохранения
    */
    loadImage: function (btn) {
        var $msg = $(this.formSelector).find('.message');

        return this._send('loadimage', 'post', function (resp) {
            $msg.removeClass('error').addClass('green');
            $msg.html('Успешно');
        }, $(btn));
    }
}