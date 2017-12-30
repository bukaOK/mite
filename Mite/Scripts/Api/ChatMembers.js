function ChatMembersLoader(chatId) {
    this.range = 50;
    this.page = 0;
    this.loading = false;
    this.chatId = chatId;
    this.creatorId = $('.chat-item[data-id="' + chatId + '"]').data('creator-id');
    this.modal = $('.chat-members.modal[data-chat-id="' + chatId + '"]');
    this.loader = this.modal.find('.dot-loader-wrapper');
    this.container = this.modal.find('.ui.items');
    this.tmpl = $.templates('#chatMemberTmpl');
    this.initListeners();
}
ChatMembersLoader.prototype.showModal = function () {
    var self = this;
    self.modal.modal('show');
    self.initSource().then(function () {
        self.loadNext();
        Scrolling.init(self.container.find('.item:last-child')[0], function () {
            self.loadNext();
        }, self.container[0]);
    });
}
ChatMembersLoader.prototype.initListeners = function () {
    var self = this;
    this.modal.find('.members-search').on('input', function () {
        self.refresh(this.value);
    });
}
ChatMembersLoader.prototype.initSource = function () {
    var self = this;
    if (self.loading)
        return false;
    self.loader.addClass('active');
    self.page = 0;
    self.loading = true;
    return $.ajax({
        url: '/chatmembers/getbychat/' + self.chatId,
        dataType: 'json',
        success: function (resp) {
            self.items = resp;
            self.container.children('.item').remove();
        },
        error: function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Ошибка при загрузке участников'
            });
        },
        complete: function () {
            self.loader.removeClass('active');
            self.loading = false;
        }
    });
}
/**
 * Следующая страница
 * @param {string} inputVal
*/
ChatMembersLoader.prototype.loadNext = function (inputVal) {
    var self = this,
        offset = self.range * self.page,
        items = [],
        html = '<p class="item" style="justify-content: center">Пользователи не найдены.</p>';
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
        html = self.tmpl.render({
            chatId: self.chatId,
            members: items
        });
    }
    self.container.append(html);
    self.page++;
    self.modal.modal('refresh');
}
ChatMembersLoader.prototype.refresh = function (inputVal) {
    this.page = 0;
    this.container.children('.item').remove();
    this.loadNext(inputVal)
}
var ChatMembers = {
    Api: {
        url: '/chatmembers/',
        /**
         * Добавляем пользователя в чат
         * @param {string} chatId
         * @param {string} userId
         * @param {HTMLElement} btn
        */
        add: function (chatId, userId, btn) {
            var self = ChatMembers.Api;
            $(btn).addClass('loading disabled');
            return self._send(self.url + 'add', 'post', {
                ChatId: chatId,
                UserId: userId
            }).done(function (resp) {
                iziToast.success({
                    title: 'Успех!',
                    message: 'Участник успешно добавлен.'
                    });
                }).then(function () {
                    $(btn).removeClass('loading disabled').parents('.item[role="listitem"]').transition({
                        onComplete: function () {
                            $(this).remove();
                        }
                    });
            });
        },
        /**
         * Исключаем пользователя из чата
         * @param {string} chatId
         * @param {string} userId
         * @param {HTMLElement} btn
        */
        exclude: function (chatId, userId, btn) {
            var self = ChatMembers.Api;
            $(btn).addClass('loading disabled');
            return self._send(self.url + 'exclude', 'post', {
                chatId: chatId,
                userId: userId
            }).done(function (resp) {
                iziToast.success({
                    title: 'Успех!',
                    message: 'Участник успешно исключен.'
                });
            }).then(function () {
                $(btn).removeClass('loading disabled').parents('.item[role="listitem"]').transition({
                    onComplete: function () {
                        $(this).remove();
                    }
                });
            });
        },
        /**
         * Выход пользователя из чата
         * @param {string} chatId
        */
        exit: function (chatId) {
            var self = ChatMembers.Api;
            return self._send(self.url + 'exit', 'post', {
                chatId: chatId
            }).done(function (resp) {
                iziToast.success({
                    title: 'Успех!',
                    message: 'Вы успешно вышли.'
                });
            });
        },
        _send: function (url, method, data) {
            var self = ChatMembers.Api;
            return $.ajax({
                url: url,
                type: method,
                data: data,
                error: function (jqXhr) {
                    iziToast.error({
                        title: 'Упс!',
                        message: 'Внутренняя ошибка'
                    });
                }
            });
        }
    }
}