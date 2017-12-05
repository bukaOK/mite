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
            url: '/api/authorservices/' + id,
            type: 'delete',
            success: function () {
                $('.tab[data-tab="services"]').masonry('remove', $('[data-service-id="' + id + '"]')[0]);
                iziToast.success({
                    title: 'Успешно!',
                    message: 'Услуга удалена.'
                });
            },
            error: function (jqXhr) {
                var error = 'Ошибка.',
                    jsonResp = jqXhr.responseJSON;
                if (jsonResp !== null && jsonResp.length > 0)
                    error = jsonResp[0];
                iziToast.error({
                    title: 'Упс!',
                    message: error
                });
            }
        })
    },
    /**
     * Грузим результаты услуги
     * @param {string} id id услуги
     * @param {JQuery<HTMLElement>} $img изображение услуги
    */
    loadGallery: function (id, $img) {
        return $.ajax({
            url: '/authorservices/servicegallery/' + id,
            success: function (resp) {
                if (resp.status === undefined) {
                    resp = JSON.parse(resp);
                }
                resp.data.unshift({
                    ImageSrc: $img.attr('src')
                });
                MiteGallery.initService(resp.data, $img);
            }
        });
    }
}