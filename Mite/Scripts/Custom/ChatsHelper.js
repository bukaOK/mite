var ChatsHelper = {
    /**
     * Сокращаем длину сообщения в элементе чата
     * @param {string} msg сообщение
    */
    truncChatItemMessage: function (msg) {
        var rgx = /<img class="em-img"[^>]+>/g,
            emojies = msg.match(rgx);
        msg = msg.replace(rgx, '');
        if (!msg) {
            msg = emojies.join('');
        } else {
            msg = ViewHelper.truncStr(msg, 16);
        }
        return msg;
    },
    ChatCreating: {
        init: function () {
            var self = this;
            self.settingImage = false;
            self.imgSetLabel = $('#imageSetLabel');
            self.createModal = $('#chatCreateModal').modal({
                allowMultiple: true,
                onApprove: function () {
                    return self.save();
                },
                onHide: function () {
                    self.imgSetLabel.css({
                        'background-image': '',
                        'background-size': '',
                        'background-color': ''
                    });
                }
            });
            self.createForm = $('#chatCreateModal .form');
            self.imageSetModal = $('#chatImageSetModal').modal({
                closable: false,
                allowMultiple: true,
                onVisible: function () {
                    self.crop.croppie('bind');
                },
                onApprove: function () {
                    self.settingImage = true;
                    self.crop.croppie('result', {
                        circle: true
                    }).then(function (res) {
                        self.createForm.find('[name="ImageSrc"]').val(res).data('length', res.length);
                        self.imgSetLabel.removeClass('loading disabled')
                            .css({
                                'background-image': 'url(' + res + ')',
                                'background-size': '100% auto',
                                'background-color': 'white'
                            });
                        self.settingImage = false;
                    });
                },
                onDeny: function () {
                    self.imgSetLabel.removeClass('loading disabled');
                }
            });
            self.crop = $('#chatImageSetModal .crop-container').croppie({
                viewport: {
                    width: 150,
                    height: 150,
                    type: 'circle'
                },
                boundary: {
                    height: 200,
                    width: '100%'
                }
            });
        },
        /**
         * Чтение изображения
         * @param {HTMLElement} target
        */
        read: function (target) {
            var self = this,
                file = target.files[0],
                label = self.imgSetLabel.addClass('loading disabled'),
                reader = new FileReader();
            reader.onload = function () {
                self.crop.croppie('bind', reader.result);
                self.imageSetModal.modal('show');
            };
            reader.readAsDataURL(file);
        },
        save: function () {
            var self = this;
            var imageSrc = self.createForm.find('[name="ImageSrc"]');
            var errors = [];
            if (!self.createForm.form('validate form')) {
                return false;
            }
            if (!imageSrc.val() || imageSrc.val().search(/data:,/) !== -1) {
                errors.push('Изображение не выбрано');
                return false;
            }
            if (imageSrc.data('length') / 1024 / 1024 > 30) {
                errors.push('Слишком большое изображение (Макс. 30 мбайт)');
            }
            if (errors.length > 0) {
                self.createForm.form('add errors', errors);
                return false;
            } else {
                var okBtn = self.createModal.find('.ok.button');
                okBtn.addClass('loading disabled');
                $.ajax({
                    url: '/chats/add',
                    type: 'post',
                    data: {
                        Name: self.createForm.find('[name="Name"]').val(),
                        ImageSrc: imageSrc.val(),
                        //ChatType: $('#ChatType:checked').val()
                        ChatType: $('#ChatType').val()
                    },
                    success: function (resp) {
                        var tmpl = $.templates('#chatItemTmpl');
                        $('.chat-items').prepend(tmpl.render(resp));
                        self.createModal.modal('hide');
                        self.createForm.removeClass('error');
                        iziToast.success({
                            title: 'Успех!',
                            message: 'Чат успешно создан'
                        });
                    },
                    error: function (jqXhr) {
                        self.createForm.form('add errors', ['Внутренняя ошибка']);
                    },
                    complete: function () {
                        okBtn.removeClass('loading disabled');
                    }
                });
            }
        }
    },
    CompanionChatCreating: {
        init: function () {
            var self = ChatsHelper.CompanionChatCreating;
            self.page = 0;
            self.range = 50;
            self.tmpl = $.templates('#possibleCompanionTmpl');
            self.chatsTmpl = $.templates('#chatItemTmpl');
            self.modal = $('#possibleChatCompanions').modal({
                onShow: function () {
                    self.initSource();
                }
            });
            self.loader = self.modal.find('.dot-loader-wrapper');
            self.container = self.modal.find('.list');
        },
        initSource: function () {
            var self = this;
            self.page = 0;
            self.container.children('.item,.message,p').remove();
            self.loader.addClass('active');
            return $.ajax({
                url: '/chats/actualfollowers',
                success: function (resp) {
                    self.loader.removeClass('active');
                    self.items = resp;
                    self.loadNext();
                },
                error: function () {
                    self.container.append('<div class="ui red message">Ошибка при загрузке пользователей.</div>');
                }
            });
        },
        refresh: function (inputVal) {
            self.page = 0;
            self.loadNext(inputVal);
        },
        loadNext: function (inputVal) {
            var self = this,
                offset = self.range * self.page,
                items = [],
                html = '<p style="text-align: center">Пользователи не найдены.</p>';
            if (offset >= self.items.length) {
                if (!self.page) {
                    self.container.append(html);
                }
                return;
            }
            if (!inputVal) {
                items = self.items.slice(offset, offset + self.range);
            } else {
                items = self.items.filter(function (member) {
                    return member.User.UserName.toLowerCase().search(inputVal.toLowerCase()) !== -1;
                });
            }
            if (self.page || items.length) {
                html = self.tmpl.render(items);
            }
            self.container.append(html);
            self.page++;
            self.modal.modal('refresh');
        },
        Events: {
            click: function (userId, userName) {
                var self = ChatsHelper.CompanionChatCreating;
                self.modal.modal('hide');
                $.post('/chats/createcompanionchat', {
                    companionId: userId
                }, function (resp) {
                    resp.CreatorId = resp.CurrentUser.Id;
                    ChatsApi.Loader.loader.after(self.chatsTmpl.render(resp));
                    ChatsApi.Loader.initChat(resp.Id, userName, resp.ChatType);
                }).fail(function () {
                    iziToast.error({
                        title: 'Упс!',
                        message: 'Ошибка при загрузке собеседников'
                    });
                });
            }
        }
    }
}