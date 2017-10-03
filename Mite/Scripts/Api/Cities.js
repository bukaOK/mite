var CitiesApi = {
    url: '/api/cities',
    tmpl: '',
    init: function ($container) {
        var self = this;
        this.tmpl = $.templates('#citiesRowTmpl');
        return $.ajax({
            type: 'get',
            url: CitiesApi.url,
            success: function (resp) {
                $container.html(self.tmpl.render(resp));
            },
            error: function (jqXhr) {
                swal('Ошибка', 'Ошибка при инициализации', 'error');
            },
        })
    },
    add: function ($btn, $form, $container) {
        $btn.addClass('loading');
        return $.ajax({
            type: 'post',
            url: CitiesApi.url,
            success: function (resp) {
                $container.prepend(CitiesApi.tmpl.render(resp));
            },
            error: function (jqXhr) {
                swal('Ошибка', 'Ошибка при добавлении', 'error');
            },
            complete: function () {
                $btn.removeClass('loading');
            },
            data: $form.serialize()
        });
    },
    remove: function ($btn, $tr, id) {
        $btn.addClass('loading');
        return $.ajax({
            type: 'delete',
            url: CitiesApi.url + '/' + id,
            success: function (resp) {
                $tr.remove();
            },
            error: function (jqXhr) {
                swal('Ошибка', 'Ошибка при удалении', 'error');
            },
            complete: function () {
                $btn.removeClass('loading');
            }
        });
    },
    update: function ($btn, $form, $tr) {
        $btn.addClass('loading');
        return $.ajax({
            type: 'put',
            url: CitiesApi.url,
            success: function (resp) {
                var html = CitiesApi.tmpl.render(resp);
                $(html).insertBefore($tr);
                $tr.remove();
            },
            data: $form.serialize(),
            error: function (jqXhr) {
                swal('Ошибка', 'Ошибка при обновлении', 'error');
            },
            complete: function () {
                $btn.removeClass('loading');
            }
        });
    },
    bindCity: function (cityName) {
        return $.ajax({
            url: '/cities/bindcity',
            data: 'cityName=' + cityName,
            type: 'post',
            success: function (resp) {
                if (resp.status === undefined) {
                    resp = JSON.parse(resp);
                }
            },
            error: function () {
                $.ajax({
                    url: '/api/cities',
                    success: function (resp) {
                        swal('Успешно');
                        $('#cityChoseSelect').dropdown();
                        $('#cityChoseModal').modal('show');
                    }
                });
            }
        });
    }
}