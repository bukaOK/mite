//AuthorServiceTypes
var ServiceTypesApi = {
    url: '/api/authorservicetypes',
    form: $('#serviceTypeForm'),
    formButton: $('#serviceTypeBtn'),
    table: $('.tab[data-tab="servicetypes"] tbody'),
    tmpl: $.templates('#serviceTypeRowTmpl'),

    init: function () {
        var self = ServiceTypesApi;

        return $.ajax({
            url: self.url,
            type: 'get',
            success: function (resp) {
                var html = self.tmpl.render(resp);
                self.table.html(html);
            },
            error: function () {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Типы услуг не загружены.'
                });
            }
        })
    },
    _send: function (method, onSuccess, onError) {
        var self = ServiceTypesApi;

        if (self.form.form('validate form')) {
            self.formButton.addClass('loading');
        }
        return $.ajax({
            url: self.url,
            type: method,
            data: self.form.serialize(),
            success: function (resp) {
                if (onSuccess != undefined) {
                    onSuccess(resp);
                }
            },
            error: function (jqXhr) {
                if (onError != undefined) {
                    onError(jqXhr);
                } else {
                    $('#serviceTypeMsg').removeClass('green').addClass('error');
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
                    self.form.form('add errors', errors);
                }
            },
            complete: function () {
                self.formButton.removeClass('loading');
            }
        });
    },
    add: function () {
        return this._send('post', function (resp) {
            var html = self.tmpl.render(resp);
            self.table.prepend(html);
        });
    },
    addByAuthor: function () {
        var self = ServiceTypesApi;
        return this._send('post', function (resp) {
            $('#serviceTypeMsg').removeClass('error')
                .addClass('green')
                .html('Успешно');
        });
    },
    update: function ($tr) {
        var self = this;
        return this._send('put', function (resp) {
            var html = self.tmpl.render(resp);
            $(html).insertBefore($tr);
            $tr.remove();
        });
    },
    remove: function ($btn, $tr, id) {
        var self = ServiceTypesApi;

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
            }
        });
    }
}
