var DailyFactsApi = {
    url: '/api/facts',
    formSelector: '#factsForm',
    init: function ($container) {
        var self = this;
        if(!self.tmpl)
            self.tmpl = $.templates('#factRowTmpl');
        return $.getJSON(self.url, function (resp) {
            $container.html(self.tmpl.render(resp));
            window.facts = resp;
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Ошибка во загрузки фактов'
            });
        });
    },
    /**
     * Обработка ответа со статусом 400
     * @param {JQueryXHR} jqXhr
     */
    _handleBadRequest: function (jqXhr) {
        var resp = jqXhr.responseJSON,
            errors = [];
        for (var key in resp.ModelState) {
            resp.ModelState[key].forEach(function (val) {
                errors.push(val);
            });
        }
        return errors[0];
    },
    /**
     * Добавление факта
     * @param {HTMLElement} btn
     * @param {JQuery<HTMLElement>} $container
     */
    add: function (btn, $container) {
        var self = this,
            $btn = $(btn),
            $form = $(self.formSelector);
        return $.post(self.url, $form.serialize(), function (resp) {
            $container.prepend(self.tmpl.render(resp));
            window.facts.push(resp);
        }).fail(function (jqXhr) {
            var errMsg = 'Внутренняя ошибка';
            if (jqXhr.status === 400) {
                errMsg = self._handleBadRequest(jqXhr);
            }
            iziToast.error({
                title: 'Упс!',
                message: errMsg
            });
        });
    },
    update: function (factId) {
        var self = this,
            $tr = $('#factRow' + factId),
            $form = $(self.formSelector);
        return $.ajax({
            type: 'put',
            url: self.url,
            success: function (resp) {
                var html = self.tmpl.render(resp);
                $(html).insertBefore($tr);
                $tr.remove();
                var replaceIndex = window.facts.findIndex(function (fItem) {
                    return resp.Id === fItem.Id;
                });
                window.facts[replaceIndex] = resp;
            },
            data: $form.serialize(),
            error: function (jqXhr) {
                var errMsg = 'Ошибка при обновлении';
                if (jqXhr.status === 400) {
                    errMsg = self._handleBadRequest(jqXhr);
                }
                iziToast.error({
                    title: 'Упс!',
                    message: errMsg
                });
            }
        });
    },
    remove: function (btn, id) {
        var self = this,
            $btn = $(btn),
            $tr = $btn.parents('tr');
        $btn.addClass('loading');
        return $.ajax({
            type: 'delete',
            url: self.url + '/' + id,
            success: function (resp) {
                $tr.remove();
            },
            error: function (jqXhr) {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при удалении.'
                });
            },
            complete: function () {
                $btn.removeClass('loading');
            }
        });
    }
}