/**
 * Класс для работы с модальными для обновления данных чатов
 * @param {string} chatId
 */
function ChatUpdater(chatId) {
    var self = this;
    self.chatId = chatId;
    self.updateModal = $('.chat-updating.modal[data-chat-id="' + chatId + '"]').modal({
        allowMultiple: true,
        onApprove: function () {
            self.save();
        }
    });
    self.imgSetLabel = self.updateModal.find('.chat-create-img');
    self.updateForm = self.updateModal.find('.form').form({
        on: 'submit',
        fields: {
            Name: {
                rules: [
                    {
                        type: 'empty',
                        prompt: 'Введите название'
                    }
                ]
            },
            MaxMembersCount: {
                optional: true,
                rules: [
                    {
                        type: 'integer',
                        prompt: 'Требуется целое кол-во участников '
                    }
                ]
            }
        }
    });
    self.updateForm.find('.ui.checkbox').checkbox();
    self.imageSetModal = $('.chat-set.modal[data-id="' + chatId + '"]').modal({
        allowMultiple: true,
        closable: false,
        onVisible: function () {
            self.crop.croppie('bind', self.fullImageData);
        },
        onApprove: function () {
            self.crop.croppie('result', {
                circle: true,
                type: 'base64'
            }).then(function (res) {
                self.croppedImage = res;
                self.imgSetLabel.css({
                    'background-image': 'url(' + res + ')',
                    'background-size': '100% auto',
                    'background-color': 'white'
                });
            });
        }
    });
    self.crop = self.imageSetModal.find('.crop-container').croppie({
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
    self.initListeners();
}
ChatUpdater.prototype.showModal = function () {
    this.updateModal.modal('show');
}
ChatUpdater.prototype.initListeners = function () {
    var self = this;
    self.imgSetLabel.children('[type="file"]').on('change', function (ev) {
        var input = this,
            progress = self.updateModal.find('.progress'),
            file = ev.target.files[0],
            reader = new FileReader(),
            fileType = file.type.split('/')[0];
        if (fileType !== 'image') {
            self.updateForm.form('add errors', ['Неизвестный тип файла']);
            return;
        } else {
            self.updateForm.removeClass('error');
        }
        reader.onload = function () {
            self.fullImageData = reader.result;
            self.imageSetModal.modal('show');
        };
        reader.onloadstart = function (ev) {
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
            $(input).val(null);
        };
        reader.readAsDataURL(file);
    });
    self.updateForm.keypress(function (ev) {
        if (ev.keycode === 13) {
            ev.preventDefault();
            self.save();
        }
    });
}
ChatUpdater.prototype.save = function () {
    var self = this,
        errors = [];
    if (!self.updateForm.form('validate form')) {
        return false;
    }
    if (self.croppedImage && self.croppedImage.length / 1024 / 1024 > 30) {
        errors.push('Слишком большое изображение (Макс. 30 мбайт)');
    }
    if (errors.length > 0) {
        self.updateForm.form('add errors', errors);
        return false;
    } else {
        var updateForm = self.updateForm;

        self.updateForm.find('[name="ImageSrc"]').val(self.croppedImage);
        $.ajax({
            url: '/chats/update',
            type: 'post',
            data: self.updateForm.serialize(),
            success: function (resp) {
                var chatItem = $('.chat-item[data-id="' + self.chatId + '"]'),
                    name = self.updateForm.find('[name="Name"]').val();
                chatItem.children('.image').attr('src', self.croppedImage);
                chatItem.find('.content>.header>.content').text(name);

                $('.chat-wrap[data-id="' + self.chatId + '"] .chat-header .ui.header>b').text(name);
                iziToast.success({
                    title: 'Успех!',
                    message: 'Чат успешно обновлен'
                });
            },
            error: function (jqXhr) {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Внутренняя ошибка'
                });
            }
        });
    }
}