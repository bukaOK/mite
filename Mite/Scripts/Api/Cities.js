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
                alert('Ошибка');
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
                alert('Ошибка');
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
                alert('Ошибка');
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
                alert('Ошибка');
            },
            complete: function () {
                $btn.removeClass('loading');
            }
        });
    }
}