var UserSettingsApi = {
    /**
     * Сохранить настройки уведомлений
     * @param {HTMLElement} elem элемент формы
    */
    updateNotifySettings: function (elem) {
        var $form = $(elem).parents('.form');
        if (!$form.form('validate form')) {
            return;
        }
        $form.addClass('loading');
        return $.ajax({
            url: '/user/settings/notifications',
            type: 'post',
            data: $form.serialize(),
            success: function () {
                $form.addClass('m-success').removeClass('error');
            },
            error: function () {
                $form.form('add errors', ['Внутренняя ошибка']);
            },
            complete: function () {
                $form.removeClass('loading');
            }
        });
    },
    /**
     * Сохранить общие настройки профиля
     * @param {HTMLElement} btn элемент формы
    */
    updateUserProfile: function (btn) {
        var $form = $(btn).parents('form');
        if (!$form.form('validate form')) {
            return;
        }
        $form.addClass('loading')
        return $.ajax({
            url: '/User/Settings/UserProfile',
            type: 'post',
            dataType: 'json',
            data: $form.serialize(),
            success: function (resp) {
                if (resp.status == undefined) {
                    resp = JSON.parse(resp);
                }
                if (resp.status == 1 || resp.status == 2) {
                    $form.form('add errors', resp.data);
                } else {
                    $form.addClass('m-success');
                }
            },
            error: function (resp) {
                $form.form('add errors', ['Внутренняя ошибка']);
            },
            complete: function (jqXhr) {
                $form.removeClass('loading');
            }
        });
    },
    /**
     * ОБновить пароль
     * @param {HTMLElement} btn элемент(кнопка) формы
    */
    changePass: function (btn) {
        var $btn = $(btn),
            $form = $btn.parents('.form');
        if (!$form.form('validate form'))
            return false;
        $btn.addClass('loading disabled');
        $.ajax({
            url: '/user/settings/changepassword',
            type: 'post',
            data: $form.serialize(),
            success: function (resp) {
                if (resp.status == 2) {
                    $form.form('add errors', [resp.message]);
                } else {
                    $form.addClass('m-success');
                }
            },
            error: function (jqXhr) {
                $form.form('add errors', ['Внутренняя ошибка']);
            },
            complete: function () {
                $btn.removeClass('loading disabled');
            }
        });
    },
    /**
     * Отправить подтверждение почты
     * @param {HTMLElement} btn кнопка
    */
    sendEmailConfirmation: function (btn) {
        var $btn = $(btn),
            $form = $btn.parents('.form');
        if (!$form.form('validate form'))
            return false;
        $btn.addClass('loading disabled');
        return $.ajax({
            url: '/user/settings/sendemailconfirmation',
            type: 'post',
            data: $form.serialize(),
            success: function (resp) {
                $form.addClass('m-success');
            },
            error: function () {
                $form.form('add errors', ['Ошибка при отправке, попробуйте позже']);
            },
            complete: function () {
                $btn.removeClass('loading disabled');
            }
        });
    },
    changeEmail: function (btn) {
        var $btn = $(btn),
            $form = $btn.parents('.form');
        if (!$form.form('is valid')) {
            return;
        }
        $btn.addClass('loading disabled');
        return $.ajax({
            url: '/user/settings/changeemail',
            type: 'post',
            data: $form.serialize(),
            success: function (resp) {
                var validErrorStatus = Settings.apiStatuses.validationError;
                if (resp.status == undefined) {
                    resp = JSON.parse(resp);
                }
                var $msg = $('#emailSendingFailedMsg');
                if (resp.status == validErrorStatus) {
                    $form.form('add errors', resp.data);
                } else {
                    $form.addClass('m-success');
                }
            },
            error: function (err) {
                $form.form('add errors', ['Внутренняя ошибка']);
            },
            complete: function () {
                $btn.removeClass('loading disabled');
            }
        });
    }
}