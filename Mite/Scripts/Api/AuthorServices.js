var AuthorServiceApi = {
    formSelector: '#authorServiceForm',
    msgSelector: '#serviceEditMsg',
    _send: function (url, type) {
        var $form = $(this.formSelector),
            $msg = $(this.msgSelector);
        if (!$form.form('validate form')) {
            return false;
        }
        if ($('#ImageBase64').val().length / 1024 / 1024 > 30) {
            $form.form('add errors', ['Изображение не должно превышать по размеру 30 мбайт.'])
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
                $('.tab[data-tab="services"]').masonry('remove', $('[data-service-id="' + id + '"]')[0])
                    .masonry('layout');
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
     * Добавляем список услуг из вк
     * @param {string|HTMLElement} formSelector селектор форм
    */
    addVkList: function (formSelector, btn) {
        var $forms = $(formSelector),
            invalid = $forms.form('validate form').some(function (validItem) {
                return !validItem;
            }), data = [];
        if (invalid)
            return null;
        $(btn).addClass('loading disabled');
        $forms.each(function (index, form) {
            var $form = $(form);
            data.push({
                ImageBase64: $form.find('[name=ImageBase64]').val(),
                VkLink: $form.find('[name=VkLink]').val(),
                VkThumbLink: $form.find('[name=VkThumbLink]').val(),
                Title: $form.find('[name=Title]').val(),
                ServiceTypeId: $form.find('[name=ServiceTypeId]').val(),
                Price: $form.find('[name=Price]').val(),
                DeadlineNum: $form.find('[name=DeadlineNum]').val(),
                Description: $form.find('[name=Description]').val(),
                VkPostCode: $form.find('[name=VkPostCode]').val()
            });
        });
        return $.ajax({
            type: 'post',
            data: {
                services: data
            },
            success: function (resp) {
                if (resp.status != Settings.apiStatuses.success) {
                    iziToast.error({
                        title: 'Упс!',
                        message: resp.data[0]
                    });
                } else {
                    location.reload();
                }
            },
            error: function () {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Внутренняя ошибка сервера'
                });
            },
            complete: function () {
                $(btn).removeClass('loading disabled');
            }
        });
    },
    /**
     * Грузим результаты услуги
     * @param {string} id id услуги
     * @param {JQuery<HTMLElement>} $img изображение услуги
    */
    loadGallery: function (id, $img) {
        return $.getJSON('/authorservices/servicegallery/' + id).done(function (resp) {
            if (resp.status === undefined) {
                resp = JSON.parse(resp);
            }
            resp.data.unshift({
                ImageSrc: $img.attr('src')
            });
            MiteGallery.initService(resp.data, $img);
        });
    }
}