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
                iziToast.error({
                    title: 'Упс!',
                    message: 'Города не загружены.'
                });
            },
        });
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
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при добавлении.'
                });
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
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при удалении.'
                });
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
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при обновлении.'
                });
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
                        $('#cityChoseSelect').dropdown();
                        $('#cityChoseModal').modal('show');
                    }
                });
            }
        });
    },
    /**
     * Загружаем города по стране в dropdown
     * @param {string} targetSel
     * @param {string} countryId
     * @returns {JQueryXHR}
     */
    loadCities: function (countryId) {
        return $.getJSON('/api/cities/' + countryId);
    }
}