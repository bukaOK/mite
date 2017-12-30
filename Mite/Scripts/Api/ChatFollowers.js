/**
 * Класс для загрузки пользователей для чата
 * @param {string} chatId
 */
function ChatFollowersLoader(chatId) {
    this.chatId = chatId;
    this.page = 0;
    this.range = 50;
    this.modal = $('.chat-followers[data-chat-id="' + chatId + '"]');
    this.container = this.modal.find('.ui.items');
    this.loader = this.container.find('.dot-loader-wrapper');
    this.tmpl = $.templates('#chatFollowerTmpl');
    this.initListeners();
}
ChatFollowersLoader.prototype.showModal = function () {
    var self = this;
    self.modal.modal('show');
    self.initSource().then(function () {
        self.loadNext();
        Scrolling.init(self.container.find('.item:last-child')[0], function () {
            self.loadNext();
        }, self.container[0]);
    });
}
ChatFollowersLoader.prototype.initListeners = function () {
    var self = this;
    this.modal.find('.foll-search').on('input', function () {
        self.refresh(this.value);
    });
}
ChatFollowersLoader.prototype.refresh = function (inputVal) {
    this.page = 0;
    this.container.children('.item').remove();
    this.loadNext(inputVal);
}
ChatFollowersLoader.prototype.initSource = function () {
    var self = this;
    if (self.loading)
        return false;
    self.loader.addClass('active');
    self.loading = true;
    return $.ajax({
        url: '/chats/chatfollowers/',
        data: {
            chatId: self.chatId
        },
        dataType: 'json',
        success: function (resp) {
            self.items = resp;
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
ChatFollowersLoader.prototype.loadNext = function (inputVal) {
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
            followers: items
        });
    }
    self.container.append(html);
    self.page++;
    self.modal.modal('refresh');
}