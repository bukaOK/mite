/**
 * Класс открытого чата
 * @typedef {{Id: string, AvatarSrc: string, UserName: string}} ShortUser
 * @param {string} chatId id чата
 * @param {string} chatName название чата
 * @param {ShortUser} currentUser текущий пользователь
 */
function PublicChat(chatId, chatName, currentUser) {
    var tmpl = $.templates('#publicChatTmpl'),
        chatItem = $('.chat-item[data-id="' + chatId + '"]');
    this.chatId = chatId;
    this.chatItem = chatItem;
    if ($('.chat.feed[data-id="' + chatId + '"]').length === 0) {
        $('#publicChatContext').append(tmpl.render({
            Id: chatId,
            Name: chatName,
            ChatType: 1,
            CreatorId: chatItem.data('creatorId'),
            ImageSrc: chatItem.children('.image').attr('src'),
            MaxMembersCount: chatItem.data('maxMembersCount')
        }));
    }
    this.creatorId = this.chatItem.data('creatorId');
    this.chatWrap = $('.chat-wrap[data-id="' + chatId + '"]');
    this.ChatLoader = new ChatLoader(chatId, currentUser.Id, true);
}
var PublicChats = {
    chats: new Object(),
    showChat: function (elem) {
        $(elem).parents('.chat-wrap').removeClass('active');
        $('.public-chat-items').parents('.column').first().show();
    },
    Loader: {
        page: 0,
        range: 30,
        initialized: false,
        loading: false,
        ended: false,
        /**
         * Инициализация загрузчика чатов
         * @param {ShortUser} currentUser текущий пользователь
        */
        init: function (currentUser) {
            var self = this;
            self.initialized = true;
            self.tmpl = $.templates('#publicChatItemTmpl');
            self.container = $('.public-chat-items');
            self.loader = self.container.find('.dot-loader-wrapper');
            self.currentUser = currentUser;
            Scrolling.init(self.loader[0], self.loadNext, self.container.parent()[0]);
            self.loadNext().then(function (resp) {
                if (resp.length === 0) {
                    self.container.children('.empty-chats').addClass('active');
                }
            });
        },
        initChat: function (chatId, chatName) {
            var chats = PublicChats.chats,
                lastChatId = $('.public-chat-items .chat-item.active').removeClass('active').data('id');
            $('.chat-item[data-id="' + chatId + '"]').addClass('active');
            if (!chats[chatId]) {
                chats[chatId] = new PublicChat(chatId, chatName, this.currentUser);
                chats[chatId].ChatLoader.loadNext();
            }
            $('#publicChatContext .chat-wrap.active').removeClass('active');
            $('.chat.feed[data-id="' + chatId + '"]').parents('.chat-wrap').addClass('active');
            if (window.innerWidth <= 767) {
                $('.public-chat-items').parents('.column').first().hide();
            }
        },
        /**
         * Подгружаем следующую страницу
         * @param {string} input поиск
        */
        loadNext: function (input) {
            var self = this;
            self.loader.addClass('active');
            return $.ajax({
                url: '/chats/getpublished',
                data: {
                    page: self.page,
                    input: input ? input: null
                },
                success: function (resp) {
                    resp.forEach(function (item) {
                        item.MembersWord = ViewHelper.getWordCase(item.MembersCount, 'участник', 'участника', 'участников');
                    });
                    self.loader.before(self.tmpl.render(resp));
                    if (resp.length < self.range)
                        self.ended = true;
                    else
                        self.ended = false;
                    self.page++;
                },
                complete: function () {
                    self.loader.removeClass('active');
                }
            });
        },
        /**
         * Обновление списка чатов при поиске
         * @param {string} inputVal Имя чата для поиска
        */
        refresh: function (inputVal) {
            $('.public-chat-items .chat-item').hide();
        }
    }
}