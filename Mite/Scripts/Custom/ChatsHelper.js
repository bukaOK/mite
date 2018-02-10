var ChatsHelper = {
    /**
     * Сокращаем длину сообщения в элементе чата
     * @param {string} msg сообщение
     * @param {number} length длина
    */
    truncChatItemMessage: function (msg, length) {
        if (!length)
            length = 16;
        var rgx = /<img class="em-img"[^>]+>/g,
            emojies = msg.match(rgx);
        msg = msg.replace(rgx, '');
        if (!msg) {
            msg = emojies.join('');
        } else {
            msg = ViewHelper.truncStr(msg, length);
        }
        return msg;
    },
    /**
     * Пересчитать кол-во новых сообщений в главном меню
     * @param {number} minusVal отнимаемое кол-во сообщений
    */
    recountLabel: function (minusVal) {
        var chatsLabel = $('.user-chats.item .label'),
            oldVal = +chatsLabel.text(),
            newVal = oldVal - minusVal;
        chatsLabel.text(newVal > 0 ? newVal : 0);
    },
    ChatCreating: {
        init: function () {
            var self = this;
            self.settingImage = false;
            self.imgSetLabel = $('#imageSetLabel');
            self.createForm = $('#chatCreateModal .form').keydown(function (ev) {
                if (ev.keycode === 13) {
                    ev.preventDefault();
                    self.save();
                }
            });
            self.createModal = $('#chatCreateModal').modal({
                allowMultiple: true,
                onApprove: function () {
                    self.save();
                },
                onHide: function () {
                    self.fullImg = null;
                    self.createForm.find('[name="ImageSrc"]').val('').data('length', 0);
                    self.createForm.find('[name="Name"]').val('');
                    self.createForm.find('[name="MaxMembersCount"]').val('');
                    self.imgSetLabel.css({
                        'background-image': '',
                        'background-size': '',
                        'background-color': ''
                    });
                }
            });
            self.imageSetModal = $('#chatImageSetModal').modal({
                closable: false,
                allowMultiple: true,
                onVisible: function () {
                    self.crop.croppie('bind', self.fullImg);
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
                progress = self.createModal.find('.progress'),
                file = target.files[0],
                reader = new FileReader();
            reader.onload = function () {
                self.fullImg = reader.result;
                self.imageSetModal.modal('show');
            };
            reader.onloadstart = function () {
                progress.show().progress();
            };
            reader.onprogress = function (ev) {
                if (ev.lengthComputable) {
                    var percentLoaded = Math.round((ev.loaded / ev.total) * 100);
                    progress.progress('set percent', percentLoaded);
                }
            };
            reader.onloadend = function () {
                progress.progress('set percent', 100);
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
                        ChatType: self.createForm.find('[name=ChatType]:checked').val()
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
            self.container.children('.item,.message').remove();
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
            var self = this;
            self.container.children('.item').remove();
            self.page = 0;
            self.loadNext(inputVal);
        },
        loadNext: function (inputVal) {
            var self = this,
                offset = self.range * self.page,
                items = [],
                emptyMsg = self.container.children('.empty');
            if (offset >= self.items.length) 
                return;

            if (!inputVal) {
                items = self.items.slice(offset, offset + self.range);
            } else {
                items = self.items.filter(function (member) {
                    return member.UserName.toLowerCase().search(inputVal.toLowerCase()) !== -1;
                });
            }
            if (items.length === 0 && self.page === 0) {
                emptyMsg.addClass('active');
            } else {
                emptyMsg.removeClass('active');
            }
            if (self.page || items.length) {
                self.container.append(self.tmpl.render(items));
            }
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