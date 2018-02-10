/**
 * Класс чата
 * @typedef {{Id: string, AvatarSrc: string, UserName: string}} ShortUser
 * @param {string} chatId id чата
 * @param {string} chatName название чата
 * @param {number} chatType тип чата
 * @param {ShortUser} currentUser текущий пользователь
 */
function Chat(chatId, chatName, chatType, currentUser) {
    var tmpl = $.templates('#chatTmpl');
    this.chatId = chatId;
    this.chatItem = $('.chat-item[data-id="' + chatId + '"]');
    if ($('.chat.feed[data-id="' + chatId + '"]').length === 0) {
        $('#chatContext').append(tmpl.render({
            Id: chatId,
            Name: chatName,
            ChatType: chatType,
            CreatorId: this.chatItem.data('creatorId'),
            ImageSrc: this.chatItem.children('.image').attr('src'),
            MaxMembersCount: this.chatItem.data('maxMembersCount')
        }));
    }
    this.creatorId = this.chatItem.data('creatorId');
    this.chatWrap = $('.chat-wrap[data-id="' + chatId + '"]');
    this.initForm(currentUser);
    this.ChatLoader = new ChatLoader(chatId, currentUser.Id);
    this.MsgFiles = new MsgFiles(chatId);
    this.ChatMembersLoader = new ChatMembersLoader(chatId);
    this.ChatFollowersLoader = new ChatFollowersLoader(chatId);
    this.ChatUpdater = new ChatUpdater(chatId);
    //this.ChatLoader.loadNext();
    this.initListeners();
    this.hasNew = false;
}
Chat.prototype.readMessages = function () {
    var self = this,
        chatHub = $.connection.chatHub,
        chatId = this.chatId,
        newLabelVal = self.ChatLoader.chat.children('.event[data-status="new"]').length;
    window.hubReady.done(function () {
        chatHub.server.readMessages(chatId);
    });
    ChatsHelper.recountLabel(newLabelVal);
}
Chat.prototype.isInView = function () {
    return document.hasFocus() && this.chatWrap.css('display') === 'block';
}
Chat.prototype.initListeners = function () {
    var self = this;
    self.chatWrap.find('.chat-menu').dropdown({
        selectOnKeydown: false,
        action: 'hide',
        context: self.chatWrap,
        onChange: function (value, text, $selectedItem) {
            switch (value) {
                case 'mem':
                    self.ChatMembersLoader.showModal();
                    break;
                case 'addMem':
                    self.ChatFollowersLoader.showModal();
                    break;
                case 'updateChat':
                    self.ChatUpdater.showModal();
                    break;
                default:
                    throw 'Unknown value';
            }
        }
    });
}
/**
 * Инициализация формы с полем сообщения и эмодзи
 * @param {Object} currentUser 
*/
Chat.prototype.initForm = function (currentUser) {
    var chatId = this.chatId,
        $form = $('.form[data-chat-id="' + chatId + '"]');
    //init send button
    $form.find('.msgSend.button').click(function () {
        ChatMessages.Api.add(chatId, currentUser, $form.find('.dialog-area').html());
    }).popup({
        inline: true,
        delay: {
            show: 150,
            hide: 0
        },
        position: 'bottom center'
    });
    new PerfectScrollbar('.em-wrapper', {
        useBothWheelAxes: true
    });
    //init emoji
    $form.find('.emoji-btn').popup({
        inline: true,
        hoverable: true,
        position: 'top center'
    });
    $form.find('.em-col').click(function () {
        EmojiHelper.addToArea($form.find('.dialog-area')[0], this.childNodes[0].cloneNode());
    }).mousedown(function (e) {
        e.preventDefault();
        e.stopPropagation();
    });
    $form.find('.emoji .tabs .item').tab();
    $form.find('.emoji .tab').each(function (index, elem) {
        new PerfectScrollbar(elem);
    });
    //init dialog-area
    $form.find('.dialog-area').keydown(function (ev) {
        var self = this;
        if (ev.which === 13 && ev.shiftKey === false) {
            ChatMessages.Api.add(chatId, currentUser, $(self).html());
            return false;
        }
    });
}
var ChatsApi = {
    /**
     * Удаление чата
     * @param {string} chatId
     * @param {Event} ev
    */
    remove: function (chatId, ev) {
        var chatItem = $('.chat-item[data-id="' + chatId + '"]'),
            chatWrap = $('.chat-wrap[data-id="' + chatId + '"]');
        ev.stopPropagation();
        return $.ajax({
            type: 'post',
            url: '/chats/remove/' + chatId,
            success: function () {
                chatItem.transition({
                    animation: 'fade left',
                    onComplete: function () {
                        $(this).remove();
                    }
                });
                if (chatItem.hasClass('active')) {
                    chatItem.removeClass('active');
                    if (chatWrap) {
                        chatWrap.transition({
                            animation: 'fade left',
                            onHide: function () {
                                $('.start.chat-wrap').addClass('active').transition('fade left');
                            }
                        }).removeClass('active');
                    }
                }
            },
            error: function () {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при удалении чата'
                });
            }
        });
    },
    /**
     * Создать чат с пользователем(в конце редирект)
     * @param {string} companionId id собеседника
     * @param {HTMLElement} btn кнопка
    */
    createCompanionChat: function (companionId, btn) {
        $(btn).addClass('loading disabled')
        return $.ajax({
            type: 'post',
            url: '/chats/createcompanionchat',
            data: {
                companionId: companionId
            },
            success: function (resp) {
                location.href = '/user/chats?chat=' + resp.Id;
            },
            error: function () {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при создании чата'
                });
            },
            complete: function () {
                $(btn).removeClass('loading disabled');
            }
        });
    },
    Loader: {
        page: 0,
        range: 30,
        loading: false,
        initialized: false,
        init: function (currentUser) {
            var self = ChatsApi.Loader;
            self.initialized = true;
            self.container = $('.chat-items');
            self.loader = $('.chat-items .dot-loader-wrapper');
            self.currentUser = currentUser;
            self.tmpl = $.templates('#chatItemTmpl');
            self.scrollbar = new PerfectScrollbar(self.container.parent()[0]);
            window.chats = new Object();
            Scrolling.init(self.loader[0], self.loadNext, self.container.parent()[0]);
            self.initSource().then(function () {
                var items = self.items, nChat,
                    hasUrlChat = UrlHelper.getObject() && UrlHelper.getObject().chat;
                if (items.length > 0 && hasUrlChat) {
                    var nIndex = items.findIndex(function (elem) {
                        return elem.Id === UrlHelper.getObject().chat;
                    });
                    nChat = items[nIndex];
                    items.unshift(nChat);
                    items.splice(nIndex, 1);
                }
                self.loadNext();
                if (items.length > 0 && hasUrlChat) {
                    nChat = items[0];
                    self.initChat(nChat.Id, nChat.Name, nChat.Type);
                }
            });
        },
        /**
         * Создаем чат
         * @param {string} chatId id чата
         * @param {string} chatName имя чата
         * @param {number} chatType тип чата
        */
        initChat: function (chatId, chatName, chatType) {
            var lastChatId = $('.chat-items .chat-item.active').removeClass('active').data('id');
            $('.chat-item[data-id="' + chatId + '"]').addClass('active')
                .find('.ui.msg-count.label').removeClass('active').text(0);
            if (!window.chats[chatId]) {
                window.chats[chatId] = new Chat(chatId, chatName, chatType, this.currentUser);
                window.chats[chatId].ChatLoader.loadNext().then(function () {
                    window.chats[chatId].readMessages();
                });
            } else {
                window.chats[chatId].readMessages();
            }
            $('.chat-wrap.active').removeClass('active');
            if (lastChatId) {
                $('.chat[data-id="' + lastChatId + '"] .event[data-status="new"]').each(function (index, elem) {
                    ChatMessages.changeStatus($(elem), ChatMessages.Api.statuses.readed);
                });
                window.chats[lastChatId].hasNew = false;
            }
            $('.chat.feed[data-id="' + chatId + '"]').parents('.chat-wrap').addClass('active');
        },
        initSource: function () {
            var self = ChatsApi.Loader,
                hasUrlChat = UrlHelper.getObject() && UrlHelper.getObject().chat;
            self.loader.addClass('active');
            return $.ajax({
                url: '/chats/getbyuser',
                success: function (resp) {
                    self.items = resp;
                },
                data: {
                    writingChat: hasUrlChat ? UrlHelper.getObject().chat : null
                },
                error: function () {
                    iziToast.error({
                        title: 'Упс!',
                        message: 'Ошибка при загрузке чатов'
                    });
                },
                complete: function () {
                    self.loader.removeClass('active');
                }
            });
        },
        refresh: function (inputVal) {
            var self = ChatsApi.Loader;
            self.page = 0;
            $('.chat-items .chat-item').hide();
            self.loadNext(inputVal);
        },
        /**
         * Грузим следующую страничку
         * @param {string} inputVal значение при поиске
        */
        loadNext: function (inputVal) {
            var self = ChatsApi.Loader,
                emptyChats = $('.empty-chats'),
                offset = self.range * self.page,
                items = [];
            if (offset >= self.items.length && $('.chat-items .chat-item:visible').length === 0) {
                emptyChats.addClass('active');
                return;
            }
            if (!inputVal || inputVal === '') {
                items = self.items.slice(offset, offset + self.range);
            } else {
                items = self.items.filter(function (chatItem) {
                    return chatItem.Name.toLowerCase().search(inputVal.toLowerCase()) !== -1;
                });
            }
            if (items.length === 0 && self.page === 0) {
                emptyChats.addClass('active');
            } else {
                emptyChats.removeClass('active');
            }
            var itemsToShow = [],
                itemsToRender = items.filter(function (item) {
                    var itemExist = $('.chat-items .chat-item[data-id="' + item.Id + '"]').length > 0;
                    if (itemExist)
                        itemsToShow.push(item);
                    return !itemExist;
                });

            itemsToRender.forEach(function (chatItem) {
                if (chatItem.LastMessage) {
                    var dateIso = chatItem.LastMessage.SendDate,
                        now = new Date();
                    dateIso += dateIso[dateIso.length - 1] === 'Z' ? '' : 'Z';
                    var sendDate = new Date(dateIso),
                        nowDateStr = DateTimeHelper.toDateString(now, 'long'),
                        sendDateStr = DateTimeHelper.toDateString(sendDate, 'long');
                    chatItem.LastMessage.SendDateStr = sendDateStr === nowDateStr
                        ? 'Сегодня' : sendDateStr;

                    chatItem.LastMessage.Message = ChatsHelper.truncChatItemMessage(chatItem.LastMessage.Message);
                    chatItem.Name = ViewHelper.truncStr(chatItem.Name, 15);
                }
            });
            self.loader.before(self.tmpl.render(itemsToRender));
            itemsToShow.forEach(function (item) {
                $('.chat-items .chat-item[data-id="' + item.Id + '"]').show();
            });
            self.container.imagesLoaded().progress(function () {
                self.scrollbar.update();
            });
            self.page++;
        }
    }
}
